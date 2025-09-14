using NanoUI.Common;
using NanoUI.Nvg;
using System.Numerics;

namespace NanoUI.Components.Docking
{
    /// <summary>
    /// DockHit.
    /// </summary>
    public struct DockHit
    {
        /// <summary>
        /// Hit areas (used also to check hit).
        /// </summary>
        public Rect Area;
        
        /// <summary>
        /// Hit areas' fills - visual clue where docking is happening.
        /// </summary>
        public Rect Fill;

        /// <summary>
        /// Overlay areas - visual clue where docking is happening.
        /// </summary>
        public Rect Overlay;

        public bool Contains(Vector2 position) => Area.Contains(position);

        public void DrawHitArea(NvgContext ctx, float cornerRadius, Color backgroundColor, BrushBase brush)
        {
            if (Area.Size == Vector2.Zero)
                return;

            // background
            ctx.BeginPath();
            ctx.RoundedRect(Area, cornerRadius);
            ctx.FillColor(backgroundColor);
            ctx.Fill();

            // fill
            if (Fill.Size != Vector2.Zero)
            {
                brush?.Draw(ctx,
                    Fill.Position, Fill.Size, null);
            }

            // we draw border last since it overwrites background & fill
            ctx.BeginPath();
            ctx.RoundedRect(Area, cornerRadius);
            ctx.StrokeWidth(1);
            ctx.StrokeColor(Color.Black);
            ctx.Stroke();
        }

        public void DrawOverlay(NvgContext ctx, Color color)
        {
            ctx.BeginPath();
            ctx.Rect(Overlay);
            ctx.FillColor(color);
            ctx.Fill();
        }
    }
}
