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

    pub fn load_image_rgba(name : &str) -> RgbaImage {
        let file = RESOURCES_DIR.get_file(name);
        match file {
            Some(file) => { image::load_from_memory(file.contents()).unwrap().to_rgba8() } 
            None => { panic!("file {} not found!", name); }
        }
    }

    pub fn load_file<'a>(name : &str) -> &'a [u8] { 
        let file = RESOURCES_DIR.get_file(name);
        match file {
            Some(file) => { file.contents() } 
            None => { panic!("file {} not found!", name); }
        }
    }
}