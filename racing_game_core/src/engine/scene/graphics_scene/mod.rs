use crate::engine::renderer::*;
use crate::engine::scene::camera::Camera;

pub mod background;
pub mod billboard;
pub mod road;

use background::*;
use billboard::*;
use road::*;

pub struct GraphicsScene {
    road : Road,
    background : Background,
    billboards : Vec<Billboard>
}

pub struct BillboardId {
    vec_id : usize
}

impl GraphicsScene {
    pub fn new(road : Road, background : Background) -> GraphicsScene {
        GraphicsScene { 
            road,
            background,
            billboards : Vec::new(),
        }
    }

    pub fn add_billboard(&mut self, billboard : Billboard) -> BillboardId {
        self.billboards.push(billboard);
        BillboardId { vec_id : self.billboards.len() - 1 }
    }

    pub fn set_camera_angle_by_road(&self, camera : &mut Camera) {
        self.road.set_camera_angle(camera);
    }   

    pub fn render(&mut self, renderer : &Renderer, camera : &Camera) {
        // Compute road render data first because background and billboards use it.
        self.road.compute_render_data(camera, &renderer);
        self.road.render(&renderer);

        self.background.render(&self.road, camera, &renderer);

        for billboard in &self.billboards {
            billboard.render(camera, &self.road, &renderer);
        }
    }
}