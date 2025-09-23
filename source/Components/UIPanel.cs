using NanoUI.Nvg;

namespace NanoUI.Components
{
    /// <summary>
    /// UIPanel is simple container widget, that only draws background (if spesified) and children.
    /// </summary>
    public class UIPanel : UIWidget
    {
        public UIPanel(UIWidget parent)
            : base(parent)
        {

        }

        /// <inheritdoc />
        public override void Draw(NvgContext ctx)
        {
            // background
            DrawBackgroundBrush(ctx);

            // draw children
            base.Draw(ctx);
        }
    }
}
