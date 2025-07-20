using NanoUI.Common;

namespace NanoUI.Svg.Data
{
    internal struct SvgGradientStop
    {
        // defines where the gradient stop is placed along the gradient vector.
        public float Offset;
        // defines the color of the gradient stop.
        public Color StopColor;
        // defines the opacity of the gradient stop.
        public float StopOpacity;

        public bool IsPercent;
    }
}