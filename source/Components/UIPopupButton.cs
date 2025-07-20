using NanoUI.Common;
using NanoUI.Nvg;
using System.Numerics;

namespace NanoUI.Components
{
    public class UIPopupButton : UIButton
    {
        // this is ctor for theme/layout generation (if you use this otherwise, set parent before using widget)
        // note : this handles also all widgets that extend this (MenuView<T>, PopupButton<T>) 
        public UIPopupButton()
        {
            // set defaults to theme impl - prevents circular reference
            MaxPopupHeight = default;
        }

        public UIPopupButton(UIWidget parent)
            :this(parent, string.Empty)
        {
        
        }
        
        // note: Check if not extended, use MaxPopupHeight????
        public UIPopupButton(UIWidget parent, string caption)
            : base(parent, caption) // no icon by default
        {
            Flags = ButtonFlags.ToggleButton;

            // use UIButton as base theme type
            ThemeType = typeof(UIButton);

            // create popup window
            _popup = new UIPopup(this);
        }

        #region Properties

        UIPopup _popup;
        public UIPopup Popup => _popup;
        
        // todo: when changed -. we must update layout?
        uint? _maxPopupHeight;
        public uint MaxPopupHeight
        {
            get => _maxPopupHeight?? GetTheme().PopupButton.MaxPopupHeight;
            set => _maxPopupHeight = value;
        }

        #endregion

        #region Events

        public override bool OnPointerUpDown(Vector2 p, PointerButton button, bool down)
        {
            if (!Disabled && down)
            {
                if (button == PointerButton.Left)
                {
                    if (!Focused)
                        RequestFocus();

                    Pushed = !Pushed;
                }

                return true;
            }
            return false;
        }

        public override bool OnFocusChanged(bool focused)
        {
            if (!focused)
            {
                Pushed = focused;

                // this is needed for example when window switches to collapsed mode
                Popup.OnFocusChanged(focused);
            }

            return base.OnFocusChanged(focused);
        }
        
        #endregion

        #region Drawing

        public override void Draw(NvgContext ctx)
        {
            if (Disabled && Pushed)
                Pushed = false;

            // show / hide popup
            Popup.Show(Pushed);

            // Draw button & possibly popup
            base.Draw(ctx);
        }

        #endregion

        #region Dispose

        // we want to override default, since we want also dispose Popup, which is in
        // different parent (Screen)
        public override void Dispose()
        {
            base.Dispose();

            _popup?.Dispose();
        }

        #endregion
    }
}