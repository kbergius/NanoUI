using NanoUI.Common;
using NanoUI.Nvg;
using System.Numerics;

namespace NanoUI.Styles
{
    /// <summary>
    /// Global pointer styles
    /// </summary>
    public struct PointerStyle
    {
        /// <summary>
        /// Default pointer type when no other spesified or ResetPointerType called.
        /// </summary>
        public int PointerType { get; set; }

        /// <summary>
        /// Marker color can be used to "mark" position in the display.
        /// </summary>
        public Color MarkerColor { get; set; }

        /// <summary>
        /// Radius for marker circle.
        /// </summary>
        public float MarkerRadius { get; set; } = 14f;

        public PointerStyle() { }

        /// <summary>
        /// Draws marker circle with color & radius to given position.
        /// </summary>
        public void DrawMarker(NvgContext ctx, Vector2 position)
        {
            ctx.BeginPath();
            ctx.Circle(position, MarkerRadius);
            ctx.FillColor(MarkerColor);
            ctx.Fill();
        }
    }
}
