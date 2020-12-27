use crate::image::*;

use super::common::{IVec2, IAABB};

pub struct Renderer {
    width : u32,
    height : u32,
    pixels_ptr : *mut u32
}

pub trait PixelReader {
    fn get_pixel_value(&self, x : u32, y : u32) -> Option<u32>;
    fn width(&self) -> u32;
    fn height(&self) -> u32;
}

impl PixelReader for RgbImage {
    fn get_pixel_value(&self, x : u32, y : u32) -> Option<u32> {
        let pixel = self.get_pixel(x, y);
        return Some((pixel[0] as u32) + 256 * (pixel[1] as u32) + 256 * 256 * (pixel[2] as u32));
    }

    fn width(&self) -> u32 { self.width() }
    fn height(&self) -> u32 { self.height() }
}

impl PixelReader for RgbaImage {
    fn get_pixel_value(&self, x : u32, y : u32) -> Option<u32> {
        let pixel = self.get_pixel(x, y);
        if pixel[3] == 0 { 
            None 
        } else {
            Some((pixel[0] as u32) + 256 * (pixel[1] as u32) + 256 * 256 * (pixel[2] as u32))
        }
    }

    fn width(&self) -> u32 { self.width() }
    fn height(&self) -> u32 { self.height() }
}

impl Renderer {
    pub fn new(width : u32, height : u32, pixels_ptr : *mut u32) -> Renderer {
        Renderer { width, height, pixels_ptr }
    }

    pub fn width(&self) -> u32 { self.width }
    pub fn height(&self) -> u32 { self.height }

    pub fn draw_image<T>(&self, image : &T, location : IVec2) where T : PixelReader {
        self.draw_subimage(
            image, 
            IAABB::new(IVec2::zero(), IVec2::new(image.width() as isize - 1, image.height() as isize - 1)), 
            location
        );
    }

    pub fn draw_subimage<T>(&self, image : &T, mut region : IAABB, location : IVec2) where T : PixelReader {
        // Make inclusive;
        region.max.x += 1;
        region.max.y += 1;

        let left_draw = if location.x < 0 { -location.x + region.min.x } else { region.min.x };
        let bottom_draw = if location.y < 0 { -location.y + region.min.y } else { region.min.y };

        let right_draw = if location.x + region.max.x - region.min.x > self.width as isize { 
            -location.x + region.min.x + self.width() as isize 
        } else { 
            region.max.x 
        };
        let top_draw = if location.y + region.max.y - region.min.y > self.height as isize { 
            -location.y + region.min.y + self.height() as isize 
        } else { 
            region.max.y 
        };

        for y in bottom_draw..top_draw {
            for x in left_draw..right_draw {
                let global_x = x as isize + location.x - region.min.x;
                let global_y = y as isize + location.y - region.min.y;
                
                let global_pos = global_x + global_y * self.width as isize;
                let mut global_pos_ptr = self.pixels_ptr;
                unsafe { global_pos_ptr = global_pos_ptr.offset(global_pos); }
                
                let pixel_value = image.get_pixel_value(x as u32, image.height() - y as u32 - 1);
                match pixel_value {
                    Some(value) => { unsafe { *global_pos_ptr = value; } }
                    None => { }
                }
            }
        }
    }

    pub fn fill_rect(&self, rect : IAABB, color : &Rgb<u8>) {
        for y in rect.min.y..rect.max.y + 1 {
            for x in rect.min.x..rect.max.x + 1 {
                let global_pos = x + y * self.width as isize;
                let global_pos_ptr = unsafe {  self.pixels_ptr.offset(global_pos) };
                let color_value = (color[0] as u32) + 256 * (color[1] as u32) + 256 * 256 * (color[2] as u32);
                unsafe { *global_pos_ptr = color_value; }
            }
        }
    } 
}