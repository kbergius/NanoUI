﻿using NanoUI.Common;
using NanoUI.Layouts;
using NanoUI.Nvg;
using System;
using System.Numerics;

namespace NanoUI.Components.Buttons
{
    // todo: we should dynamically calculate UpDownButton height based on NumericTextBox height
    // possible we perform layout & set flag need recalculate & set in draw real positions & sizes
    public class UINumericUpDown<T> : UIWidget where T : INumber<T>
    {
        const int MINIMUM_TEXT_BOX_WIDTH = 50;

        UINumericTextBox<T> _numericTextBox;
        
        UpDownButton<T> _upDownButton;

        public Action<T>? ValueChanged;
        public Action? InvalidFormat;

        public UINumericUpDown(UIWidget parent, T value, NumericFormat numericFormat = NumericFormat.NONE)
            :base(parent)
        {
            ChildrenLayout = new StackLayout(Orientation.Horizontal, LayoutAlignment.Fill);

            // note we get all values from numeeric text box
            _numericTextBox = new UINumericTextBox<T>(this, value, numericFormat);
            _numericTextBox.ValueChanged += (val) =>
            {
                ValueChanged?.Invoke(val);
            };

            _numericTextBox.InvalidFormat += () => InvalidFormat?.Invoke();

            _upDownButton = new UpDownButton<T>(this);

            // wrap updown change
            // we get +Step Or -Step
            _upDownButton.ButtonPushed += (val) =>
            {
                // fires TextBox.TextChanged ==> NumeericTextBox.ValueChanged
                _numericTextBox.SetChange(val);
            };
        }

        #region Properties

        internal UINumericTextBox<T> TextBox => _numericTextBox;

        // minimum & maximum allowed values
        // note: we set defaults (0 - 1) if not specified
        public T Min
        {
            get => _numericTextBox.Min?? T.Zero;
            set => _numericTextBox.Min = value;
        }
        public T Max
        {
            get => _numericTextBox.Max ?? T.One;
            set => _numericTextBox.Max = value;
        }

        public string Units
        {
            get => _numericTextBox.Units;
            set => _numericTextBox.Units = value;
        }

        // updown step
        public T Step
        {
            get => _upDownButton.Step;
            set => _upDownButton.Step = value;
        }

        #endregion

        #region Methods

        public string GetFormatted()
        {
            return _numericTextBox.GetFormatted();
        }

        // sets value as text to underlyieng textbox that triggers value changed
        public void SetValue(T value)
        {
            _numericTextBox.Text = $"{value}";
        }

        // This is here to support nested & custom settings
        public virtual void SetTextBoxWidth(int width)
        {
            _numericTextBox.FixedSize = new Vector2(width, 0);
        }

        #endregion

        #region Layout

        public override void PerformLayout(NvgContext ctx)
        {
            // todo: _upDownButton height (dimension) is not same as numeric text box height
            // (draws also border outside?)

            var minimumWidth = MINIMUM_TEXT_BOX_WIDTH + (int)GetTheme().UpDownButton.Dimension * 2;

            if (Size.X < minimumWidth)
            {
                Size = new Vector2(minimumWidth, Size.Y);
            }

            _numericTextBox.FixedSize = new Vector2(Size.X - (GetTheme().UpDownButton.Dimension * 2), 0);

            base.PerformLayout(ctx);
        }

        #endregion
    }
}
