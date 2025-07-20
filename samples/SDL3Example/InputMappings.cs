using NanoUI.Common;
using System;
using static SDL3.SDL;

namespace SDL3Example
{
    // todo : check if osx & convert modifiers right - so no super modifier
    public static class InputMappings
    {
        public static KeyModifiers GetKeyModifiers(SDL_Keymod keymod)
        {
            var modifiers = KeyModifiers.None;
            
            if((keymod & SDL_Keymod.SDL_KMOD_CTRL) != 0)
            {
                modifiers |= KeyModifiers.Control;
            }

            if ((keymod & SDL_Keymod.SDL_KMOD_SHIFT) != 0)
            {
                modifiers |= KeyModifiers.Shift;
            }

            if ((keymod & SDL_Keymod.SDL_KMOD_ALT) != 0)
            {
                modifiers |= KeyModifiers.Alt;
            }

            return modifiers;
        }
       
        // Key up/down
        // note: we set result to Key.Unknown when
        // - key modifier
        // - not supported (NonUSHash, NonUSBackslash, NumLockClear)
        public static Key GetKey(SDL_Scancode scanCode) => scanCode switch
        {
            SDL_Scancode.SDL_SCANCODE_UNKNOWN => Key.Unknown,
            SDL_Scancode.SDL_SCANCODE_A => Key.A,
            SDL_Scancode.SDL_SCANCODE_B => Key.B,
            SDL_Scancode.SDL_SCANCODE_C => Key.C,
            SDL_Scancode.SDL_SCANCODE_D => Key.D,
            SDL_Scancode.SDL_SCANCODE_E => Key.E,
            SDL_Scancode.SDL_SCANCODE_F => Key.F,
            SDL_Scancode.SDL_SCANCODE_G => Key.G,
            SDL_Scancode.SDL_SCANCODE_H => Key.H,
            SDL_Scancode.SDL_SCANCODE_I => Key.I,
            SDL_Scancode.SDL_SCANCODE_J => Key.J,
            SDL_Scancode.SDL_SCANCODE_K => Key.K,
            SDL_Scancode.SDL_SCANCODE_L => Key.L,
            SDL_Scancode.SDL_SCANCODE_M => Key.M,
            SDL_Scancode.SDL_SCANCODE_N => Key.N,
            SDL_Scancode.SDL_SCANCODE_O => Key.O,
            SDL_Scancode.SDL_SCANCODE_P => Key.P,
            SDL_Scancode.SDL_SCANCODE_Q => Key.Q,
            SDL_Scancode.SDL_SCANCODE_R => Key.R,
            SDL_Scancode.SDL_SCANCODE_S => Key.S,
            SDL_Scancode.SDL_SCANCODE_T => Key.T,
            SDL_Scancode.SDL_SCANCODE_U => Key.U,
            SDL_Scancode.SDL_SCANCODE_V => Key.V,
            SDL_Scancode.SDL_SCANCODE_W => Key.W,
            SDL_Scancode.SDL_SCANCODE_X => Key.X,
            SDL_Scancode.SDL_SCANCODE_Y => Key.Y,
            SDL_Scancode.SDL_SCANCODE_Z => Key.Z,
            SDL_Scancode.SDL_SCANCODE_1 => Key.Number1,
            SDL_Scancode.SDL_SCANCODE_2 => Key.Number2,
            SDL_Scancode.SDL_SCANCODE_3 => Key.Number3,
            SDL_Scancode.SDL_SCANCODE_4 => Key.Number4,
            SDL_Scancode.SDL_SCANCODE_5 => Key.Number5,
            SDL_Scancode.SDL_SCANCODE_6 => Key.Number6,
            SDL_Scancode.SDL_SCANCODE_7 => Key.Number7,
            SDL_Scancode.SDL_SCANCODE_8 => Key.Number8,
            SDL_Scancode.SDL_SCANCODE_9 => Key.Number9,
            SDL_Scancode.SDL_SCANCODE_0 => Key.Number0,
            SDL_Scancode.SDL_SCANCODE_RETURN => Key.Enter,
            SDL_Scancode.SDL_SCANCODE_ESCAPE => Key.Escape,
            SDL_Scancode.SDL_SCANCODE_BACKSPACE => Key.Backspace,
            SDL_Scancode.SDL_SCANCODE_TAB => Key.Tab,
            SDL_Scancode.SDL_SCANCODE_SPACE => Key.Space,
            SDL_Scancode.SDL_SCANCODE_MINUS => Key.Minus,
            SDL_Scancode.SDL_SCANCODE_EQUALS => Key.Equal,
            SDL_Scancode.SDL_SCANCODE_LEFTBRACKET => Key.LeftBracket,
            SDL_Scancode.SDL_SCANCODE_RIGHTBRACKET => Key.RightBracket,
            SDL_Scancode.SDL_SCANCODE_BACKSLASH => Key.BackSlash,
            SDL_Scancode.SDL_SCANCODE_NONUSHASH => Key.Unknown, // TODO
            SDL_Scancode.SDL_SCANCODE_SEMICOLON => Key.Semicolon,
            SDL_Scancode.SDL_SCANCODE_APOSTROPHE => Key.Apostrophe,
            SDL_Scancode.SDL_SCANCODE_GRAVE => Key.GraveAccent,
            SDL_Scancode.SDL_SCANCODE_COMMA => Key.Comma,
            SDL_Scancode.SDL_SCANCODE_PERIOD => Key.Period,
            SDL_Scancode.SDL_SCANCODE_SLASH => Key.Slash,
            SDL_Scancode.SDL_SCANCODE_CAPSLOCK => Key.CapsLock,
            SDL_Scancode.SDL_SCANCODE_F1 => Key.F1,
            SDL_Scancode.SDL_SCANCODE_F2 => Key.F2,
            SDL_Scancode.SDL_SCANCODE_F3 => Key.F3,
            SDL_Scancode.SDL_SCANCODE_F4 => Key.F4,
            SDL_Scancode.SDL_SCANCODE_F5 => Key.F5,
            SDL_Scancode.SDL_SCANCODE_F6 => Key.F6,
            SDL_Scancode.SDL_SCANCODE_F7 => Key.F7,
            SDL_Scancode.SDL_SCANCODE_F8 => Key.F8,
            SDL_Scancode.SDL_SCANCODE_F9 => Key.F9,
            SDL_Scancode.SDL_SCANCODE_F10 => Key.F10,
            SDL_Scancode.SDL_SCANCODE_F11 => Key.F11,
            SDL_Scancode.SDL_SCANCODE_F12 => Key.F12,
            SDL_Scancode.SDL_SCANCODE_PRINTSCREEN => Key.PrintScreen,
            SDL_Scancode.SDL_SCANCODE_SCROLLLOCK => Key.ScrollLock,
            SDL_Scancode.SDL_SCANCODE_PAUSE => Key.Pause,
            SDL_Scancode.SDL_SCANCODE_INSERT => Key.Insert,
            SDL_Scancode.SDL_SCANCODE_HOME => Key.Home,
            SDL_Scancode.SDL_SCANCODE_PAGEUP => Key.PageUp,
            SDL_Scancode.SDL_SCANCODE_DELETE => Key.Delete,
            SDL_Scancode.SDL_SCANCODE_END => Key.End,
            SDL_Scancode.SDL_SCANCODE_PAGEDOWN => Key.PageDown,
            SDL_Scancode.SDL_SCANCODE_RIGHT => Key.Right,
            SDL_Scancode.SDL_SCANCODE_LEFT => Key.Left,
            SDL_Scancode.SDL_SCANCODE_DOWN => Key.Down,
            SDL_Scancode.SDL_SCANCODE_UP => Key.Up,
            SDL_Scancode.SDL_SCANCODE_NUMLOCKCLEAR => Key.Unknown, // TODO
            SDL_Scancode.SDL_SCANCODE_KP_DIVIDE => Key.KeypadDivide,
            SDL_Scancode.SDL_SCANCODE_KP_MULTIPLY => Key.KeypadMultiply,
            SDL_Scancode.SDL_SCANCODE_KP_MINUS => Key.KeypadSubtract,
            SDL_Scancode.SDL_SCANCODE_KP_PLUS => Key.KeypadAdd,
            SDL_Scancode.SDL_SCANCODE_KP_ENTER => Key.KeypadEnter,
            SDL_Scancode.SDL_SCANCODE_KP_1 => Key.Keypad1,
            SDL_Scancode.SDL_SCANCODE_KP_2 => Key.Keypad2,
            SDL_Scancode.SDL_SCANCODE_KP_3 => Key.Keypad3,
            SDL_Scancode.SDL_SCANCODE_KP_4 => Key.Keypad4,
            SDL_Scancode.SDL_SCANCODE_KP_5 => Key.Keypad5,
            SDL_Scancode.SDL_SCANCODE_KP_6 => Key.Keypad6,
            SDL_Scancode.SDL_SCANCODE_KP_7 => Key.Keypad7,
            SDL_Scancode.SDL_SCANCODE_KP_8 => Key.Keypad8,
            SDL_Scancode.SDL_SCANCODE_KP_9 => Key.Keypad9,
            SDL_Scancode.SDL_SCANCODE_KP_0 => Key.Keypad0,
            SDL_Scancode.SDL_SCANCODE_KP_PERIOD => Key.KeypadDecimal,
            SDL_Scancode.SDL_SCANCODE_NONUSBACKSLASH => Key.Unknown, // TODO
            // Key modifiers!!!
            SDL_Scancode.SDL_SCANCODE_LCTRL => Key.Unknown,
            SDL_Scancode.SDL_SCANCODE_LSHIFT => Key.Unknown,
            SDL_Scancode.SDL_SCANCODE_LALT => Key.Unknown,
            SDL_Scancode.SDL_SCANCODE_LGUI => Key.Unknown, // Windows, Command, Meta
            SDL_Scancode.SDL_SCANCODE_RCTRL => Key.Unknown,
            SDL_Scancode.SDL_SCANCODE_RSHIFT => Key.Unknown,
            SDL_Scancode.SDL_SCANCODE_RALT => Key.Unknown,
            SDL_Scancode.SDL_SCANCODE_RGUI => Key.Unknown, // Windows, Command, Meta
            _ => Key.Unknown,
        };

