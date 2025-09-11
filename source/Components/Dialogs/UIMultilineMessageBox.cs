using NanoUI.Common;
using NanoUI.Layouts;
using NanoUI.Nvg;
using System;
using System.Numerics;

namespace NanoUI.Components.Dialogs
{
    /// <summary>
    /// UIMultilineMessageBox.
    /// </summary>
    public class UIMultilineMessageBox : UIDialog
    {
        bool _inited;
        UIScrollableLabel? _multilineText;

        Action<UIWidget, int>? _buttonClicked;

        // this is ctor for theme/layout generation (if you use this otherwise, set parent before using widget)
        public UIMultilineMessageBox()
        {

        }

        // note: it is not recommended to call in your code. Instead call Screen.GetDialog<MultilineMessageBox>.
        // if you still want to call this, you are responsible to handle dispose new instance manually
        public UIMultilineMessageBox(UIScreen screen)
            : base(screen)
        {
            ChildrenLayout = new StackLayout(Orientation.Vertical, LayoutAlignment.Middle) { Spacing = new Vector2(10) };
            FixedSize = new Vector2(350, 400);

            _multilineText = new UIScrollableLabel(this);
            _multilineText.FixedSize = new Vector2(300, 285);

            // Buttons
            UIWidget buttonPanel = new UIWidget(this);
            buttonPanel.ChildrenLayout = new StackLayout(Orientation.Horizontal, LayoutAlignment.Middle) { Spacing = new Vector2(10) };
            buttonPanel.Margin = ButtonPanelMargin;

            _okButton = new UIButton(buttonPanel, "OK");
            _okButton.FixedSize = new Vector2(90, 0);
            _okButton.Clicked += () =>
            {
                if(_caller != null)
                {
                    _buttonClicked?.Invoke(_caller, 0);
                }
                
                Close();
            };

            _altButton = new UIButton(buttonPanel, "Cancel");
            _altButton.FixedSize = new Vector2(90, 0);
            _altButton.Visible = false;
            _altButton.Clicked += () =>
            {
                if(_caller != null)
                {
                    _buttonClicked?.Invoke(_caller, 1);
                }
                
                Close();
            };
        }

        #region Properties

        public string Text
        {
            set => _multilineText?.SetText(value);
        }

        UIButton? _okButton;
        public UIButton? OKButton => _okButton;
        
        UIButton? _altButton;
        public UIButton? AltButton => _altButton;
        
        #endregion

        #region Methods

        // we use caller as identifier
        public void SetCallback(UIWidget caller, Action<UIWidget, int> action)
        {
            _caller = caller;
            _buttonClicked = action;
            Visible = true;

            _inited = false;
        }

        #endregion

        #region Drawing

        /// <inheritdoc />
        public override void Draw(NvgContext ctx)
        {
            if (!_inited)
            {
                _inited = true;

                // delayed
                ReInit(ctx);
            }

            base.Draw(ctx);
        }

        #endregion
    }
}
