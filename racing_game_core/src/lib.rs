extern crate image;
extern crate libc;

mod storage;

mod engine;
use engine::scene::*;
use engine::scene::road::*;
use engine::scene::background::*;
use engine::scene::game_object::*;
use engine::scene::camera::*;
use engine::common::Vec3;
use storage::Storage;

pub struct Game {
    screen_width : u32, 
    screen_height : u32,
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


        let road_tex = Storage::load_image_rgb("road_tex.png");
        let keypoints = vec![
            Keypoint { road_distance : 0.0, height : 0.0 },
            Keypoint { road_distance : 20.0, height : 1.0 },
            Keypoint { road_distance : 40.0, height : 0.0 },
            Keypoint { road_distance : 60.0, height : 2.0 },
            Keypoint { road_distance : 80.0, height : 0.0 },
            Keypoint { road_distance : 100.0, height : 4.0 },
            Keypoint { road_distance : 110.0, height : 4.0 },
            Keypoint { road_distance : 120.0, height : 0.0 }
        ];
        let curvatures = vec![ 
            Curvature { start : 20.0, end : 30.0, strength :  0.01 },
            Curvature { start : 60.0, end : 80.0, strength : -0.01 }
        ];
        let road_data = RoadData::new(keypoints, curvatures, 120.0);
        let road = Road::new(road_tex, road_data);

        let background = Background::new(Storage::load_image_rgb("background.png"), 10);

        let camera = Camera { 
            distance : 0.0,
            y : 1.0, 
            angle : 0.0,
            viewport_width : 1.6, 
            viewport_height : 0.9, 
            near_plane : 1.0, 
            far_plane : 100.0 
        };

        let mut scene = Scene::new(road, background, camera);

        let billboard_factory = BillboardFactory::new(
            &Storage::load_image_rgba("test_spritesheet.png"), 
            Storage::load_file("test_spritesheet.meta")
        );

        let go = GameObject::new(vec![], vec![
            billboard_factory.construct(Vec3::new(1.0, 0.0, 0.0), 1.0),
            billboard_factory.construct(Vec3::new(-1.0, 0.0, 0.0), 1.0),
        ]);
        let id = scene.add_gameobject(go);
        scene.set_gameobject_position(id, Vec3::new(0.0, 0.0, 20.0));

        Game { scene, screen_width, screen_height }
    }

    pub fn update(&mut self, delta_time : f32) {
        self.scene.camera.distance += delta_time * 10.0;
        self.scene.camera.angle = self.scene.road.get_camera_angle(&self.scene.camera);
        if self.scene.camera.distance > 120.0 { self.scene.camera.distance -= 120.0; }
    }

    pub fn redraw(&mut self, pixels : *mut u32) {
        self.scene.render(self.screen_width, self.screen_height, pixels);
    }
}