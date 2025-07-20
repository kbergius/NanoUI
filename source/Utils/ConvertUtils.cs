using NanoUI.Common;
using System;
using System.Collections.Generic;

namespace NanoUI.Utils
{
    public static class ConvertUtils
    {
        // icon strings (we convert int to string)
        // note: we must use this since char.ConvertFromUtf32(int) allocates new string
        static Dictionary<int, string> _iconStrings = new();

        public static ReadOnlySpan<char> GetIconString(int icon)
        {
            if (!_iconStrings.TryGetValue(icon, out var str))
            {
                _iconStrings.Add(icon, char.ConvertFromUtf32(icon));
            }
            return str;
        }

        public static TextAlignment ConvertHorizontalAlign(TextHorizontalAlign horizontalAlign)
        {
            switch (horizontalAlign)
            {
                case TextHorizontalAlign.Left:
                    return TextAlignment.Left;
                case TextHorizontalAlign.Center:
                    return TextAlignment.Center;
                case TextHorizontalAlign.Right:
                    return TextAlignment.Right;
            }
            return TextAlignment.Left;
        }
        public static TextAlignment ConvertVerticalAlign(TextVerticalAlign verticalAlign)
        {
            switch (verticalAlign)
            {
                case TextVerticalAlign.Top:
                    return TextAlignment.Top;
                case TextVerticalAlign.Middle:
                    return TextAlignment.Middle;
                case TextVerticalAlign.Bottom:
                    return TextAlignment.Bottom;
            }
            return TextAlignment.Top;
        }

        public static TextAlignment ConvertTextAlign(TextHorizontalAlign horizontalAlign, TextVerticalAlign verticalAlign)
        {
            return ConvertHorizontalAlign(horizontalAlign) | ConvertVerticalAlign(verticalAlign);
        }
    }
}
