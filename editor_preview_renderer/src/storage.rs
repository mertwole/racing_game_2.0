// Temporary code, will be removed after adding support to store road and background textures in .rmap files.
extern crate include_dir;

use include_dir::{include_dir, Dir};
use crate::image::{RgbImage, RgbaImage};

const RESOURCES_DIR : Dir = include_dir!("./resources");

pub struct Storage { }

impl Storage {
    pub fn load_image_rgb(name : &str) -> RgbImage {
        let file = RESOURCES_DIR.get_file(name);
        match file {
            Some(file) => { image::load_from_memory(file.contents()).unwrap().to_rgb8() } 
            None => { panic!("file {} not found!", name); }
        }
    }
}