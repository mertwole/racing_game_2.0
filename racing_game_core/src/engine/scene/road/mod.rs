use crate::image::{RgbImage, Rgb};

use crate::engine::renderer::Renderer;
use crate::engine::common::*;
use crate::engine::scene::camera::*;

struct Keypoint {
    pub road_distance : f32,
    pub height : f32
}

struct Curvature {
    pub start : f32,
    pub end : f32,
    pub strength : f32
}

struct RoadData {
    keypoints : Vec<Keypoint>,
    curvatures : Vec<Curvature>
}

impl RoadData {
    pub fn new_test() -> RoadData {
        let keypoints = vec![
            Keypoint { road_distance : 0.0, height : 0.0 },
            Keypoint { road_distance : 20.0, height : 0.0 },
            Keypoint { road_distance : 40.0, height : 2.0 },
            Keypoint { road_distance : 50.0, height : 2.0 },
            Keypoint { road_distance : 70.0, height : 0.0 },
            Keypoint { road_distance : 100.0, height : 4.0 },
            Keypoint { road_distance : 1000.0, height : 4.0 }
        ];

        let curvatures = vec![ 
            Curvature { start : 20.0, end : 30.0, strength :  0.01 },
            Curvature { start : 60.0, end : 80.0, strength : -0.01 }
        ];

        RoadData { keypoints, curvatures }
    }

    fn smoothstep(a : f32, b : f32, mut t : f32) -> f32 {
        t = t * t * (3.0 - 2.0 * t);
        return a + (b - a) * t;
    }

    fn smoothstep_intersect(source : &Vec2, direction : &Vec2, start_point : &Vec2, end_point : &Vec2) -> Option<f32> {
        // y = k * x + b
        let k = direction.y / direction.x;
        let b = source.y - k * source.x;

        let value_in_point = |dist : f32| -> f32 {
            Self::smoothstep(start_point.y, end_point.y, (dist - start_point.x) / (end_point.x - start_point.x))
        };

        let mut left_point = Vec2::new(start_point.x, k * start_point.x + b);
        let mut right_point = Vec2::new(end_point.x, k * end_point.x + b);
        // TODO : setup number of steps
        for _ in 0..64 {
            let middle_point = &(&left_point + &right_point) * 0.5;

            let left_point_upside = left_point.y > value_in_point(left_point.x);
            let middle_point_upside = middle_point.y > value_in_point(middle_point.x);
            let right_point_upside = right_point.y > value_in_point(right_point.x);

            if middle_point_upside && !right_point_upside { left_point = middle_point; continue; }
            if !middle_point_upside && right_point_upside { left_point = middle_point; continue; }

            if left_point_upside && !middle_point_upside { right_point = middle_point; continue; }
            if !left_point_upside && middle_point_upside { right_point = middle_point; continue; }

            return None;
        }

        Some((left_point.x + right_point.x) * 0.5)
    }

    fn intersect_road(&self, ray_source : &Vec2, ray_direction : &Vec2) -> Option<f32> {
        let ray_source = ray_source + &Vec2::new(0.0, self.get_road_height(ray_source.x));

        for i in 0..self.keypoints.len() - 1 {
            let curr_keypoint = &self.keypoints[i];
            let next_keypoint = &self.keypoints[i + 1];

            let intersection_x = Self::smoothstep_intersect(
                &ray_source, 
                ray_direction, 
                &Vec2::new(curr_keypoint.road_distance, curr_keypoint.height), 
                &Vec2::new(next_keypoint.road_distance, next_keypoint.height)
            );

            if intersection_x.is_none() { continue; }
            let intersection_x = intersection_x.unwrap();

            if intersection_x >= curr_keypoint.road_distance && intersection_x <= next_keypoint.road_distance {
                if intersection_x > ray_source.x { 
                    return Some(intersection_x);
                }
            }
        }
 
        None
    }

