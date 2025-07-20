using NanoUI.Common;
using NanoUI.Layouts;
using NanoUI.Nvg;
using System;
using System.Numerics;

namespace NanoUI.Components.Menus
{
    // todo: somehow dosn't use hover color change
    public class UIMenu : UIPopupButton
    {
        // this is called from the child menu item buttons, if they have ItemId specified
        // handles clicks & shortcut matches.
        public Action<int> MenuItemSelected;

        public UIMenu()
        {
            _popupWidth = 0;
        }

        public UIMenu(UIWidget parent)
            : base(parent)
        {
        }

        public UIMenu(UIWidget parent, string caption)
            : base(parent, caption)
        {
            // by default we don't show any icon
            Icon = -1;
            // if icon is specified it is shown by default right
            IconAlign = IconAlign.Right;
            TextHorizontalAlignment = TextHorizontalAlign.Left;

            ThemeType = typeof(UIMenu);
            
            // popup
            Popup.ChildrenLayout = new StackLayout(Orientation.Vertical, LayoutAlignment.Minimum);
            Popup.RelativePosition = PopupPosition.Bottom;
            Popup.AnchorSize = 0;
        }

        #region Properties

        int? _popupWidth;
        public int PopupWidth
        {
            get => _popupWidth ?? GetTheme().Menu.PopupWidth;
            set => _popupWidth = value;
        }

        #endregion

        #region Layout

        public override void PerformLayout(NvgContext ctx)
        {
            Popup.FixedSize = new Vector2(PopupWidth, 0);

            base.PerformLayout(ctx);
        }

        #endregion

        #region Events

        // to get (shortcut) event:
        // - this must be focused (in focusbar) OR
        // - parent must be menubar, that is directly in window children list & window is focused/active OR
        // - parent must be menubar, that is directly in screen children list
        public override bool OnKeyUpDown(Key key, bool down, KeyModifiers modifiers)
        {
            // forward to popup (shortcuts)
            return Popup.OnKeyUpDown(key, down, modifiers);
        }

        // this is called from menu items when they are clicked or shortcut matches
        public virtual void OnMenuItemSelected(int menuItemId)
        {
            // this closes popups
            RequestFocus();

            MenuItemSelected?.Invoke(menuItemId);
        }

        public override bool OnPointerUpDown(Vector2 p, PointerButton button, bool down)
        {
            return base.OnPointerUpDown(p, button, down);
        }

        #endregion
    }
}