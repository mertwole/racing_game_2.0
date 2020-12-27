extern crate image;
use image::RgbImage;

extern crate core;
use core::Game;

mod render;
mod window;
use render::*;
use window::*;

const SCREEN_WIDTH : u32 = 1920 / 2;
const SCREEN_HEIGHT : u32 = 1080 / 2;

fn main() {
    let mut window = Window::open(WindowParameters { width : SCREEN_WIDTH, height : SCREEN_HEIGHT, title : String::from("title")});
    let mut render = Render::new(SCREEN_WIDTH, SCREEN_HEIGHT);

    let mut game = Game::init(SCREEN_WIDTH, SCREEN_HEIGHT);

    let mut pixels : Vec<u32> = Vec::with_capacity(SCREEN_WIDTH as usize * SCREEN_HEIGHT as usize);
    for _ in 0..SCREEN_WIDTH * SCREEN_HEIGHT { pixels.push(0); }

    loop {
        window.get_events();
        if window.should_close() { break; }

        let delta_time = window.get_time() as f32;
        window.set_time(0.0);

        //println!("{}", delta_time);

        for i in 0..SCREEN_WIDTH * SCREEN_HEIGHT { pixels[i as usize] = 0; }

        game.update(delta_time);
        game.redraw(pixels.as_mut_ptr());

        render.render_from_raw(&mut window, SCREEN_WIDTH, SCREEN_HEIGHT, pixels.as_ptr());
    }
}
