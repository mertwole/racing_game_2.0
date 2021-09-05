extern crate image;
extern crate libc;
//extern crate glfw;

mod storage;
mod engine;
pub mod game;

use game::*;

// FFI
#[no_mangle]
extern "C" fn game_init_ffi(screen_width : u32, screen_height : u32) -> *mut libc::c_void {
    let game = Game::init(screen_width, screen_height);
    unsafe { std::mem::transmute(Box::new(game)) }
}

#[no_mangle]
extern "C" fn game_update_ffi(self_ : *mut libc::c_void, delta_time : f32) {
    let self_ = unsafe { (self_ as *mut Game).as_mut().unwrap() };
    self_.update(delta_time);
}

#[no_mangle]
extern "C" fn game_redraw_ffi(self_ : *mut libc::c_void, pixels : *mut u32) {
    let self_ = unsafe { (self_ as *mut Game).as_mut().unwrap() };
    self_.redraw(pixels);
}