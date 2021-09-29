extern crate image;

extern crate engine;

use engine::scene::*;
use engine::scene::road::*;
use engine::scene::background::*;
use engine::scene::game_object::*;
use engine::scene::camera::*;
use engine::common::Vec3;
use engine::rmap_deserializer::RmapDeserializer;

mod storage;
use storage::Storage;

#[no_mangle]
unsafe extern "C" fn render_preview(
    data : *const u8, data_len : i32, 
    camera_distance : f32, 
    out_width : i32, out_height : i32, out_pixels : *mut u32
) {
    let camera = Camera { 
        position : Vec3::new(0.0, 1.0, camera_distance),
        angle : 0.0,
        viewport_width : 1.6, 
        viewport_height : 0.9, 
        near_plane : 1.0, 
        far_plane : 100.0 
    };

    let data = std::slice::from_raw_parts(data, data_len as usize);
    let deserializer = RmapDeserializer::load_from_file(data);
    let mut scene = deserializer.generate_scene(0, Storage::load_image_rgb("road_tex.png"), Storage::load_image_rgb("background.png"), camera);

    scene.render(out_width as u32, out_height as u32, out_pixels);
}