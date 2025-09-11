using NanoUI.Components.Simple;
using NanoUI.Nvg;
using System.Numerics;

namespace NanoUI.Components.Menus
{
    public class UIMenuSeparator : UISeparator
    {
        public UIMenuSeparator(UIWidget parent)
            :base(parent)
        {
            Height = 1;
            FixedSize = new Vector2(0, Height);
        }

        #region Layout

        /// <inheritdoc />
        public override void PerformLayout(NvgContext ctx)
        {
            base.PerformLayout(ctx);

            // stretch with parent
            if (Parent != null)
                Size = new Vector2(Parent.Size.X, Size.Y);
        }

        #endregion
    }
}
