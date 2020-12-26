use crate::image::*;

use crate::engine::renderer::Renderer;
use crate::engine::common::{IVec2, IAABB};
use crate::storage::Storage;

mod road;
mod billboard;
mod camera;
use road::*;
use camera::*;
use billboard::*;

pub struct Scene {
    screen_resolution : IVec2,
    camera : Camera,
    road : Road,

    billboard_test : Vec<Billboard>
}

impl Scene {
    pub fn new(screen_resolution : IVec2, road_tex : RgbImage) -> Scene {
        let road = Road::new(road_tex);
        let camera = Camera { 
            distance : 0.0,
            y : 1.0, 
            angle : 0.0,
            viewport_width : 1.6, 
            viewport_height : 0.9, 
            near_plane : 1.0, 
            far_plane : 1000.0 
        };

        let billboard_factory = BillboardFactory::new(
            &Storage::load_image_rgba("test_line.png"), 
            Storage::load_file("test_line.meta")
        );

        let billboard_test = vec![
            //billboard_factory.construct(60.0, 0.0, 1.0)
        ];

        Scene { camera, road, screen_resolution, billboard_test }
    }

    pub fn test_move_cam(&mut self, delta_time : f32) {
        self.camera.distance += delta_time;
        //self.camera.angle = self.road.get_camera_angle(&self.camera);
    }

    pub fn render(&mut self, pixels_ptr : *mut u32) {
        let renderer = Renderer::new(self.screen_resolution.x as u32, self.screen_resolution.y as u32, pixels_ptr);

        self.road.compute_render_data(&self.camera, &renderer);
        self.road.render(&renderer);

        for billboard in &self.billboard_test {
            billboard.render(&self.camera, &self.road, &renderer);
        }
    }
}