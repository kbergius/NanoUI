using NanoUI.Common;
using NanoUI.Nvg;
using System;
using System.Numerics;
using NanoUI.Layouts;

namespace NanoUI.Components.Colors
{
    // note: this returns RGB values, A is always "full" (255)
    public class UIColorPicker : UIPopupButton
    {
        Color _currentColor;

        // this is not used anywhere here (its just if user want to watch colorwheel changes)
        public Action<Color>? Changed;
        public Action<Color>? FinalColor;

        StackLayout _popupLayout;

        public UIColorPicker(UIWidget parent)
            : this(parent, Color.Orange) { }

        public UIColorPicker(UIWidget parent, Color color)
            : base(parent, string.Empty)
        {
            DisablePointerFocus = true;

            IconAlign = IconAlign.Right;
            Icon = GetTheme().Fonts.IconCaretDown;

            // padding for popup
            _popupLayout = new StackLayout(Orientation.Vertical, LayoutAlignment.Middle);
            Popup.ChildrenLayout = _popupLayout;
            Popup.RelativePosition = PopupPosition.Bottom;
            Popup.AnchorSize = 0;
            Popup.Margin = new Thickness(10);

            // set the color wheel to the specified color
            _colorWheel = new UIColorWheel(Popup, color);

            // set start color
            Color = color;

            // set the pick button to the specified color
            _pickButton = new UIButton(Popup, "Pick");
            // we don't want to receive pointerfocus since it modifies color
            _pickButton.DisablePointerFocus = true;

            // set the reset button to the specified color
            _cancelButton = new UIButton(Popup, "Cancel");
            // we don't want to receive pointerfocus since it modifies color
            _cancelButton.DisablePointerFocus = true;

            _colorWheel.ColorChanged += (value) =>
            {
                Changed?.Invoke(value);
            };

            _pickButton.Clicked += () =>
            {
                if (Pushed)
                {
                    _currentColor = _colorWheel.GetColor();

                    // close popup
                    Pushed = false;

                    Color = _currentColor;
                    FinalColor?.Invoke(_currentColor);
                }
            };

            _cancelButton.Clicked += () =>
            {
                // close popup
                Pushed = false;

                Color = _currentColor;

                FinalColor?.Invoke(_currentColor);
            };
        }

        #region Properties

        // Get the current color
        public virtual Color Color
        {
            get => _currentColor;
            set
            {
                // Ignore other calls when the user is currently editing
                if (!Pushed)
                {
                    _currentColor = value;
                    _colorWheel.SetColor(_currentColor);

                    BackgroundFocused = BackgroundUnfocused = new SolidBrush(_currentColor);
                }
            }
        }

        UIColorWheel _colorWheel;
        public UIColorWheel ColorWheel => _colorWheel;

        UIButton _pickButton;
        public UIButton PickButton => _pickButton;
        
        UIButton _cancelButton;
        public UIButton CancelButton => _cancelButton;

        #endregion

        #region Layout

        // todo : should we limit colorWheel size & popup height, if width exceeds some value
        /// <inheritdoc />
        public override void PerformLayout(NvgContext ctx)
        {
            var buttonHeight = _pickButton.PreferredSize(ctx).Y;

            Popup.FixedSize = new Vector2(Size.X, Size.X + 2 * buttonHeight);
            Popup.Size = Popup.FixedSize;

            Popup.PerformLayout(ctx);

            var contentWidth = Size.X - (Padding.Horizontal * 2);

            _colorWheel.Size = new Vector2(contentWidth);

            _pickButton.FixedSize = new Vector2(contentWidth, buttonHeight);
            _cancelButton.FixedSize = new Vector2(contentWidth, buttonHeight);
        }

        #endregion
    }
}
