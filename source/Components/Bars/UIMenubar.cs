using NanoUI.Common;
using NanoUI.Nvg;

namespace NanoUI.Components.Bars
{
    // note: this doesn't pass OnKeyChar event to children, because widget needs to be in focuspath

    /// <summary>
    /// UIMenubar is only widget, that can receive OnKeyUpDown event while not in UIScreen's focus path.
    /// </summary>
    /// <remarks>
    /// To get this OnKeyUpDown event parent of this (screen or window) must be in focus path (screen is allways).
    /// The main use case is to pass keyboard shortcuts to menu items.
    /// This means also that you can have multiple windows with menubars, but only the active/focused
    /// window gets event and passes it to this widget.
    /// </remarks>
    public class UIMenubar : UIWidgetbar
    {
        /// <inheritdoc />
        public UIMenubar() { }

        /// <inheritdoc />
        public UIMenubar(UIWidget parent)
            : base(parent)
        {
            DisablePointerFocus = true;
        }

        #region Events

        /// <inheritdoc />
        public override bool OnKeyUpDown(Key key, bool down, KeyModifiers modifiers)
        {
            foreach (var child in Children.AsReadOnlySpan())
            {
                if (!child.Visible || child.Disabled)
                    continue;

                if (child.OnKeyUpDown(key, down, modifiers))
                {
                    return true;
                }
            }

            return base.OnKeyUpDown(key, down, modifiers);
        }

        #endregion

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
