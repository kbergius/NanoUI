using Microsoft.Xna.Framework.Input;
using System;
using static SDL3.SDL;
// map to NanoUI
using UIKey = NanoUI.Common.Key;
using UIKeyModifiers = NanoUI.Common.KeyModifiers;
using UIPointerType = NanoUI.Common.PointerType;

namespace FNAExample
{
    // todo : check if osx & convert modifiers right - so no super modifier 
    public static class InputMappings
    {
        // store modifier keys status (down = true, up = false)
        static bool[] _modifiers = new bool[6];

        public static UIKeyModifiers GetKeyModifiers()
        {
            var modifier = UIKeyModifiers.None;

            // shift
            if (_modifiers[0] || _modifiers[3])
            {
                modifier |= UIKeyModifiers.Shift;
            }

            // control
            if (_modifiers[1] || _modifiers[4])
            {
                modifier |= UIKeyModifiers.Control;
            }

            // alt
            if (_modifiers[2] || _modifiers[5])
            {
                modifier |= UIKeyModifiers.Alt;
            }

            return modifier;
        }

        // Get NanoUI key from FNA key
        public static bool TryGetKey(Keys key, bool down, out UIKey uiKey)
        {
            // default for modifiers
            uiKey = UIKey.Unknown;

            // check modifier keys first
            switch (key)
            {
                case Keys.LeftShift:
                    _modifiers[0] = down;
                    break;
                case Keys.LeftControl:
                    _modifiers[1] = down;
                    break;
                case Keys.LeftAlt:
                    _modifiers[2] = down;
                    break;
                case Keys.RightShift:
                    _modifiers[3] = down;
                    break;
                case Keys.RightControl:
                    _modifiers[4] = down;
                    break;
                case Keys.RightAlt:
                    _modifiers[5] = down;
                    break;
                default:
                    uiKey = GetKey(key);
                    break;
            }

            return uiKey != UIKey.Unknown;
        }

        static UIKey GetKey(Keys key) => key switch
        {
            Keys.Back => UIKey.Backspace,
            Keys.Tab => UIKey.Tab,
            Keys.Enter => UIKey.Enter,
            Keys.CapsLock => UIKey.CapsLock,
            Keys.Escape => UIKey.Escape,
            Keys.Space => UIKey.Space,
            Keys.PageUp => UIKey.PageUp,
            Keys.PageDown => UIKey.PageDown,
            Keys.End => UIKey.End,
            Keys.Home => UIKey.Home,
            Keys.Left => UIKey.Left,
            Keys.Right => UIKey.Right,
            Keys.Up => UIKey.Up,
            Keys.Down => UIKey.Down,
            Keys.PrintScreen => UIKey.PrintScreen,
            Keys.Insert => UIKey.Insert,
            Keys.Delete => UIKey.Delete,
            >= Keys.D0 and <= Keys.D9 => UIKey.Number0 + (key - Keys.D0),
            >= Keys.A and <= Keys.Z => UIKey.A + (key - Keys.A),
            >= Keys.NumPad0 and <= Keys.NumPad9 => UIKey.Keypad0 + (key - Keys.NumPad0),
            Keys.Multiply => UIKey.KeypadMultiply,
            Keys.Add => UIKey.KeypadAdd,
            Keys.Subtract => UIKey.KeypadSubtract,
            Keys.Decimal => UIKey.KeypadDecimal,
            Keys.Divide => UIKey.KeypadDivide,
            >= Keys.F1 and <= Keys.F12 => UIKey.F1 + (key - Keys.F1),
            Keys.NumLock => UIKey.NumLock,
            Keys.Scroll => UIKey.ScrollLock,
            Keys.LeftShift => UIKey.ShiftLeft,
            Keys.LeftControl => UIKey.ControlLeft,
            Keys.LeftAlt => UIKey.AltLeft,
            Keys.OemSemicolon => UIKey.Semicolon,
            Keys.OemPlus => UIKey.Equal,
            Keys.OemComma => UIKey.Comma,
            Keys.OemMinus => UIKey.Minus,
            Keys.OemPeriod => UIKey.Period,
            Keys.OemQuestion => UIKey.Slash,
            Keys.OemTilde => UIKey.GraveAccent,
            Keys.OemOpenBrackets => UIKey.LeftBracket,
            Keys.OemCloseBrackets => UIKey.RightBracket,
            Keys.OemPipe => UIKey.BackSlash,
            Keys.OemQuotes => UIKey.Apostrophe,
            _ => UIKey.Unknown,
        };

        public static SDL_SystemCursor GetMouseCursor(UIPointerType pointerType) => pointerType switch
        {
            UIPointerType.Arrow => SDL_SystemCursor.SDL_SYSTEM_CURSOR_DEFAULT,
            UIPointerType.IBeam => SDL_SystemCursor.SDL_SYSTEM_CURSOR_TEXT,
            UIPointerType.Crosshair => SDL_SystemCursor.SDL_SYSTEM_CURSOR_CROSSHAIR,
            UIPointerType.Hand => SDL_SystemCursor.SDL_SYSTEM_CURSOR_POINTER,
            UIPointerType.No => SDL_SystemCursor.SDL_SYSTEM_CURSOR_NOT_ALLOWED,
            UIPointerType.SizeAll => SDL_SystemCursor.SDL_SYSTEM_CURSOR_MOVE,
            UIPointerType.SizeNESW => SDL_SystemCursor.SDL_SYSTEM_CURSOR_NESW_RESIZE,
            UIPointerType.SizeNS => SDL_SystemCursor.SDL_SYSTEM_CURSOR_NS_RESIZE,
            UIPointerType.SizeNWSE => SDL_SystemCursor.SDL_SYSTEM_CURSOR_NWSE_RESIZE,
            UIPointerType.SizeWE => SDL_SystemCursor.SDL_SYSTEM_CURSOR_EW_RESIZE,
            UIPointerType.Wait => SDL_SystemCursor.SDL_SYSTEM_CURSOR_WAIT,
            UIPointerType.WaitArrow => SDL_SystemCursor.SDL_SYSTEM_CURSOR_PROGRESS,
            _ => throw new ArgumentOutOfRangeException(nameof(pointerType), $"Not expected pointer type value: {pointerType}"),
        };
    }
}
