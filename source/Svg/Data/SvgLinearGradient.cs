using NanoUI.Common;
using NanoUI.Nvg.Data;
using System;
using System.Numerics;

namespace NanoUI.Svg.Data
{
    // note: we treat all values as percentages
    internal struct SvgLinearGradient
    {
        // Defines a unique id for the <linearGradient> element
        public string id;
        // The x position of the starting point of the vector gradient. Default is 0%
        public float x1;
        // The x position of the ending point of the vector gradient. Default is 100%
        public float x2;
        // The y position of the starting point of the vector gradient. Default is 0%
        public float y1;
        // y2	The y position of the ending point of the vector gradient. Default is 0%
        public float y2;

        // collect stops here
        public SvgGradientStop[] Stops;

        public bool TryGetPaint(NvgBounds bounds, out Paint paint)
        {
            // not valid values
            if (Stops == null || Stops.Length < 2 && string.IsNullOrEmpty(id))
            {
                paint = default;
                return false;
            }

            // convert to proportional values (range 0 .. 1)
            float sx = Math.Clamp(x1 / 100, 0, 1);
            float sy = Math.Clamp(y1 / 100, 0, 1);
            float ex = Math.Clamp(x2 / 100, 0, 1);
            float ey = Math.Clamp(y2 / 100, 0, 1);
            
            // convert to world space
            Vector2 start = bounds.Min + new Vector2(sx, sy) * (bounds.Max - bounds.Min);
            Vector2 end = bounds.Min + new Vector2(ex, ey) * (bounds.Max - bounds.Min);

            // get colors
            // start
            Color startColor = new Color(Stops[0].StopColor, Math.Clamp(Stops[0].StopOpacity, 0, 1));
            // we support only 2 stops - take last
            Color endColor = new Color(
                Stops[Stops.Length - 1].StopColor,
                Math.Clamp(Stops[Stops.Length - 1].StopOpacity, 0, 1));

            // create paint
            paint = Paint.LinearGradient(start, end, startColor, endColor);
            
            return true;
        }
    }
}