    fn get_road_height(&self, distance_proj : f32) -> f32 {
        for i in 1..self.keypoints.len() {
            if self.keypoints[i].road_distance > distance_proj {
                let prev_keypoint = &self.keypoints[i - 1];
                let curr_keypoint = &self.keypoints[i];

                let ratio = (distance_proj - prev_keypoint.road_distance) / (curr_keypoint.road_distance - prev_keypoint.road_distance);
                return Self::smoothstep(prev_keypoint.height, curr_keypoint.height, ratio);
            }
        }

        0.0
    }

    pub fn get_distance_proj(&self, camera : &Camera, screen_pixel_y_norm : f32) -> Option<f32> {
        let mut ray_source = Vec2::new(camera.distance, camera.y);
        let screen_point = Vec2::new(camera.distance + camera.near_plane, camera.y - camera.viewport_height * 0.5 + camera.viewport_height * screen_pixel_y_norm);
        let mut ray_direction = &screen_point - &ray_source;

        let (angle_sin, angle_cos) = camera.angle.sin_cos();

        // Rotate camera by center of near plane

        ray_direction = Vec2::new(
            angle_cos * ray_direction.x - angle_sin * ray_direction.y, 
            angle_sin * ray_direction.x + angle_cos * ray_direction.y
        ).normalized();

        let mut ray_to_center = Vec2::new(camera.near_plane, 0.0);
        ray_to_center = Vec2::new(
            angle_cos * ray_to_center.x - angle_sin * ray_to_center.y, 
            angle_sin * ray_to_center.x + angle_cos * ray_to_center.y
        );

        ray_source = &ray_source + &(&Vec2::new(camera.near_plane, 0.0) - &ray_to_center);

        match self.intersect_road(&ray_source, &ray_direction) {
            None => { return None; }
            Some(mut intersection_x) => { 
                intersection_x -= camera.distance; 
                if intersection_x > camera.near_plane && intersection_x < camera.far_plane { 
                    return Some(intersection_x);
                }
            }
        }

        None
    }

    pub fn get_width(&self, camera : &Camera, distance_proj : f32) -> f32 {
        return 1.0 / distance_proj * camera.near_plane;
    }

    pub fn apply_offset(&self, prev_distance_proj : f32, distance_proj : f32, offset : &mut f32, offset_delta : &mut f32) {
        let mut curr_dist = prev_distance_proj;

        for curvature in &self.curvatures {
            if curvature.end > curr_dist && curvature.start < distance_proj {
                let no_curvature_length = if curvature.start > curr_dist { curvature.start - curr_dist } else { 0.0 };
                let mut curvature_length = curvature.end - curr_dist;
                if curvature_length > curvature.end - curvature.start { 
                    curvature_length = curvature.end - curvature.start; 
                }
                if curvature_length > distance_proj - curr_dist {
                    curvature_length = distance_proj - curr_dist;
                }

                *offset += *offset_delta * no_curvature_length;

                *offset += *offset_delta * curvature_length + curvature.strength * curvature_length * curvature_length * 0.5;
                *offset_delta += curvature.strength * curvature_length;

                curr_dist = curvature.end + 0.0001;
                if curr_dist > distance_proj { return; }
            }
        }

        *offset += *offset_delta * (distance_proj - curr_dist);
    }

    pub fn get_camera_angle(&self, distance_proj : f32) -> f32 {
        for i in 0..self.keypoints.len() - 1 {
            let curr_keypoint = &self.keypoints[i];
            let next_keypoint = &self.keypoints[i + 1];

            if curr_keypoint.road_distance > distance_proj || next_keypoint.road_distance < distance_proj { continue; }
 
            let ratio = (distance_proj - curr_keypoint.road_distance) / (next_keypoint.road_distance - curr_keypoint.road_distance);
            let mut angle_tan = 6.0 * ratio * (1.0 - ratio);
            angle_tan *= next_keypoint.height - curr_keypoint.height;
            angle_tan /= next_keypoint.road_distance - curr_keypoint.road_distance;
            return angle_tan.atan();
        }

        0.0
    }
}   

