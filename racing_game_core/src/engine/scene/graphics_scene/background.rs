use image::*;

use crate::engine::renderer::Renderer;
use crate::engine::common::{IVec2, IAABB};

use super::road::Road;
use crate::engine::scene::camera::Camera;

pub struct Background {
    image : RgbImage,
    visible_center_y : u32,
    x_offset : u32
}

impl Background {
    // Visible_center_y :
    // y coord of image that will be visible when looking horizontally from camera position.
    pub fn new(image : RgbImage, visible_center_y : u32) -> Background {
        Background { image, visible_center_y, x_offset : 0 }
    }

    pub fn set_offset(&mut self, offset : u32) {
        let img_width = self.image.width();
        self.x_offset = (offset % img_width + img_width) % img_width;
    }

    pub fn render(&self, road : &Road, camera : &Camera, renderer : &Renderer) {
        assert_eq!(renderer.width() <= self.image.width(), true);

        let near_plane_center_offset = camera.near_plane * camera.angle;
        let near_plane_center_offset_px = (near_plane_center_offset / camera.viewport_height) * (renderer.height() as f32);
        let image_center_screen_y = renderer.height() as isize / 2 - near_plane_center_offset_px as isize;
        let render_y = image_center_screen_y - self.visible_center_y as isize;
        
        let mut draw_region = IAABB::new(
            IVec2::zero(), 
            IVec2::new(self.image.width() as isize - 1, self.image.height() as isize - 1)
        );

        draw_region.min.y = road.get_line_count() as isize - render_y;
        if draw_region.min.y < 0 { draw_region.min.y = 0; }

        let read_x = (
            self.x_offset - renderer.width() / 2 + self.image.width() / 2 
            + self.image.width() // Because remainder, not modulo.
        ) % self.image.width();

        draw_region.min.x = read_x as isize;
        draw_region.max.x = (read_x + renderer.width()) as isize;

        if draw_region.max.x <= self.image.width() as isize - 1 { // Don't repeat.
            renderer.draw_subimage(&self.image, draw_region, IVec2::new(0, road.get_line_count() as isize));
        } else { // Repeat.
            let mut draw_region_0 = IAABB::new(draw_region.min.clone(), draw_region.max.clone());
            let mut draw_region_1 = IAABB::new(draw_region.min.clone(), draw_region.max.clone());

            draw_region_0.max.x = self.image.width() as isize - 1;
            
            draw_region_1.min.x = 0;
            draw_region_1.max.x = draw_region.max.x % self.image.width() as isize;

            renderer.draw_subimage(&self.image, draw_region_1, 
                IVec2::new(draw_region_0.max.x - draw_region_0.min.x + 1, road.get_line_count() as isize)
            );
            renderer.draw_subimage(&self.image, draw_region_0, 
                IVec2::new(0, road.get_line_count() as isize)
            );
        }
    }
}