using NanoUI.Common;
using NanoUI.Nvg;
using System.Numerics;

namespace NanoUI.Components.Menus
{
    // note: functions/actions are inherited from UIMenu, but theme from UIMenuItem
    public class UIMenuSubmenu : UIMenu
    {
        public UIMenuSubmenu()
        {
            
        }

        public UIMenuSubmenu(UIWidget parent, string caption)
            : base(parent, caption)
        {
            Border = false;

            // popup
            Popup.RelativePosition = PopupPosition.RightTop;

            ThemeType = typeof(UIMenuItem);
        }

        #region Properties

        Thickness? _padding;
        public override Thickness Padding
        {
            get => _padding ?? GetTheme().MenuItem.Padding;
            set => _padding = value;
        }

        #endregion

        #region Layout

        public override void PerformLayout(NvgContext ctx)
        {
            Icon = GetTheme().Fonts.IconCaretRight;

            base.PerformLayout(ctx);

            // check if this is really in context menu
            var parent = Parent;

            while (parent != null)
            {
                // we must check for popups first, because their parent is Screen
                if (parent is UIPopup popup)
                {
                    var popupButton = popup.GetParentButton();

                    // check nested
                    if (popupButton is UIMenuSubmenu menuButton && menuButton != null)
                    {
                        // change calculation starting with popup button
                        parent = menuButton;
                    }
                    else if (popup is UIContextMenu contextMenu)
                    {
                        // we found ultimate parent - set fixed width same as in context menu
                        FixedSize = new Vector2(contextMenu.FixedSize.X, 0);
                        break;
                    }
                }

                parent = parent.Parent;
            }

            // stretch with parent
            if(Parent != null)
                Size = new Vector2(Parent.Size.X, Size.Y);
        }

        #endregion
    }
}
