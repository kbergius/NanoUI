using NanoUI.Nvg;

namespace NanoUI.Components.Bars
{
    /// <summary>
    /// UIToolbar.
    /// </summary>
    public class UIToolbar : UIWidgetbar
    {
        /// <inheritdoc />
        public UIToolbar() { }

        /// <inheritdoc />
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
