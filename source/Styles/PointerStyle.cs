using NanoUI.Common;
using NanoUI.Nvg;
using System.Numerics;

namespace NanoUI.Styles
{
    /// <summary>
    /// Global pointer style
    /// </summary>
    public struct PointerStyle
    {
        /// <summary>
        /// Default pointer type when no other specified or ResetPointerType called.
        /// </summary>
        public int PointerType { get; set; }

        /// <summary>
        /// Pointer marker color can be used to "mark" position in the display.
        /// </summary>
        /// <remarks>Can be used in post draw phase.</remarks>
        public Color MarkerColor { get; set; }

        /// <summary>
        /// Radius for pointer marker circle. Default: 14.
        /// </summary>
        public float MarkerRadius { get; set; } = 14f;

        /// <summary>
        /// Constructor.
        /// </summary>
        public PointerStyle() { }

        /// <summary>
        /// Draws pointer marker circle with color & radius to given position.
        /// </summary>
        /// <param name="ctx">NvgContext</param>
        /// <param name="position">Circle center</param>
        public void DrawMarker(NvgContext ctx, Vector2 position)
        {
            ctx.BeginPath();
            ctx.Circle(position, MarkerRadius);
            ctx.FillColor(MarkerColor);
            ctx.Fill();
        }
    }
}
