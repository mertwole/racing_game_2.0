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

    position : Vec3
}

pub struct GameObjectMeta {
    collider_ids : Vec<ColliderId>,
    billboard_ids : Vec<BillboardId>,

    position : Vec3
}

impl GameObject {
    pub fn new(colliders : Vec<Collider>, billboards : Vec<Billboard>) -> GameObject {
        GameObject { colliders, billboards, position : Vec3::zero() }
    }   

    pub fn set_position(&mut self, position : Vec3) {
        self.position = position;
    }

    pub fn to_meta(self, physics_scene : &mut PhysicsScene, graphics_scene : &mut GraphicsScene) -> GameObjectMeta {
        let GameObject { colliders, billboards, position } = self;

        let mut collider_ids = Vec::new();
        let mut billboard_ids = Vec::new();

        for collider in colliders {
            let id = physics_scene.add_collider(collider);
            collider_ids.push(id);
        }

        for billboard in billboards {
            let id = graphics_scene.add_billboard(billboard);
            billboard_ids.push(id);
        }

        GameObjectMeta {
            collider_ids,
            billboard_ids,
            position
        }
    }
}