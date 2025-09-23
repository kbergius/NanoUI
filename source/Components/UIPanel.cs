using NanoUI.Nvg;

namespace NanoUI.Components
{
    /// <summary>
    /// UIPanel is a simple container widget, that only draws background (if spesified) and children.
    /// </summary>
    public class UIPanel : UIWidget
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="parent">Parent widget. Can't be null</param>
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
