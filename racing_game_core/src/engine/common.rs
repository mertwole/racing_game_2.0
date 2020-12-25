use std::ops;

// region IAABB
pub struct IAABB{
    pub min : IVec2,
    pub max : IVec2
}

impl IAABB {
    pub fn new(min : IVec2, max : IVec2) -> IAABB {
        IAABB { min, max }
    } 

    pub fn clone(&self) -> IAABB {
        IAABB { min : self.min.clone(), max : self.max.clone() }
    }

    pub fn expand(&mut self, value : IVec2) {
        self.min = &self.min - &value;
        self.max = &self.max + &value;
    }
}

// endregion

// region Vec2 

pub struct Vec2 {
    pub x : f32,
    pub y : f32
}

impl Vec2{
    pub fn new(x : f32, y : f32) -> Vec2 {
        Vec2 { x, y }
    }

    pub fn clone(&self) -> Vec2 {
        Vec2 { x : self.x, y : self.y }
    }

    pub fn dot(&self, rhs : &Vec2) -> f32 {
        self.x * rhs.x + self.y * rhs.y
    }

    pub fn sqr_len(&self) -> f32{
        self.dot(&self)
    }

    pub fn len(&self) -> f32 {
        self.sqr_len().sqrt()
    } 

    pub fn normalized(&self) -> Vec2 {
        let len = self.len();
        Vec2 { x : self.x / len, y : self.y / len }
    }

    pub fn ivec2(&self) -> IVec2 {
        IVec2 { x : self.x as isize, y : self.y as isize }
    }
}

impl ops::Add<&Vec2> for &Vec2 {
    type Output = Vec2;
    fn add(self, rhs: &Vec2) -> Vec2 {
        Vec2::new(self.x + rhs.x, self.y + rhs.y)
    }
}

impl ops::Sub<&Vec2> for &Vec2 {
    type Output = Vec2;
    fn sub(self, rhs: &Vec2) -> Vec2 {
        Vec2::new(self.x - rhs.x, self.y - rhs.y)
    }
}

impl ops::Mul<f32> for &Vec2 {
    type Output = Vec2;
    fn mul(self, rhs: f32) -> Vec2 {
        Vec2::new(self.x * rhs, self.y * rhs)
    }
}

impl ops::Mul<&Vec2> for f32 {
    type Output = Vec2;
    fn mul(self, rhs: &Vec2) -> Vec2 {
        Vec2::new(self * rhs.x, self * rhs.y)
    }
}

//endregion

// region IVec2 
#[derive(Clone, Copy)]
pub struct IVec2{
    pub x : isize,
    pub y : isize
}

impl IVec2{
    pub fn new(x : isize, y : isize) -> IVec2{
        IVec2 { x, y }
    }

    pub fn zero() -> IVec2{
        IVec2 { x : 0, y : 0 }
    }

    pub fn vec2(&self) -> Vec2{
        Vec2 { x : self.x as f32, y : self.y as f32 }
    }

    pub fn clone(&self) -> IVec2{
        IVec2 { x : self.x, y : self.y }
    }

    pub fn sqr_len(&self) -> isize {
        self.x * self.x + self.y * self.y
    }

    pub fn len(&self) -> f32 {
        (self.sqr_len() as f32).sqrt()
    }

    pub fn dot(&self, rhs : &IVec2) -> isize {
        self.x * rhs.x + self.y * rhs.y
    }
}

impl ops::Add<&IVec2> for &IVec2 {
    type Output = IVec2;
    fn add(self, rhs: &IVec2) -> IVec2 {
        IVec2::new(self.x + rhs.x, self.y + rhs.y)
    }
}

impl ops::Sub<&IVec2> for &IVec2 {
    type Output = IVec2;
    fn sub(self, rhs: &IVec2) -> IVec2 {
        IVec2::new(self.x - rhs.x, self.y - rhs.y)
    }
}

impl ops::Div<isize> for &IVec2 {
    type Output = IVec2;
    fn div(self, rhs: isize) -> IVec2 {
        IVec2::new(self.x / rhs, self.y / rhs)
    }
}

// endregion