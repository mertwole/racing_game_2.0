use crate::engine::scene::*;
use crate::engine::scene::road::*;
use crate::engine::scene::background::*;
use crate::engine::scene::game_object::*;
use crate::engine::scene::camera::*;
use crate::engine::common::Vec3;

use crate::storage::Storage;

mod input;
use input::*;

pub struct Game {
    screen_width : u32, 
    screen_height : u32,
    scene : Scene,
    input : Input,
    player_go_id : GameObjectId
}

impl Game {
    // key is GLFW key code
    pub fn keyboard_input(&mut self, key : i32, pressed : bool) {
        self.input.keyboard_input(key, pressed);
    }

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
            position : Vec3::new(0.0, 1.0, 0.0),
            angle : 0.0,
            viewport_width : 1.6, 
            viewport_height : 0.9, 
            near_plane : 1.0, 
            far_plane : 100.0 
        };

        let mut scene = Scene::new(road, background, camera);

        //scene.load_from_file(Storage::load_file("test_map.rmap"));

        let billboard_factory = BillboardFactory::new(
            &Storage::load_image_rgba("test_spritesheet.png"), 
            Storage::load_file("test_spritesheet.meta")
        );

        let player_go = GameObject::new(vec![], vec![
            billboard_factory.construct(Vec3::new(0.0, 0.0, 2.5), 0.5),
        ]);
        let player_go_id = scene.add_gameobject(player_go);

        // let go = GameObject::new(vec![], vec![
        //     billboard_factory.construct(Vec3::new(1.0, 0.0, 0.0), 1.0),
        //     billboard_factory.construct(Vec3::new(-1.0, 0.0, 0.0), 1.0)
        // ]);
        // let id = scene.add_gameobject(go);
        // scene.set_gameobject_position(id, Vec3::new(0.0, 0.0, 20.0));
        
        let mut input = Input::new();
        input.bind_key_action(input::KEY_LEFT, InputAction::SteerLeft);
        input.bind_key_action(input::KEY_RIGHT, InputAction::SteerRight);

        Game { scene, screen_width, screen_height, input, player_go_id }
    }

    pub fn update(&mut self, delta_time : f32) {
        let input_queue = self.input.get_queue();
        for action in input_queue {
            match action {
                InputAction::SteerLeft => { self.scene.camera.position.x -= delta_time * 1.5; }
                InputAction::SteerRight => { self.scene.camera.position.x += delta_time * 1.5; }
            }
        }

        self.scene.camera.position.z += delta_time * 10.0;
        self.scene.camera.angle = self.scene.road.get_camera_angle(&self.scene.camera);
        self.scene.set_gameobject_position(self.player_go_id, self.scene.camera.position);
        if self.scene.camera.position.z > 120.0 { self.scene.camera.position.z -= 120.0; }
    }

    pub fn redraw(&mut self, pixels : *mut u32) {
        self.scene.render(self.screen_width, self.screen_height, pixels);
    }
}