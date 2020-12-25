use crate::image::*;

use crate::engine::renderer::Renderer;
use crate::engine::common::{IVec2, IAABB};

mod road;
mod camera;
use road::*;
use camera::*;

pub struct Scene {
    screen_resolution : IVec2,
    camera : Camera,
    road : Road
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
        Scene { camera, road, screen_resolution }
    }

    pub fn test_move_cam(&mut self, delta_time : f32) {
        self.camera.distance += delta_time;
        self.camera.angle = self.road.get_camera_angle(&self.camera);
    }

    pub fn render(&mut self, pixels_ptr : *mut u32) {
        let renderer = Renderer::new(self.screen_resolution.x as u32, self.screen_resolution.y as u32, pixels_ptr);

        self.road.prepare_to_render(&self.camera, &renderer);
        self.road.render(&renderer);
    }
}