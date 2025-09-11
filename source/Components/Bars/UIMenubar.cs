using NanoUI.Common;
using NanoUI.Nvg;

namespace NanoUI.Components.Bars
{
    // this is only widget, that can receive OnKeyUpDown event while not in focus path
    // to get this event parent of this (screen or window) must be in focus path (screen is allways)
    // the main usage case is to pass keyboard shortcuts to menuitems

    // note: this means you can have multiple windows with menubars, but only the active/focused
    // window gets event and passes it to this widget
    // note2: this doesn't pass OnKeyChar event to children, because widget needs to be in focuspath
    public class UIMenubar : UIWidgetbar
    {
        public UIMenubar() { }

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
