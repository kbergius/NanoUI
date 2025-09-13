using NanoUI.Common;
using NanoUI.Layouts;
using System;

namespace NanoUI.Components.Menus
{
    // todo: context menu holds menubuttons, but these can't have shortcuts(?)

    /// <summary>
    /// UIContextMenu.
    /// </summary>
    public class UIContextMenu : UIPopup
    {
        // this is called from the child menu item buttons, if they have ItemId specified
        // handles clicks & shortcut matches.
        public Action<int>? MenuItemSelected;

        UIWidget? _owner;

        /// <inheritdoc />
        public UIContextMenu() { }

        /// <inheritdoc />
        public UIContextMenu(UIWidget owner)
            : base(owner.Screen)
        {
            _owner = owner;
            owner.ContextMenu = this;

            ChildrenLayout = new StackLayout(Orientation.Vertical, LayoutAlignment.Minimum);
            AnchorSize = 0;

            ThemeType = typeof(UIContextMenu);
        }

        #region Events

        // this is called from menu items when they are clicked or shortcut matches
        public virtual void OnMenuItemSelected(int menuItemId)
        {
            // closes this
            _owner?.RequestFocus();

            MenuItemSelected?.Invoke(menuItemId);
        }

        // this is fired from scene
        // this could be in base Popup?

        /// <inheritdoc />
        public override bool OnFocusChanged(bool focused)
        {
            if (!Disabled)
            {
                // set values
                Focused = focused;
                Visible = focused;

                if (!focused)
                {
                    // close child popups
                    RecursiveCloseChildPopups(this);

                    // set focus to owner
                    //_owner.RequestFocus();
                    // lost focus => propagate to owner
                    _owner?.FindParentWindow()?.OnFocusChanged(false);
                }
                else if(Screen != null)
                {
                    // bring window to front
                    _owner?.FindParentWindow()?.MoveToLast();

                    Position = Screen.PointerPosition;
                    RequestFocus();

                    // keep owner window focused (this parent is screen)
                    _owner?.FindParentWindow()?.OnFocusChanged(true);
                }

                return true;
            }

            return base.OnFocusChanged(focused);
        }

        #endregion
    }
}
