extern crate image;
extern crate libc;

mod storage;

mod engine;
use engine::scene::Scene;
use storage::Storage;
use engine::common::IVec2;

pub struct Game {
    scene : Scene
}

// FFI
impl Game {
    #[no_mangle]
    pub extern "C" fn init_ffi(screen_width : u32, screen_height : u32) -> *mut libc::c_void {
        let game = Game::init(screen_width, screen_height);
        unsafe { std::mem::transmute(Box::new(game)) }
    }

    #[no_mangle]
    pub extern "C" fn update_ffi(self_ : *mut libc::c_void, delta_time : f32) {
        let self_ = unsafe { (self_ as *mut Game).as_mut().unwrap() };
        self_.update(delta_time);
    }

    #[no_mangle]
    pub extern "C" fn redraw_ffi(self_ : *mut libc::c_void, pixels : *mut u32) {
        let self_ = unsafe { (self_ as *mut Game).as_mut().unwrap() };
        self_.redraw(pixels);
    }
}

impl Game {
    pub fn init(screen_width : u32, screen_height : u32) -> Game {
        let scene = Scene::new(IVec2::new(screen_width as isize, screen_height as isize), Storage::load_image_rgb("road_tex.png"));
        Game { scene }
    }

    pub fn update(&mut self, delta_time : f32) {
        self.scene.test_move_cam(5.0 * delta_time);
    }

    pub fn redraw(&mut self, pixels : *mut u32) {
        self.scene.render(pixels);
    }
}