use crate::engine::common::Vec3;

pub struct Collider {
    // Position(of center) : x is offset, y is height, z is road distance.
    position : Vec3,
    size : Vec3,
    min : Vec3,
    max : Vec3,

    pub id : isize
}

impl Collider {
    fn compute_min_max(&mut self) {
        self.min = Vec3::new(
            self.position.x - self.size.x * 0.5, 
            self.position.y - self.size.y * 0.5, 
            self.position.z - self.size.z * 0.5
        );

        self.max = Vec3::new(
            self.position.x + self.size.x * 0.5, 
            self.position.y + self.size.y * 0.5, 
            self.position.z + self.size.z * 0.5
        );
    }

    pub fn new(size : Vec3, position : Vec3, id : isize) -> Collider {
        let mut collider = Collider { 
            size,
            position,
            min : Vec3::zero(),
            max : Vec3::zero(),
            id 
        };

        collider.compute_min_max();
        collider
    }

    pub fn position(&self) -> Vec3 { self.position }

    pub fn set_position(&mut self, position : Vec3) {
        self.position = position;
        self.compute_min_max();
    }

    pub fn collision(&self, other : &Collider) -> bool {
        fn interval_intersection(first : (f32, f32), second : (f32, f32)) -> bool {
            !(first.1 < second.0 || first.0 > second.1)
        }

        interval_intersection(
            (self.min.x, self.max.x), (other.min.x, other.max.x)
        ) 
        && 
        interval_intersection(
            (self.min.y, self.max.y), (other.min.y, other.max.y)
        )
        &&
        interval_intersection(
            (self.min.z, self.max.z), (other.min.z, other.max.z)
        )
    }
}