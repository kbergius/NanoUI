using NanoUI.Common;
using NanoUI.Components.Bars;
using System.Numerics;

namespace NanoUI.Components.Docking
{
    // todo: there should be easy mechanics to set title etc (probably context menu)

    /// <summary>
    /// DockTitlebar supports only dock nodes.
    /// </summary>
    public class DockTitlebar : UITitlebar
    {
        /// <inheritdoc />
        public DockTitlebar() { }

        // todo: dock node name

        /// <inheritdoc />
        public DockTitlebar(DockNode parent)
            :base(parent, "DockNode")
        {
            // todo: in properties
            BackgroundFocused = GetTheme().Docks.TitleBackgroundFocused;
            BackgroundUnfocused = GetTheme().Docks.TitleBackgroundUnfocused;

            string? fontType = GetTheme().Docks.TitleFontType;
            if (fontType != null)
            {
                FontType = fontType;
            }
            
            FontSize = GetTheme().Docks.TitleFontSize;
            ButtonSize = GetTheme().Docks.TitleButtonSize;
        }

        #region Events

        /// <inheritdoc />
        public override bool OnPointerUpDown(Vector2 p, PointerButton button, bool down)
        {
            // check first if titlebar button handle event
            if(base.OnPointerUpDown(p, button, down))
            {
                return true;
            }

            // we handle - pointer on label
            // note: we don't need to "clear" drag widget since UIScreen does it automatically when pointer UP
            if (down)
            {
                Screen?.SetDragWidget(this);
            }
            
            return true;
        }

        /// <inheritdoc />
        public override bool OnPointerDrag(Vector2 p, Vector2 rel)
        {
            // check if this is dragged outside title area

            if (!Contains(p))
            {
                // remove drag widget, so we can start detach process
                Screen?.SetDragWidget(null);

                // signal parent dock node that it should be detached
                if (Parent != null && Parent.OnDetach(this))
                {

                }

                return true;
            }

            return false;
        }

        #endregion
    }
}