        public static PointerButton GetPointerButton(byte button)
        {
            // which button?
            switch (button)
            {
                case 1:
                    return PointerButton.Left;
                case 2:
                    return PointerButton.Middle;
                case 3:
                    return PointerButton.Right;
                case 4:
                    return PointerButton.Button4;
                case 5:
                    return PointerButton.Button5;
            }

            return PointerButton.Left;
        }

        // todo: check these
        public static SDL_SystemCursor GetSDLSystemCursor(PointerType pointerType) => pointerType switch
        {
            PointerType.Arrow => SDL_SystemCursor.SDL_SYSTEM_CURSOR_DEFAULT,
            PointerType.IBeam => SDL_SystemCursor.SDL_SYSTEM_CURSOR_TEXT,
            PointerType.Crosshair => SDL_SystemCursor.SDL_SYSTEM_CURSOR_CROSSHAIR,
            PointerType.Hand => SDL_SystemCursor.SDL_SYSTEM_CURSOR_POINTER,
            PointerType.No => SDL_SystemCursor.SDL_SYSTEM_CURSOR_NOT_ALLOWED,
            PointerType.SizeAll => SDL_SystemCursor.SDL_SYSTEM_CURSOR_MOVE,
            PointerType.SizeNESW => SDL_SystemCursor.SDL_SYSTEM_CURSOR_NESW_RESIZE,
            PointerType.SizeNS => SDL_SystemCursor.SDL_SYSTEM_CURSOR_NS_RESIZE,
            PointerType.SizeNWSE => SDL_SystemCursor.SDL_SYSTEM_CURSOR_NWSE_RESIZE,
            PointerType.SizeWE => SDL_SystemCursor.SDL_SYSTEM_CURSOR_EW_RESIZE,
            PointerType.Wait => SDL_SystemCursor.SDL_SYSTEM_CURSOR_WAIT,
            PointerType.WaitArrow => SDL_SystemCursor.SDL_SYSTEM_CURSOR_PROGRESS,
            _ => throw new ArgumentOutOfRangeException(nameof(pointerType), $"Not expected pointer type value: {pointerType}"),
        };
    }
}