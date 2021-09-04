use crate::engine::common::Vec3;
use crate::engine::renderer::Renderer;

use std::path::Path;

#[macro_use]
use arrayref::*;
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

    fn read_i32(file_contents : &[u8], read_pos : &mut usize, is_little_endian : bool) -> i32 {
        let i32_convert_func = 
        if is_little_endian { i32::from_le_bytes } 
        else { i32::from_be_bytes };

        let out = i32_convert_func([
            file_contents[*read_pos + 0],
            file_contents[*read_pos + 1],
            file_contents[*read_pos + 2],
            file_contents[*read_pos + 3]
        ]);

        *read_pos += 4;

        out
    }

    fn read_f32(file_contents : &[u8], read_pos : &mut usize, is_little_endian : bool) -> f32 {
        let f32_convert_func = 
        if is_little_endian { f32::from_le_bytes } 
        else { f32::from_be_bytes };

        let out = f32_convert_func([
            file_contents[*read_pos + 0],
            file_contents[*read_pos + 1],
            file_contents[*read_pos + 2],
            file_contents[*read_pos + 3]
        ]);

        *read_pos += 4;

        out
    }

    fn read_vec3(file_contents : &[u8], read_pos : &mut usize, is_little_endian : bool) -> Vec3 {
        let mut vec = Vec3::zero();
        vec.x = Self::read_f32(file_contents, read_pos, is_little_endian);
        vec.y = Self::read_f32(file_contents, read_pos, is_little_endian);
        vec.z = Self::read_f32(file_contents, read_pos, is_little_endian);
        vec
    }

    fn read_slice(file_contents : &[u8], read_pos : &mut usize, len : usize) -> Vec<u8> {
        let mut data = Vec::new();
        data.clone_from_slice(&file_contents[*read_pos..*read_pos + len]);
        *read_pos += len;
        data
    }

    pub fn load_from_file(&mut self, file_contents : &[u8]) {
        let mut read_pos = 0;

        let is_little_endian = file_contents[read_pos] == 0x00;
        read_pos += 1;

        // Read billboards.
        let num_billboards = Self::read_i32(file_contents, &mut read_pos, is_little_endian);
        let mut billboard_factories = Vec::with_capacity(num_billboards as usize);
        for i in 0..num_billboards {
            let num_images = Self::read_i32(file_contents, &mut read_pos, is_little_endian);
            let mut images = Vec::with_capacity(num_images as usize);
            let mut image_sizes = Vec::with_capacity(num_images as usize);

            for _ in 0..num_images { image_sizes.push(Self::read_i32(file_contents, &mut read_pos, is_little_endian)); }

            for j in 0..num_images {
                let img_size = image_sizes[j as usize] as usize;
                let data = Self::read_slice(file_contents, &mut read_pos, img_size);
                let img = image::load_from_memory_with_format(data.as_ref(), ImageFormat::Png)
                .expect("failed to read PNG image from .rmap");
                images.push(img.to_rgba8()); 
            }

            billboard_factories.push(BillboardFactory::from_lod_images(images));
        }
        // Read game objects.
        let num_game_objects = Self::read_i32(file_contents, &mut read_pos, is_little_endian);
        let mut game_objects = Vec::with_capacity(num_game_objects as usize);
        for i in 0..num_game_objects {
            let num_colliders = Self::read_i32(file_contents, &mut read_pos, is_little_endian);
            let mut colliders = Vec::with_capacity(num_colliders as usize);
            for _ in 0..num_colliders {
                let pos = Self::read_vec3(file_contents, &mut read_pos, is_little_endian);
                let size = Self::read_vec3(file_contents, &mut read_pos, is_little_endian);
                // TODO : read/assign ID.
                let collider = Collider::new(pos, size, 0);
                colliders.push(collider);
            }

            let num_bb = Self::read_i32(file_contents, &mut read_pos, is_little_endian);
            let mut billboards = Vec::with_capacity(num_bb as usize);
            for _ in 0..num_bb {
                let billboard_id = Self::read_i32(file_contents, &mut read_pos, is_little_endian);
                let pos = Self::read_vec3(file_contents, &mut read_pos, is_little_endian);
                let size = Self::read_f32(file_contents, &mut read_pos, is_little_endian);
                let billboard = billboard_factories[billboard_id as usize].construct(pos, size);
                billboards.push(billboard);
            }

            let game_object = GameObject::new(colliders, billboards);
            game_objects.push(game_object);
        }
    }
}