use crate::image::*;

use crate::common::{Vec2, Vec3};
use crate::scene::*;
use crate::scene::background::*;
use crate::scene::game_object::*;
use crate::scene::camera::*;
use crate::scene::road::*;

struct Track {
    road_data : RoadData,
    // Ids from common list representer in .rmap file.
    game_object_ids : Vec<usize>,
    // And their positions respectively.
    game_object_positions : Vec<Vec3>
}

pub struct RmapDeserializer {
    game_objects : Vec<GameObject>,
    tracks : Vec<Track>
}

impl RmapDeserializer {
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

    fn read_vec2(file_contents : &[u8], read_pos : &mut usize, is_little_endian : bool) -> Vec2 {
        let mut vec = Vec2::zero();
        vec.x = Self::read_f32(file_contents, read_pos, is_little_endian);
        vec.y = Self::read_f32(file_contents, read_pos, is_little_endian);
        vec
    }

    fn read_vec3(file_contents : &[u8], read_pos : &mut usize, is_little_endian : bool) -> Vec3 {
        let mut vec = Vec3::zero();
        vec.x = Self::read_f32(file_contents, read_pos, is_little_endian);
        vec.y = Self::read_f32(file_contents, read_pos, is_little_endian);
        vec.z = Self::read_f32(file_contents, read_pos, is_little_endian);
        vec
    }

    fn read_slice(file_contents : &[u8], read_pos : &mut usize, len : usize) -> Vec<u8> {
        let mut data = Vec::with_capacity(len);
        for _ in 0..len { data.push(0); }
        data.clone_from_slice(&file_contents[*read_pos..*read_pos + len]);
        *read_pos += len;
        data
    }

    pub fn load_from_file(file_contents : &[u8]) -> RmapDeserializer {
        let mut read_pos = 0;

        let is_little_endian = file_contents[read_pos] == 0x00;
        read_pos += 1;

        // Read billboards.
        let num_billboards = Self::read_i32(file_contents, &mut read_pos, is_little_endian);
        let mut billboard_factories = Vec::with_capacity(num_billboards as usize);
        for _ in 0..num_billboards {
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
        for _ in 0..num_game_objects {
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
        // Read tracks.
        let num_tracks = Self::read_i32(file_contents, &mut read_pos, is_little_endian);
        let mut tracks = Vec::with_capacity(num_tracks as usize);
        for _ in 0..num_tracks {
            let num_keypoints = Self::read_i32(file_contents, &mut read_pos, is_little_endian);
            let mut keypoints = Vec::with_capacity(num_keypoints as usize);
            for _ in 0..num_keypoints {
                let keypoint = Self::read_vec2(file_contents, &mut read_pos, is_little_endian);
                keypoints.push(Keypoint { road_distance : keypoint.x, height : keypoint.y });
            }

            let num_curvatures = Self::read_i32(file_contents, &mut read_pos, is_little_endian);
            let mut curvatures = Vec::with_capacity(num_curvatures as usize);
            for _ in 0..num_curvatures {
                let start = Self::read_f32(file_contents, &mut read_pos, is_little_endian);
                let length = Self::read_f32(file_contents, &mut read_pos, is_little_endian);
                let strength = Self::read_f32(file_contents, &mut read_pos, is_little_endian);
                curvatures.push(Curvature { start, end : start + length, strength });
            }

            let num_go = Self::read_i32(file_contents, &mut read_pos, is_little_endian);
            let mut go_ids = Vec::with_capacity(num_go as usize);
            let mut go_positions = Vec::with_capacity(num_go as usize);
            for _ in 0..num_go {
                let id = Self::read_i32(file_contents, &mut read_pos, is_little_endian);
                let pos = Self::read_vec2(file_contents, &mut read_pos, is_little_endian);
                go_ids.push(id as usize);
                go_positions.push(Vec3::new(pos.x, 0.0, pos.y));
            }

            // TODO : load length.
            let road_data = RoadData::new(keypoints, curvatures, 200.0);
            let track = Track { 
                game_object_ids : go_ids, 
                game_object_positions : go_positions, 
                road_data 
            };

            tracks.push(track);
        }

        RmapDeserializer { tracks, game_objects }
    }

    pub fn generate_scene(&self, track_id : usize, road_tex : RgbImage, bg_tex : RgbImage) -> Scene {
        let track = &self.tracks[track_id];

        //let road_tex = Storage::load_image_rgb("road_tex.png");
        let road = Road::new(road_tex, track.road_data.clone());

        //let bg_tex = Storage::load_image_rgb("background.png");
        let background = Background::new(bg_tex, 10);

        let camera = Camera { 
            position : Vec3::new(0.0, 1.0, 0.0),
            angle : 0.0,
            viewport_width : 1.6, 
            viewport_height : 0.9, 
            near_plane : 1.0, 
            far_plane : 100.0 
        };

        let mut scene = Scene::new(road, background, camera);

        for i in 0..track.game_object_ids.len() {
            let id = track.game_object_ids[i];
            let pos = track.game_object_positions[i];

            let scene_id = scene.add_gameobject(self.game_objects[id].clone());
            scene.set_gameobject_position(scene_id, pos);
        }

        scene
    }
}
