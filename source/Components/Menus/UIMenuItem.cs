using NanoUI.Common;
using NanoUI.Nvg;
using System.Numerics;

namespace NanoUI.Components.Menus
{
    /// <summary>
    /// UIMenuItem.
    /// </summary>
    public class UIMenuItem : UIButton
    {
        public UIMenuItem()
        {
            _padding = new();
        }

        public UIMenuItem(UIWidget parent, string caption = "Untitled", int icon = -1, int itemId = -1)
            : base(parent, caption, icon)
        {
            IconAlign = IconAlign.Left;
            TextHorizontalAlignment = TextHorizontalAlign.Left;
            
            Border = false;

            ItemId = itemId;

            ThemeType = typeof(UIMenuItem);
        }

        #region Properties

        // if this is non-negative, thi will try to find its parent menu button & invoke clicked
        // event there. otherwise it will pass input event to base class.
        // note: if you use item ids, you can just wrap the main menu button action & you need not wrap
        // all menuitem clicked events individually
        // note2: works same way with shortcuts
        // note3: it would be easier to deal with ids, if you determine them with some custom enum
        public int ItemId { get; set; } = -1;

        Thickness? _padding;
        public override Thickness Padding
        {
            get => _padding ?? GetTheme().MenuItem.Padding;
            set => _padding = value;
        }

        public Shortcut? Shortcut { get; set; }

        #endregion

        #region Layout

        public override void PerformLayout(NvgContext ctx)
        {
            base.PerformLayout(ctx);
            // stretch with parent
            if(Parent != null)
                Size = new Vector2(Parent.Size.X, Size.Y);
        }

        #endregion

        #region Events

        public override bool OnPointerUpDown(Vector2 p, PointerButton button, bool down)
        {
            if(ItemId >= 0 && button == PointerButton.Left && !down)
            {
                // invoke event in main menu button
                InvokeMainMenuEvent();
            }

            // let the base button do their thing
            return base.OnPointerUpDown(p, button, down);
        }

        public override bool OnKeyUpDown(Key key, bool down, KeyModifiers modifiers)
        {
            if (down && ItemId >= 0 && Shortcut.HasValue && Shortcut.Value.Match(key, modifiers))
            {
                // invoke event in main menu button
                InvokeMainMenuEvent();

                // found match - don't go further
                return true;
            }

            return base.OnKeyUpDown(key, down, modifiers);
        }

        #endregion

        #region Drawing

        public override void Draw(NvgContext ctx)
        {
            base.Draw(ctx);

            // draw shortcut
            if (!Shortcut.HasValue)
                return;

            ctx.FontSize(FontSize);
            ctx.FontFaceId(FontFaceId);

            ctx.TextAlign(TextAlignment.Left | TextAlignment.Middle);
            ctx.FillColor(TextColor);

            // note: top left
            ctx.Text(Position.X + Size.X - 60, Position.Y + Size.Y * 0.5f, Shortcut.Value.ToString());
        }

        #endregion

        #region Private

        void InvokeMainMenuEvent()
        {
            // find main menu button or context menu
            UIMenu? mainButton = null;
            UIContextMenu? contextMenu = null;

            var parent = Parent;

            while (parent != null)
            {
                // we must check for popups first, because their parent is Screen
                //if (parent is Popup popup &&
                //    popup.GetParentButton() is MenuButton menuButton && menuButton != null)
                if (parent is UIPopup popup)
                {
                    var popupButton = popup.GetParentButton();

                    // check nested
                    if (popupButton is UIMenu menuButton && menuButton != null)
                    {
                        mainButton = menuButton;

                        // change calculation starting with popup button
                        parent = mainButton;
                    }
                    else if(popup is UIContextMenu context)
                    {
                        // we found ultimate parent
                        contextMenu = context;
                        break;
                    }
                }

                parent = parent.Parent;
            }

            // if found, invoke event
            if(contextMenu != null)
            {
                contextMenu.OnMenuItemSelected(ItemId);
            }
            else
            {
                mainButton?.OnMenuItemSelected(ItemId);
            }
        }

        #endregion
    }
}
