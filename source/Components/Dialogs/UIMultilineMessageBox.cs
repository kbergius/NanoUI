using NanoUI.Common;
using NanoUI.Layouts;
using NanoUI.Nvg;
using System;
using System.Numerics;

namespace NanoUI.Components.Dialogs
{
    /// <summary>
    /// UIMultilineMessageBox.
    /// It is not recommended to create this in your code.
    /// Instead use screen's GetDialog<UIMultilineMessageBox>().
    /// If you still want to create this manually, you are responsible to handle disposing new instance.
    /// </summary>
    public class UIMultilineMessageBox : UIDialog
    {
        bool _inited;
        UIScrollableLabel? _multilineText;

        Action<UIWidget, int>? _buttonClicked;

        /// <inheritdoc />
        public UIMultilineMessageBox()
        {

        }

        /// <inheritdoc />
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

        /// <summary>
        /// Text
        /// </summary>
        public string Text
        {
            set => _multilineText?.SetText(value);
        }

        UIButton? _okButton;

        /// <summary>
        /// OK button.
        /// </summary>
        public UIButton? OKButton => _okButton;
        
        UIButton? _altButton;

        /// <summary>
        /// Alt button.
        /// </summary>
        public UIButton? AltButton => _altButton;

        #endregion

        #region Methods

        /// <summary>
        /// Sets callback. Use caller as an owner.
        /// </summary>
        /// <param name="caller">Caller</param>
        /// <param name="action">Action</param>
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
