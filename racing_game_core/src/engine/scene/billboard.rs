use std::rc::Rc;
use std::slice;
use std::mem;

use image::*;

use crate::engine::renderer::Renderer;
use crate::engine::common::{IVec2, IAABB};

use super::road::Road;
use super::camera::Camera;

struct SpriteDescr {
    pos_x : u32,
    pos_y : u32,

    width : u32,
    height : u32
}

struct Lod{
    pub image : RgbaImage,
    width : u32
}

#[derive(Clone)]
pub struct Billboard{
    pub road_distance : f32,
    pub offset : f32,
    pub width : f32,

    lods : Rc<Vec<Lod>>
}

pub struct BillboardFactory {
    lods : Rc<Vec<Lod>>
}

impl BillboardFactory {
    fn load_lods_from_file(spritesheet : &RgbaImage, meta_file_content : &[u8]) -> Vec<Lod> {
        let sprites_data_raw = meta_file_content.as_ptr() as *const SpriteDescr;
        let sprites_data_count = meta_file_content.len() / mem::size_of::<SpriteDescr>();
        let sprites_data = unsafe { slice::from_raw_parts(sprites_data_raw, sprites_data_count) };

        let mut lods : Vec<Lod> = Vec::with_capacity(sprites_data.len());
        for sprite_data in sprites_data {
            let mut lod = Lod { image : RgbaImage::new(sprite_data.width, sprite_data.height), width : sprite_data.width };

            for y in 0..sprite_data.height {
                for x in 0..sprite_data.width {
                    lod.image.put_pixel(x, y, *spritesheet.get_pixel(x + sprite_data.pos_x, y + sprite_data.pos_y));
                }
            }

            lods.push(lod);
        }

        lods
    }

    pub fn new(spritesheet : &RgbaImage, meta_file_content : &[u8]) -> BillboardFactory {
        let lods = Self::load_lods_from_file(spritesheet, meta_file_content);
        BillboardFactory { lods : Rc::from(lods) }
    }

    pub fn construct(&self, road_distance : f32, offset : f32, width : f32) -> Billboard {
        Billboard { lods : self.lods.clone(), road_distance, offset, width }
    }
}  

impl Billboard{
    fn get_lod_id(&self, width_px : f32) -> u32 {
        let mut closest_lod = 0u32;
        for i in 0..self.lods.len() {
            if self.lods[i].width as f32 > width_px { 
                closest_lod += 1;
                if closest_lod >= self.lods.len() as u32 { closest_lod = self.lods.len() as u32 - 1; } 
            } else { 
                if i == 0 { return 0; }

                let to_curr_lod = self.lods[i - 1].width as f32 - width_px;
                let to_next_lod = width_px - self.lods[i].width as f32;

                return if to_next_lod > to_curr_lod { closest_lod - 1 } else { closest_lod }
            }
        }

        return closest_lod;
    }

    pub fn render(&self, camera : &Camera, road : &Road, renderer : &Renderer) {
        let mut dist_to_camera = self.road_distance - camera.distance;
        if dist_to_camera < 0.0 {
            dist_to_camera += road.get_length();
        }
        let width_px = renderer.width() as f32 * self.width * camera.near_plane / dist_to_camera;
        let lod = &self.lods[self.get_lod_id(width_px) as usize];

        // process 2 more segments : 
        // (camera.near_plane..first ground intersection) <- billboards near the camera
        // and 
        // (last ground intersection..camera.far_plane) <- billboards hidden by the last heel
        for y in (0..road.get_line_count() + 1).rev() {
            let next_y_dist_proj = road.get_distance_proj(y - 1).unwrap_or(camera.near_plane + camera.distance);
            let mut distance_proj = road.get_distance_proj(y).unwrap_or(camera.distance + camera.far_plane);
            if distance_proj < next_y_dist_proj { 
                distance_proj += road.get_length(); 
            }

            let mut visible = distance_proj >= self.road_distance && next_y_dist_proj <= self.road_distance;
            let road_distance_next_lap = self.road_distance + road.get_length();
            visible = visible || distance_proj >= road_distance_next_lap && next_y_dist_proj <= road_distance_next_lap;
            if !visible { continue; }

            let global_camera_y = camera.y + road.get_height(camera.distance);

            let min_visible_height = if y != 0 {
                // normal occlusion mode (first ground intersection..camera.far_plane)
                let mut next_y_dist_to_camera = next_y_dist_proj - camera.distance;
                if next_y_dist_to_camera < 0.0 {
                    next_y_dist_to_camera += road.get_length();
                }

                global_camera_y + 
                (road.get_height(next_y_dist_proj) - global_camera_y) / next_y_dist_to_camera * dist_to_camera 
            } else {
                // close occlusion mode (camera.near_plane..first ground intersection)
                let y0_distance_proj = road.get_distance_proj(0).unwrap();
                let y0_height = road.get_height(y0_distance_proj);
                global_camera_y - (global_camera_y - y0_height) / (y0_distance_proj - camera.distance) * dist_to_camera
            };

            let min_visible_image_y = min_visible_height - road.get_height(self.road_distance);
            let px_height = (camera.viewport_height / renderer.height() as f32) * dist_to_camera / camera.near_plane;
            let mut min_visible_image_y_px = (min_visible_image_y / px_height) as isize;
            if min_visible_image_y_px < 0 { min_visible_image_y_px = 0; }

            let draw_region = IAABB::new(
                IVec2::new(0, min_visible_image_y_px), 
                IVec2::new(lod.image.width() as isize - 1, lod.image.height() as isize - 1)
            );
            if draw_region.min.y >= draw_region.max.y { break; }
            
            let offset = (self.offset + road.get_offset(self.road_distance)) * camera.near_plane / (dist_to_camera);       
            let offset_px = (offset * renderer.width() as f32) as isize - lod.image.width() as isize / 2;
            let left_bottom = IVec2::new(renderer.width() as isize / 2 + offset_px, y as isize);
            renderer.draw_subimage(&lod.image, draw_region, left_bottom);
        }
    }
}