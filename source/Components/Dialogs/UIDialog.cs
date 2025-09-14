using NanoUI.Common;
using NanoUI.Nvg;
using System.ComponentModel;

namespace NanoUI.Components.Dialogs
{
    /// <summary>
    /// UIDialog is a base class, that is registered & stored in UIScreen.
    /// Use Screen.GetDialog<T>.
    /// </summary>
    public class UIDialog : UIWindow
    {
        /// <summary>
        /// Caller is used to set focus back to caller widget when dialog closes.
        /// </summary>
        protected UIWidget? _caller;

        /// <inheritdoc />
        public UIDialog()
        {
            // set defaults to theme impl - prevents circular reference
            _buttonPanelMargin = new();
        }

        /// <inheritdoc />
        protected UIDialog(UIScreen screen)
            : base(screen, string.Empty, ScrollbarType.NONE)
        {
            // init in screen creation, create titlebar with empty string

            Visible = false;
            Modal = true;
            //TitleAlign = TextHorizontalAlign.Center;

            ThemeType = typeof(UIDialog);
        }

        #region Properties

        Thickness? _buttonPanelMargin;

        /// <summary>
        /// ButtonPanelMargin.
        /// </summary>
        [Category(Globals.CATEGORY_LAYOUT)]
        public Thickness ButtonPanelMargin
        {
            get => _buttonPanelMargin?? GetTheme().Dialog.ButtonPanelMargin;
            set => _buttonPanelMargin = value;
        }

        #endregion

        #region Methods

        /// <summary>
        /// ReInit.
        /// </summary>
        protected void ReInit(NvgContext ctx)
        {
            // center
            Center();

            PerformLayout(ctx);
            RequestFocus();
        }

        /// <summary>
        /// Reset.
        /// Note: we use normally same dialog instances, that are stored in UIScreen.
        /// When you get dialog from screen, screen calls Reset(), so you can have clean state.
        /// </summary>
        public virtual void Reset()
        {
        
        }

        /// <inheritdoc />
        public override void Close()
        {
            // note: we override base since it disposes widget

            if (_caller != null)
            {
                _caller.RequestFocus();
            }
            else
            {
                // todo: get window!
                Screen?.RequestFocus();
            }

            Visible = false;
        }

        #endregion
    }
}
