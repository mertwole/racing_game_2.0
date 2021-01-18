#![allow(dead_code)]
use std::collections::HashMap;

// GLFW key ids
pub const KEY_UNKNOWN: i32 = -1;
pub const KEY_SPACE: i32 = 32;
pub const KEY_APOSTROPHE: i32 = 39;
pub const KEY_COMMA: i32 = 44;
pub const KEY_MINUS: i32 = 45;
pub const KEY_PERIOD: i32 = 46;
pub const KEY_SLASH: i32 = 47;
pub const KEY_0: i32 = 48;
pub const KEY_1: i32 = 49;
pub const KEY_2: i32 = 50;
pub const KEY_3: i32 = 51;
pub const KEY_4: i32 = 52;
pub const KEY_5: i32 = 53;
pub const KEY_6: i32 = 54;
pub const KEY_7: i32 = 55;
pub const KEY_8: i32 = 56;
pub const KEY_9: i32 = 57;
pub const KEY_SEMICOLON: i32 = 59;
pub const KEY_EQUAL: i32 = 61;
pub const KEY_A: i32 = 65;
pub const KEY_B: i32 = 66;
pub const KEY_C: i32 = 67;
pub const KEY_D: i32 = 68;
pub const KEY_E: i32 = 69;
pub const KEY_F: i32 = 70;
pub const KEY_G: i32 = 71;
pub const KEY_H: i32 = 72;
pub const KEY_I: i32 = 73;
pub const KEY_J: i32 = 74;
pub const KEY_K: i32 = 75;
pub const KEY_L: i32 = 76;
pub const KEY_M: i32 = 77;
pub const KEY_N: i32 = 78;
pub const KEY_O: i32 = 79;
pub const KEY_P: i32 = 80;
pub const KEY_Q: i32 = 81;
pub const KEY_R: i32 = 82;
pub const KEY_S: i32 = 83;
pub const KEY_T: i32 = 84;
pub const KEY_U: i32 = 85;
pub const KEY_V: i32 = 86;
pub const KEY_W: i32 = 87;
pub const KEY_X: i32 = 88;
pub const KEY_Y: i32 = 89;
pub const KEY_Z: i32 = 90;
pub const KEY_LEFT_BRACKET: i32 = 91;
pub const KEY_BACKSLASH: i32 = 92;
pub const KEY_RIGHT_BRACKET: i32 = 93;
pub const KEY_GRAVE_ACCENT: i32 = 96;
pub const KEY_WORLD_1: i32 = 161;
pub const KEY_WORLD_2: i32 = 162;
pub const KEY_ESCAPE: i32 = 256;
pub const KEY_ENTER: i32 = 257;
pub const KEY_TAB: i32 = 258;
pub const KEY_BACKSPACE: i32 = 259;
pub const KEY_INSERT: i32 = 260;
pub const KEY_DELETE: i32 = 261;
pub const KEY_RIGHT: i32 = 262;
pub const KEY_LEFT: i32 = 263;
pub const KEY_DOWN: i32 = 264;
pub const KEY_UP: i32 = 265;
pub const KEY_PAGE_UP: i32 = 266;
pub const KEY_PAGE_DOWN: i32 = 267;
pub const KEY_HOME: i32 = 268;
pub const KEY_END: i32 = 269;
pub const KEY_CAPS_LOCK: i32 = 280;
pub const KEY_SCROLL_LOCK: i32 = 281;
pub const KEY_NUM_LOCK: i32 = 282;
pub const KEY_PRINT_SCREEN: i32 = 283;
pub const KEY_PAUSE: i32 = 284;
pub const KEY_F1: i32 = 290;
pub const KEY_F2: i32 = 291;
pub const KEY_F3: i32 = 292;
pub const KEY_F4: i32 = 293;
pub const KEY_F5: i32 = 294;
pub const KEY_F6: i32 = 295;
pub const KEY_F7: i32 = 296;
pub const KEY_F8: i32 = 297;
pub const KEY_F9: i32 = 298;
pub const KEY_F10: i32 = 299;
pub const KEY_F11: i32 = 300;
pub const KEY_F12: i32 = 301;
pub const KEY_F13: i32 = 302;
pub const KEY_F14: i32 = 303;
pub const KEY_F15: i32 = 304;
pub const KEY_F16: i32 = 305;
pub const KEY_F17: i32 = 306;
pub const KEY_F18: i32 = 307;
pub const KEY_F19: i32 = 308;
pub const KEY_F20: i32 = 309;
pub const KEY_F21: i32 = 310;
pub const KEY_F22: i32 = 311;
pub const KEY_F23: i32 = 312;
pub const KEY_F24: i32 = 313;
pub const KEY_F25: i32 = 314;
pub const KEY_KP_0: i32 = 320;
pub const KEY_KP_1: i32 = 321;
pub const KEY_KP_2: i32 = 322;
pub const KEY_KP_3: i32 = 323;
pub const KEY_KP_4: i32 = 324;
pub const KEY_KP_5: i32 = 325;
pub const KEY_KP_6: i32 = 326;
pub const KEY_KP_7: i32 = 327;
pub const KEY_KP_8: i32 = 328;
pub const KEY_KP_9: i32 = 329;
pub const KEY_KP_DECIMAL: i32 = 330;
pub const KEY_KP_DIVIDE: i32 = 331;
pub const KEY_KP_MULTIPLY: i32 = 332;
pub const KEY_KP_SUBTRACT: i32 = 333;
pub const KEY_KP_ADD: i32 = 334;
pub const KEY_KP_ENTER: i32 = 335;
pub const KEY_KP_EQUAL: i32 = 336;
pub const KEY_LEFT_SHIFT: i32 = 340;
pub const KEY_LEFT_CONTROL: i32 = 341;
pub const KEY_LEFT_ALT: i32 = 342;
pub const KEY_LEFT_SUPER: i32 = 343;
pub const KEY_RIGHT_SHIFT: i32 = 344;
pub const KEY_RIGHT_CONTROL: i32 = 345;
pub const KEY_RIGHT_ALT: i32 = 346;
pub const KEY_RIGHT_SUPER: i32 = 347;
pub const KEY_MENU: i32 = 348;
pub const KEY_LAST: i32 = KEY_MENU;

pub struct Input {
    bound_keys : HashMap<i32, Vec<InputAction>>,
    pressed_keys : HashMap<i32, bool>
}

#[derive(Copy, Clone)]
pub enum InputAction {
    SteerLeft,
    SteerRight
}

impl Input {
    pub fn new() -> Input {
        Input { 
            bound_keys : HashMap::new(),
            pressed_keys : HashMap::new() 
        }
    }

    pub fn keyboard_input(&mut self, key : i32, pressed : bool) {
        match self.pressed_keys.get_mut(&key) {
            Some(state) => { *state = pressed; }
            None => { }
        }
    }   

    pub fn bind_key_action(&mut self, key : i32, action : InputAction) {
        if !self.pressed_keys.contains_key(&key) {
            self.pressed_keys.insert(key, false);
        }

        match self.bound_keys.get_mut(&key) {
            Some(actions) => { actions.push(action); },
            None => { self.bound_keys.insert(key, vec![action]); }
        }
    }

    pub fn get_queue(&mut self) -> Vec<InputAction> {
        let mut queue : Vec<InputAction> = Vec::new();

        for (&key, &is_pressed) in &self.pressed_keys {
            if is_pressed {
                let mut bound_actions = (*self.bound_keys.get(&key).unwrap()).clone();
                queue.append(&mut bound_actions);
            }
        }

        queue
    }
}