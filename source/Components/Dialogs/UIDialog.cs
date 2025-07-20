using NanoUI.Common;
using NanoUI.Nvg;
using System.ComponentModel;

namespace NanoUI.Components.Dialogs
{
    // this is base class that is registered & stored in screen, so user can call Screen.GetDialog<T>
    public class UIDialog : UIWindow
    {
        // caller is used to set focus back to caller when dialog closes
        protected UIWidget _caller;

        // this is ctor for theme/layout generation (if you use this otherwise, set parent before using widget)
        public UIDialog()
        {
            // set defaults to theme impl - prevents circular reference
            _buttonPanelMargin = new();
        }

        // init in screen creation, create titlebar with empty string
        protected UIDialog(UIScreen screen)
            : base(screen, string.Empty, ScrollbarType.NONE)
        {
            Visible = false;
            Modal = true;
            //TitleAlign = TextHorizontalAlign.Center;

            ThemeType = typeof(UIDialog);
        }

        #region Properties

        Thickness? _buttonPanelMargin;
        [Category(Globals.CATEGORY_LAYOUT)]
        public Thickness ButtonPanelMargin
        {
            get => _buttonPanelMargin?? GetTheme().Dialog.ButtonPanelMargin;
            set => _buttonPanelMargin = value;
        }

        #endregion

        #region Methods

        protected void ReInit(NvgContext ctx)
        {
            // center
            Center();

            PerformLayout(ctx);
            RequestFocus();
        }

        // note: we use normally same dialogs that are stored in Screen.
        // when you get dialog from screen, screen calls Reset, so you can have clean state
        public virtual void Reset()
        {
        
        }

        // note: we override base since it disposes widget
        public override void Close()
        {
            if (_caller != null)
            {
                _caller.RequestFocus();
            }
            else
            {
                // todo: get window!
                Screen.RequestFocus();
            }

            Visible = false;
        }

        #endregion
    }
}