struct LineRenderData {
    distance_proj : f32,
    is_horz_line : bool,
    height : f32,
    offset : f32,
    width : f32
}

struct RenderData {
    lines : Vec<LineRenderData>
}

pub struct Road {
    data : RoadData,
    road_tex : RgbImage,
    render_data : RenderData
}

impl Road {
    pub fn new(road_tex : RgbImage) -> Road {
        Road { 
            road_tex, 
            data : RoadData::new_test(), 
            render_data : RenderData { lines : Vec::new() } 
        }
    }

    pub fn get_camera_angle(&self, camera : &Camera) -> f32 {
        self.data.get_camera_angle(camera.distance + camera.near_plane)
    }

    pub fn prepare_to_render(&mut self, camera : &Camera, renderer : &Renderer) {
        let lines_density = 1.0;
        let mut horz_lines_accum = camera.distance % (2.0 * lines_density);
        let mut prev_distance_proj = 0.0;
        let mut is_horz_line = false;
        let mut offset = 0.0;
        let mut offset_delta = 0.0;

        self.render_data.lines.truncate(0);

        for y in 0..renderer.height() as isize {
            let distance_proj = 
            match self.data.get_distance_proj(camera, (y as f32 + 0.5) / renderer.height() as f32) {
                Some(proj) => { proj }
                None => { break; }
            };

            // Horz lines. 
            if prev_distance_proj != 0.0 { 
                let segment_length = distance_proj - prev_distance_proj;
                horz_lines_accum += segment_length; 
            }
            if horz_lines_accum > lines_density { 
                is_horz_line = !is_horz_line; 
                horz_lines_accum = horz_lines_accum % lines_density;            
            }

            // Horizontal offset
            if prev_distance_proj != 0.0 {
                self.data.apply_offset(prev_distance_proj + camera.distance, distance_proj + camera.distance, &mut offset, &mut offset_delta);
            }
            
            prev_distance_proj = distance_proj;

            self.render_data.lines.push(LineRenderData {
                distance_proj : distance_proj + camera.distance,
                is_horz_line,
                offset : offset * (camera.near_plane / distance_proj),
                width : self.data.get_width(camera, distance_proj),
                height : self.data.get_road_height(distance_proj + camera.distance)
            });
        }
    }

    pub fn render(&self, renderer : &Renderer) {
        for y in 0..self.render_data.lines.len() {
            let line_render_data = &self.render_data.lines[y];

            let offset_px = (line_render_data.offset * renderer.width() as f32) as isize;
            let width_px = (line_render_data.width * renderer.width() as f32) as isize;

            let left_px = renderer.width() as isize / 2 - width_px / 2 + offset_px;
            let right_px = left_px + width_px - 1;

            let road_tex_region = if line_render_data.is_horz_line {
                IAABB::new(
                    IVec2::new(0, self.road_tex.height() as isize - width_px), 
                    IVec2::new(width_px - 1, self.road_tex.height() as isize - width_px)
                )
            } else {
                IAABB::new(
                    IVec2::new(self.road_tex.width() as isize - width_px, width_px - 1), 
                    IVec2::new(self.road_tex.width() as isize - 1, width_px - 1)
                )
            };

            let ground_color = if line_render_data.is_horz_line { Rgb([0, 255, 0]) } else { Rgb([0, 160, 0])};

            if left_px > 0 {
                renderer.fill_rect(IAABB::new(IVec2::new(0, y as isize), IVec2::new(left_px - 1, y as isize)), &ground_color);
            }
            if right_px < renderer.width() as isize - 1 {
                renderer.fill_rect(IAABB::new(IVec2::new(right_px + 1, y as isize), IVec2::new(renderer.width() as isize - 1, y as isize)), &ground_color);
            }

            renderer.draw_subimage(&self.road_tex, road_tex_region, IVec2::new(left_px, y as isize));
        }
    }
}