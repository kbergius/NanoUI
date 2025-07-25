﻿using Silk.NET.Input;
using System;
// map to NanoUI
using UIKey = NanoUI.Common.Key;
using UIKeyModifiers = NanoUI.Common.KeyModifiers;
using UIPointerButton = NanoUI.Common.PointerButton;
using UIPointerType = NanoUI.Common.PointerType;

namespace VeldridExample
{
    // todo : check if osx & convert modifiers right - so no super modifier 
    public static class InputMappings
    {
        public static UIKeyModifiers KeyModifiers { get; private set; } = UIKeyModifiers.None;

        static void SetModifierKey(UIKeyModifiers modifier, bool down)
        {
            if (down)
            {
                if((KeyModifiers & modifier) == 0)
                    KeyModifiers |= modifier;
            }
            else
            {
                if ((KeyModifiers & modifier) != 0)
                    KeyModifiers &= ~modifier;
            }
        }

        // note: modifier keys returns false (they are stored/removed in this class - not in main program)
        // isRepeat marks that we store this key & use is with when key is down
        // (normally OnKeyDown/Up is fired only once)
        // used primarily with non-char keys: Backspace, Delete (todo: navigation keys: up, down, left, right)

        // note: since NanoUI keys are 1:1 with Silk.NET.Input keys, we could just check modifiers & repeat keys & Unknown
        // and convert rest straight away. this way it's still easier(?) to understand, if user uses some other windowing/key system.
        public static bool TryMapKey(Key key, bool down, out UIKey result, out bool isRepeat)
        {
            // default
            isRepeat = false;

            UIKey KeyToUIKey(Key keyToConvert, Key startKey1, UIKey startKey2)
            {
                int changeFromStart1 = (int)keyToConvert - (int)startKey1;
                return startKey2 + changeFromStart1;
            }

            if (key >= Key.F1 && key <= Key.F25)
            {
                result = KeyToUIKey(key, Key.F1, UIKey.F1);
                return true;
            }
            else if (key >= Key.Keypad0 && key <= Key.Keypad9)
            {
                result = KeyToUIKey(key, Key.Keypad0, UIKey.Keypad0);
                return true;
            }
            else if (key >= Key.A && key <= Key.Z)
            {
                result = KeyToUIKey(key, Key.A, UIKey.A);
                return true;
            }
            else if (key >= Key.Number0 && key <= Key.Number9)
            {
                result = KeyToUIKey(key, Key.Number0, UIKey.Number0);
                return true;
            }

            switch (key)
            {
                case Key.ShiftLeft:
                    result = UIKey.ShiftLeft;
                    SetModifierKey(UIKeyModifiers.Shift, down);
                    return false;
                case Key.ShiftRight:
                    result = UIKey.ShiftRight;
                    SetModifierKey(UIKeyModifiers.Shift, down);
                    return false;
                case Key.ControlLeft:
                    result = UIKey.ControlLeft;
                    SetModifierKey(UIKeyModifiers.Control, down);
                    return false;
                case Key.ControlRight:
                    result = UIKey.ControlRight;
                    SetModifierKey(UIKeyModifiers.Control, down);
                    return false;
                case Key.AltLeft:
                    result = UIKey.AltLeft;
                    SetModifierKey(UIKeyModifiers.Alt, down);
                    return false;
                case Key.AltRight:
                    result = UIKey.AltRight;
                    SetModifierKey(UIKeyModifiers.Alt, down);
                    return false;
                case Key.Menu:
                    result = UIKey.Menu;
                    return true;
                case Key.Up:
                    result = UIKey.Up;
                    return true;
                case Key.Down:
                    result = UIKey.Down;
                    return true;
                case Key.Left:
                    result = UIKey.Left;
                    return true;
                case Key.Right:
                    result = UIKey.Right;
                    return true;
                case Key.Enter:
                    result = UIKey.Enter;
                    return true;
                case Key.Escape:
                    result = UIKey.Escape;
                    return true;
                case Key.Space:
                    result = UIKey.Space;
                    return true;
                case Key.Tab:
                    result = UIKey.Tab;
                    return true;
                case Key.Backspace:
                    result = UIKey.Backspace;
                    isRepeat = true;
                    return true;
                case Key.Insert:
                    result = UIKey.Insert;
                    return true;
                case Key.Delete:
                    result = UIKey.Delete;
                    isRepeat = true;
                    return true;
                case Key.PageUp:
                    result = UIKey.PageUp;
                    return true;
                case Key.PageDown:
                    result = UIKey.PageDown;
                    return true;
                case Key.Home:
                    result = UIKey.Home;
                    return true;
                case Key.End:
                    result = UIKey.End;
                    return true;
                case Key.CapsLock:
                    result = UIKey.CapsLock;
                    return true;
                case Key.ScrollLock:
                    result = UIKey.ScrollLock;
                    return true;
                case Key.PrintScreen:
                    result = UIKey.PrintScreen;
                    return true;
                case Key.Pause:
                    result = UIKey.Pause;
                    return true;
                case Key.NumLock:
                    result = UIKey.NumLock;
                    return true;
                case Key.KeypadDivide:
                    result = UIKey.KeypadDivide;
                    return true;
                case Key.KeypadMultiply:
                    result = UIKey.KeypadMultiply;
                    return true;
                case Key.KeypadSubtract:
                    result = UIKey.KeypadSubtract;
                    return true;
                case Key.KeypadAdd:
                    result = UIKey.KeypadAdd;
                    return true;
                case Key.KeypadDecimal:
                    result = UIKey.KeypadDecimal;
                    return true;
                case Key.KeypadEnter:
                    result = UIKey.KeypadEnter;
                    return true;
                case Key.KeypadEqual:
                    result = UIKey.KeypadEqual;
                    return true;
                case Key.GraveAccent:
                    result = UIKey.GraveAccent;
                    return true;
                case Key.Minus:
                    result = UIKey.Minus;
                    return true;
                case Key.Equal:
                    result = UIKey.Equal;
                    return true;
                case Key.LeftBracket:
                    result = UIKey.LeftBracket;
                    return true;
                case Key.RightBracket:
                    result = UIKey.RightBracket;
                    return true;
                case Key.Semicolon:
                    result = UIKey.Semicolon;
                    return true;
                case Key.Apostrophe:
                    result = UIKey.Apostrophe;
                    return true;
                case Key.Comma:
                    result = UIKey.Comma;
                    return true;
                case Key.Period:
                    result = UIKey.Period;
                    return true;
                case Key.Slash:
                    result = UIKey.Slash;
                    return true;
                case Key.BackSlash:
                    result = UIKey.BackSlash;
                    return true;
                case Key.World1:
                    result = UIKey.World1;
                    return true;
                case Key.World2:
                    result = UIKey.World2;
                    return true;
                case Key.SuperLeft: // osx - todo :we should convert this to "standard" modifiers
                    result = UIKey.SuperLeft;
                    return true;
                case Key.SuperRight:  // osx- todo : we should convert this to "standard" modifiers
                    result = UIKey.SuperRight;
                    return true;
                default:
                    result = UIKey.Unknown;
                    return false;
            }
        }

