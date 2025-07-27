using NanoUI.Common;
using NanoUI.Nvg.Data;
using System;
using System.Numerics;

namespace NanoUI.Svg.Data
{
    // note: we treat all values as percentages

    // todo: this is not entirely correct!
    internal struct SvgRadialGradient
    {
        // Defines a unique id for the <radialGradient> element
        public string id;
        // The x position of the end circle of the radial gradient. Default is 50%
        public float cx;
        // The y position of the end circle of the radial gradient. Default is 50%
        public float cy;
        // The radius of the start circle of the radial gradient. Default is 0%
        //public float fr;
        // The x position of the start circle of the radial gradient. Default is 50%
        public float fx;
        // The y position of the start circle of the radial gradient. Default is 50%
        public float fy;
        // The radius of the end circle of the radial gradient. Default is 50%
        public float r;

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

            // convert to proportional values
            float _cx = Math.Clamp(cx / 100, 0, 1);
            float _cy = Math.Clamp(cy / 100, 0, 1);
            //float _r1 = Math.Clamp(fx / 100, 0, 1);
            //float _r2 = Math.Clamp(fy / 100, 0, 1);
            float _r = Math.Clamp(r / 100, 0, 1);
            
            // support misplaced center
            Vector2 center = bounds.Min + (new Vector2(_cx, _cy) * (bounds.Max - bounds.Min));

            Vector2 rad = bounds.Min + new Vector2(_r) * (bounds.Max - bounds.Min);
            rad = new Vector2(_r) * (bounds.Max - bounds.Min);

            float inr = 0;
            float outr = rad.Y < rad.X? rad.Y : rad.X;

            Color innerColor = new Color(Stops[0].StopColor, Math.Clamp(Stops[0].StopOpacity, 0, 1));
            Color outerColor = new Color(
                Stops[Stops.Length - 1].StopColor,
                Math.Clamp(Stops[Stops.Length - 1].StopOpacity, 0, 1));

            // create paint
            paint = Paint.RadialGradient(center, inr, outr, innerColor, outerColor);

            return true;
        }
    }
}
