using NanoUI.Common;
using NanoUI.Components.Colors;
using NanoUI.Layouts;
using NanoUI.Nvg;
using System;
using System.Numerics;

namespace NanoUI.Components.Dialogs
{
    // todo: show current color (alpha maybe < 255)
    // todo : Named colors dropdown
    // todo : display theme based common colors

    /// <summary>
    /// UIColorDialog.
    /// Note: it is not recommended to create this in your code.
    /// Instead use screen's GetDialog<ColorDialog>().
    /// If you still want to create this manually, you are responsible to handle disposing new instance.
    /// </summary>
    public class UIColorDialog : UIDialog
    {
        Color _currentColor;

        // note: this is singleton in Screen - so user must set callback (Action) when starting using this
        Action<UIWidget, Color>? _colorSelected;

        UIColorWheel? _colorWheel;
        UIAlphaBar? _alphaBar;

        bool _inited;

        UINumericTextBox<byte>? _redText;
        UINumericTextBox<byte>? _greenText;
        UINumericTextBox<byte>? _blueText;
        UINumericTextBox<byte>? _alphaText;

        // flag to prevent circular event firing
        bool _textChanging;

        /// <inheritdoc />
        public UIColorDialog()
        {

        }

        /// <inheritdoc />
        public UIColorDialog(UIScreen screen)
            : base(screen)
        {
            Title = "ColorDialog";

            // main layout (colors & buttons)
            ChildrenLayout = new StackLayout(Orientation.Vertical, LayoutAlignment.Middle);
            FixedSize = new Vector2(400, 253);

            var colorPanel = new UIWidget(this);
            colorPanel.ChildrenLayout = new StackLayout(Orientation.Horizontal)
            {
                Alignment = LayoutAlignment.Fill,
                Spacing = new Vector2(15)
            };

            // Color wheel
            _colorWheel = new UIColorWheel(colorPanel);
            _colorWheel.Size = new Vector2(150);

            // Aplha bar
            _alphaBar = new UIAlphaBar(colorPanel);
            _alphaBar.Size = new Vector2(20, 150);
            // do not show pointer hover
            _alphaBar.DisablePointerFocus = true;

            // Value panel
            GridLayout layout = new GridLayout(Orientation.Horizontal, 2,
                                     LayoutAlignment.Middle)
            { Spacing = new Vector2(5) };
            layout.SetSpacing(0, 10);

            var valuePanel = new UIWidget(colorPanel);
            valuePanel.ChildrenLayout = layout;
            valuePanel.Size = new Vector2(240, 48);

            int valueWidth = 100;

            new UILabel(valuePanel, "Red: ");
            _redText = new UINumericTextBox<byte>(valuePanel, 0);
            _redText.FixedSize = new Vector2(valueWidth, 0);
            _redText.ValueChanged += (val) =>
            {
                NumericChange(_redText);
            };

            new UILabel(valuePanel, "Green: ");
            _greenText = new UINumericTextBox<byte>(valuePanel, 0);
            _greenText.FixedSize = new Vector2(valueWidth, 0);
            _greenText.ValueChanged += (val) =>
            {
                NumericChange(_greenText);
            };

            new UILabel(valuePanel, "Blue: ");
            _blueText = new UINumericTextBox<byte>(valuePanel, 0);
            _blueText.FixedSize = new Vector2(valueWidth, 0);
            _blueText.ValueChanged += (val) =>
            {
                NumericChange(_blueText);
            };

            new UILabel(valuePanel, "Alpha: ");
            _alphaText = new UINumericTextBox<byte>(valuePanel, 0);
            _alphaText.FixedSize = new Vector2(valueWidth, 0);
            _alphaText.ValueChanged += (val) =>
            {
                NumericChange(_alphaText);
            };

            // Buttons
            UIWidget buttonPanel = new UIWidget(this);
            buttonPanel.ChildrenLayout = new StackLayout(Orientation.Horizontal, LayoutAlignment.Middle) { Spacing = new Vector2(10) };
            buttonPanel.Margin = ButtonPanelMargin;

            _pickButton = new UIButton(buttonPanel, "Pick");
            _pickButton.FixedSize = new Vector2(90, 0);
            _pickButton.Clicked += () =>
            {
                if (_caller != null)
                {
                    _colorSelected?.Invoke(_caller, _currentColor);
                }
                    
                Close();
            };

            _cancelButton = new UIButton(buttonPanel, "Cancel");
            _cancelButton.FixedSize = new Vector2(90, 0);
            _cancelButton.Clicked += () =>
            {
                Close();
            };

            // wrap color whreel & alpha bar
            _colorWheel.ColorChanged += (color) =>
            {
                SetCurrentColor(color, _alphaBar.AlphaValue);
            };

            _alphaBar.AlphaChanged += (val) =>
            {
                SetCurrentColor(_currentColor, val);
            };
        }

        #region Properties

        UIButton? _pickButton;
        public UIButton? PickButton => _pickButton;
        
        UIButton? _cancelButton;
        public UIButton? CancelButton => _cancelButton;

        #endregion

        #region Methods

        /// <summary>
        /// Since this is mostly used as a singleton (called UIScreen.GetDialog<ColorDialog>()),
        /// you must set correct callback when using this (use caller as an owner).
        /// </summary>
        public void SetCallback(UIWidget caller, Action<UIWidget, Color> colorSelected)
        {
            _caller = caller;
            _colorSelected = colorSelected;
            Visible = true;

            _inited = false;
        }

        public void SetColor(Color color)
        {
            // must set alpha first
            if(_alphaBar != null)
            {
                _alphaBar.AlphaValue = color.A;
            }
            
            _colorWheel?.SetColor(color);
        }

        #endregion

        #region Drawing

        /// <inheritdoc />
        public override void Draw(NvgContext ctx)
        {
            if (!_inited)
            {
                // delayed set
                _inited = true;

                ReInit(ctx);
            }

            base.Draw(ctx);
        }

        #endregion

        #region Private

        void NumericChange(UINumericTextBox<byte> caller)
        {
            // we must prevent circular event firing
            // (only 1 numeric text box change registered)
            if (!_inited || _textChanging || !caller.Focused)
                return;

            _textChanging = true;

            if (_redText != null && _greenText != null &&
                _blueText != null && _alphaText != null)
            {
                SetColor(new Color(
                    _redText.CurrentValue,
                    _greenText.CurrentValue,
                    _blueText.CurrentValue,
                    _alphaText.CurrentValue));
            }

            _textChanging = false;
        }

        void SetCurrentColor(Color rgb, byte alpha)
        {
            var col = new Color(rgb.R, rgb.G, rgb.B, alpha);

            _currentColor = col;

            if(_redText != null)
            {
                _redText.Text = col.R.ToString();
            }
            if (_greenText != null)
            {
                _greenText.Text = col.G.ToString();
            }
            if (_blueText != null)
            {
                _blueText.Text = col.B.ToString();
            }
            if (_alphaText != null)
            {
                _alphaText.Text = col.A.ToString();
            }
        }

        #endregion
    }
}