        public static UIPointerButton MapMouseButtons(MouseButton mouseButton) => mouseButton switch
        {
            MouseButton.Left => UIPointerButton.Left,
            MouseButton.Middle => UIPointerButton.Middle,
            MouseButton.Right => UIPointerButton.Right,
            MouseButton.Button4 => UIPointerButton.Button4,
            MouseButton.Button5 => UIPointerButton.Button5,
            MouseButton.Button6 => UIPointerButton.Button6,
            MouseButton.Button7 => UIPointerButton.Button7,
            MouseButton.Button8 => UIPointerButton.Button8,
            MouseButton.Button9 => UIPointerButton.Button9,
            MouseButton.Button10 => UIPointerButton.Button10,
            MouseButton.Button11 => UIPointerButton.Button11,
            MouseButton.Button12 => UIPointerButton.Button12,
            _ => UIPointerButton.LastButton
        };

        public static StandardCursor GetCursorType(int pointerType, out CursorType cursorType)
        {
            if (pointerType < 0 || pointerType >= Enum.GetValues<UIPointerType>().Length)
            {
                cursorType = CursorType.Custom;
                return StandardCursor.Default;
            }

            cursorType = CursorType.Standard;
            return GetCursor((UIPointerType)pointerType);
        }

        static StandardCursor GetCursor(UIPointerType pointerType) => pointerType switch
        {
            UIPointerType.Arrow => StandardCursor.Arrow,
            UIPointerType.Crosshair => StandardCursor.Crosshair,
            UIPointerType.Hand => StandardCursor.Hand,
            UIPointerType.IBeam => StandardCursor.IBeam,
            UIPointerType.No => StandardCursor.NotAllowed,
            UIPointerType.SizeAll => StandardCursor.ResizeAll,
            UIPointerType.SizeNESW => StandardCursor.NeswResize,
            UIPointerType.SizeNS => StandardCursor.VResize,
            UIPointerType.SizeNWSE => StandardCursor.NwseResize,
            UIPointerType.SizeWE => StandardCursor.HResize,
            UIPointerType.Wait => StandardCursor.Wait,
            UIPointerType.WaitArrow => StandardCursor.WaitArrow,
            _ => StandardCursor.Default
        };
    }
}
