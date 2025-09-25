using NanoUI.Common;
using System;
using System.Collections.Generic;

namespace NanoUI.Utils
{
    /// <summary>
    /// ConvertUtils provides some common conversions.
    /// </summary>
    public static class ConvertUtils
    {
        static Dictionary<int, string> _iconStrings = new();

        /// <summary>
        /// Converts icon int to string and stores it.
        /// </summary>
        /// <remarks>Prevents allocating new strings</remarks>
        public static ReadOnlySpan<char> GetIconString(int icon)
        {
            if (!_iconStrings.TryGetValue(icon, out var str))
            {
                _iconStrings.Add(icon, char.ConvertFromUtf32(icon));
            }

            return str;
        }

        /// <summary>
        /// Converts horizontal alignment to TextAlignment.
        /// </summary>
        /// <param name="horizontalAlign">TextHorizontalAlign</param>
        /// <returns>TextAlignment</returns>
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

        /// <summary>
        /// Converts vertical alignment to TextAlignment.
        /// </summary>
        /// <param name="verticalAlign"></param>
        /// <returns></returns>
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

        /// <summary>
        /// Converts separate horizontalAlign & verticalAlign to 1 TextAlignment value.
        /// </summary>
        /// <param name="horizontalAlign">TextHorizontalAlign</param>
        /// <param name="verticalAlign">TextVerticalAlign</param>
        /// <returns>TextAlignment</returns>
        public static TextAlignment ConvertTextAlign(TextHorizontalAlign horizontalAlign, TextVerticalAlign verticalAlign)
        {
            return ConvertHorizontalAlign(horizontalAlign) | ConvertVerticalAlign(verticalAlign);
        }
    }
}
