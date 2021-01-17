use crate::engine::common::Vec3;
use crate::engine::scene::graphics_scene::{billboard::Billboard, GraphicsScene, BillboardId};
use crate::engine::scene::physics_scene::{collider::Collider, PhysicsScene, ColliderId};

// GameObject contains colliders and billboards itself.
// 
// GameObjectMeta contains only ids for colliders contained in PhysicsScene 
// and billboards contained in GraphicsScene.

pub struct GameObject {
    colliders : Vec<Collider>,
    billboards : Vec<Billboard>,
    // Position : x is offset, y is height, z is road distance. 
    position : Vec3
}

pub struct GameObjectMeta {
    collider_ids : Vec<ColliderId>,
    billboard_ids : Vec<BillboardId>,

    collider_local_positions : Vec<Vec3>,
    billboard_local_positions : Vec<Vec3>,
    // Position : x is offset, y is height, z is road distance. 
    position : Vec3
}

impl GameObject {
    pub fn new(colliders : Vec<Collider>, billboards : Vec<Billboard>) -> GameObject {
        GameObject { colliders, billboards, position : Vec3::zero() }
    }   

    pub fn to_meta(self, physics_scene : &mut PhysicsScene, graphics_scene : &mut GraphicsScene) -> GameObjectMeta {
        let GameObject { mut colliders, billboards, position } = self;

        let mut collider_ids = Vec::new();
        let mut billboard_ids = Vec::new();

        let mut collider_local_positions = Vec::new();
        let mut billboard_local_positions = Vec::new();

        // cache min and max values
        for collider in &mut colliders {
            collider.compute_min_max();
        }

        for collider in colliders {
            collider_local_positions.push(collider.position);
            let id = physics_scene.add_collider(collider);
            collider_ids.push(id);
        }

        for billboard in billboards {
            billboard_local_positions.push(billboard.position);
            let id = graphics_scene.add_billboard(billboard);
            billboard_ids.push(id);
        }

        GameObjectMeta {
            collider_ids,
            billboard_ids,

            collider_local_positions,
            billboard_local_positions,

            position
        }
    }
}

impl GameObjectMeta {
    pub fn set_position(&mut self, position : Vec3, physics_scene : &mut PhysicsScene, graphics_scene : &mut GraphicsScene) {
        self.position = position;

        for i in 0..self.collider_ids.len() {
            let collider_id = self.collider_ids[i];
            let collider = physics_scene.get_collider_mut(collider_id);
            collider.position = self.collider_local_positions[i] + self.position;
            collider.compute_min_max();
        }

        for i in 0..self.billboard_ids.len() {
            let billboard_id = self.billboard_ids[i];
            let billboard = graphics_scene.get_billboard_mut(billboard_id);
            billboard.position = self.billboard_local_positions[i] + self.position;
        }
    }
}