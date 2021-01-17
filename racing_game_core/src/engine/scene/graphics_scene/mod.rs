use crate::engine::renderer::*;
use crate::engine::scene::camera::Camera;

pub mod billboard;

use billboard::*;
use super::road::Road;

pub struct GraphicsScene {
    billboards : Vec<Billboard>
}

#[derive(Clone, Copy)]
pub struct BillboardId {
    vec_id : usize
}

impl GraphicsScene {
    pub fn new() -> GraphicsScene {
        GraphicsScene { 
            billboards : Vec::new(),
        }
    }

    pub fn add_billboard(&mut self, billboard : Billboard) -> BillboardId {
        self.billboards.push(billboard);
        BillboardId { vec_id : self.billboards.len() - 1 }
    }

    pub fn get_billboard_mut(&mut self, id : BillboardId) -> &mut Billboard {
        &mut self.billboards[id.vec_id]
    }

    pub fn render(&mut self, renderer : &Renderer, road : &Road, camera : &Camera) {
        for billboard in &self.billboards {
            billboard.render(camera, road, &renderer);
        }
    }
}