using NanoUI.Common;
using NanoUI.Nvg;
using System.Numerics;

namespace NanoUI.Styles
{
    /// <summary>
    /// PointerStyle.
    /// </summary>
    public struct PointerStyle
    {
        // this is default pointer type when no other spesified or ResetCursor called
        public int PointerType { get; set; }

        // marker that is shown for example when docking process is ongoing (draws circle in the pointer position)
        // used by now in Docking
        public Color MarkerColor { get; set; }

        // todo: should this be in globals?
        public float MarkerRadius { get; set; } = 14f;

        public PointerStyle() { }

        // this is the marker for dock widget in redocking process
        // - we don't want to draw whole redocking window content
        // - redocking may not happen (target docking area is not spesified)
        // note: could be used elsewhere
        public void DrawMarker(NvgContext ctx, Vector2 position)
        {
            ctx.BeginPath();
            ctx.Circle(position, MarkerRadius);
            ctx.FillColor(MarkerColor);
            ctx.Fill();
        }
    }
}
