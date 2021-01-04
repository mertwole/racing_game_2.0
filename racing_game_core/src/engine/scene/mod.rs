use crate::image::*;

use crate::engine::renderer::Renderer;
use crate::engine::common::{IVec2};
use crate::storage::Storage;

mod road;
mod billboard;
mod camera;
mod background;
use road::*;
use camera::*;
use billboard::*;
use background::*;

pub struct Scene {
    camera : Camera,
    road : Road,
    background : Background,
    billboard_test : Vec<Billboard>,

    test_back_offset : u32
}

impl Scene {
    pub fn new() -> Scene {
        let road_tex = Storage::load_image_rgb("road_tex.png");
        let road = Road::new(road_tex);

        let camera = Camera { 
            distance : 0.0,
            y : 1.0, 
            angle : 0.0,
            viewport_width : 1.6, 
            viewport_height : 0.9, 
            near_plane : 1.0, 
            far_plane : 100.0 
        };

        let billboard_factory = BillboardFactory::new(
            &Storage::load_image_rgba("test_line.png"), 
            Storage::load_file("test_line.meta")
        );

        let billboard_factory = BillboardFactory::new(
            &Storage::load_image_rgba("test_spritesheet.png"), 
            Storage::load_file("test_spritesheet.meta")
        );

        let mut billboard_test = vec![
            //billboard_factory.construct(110.0, 0.0, 1.0),
            billboard_factory.construct(50.0, 1.0, 1.0),
            //billboard_factory.construct(40.0, 1.0, 1.0),
            //billboard_factory.construct(30.0, 1.0, 1.0),
            //billboard_factory.construct(20.0, 1.0, 1.0),
            //billboard_factory.construct(10.0, 1.0, 1.0),
        ];

        let billboard_factory = BillboardFactory::new(
            &Storage::load_image_rgba("test_spritesheet.png"), 
            Storage::load_file("test_spritesheet.meta")
        );

        billboard_test.push(billboard_factory.construct(0.0, 0.0, 0.4));

        let background = Background::new(Storage::load_image_rgb("background.png"), 10);

        Scene { camera, road, background, billboard_test, test_back_offset : 0 }
    }

    pub fn test_move_cam(&mut self, delta_time : f32) {
        self.camera.distance += delta_time;
        if self.camera.distance > 120.0 { self.camera.distance -= 120.0; }
        self.road.set_camera_angle(&mut self.camera);

        self.billboard_test.last_mut().unwrap().road_distance = (self.camera.distance + self.camera.near_plane + 1.5) % 120.0;

        self.test_back_offset += 1;
        self.background.set_offset(self.test_back_offset);
    }

    pub fn render(&mut self, width : u32, height : u32, pixels_ptr : *mut u32) {
        let renderer = Renderer::new(width, height, pixels_ptr);

        self.road.compute_render_data(&self.camera, &renderer);
        self.road.render(&renderer);

        self.background.render(&self.road, &self.camera, &renderer);

        for billboard in &self.billboard_test {
            billboard.render(&self.camera, &self.road, &renderer);
        }
    }
}