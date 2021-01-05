use crate::engine::common::Vec3;
use crate::engine::renderer::Renderer;
use crate::storage::Storage;

mod physics_scene;
mod graphics_scene;
mod game_object;
pub mod camera;

use physics_scene::*;
use graphics_scene::*;
use game_object::*;
use camera::Camera;

use graphics_scene::background::*;
use graphics_scene::billboard::*;
use graphics_scene::road::*;

pub struct Scene {
    camera : Camera,

    physics_scene : PhysicsScene,
    graphics_scene : GraphicsScene,

    game_objects : Vec<GameObjectMeta>
}

#[derive(Clone, Copy)]
pub struct GameObjectId {
    vec_id : usize
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

        let background = Background::new(Storage::load_image_rgb("background.png"), 10);

        Scene { 
            camera,

            physics_scene : PhysicsScene::new(),
            graphics_scene : GraphicsScene::new(road, background),

            game_objects : Vec::new()
        }
    }

    pub fn add_gameobject(&mut self, game_object : GameObject) -> GameObjectId {
        let meta = game_object.to_meta(&mut self.physics_scene, &mut self.graphics_scene);
        self.game_objects.push(meta);
        GameObjectId { vec_id : self.game_objects.len() - 1 }
    }

    pub fn set_gameobject_position(&mut self, id : GameObjectId, position : Vec3) {
        self.game_objects[id.vec_id]
        .set_position(position, &mut self.physics_scene, &mut self.graphics_scene);
    } 

    pub fn update(&mut self, delta_time : f32) {
        self.camera.distance += delta_time * 10.0;
        if self.camera.distance > 120.0 { self.camera.distance -= 120.0; }

        self.graphics_scene.set_camera_angle_by_road(&mut self.camera);
    }

    pub fn render(&mut self, width : u32, height : u32, pixels_ptr : *mut u32) {
        let renderer = Renderer::new(width, height, pixels_ptr);
        self.graphics_scene.render(&renderer, &self.camera);
    }
}