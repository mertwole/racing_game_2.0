use crate::engine::common::Vec3;
use crate::engine::renderer::Renderer;

use std::path::Path;

use image::*;

mod physics_scene;
mod graphics_scene;
pub mod game_object;
pub mod camera;
pub mod road;
pub mod background;

use physics_scene::*;
use graphics_scene::*;
use game_object::*;
use camera::Camera;
use road::*;
use background::*;

pub use physics_scene::collider::*;
pub use graphics_scene::billboard::*;

pub struct Scene {
    pub camera : Camera,

    physics_scene : PhysicsScene,
    graphics_scene : GraphicsScene,

    pub road : Road,
    pub background : Background,

    game_objects : Vec<GameObjectMeta>
}

#[derive(Clone, Copy)]
pub struct GameObjectId {
    vec_id : usize
}

impl Scene {
    pub fn new(road : Road, background : Background, camera : Camera) -> Scene {
        Scene { 
            camera,
            physics_scene : PhysicsScene::new(),
            graphics_scene : GraphicsScene::new(),
            road,
            background,
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
        .set_position(position, self.road.get_length(), &mut self.physics_scene, &mut self.graphics_scene);
    } 

    pub fn render(&mut self, width : u32, height : u32, pixels_ptr : *mut u32) {
        let renderer = Renderer::new(width, height, pixels_ptr);
        // Compute road render data first because background and billboards use it.
        self.road.compute_render_data(&self.camera, &renderer);
        self.road.render(&renderer);
        self.background.render(&self.road, &self.camera, &renderer);
        self.graphics_scene.render(&renderer, &self.road, &self.camera);
    }
}