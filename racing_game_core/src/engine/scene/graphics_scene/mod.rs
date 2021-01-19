use std::cmp::Ordering;

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

    // TODO : Optimize 
    // --store sorted collection of billboards
    // --store static billboards in one collection and dynamic in another
    pub fn render(&mut self, renderer : &Renderer, road : &Road, camera : &Camera) {
        let mut billboards = self.billboards.clone();
        
        billboards.sort_by(|a, b| -> Ordering { 
            let mut a_dist = a.position.z - camera.position.z; 
            if a_dist < 0.0 { a_dist += road.get_length(); }
            let mut b_dist = b.position.z - camera.position.z; 
            if b_dist < 0.0 { b_dist += road.get_length(); }
            // Sort by descending
            b_dist.partial_cmp(&a_dist).unwrap()
        });

        for billboard in &billboards {
            billboard.render(camera, road, &renderer);
        }
    }
}