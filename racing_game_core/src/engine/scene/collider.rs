pub struct Collider {
    length : f32,
    start_distance_proj : f32,
    width : f32,
    offset : f32
}

impl Collider {
    pub fn new(length : f32, start_distance_proj : f32, width : f32, offset : f32) -> Collider {
        Collider { 
            length,
            start_distance_proj,
            width,
            offset 
        }
    }

    pub fn collision(&self, other : &Collider) -> bool {
        fn interval_intersection(first : (f32, f32), second : (f32, f32)) -> bool {
            !(first.1 < second.0 || first.0 > second.1)
        }

        interval_intersection(
            (self.start_distance_proj, self.start_distance_proj + self.length), 
            (other.start_distance_proj, other.start_distance_proj + other.length)
        ) 
        && 
        interval_intersection(
            (self.offset - self.width * 0.5, self.offset + self.width * 0.5), 
            (other.offset - other.width * 0.5, other.offset + other.width * 0.5)
        )
    }
}