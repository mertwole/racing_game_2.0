pub mod collider;

use collider::*;

pub struct PhysicsScene {
    colliders : Vec<Collider>
}

pub struct CollisionEvent {
    pub first_id : isize,
    pub second_id : isize
}

#[derive(Clone, Copy)]
pub struct ColliderId {
    vec_id : usize
}

impl PhysicsScene {
    pub fn new() -> PhysicsScene {
        PhysicsScene {
            colliders : Vec::new()
        }
    }

    pub fn add_collider(&mut self, collider : Collider) -> ColliderId {
        self.colliders.push(collider);
        ColliderId { vec_id : self.colliders.len() - 1 }
    }

    pub fn get_collider_mut(&mut self, id : ColliderId) -> &mut Collider {
        &mut self.colliders[id.vec_id]
    }

    pub fn resolve_collisions(&self) -> Vec<CollisionEvent> {
        let mut collision_events = Vec::new();

        for i in 0..self.colliders.len() - 1 {
            for j in i + 1..self.colliders.len() {
                if self.colliders[i].collision(&self.colliders[j]) {
                    collision_events.push(
                        CollisionEvent { 
                            first_id : self.colliders[i].id, 
                            second_id : self.colliders[j].id 
                        }
                    ) 
                }
            }
        }

        collision_events
    }
}