using NanoUI.Nvg;

namespace NanoUI.Components.Bars
{
    public class UIToolbar : UIWidgetbar
    {
        public UIToolbar() { }

        public UIToolbar(UIWidget parent)
            : base(parent)
        {
            DisablePointerFocus = true;
        }

        #region Drawing

        /// <inheritdoc />
        public override void Draw(NvgContext ctx)
        {
            DrawBackgroundBrush(ctx);

            base.Draw(ctx);

            this.DrawBorder(ctx, false);
        }

        #endregion
    }
}
