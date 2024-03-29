use std::rc::Rc;
use std::slice;
use std::mem;

use image::*;

use crate::renderer::Renderer;
use crate::common::{IVec2, IAABB, Vec3};

use crate::scene::road::Road;
use crate::scene::camera::Camera;

struct SpriteDescr {
    pos_x : u32,
    pos_y : u32,

    width : u32,
    height : u32
}

struct Lod{
    image : RgbaImage,
    width : u32
}

#[derive(Clone)]
pub struct Billboard{
    // Position : x is offset, y is height, z is road distance. 
    pub position : Vec3,
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

    pub fn from_lod_images(images : Vec<RgbaImage>) -> BillboardFactory {
        let mut lods = Vec::with_capacity(images.len());
        for img in images { 
            let width = img.width();
            lods.push(Lod { image : img, width : width }); 
        }
        BillboardFactory { lods : Rc::from(lods) }
    }

    pub fn new(spritesheet : &RgbaImage, meta_file_content : &[u8]) -> BillboardFactory {
        let lods = Self::load_lods_from_file(spritesheet, meta_file_content);
        BillboardFactory { lods : Rc::from(lods) }
    }

    pub fn construct(&self, position : Vec3, width : f32) -> Billboard {
        Billboard { lods : self.lods.clone(), position, width }
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

    fn render_cutted(
        &self, road : &Road, renderer : &Renderer, camera : &Camera, 
        lod : &Lod, draw_y : isize, min_visible_height : f32, dist_to_camera : f32
    ) {
        let min_visible_image_y = min_visible_height - road.get_height(self.position.z) - self.position.y;

        let px_height = (camera.viewport_height / renderer.height() as f32) * dist_to_camera / camera.near_plane;
        let mut min_visible_image_y_px = (min_visible_image_y / px_height) as isize;
        if min_visible_image_y_px < 0 { 
            min_visible_image_y_px = 0; 
        }

        let draw_region = IAABB::new(
            IVec2::new(0, min_visible_image_y_px), 
            IVec2::new(lod.image.width() as isize - 1, lod.image.height() as isize - 1)
        );
        if draw_region.min.y >= draw_region.max.y { return; }

        let offset = (self.position.x + road.get_offset(self.position.z)) * camera.near_plane / (dist_to_camera);       
        let offset_px = (offset * renderer.width() as f32) as isize - lod.image.width() as isize / 2;
        let left_bottom = IVec2::new(renderer.width() as isize / 2 + offset_px, draw_y);
        renderer.draw_subimage(&lod.image, draw_region, left_bottom);
    }   

    pub(super) fn render(&self, camera : &Camera, road : &Road, renderer : &Renderer) {
        let mut dist_to_camera = self.position.z - camera.position.z;
        if dist_to_camera < 0.0 {
            dist_to_camera += road.get_length();
        }
        let width_px = renderer.width() as f32 * self.width * camera.near_plane / dist_to_camera;
        let lod = &self.lods[self.get_lod_id(width_px) as usize];

        // process (camera's near plane .. last segment)
        for y in -1..road.get_line_count() as isize - 1 {
            let distance_proj = if y == -1 { 
                (camera.position.z + camera.near_plane) % road.get_length()
            } else { 
                road.get_distance_proj(y as usize).unwrap() 
            };
            let next_distance_proj = road.get_distance_proj((y + 1) as usize).unwrap();
            let next_distance_proj_global = 
            if distance_proj > next_distance_proj { 
                next_distance_proj + road.get_length()
            } else { 
                next_distance_proj 
            };

            let visible = 
            next_distance_proj_global - self.position.z >= -std::f32::EPSILON && 
            self.position.z - distance_proj >= -std::f32::EPSILON;
            if !visible { continue; }

            let global_camera_y = camera.position.y + road.get_height(camera.position.z);
            // interpolate height between camera and next distance
            let min_visible_height = 
            (global_camera_y * (next_distance_proj_global - self.position.z) 
            + road.get_height(next_distance_proj) * (self.position.z - camera.position.z))
            / (next_distance_proj_global - camera.position.z);

            self.render_cutted(
                road, renderer, camera, 
                lod, y as isize + 1, min_visible_height, dist_to_camera
            );
            return;
        }

        // process (last segment .. camera's far plane)
        let last_y_distance = road.get_distance_proj(road.get_line_count() - 1).unwrap();
        let camera_far_plane_global = camera.position.z + camera.far_plane;
        let mut last_y_distance_to_cam = last_y_distance - camera.position.z;
        if last_y_distance_to_cam < 0.0 { last_y_distance_to_cam += road.get_length(); }
        if camera_far_plane_global > self.position.z && self.position.z > last_y_distance {
            let global_camera_y = camera.position.y + road.get_height(camera.position.z);
            // interpolate height between camera and last y's distance
            let min_visible_height = 
            global_camera_y + 
            (road.get_height(last_y_distance) - global_camera_y) 
            / last_y_distance_to_cam * dist_to_camera;

            self.render_cutted(
                road, renderer, camera, 
                lod, road.get_line_count() as isize, min_visible_height, dist_to_camera
            );
        }
    }
}