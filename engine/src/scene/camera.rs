use crate::common::Vec3;

pub struct Camera{
    pub position : Vec3,
    pub angle : f32,
    pub viewport_width : f32,
    pub viewport_height : f32,
    pub near_plane : f32,
    pub far_plane : f32
}