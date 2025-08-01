﻿using NanoUI.Common;
using NanoUI.Utils;
using System;
using System.Numerics;

namespace NanoUI.Components
{
    public class UINumericTextBox<T> : UITextField where T : INumber<T>
    {
        // needed when we parse failed --> reset
        T _currentValue = T.Zero;

        // numeric format (decimals)
        NumericFormat _numericFormat = NumericFormat.NONE;

        public Action<T>? ValueChanged;
        public Action? InvalidFormat;

        public UINumericTextBox(UIWidget parent)
            : this(parent, T.Zero)
        {

        }

        public UINumericTextBox(UIWidget parent, T value, NumericFormat numericFormat = NumericFormat.NONE)
            : base(parent)
        {
            Editable = true;
            TextHorizontalAlignment = TextHorizontalAlign.Right;

            _currentValue = value;
            _numericFormat = numericFormat;

            // set text with right format
            Text = DoGetFormatted(_currentValue);

            // wrap base value changed
            TextChanged += CheckValue;
        }

        #region Properties

        public T CurrentValue => _currentValue;

        // minimum & maximum allowed values
        public T? Min { get; set; }
        public T? Max { get; set; }

        #endregion

        #region Methods

        public string GetFormatted()
        {
            return DoGetFormatted(_currentValue);
        }

        // outside call like updown button
        public void SetChange(T valueChange)
        {
            if (T.TryParse((_currentValue + valueChange).ToString(), null, out var val))
            {
                if (Min != null && Max != null && Min < Max && (val < Min || val > Max))
                {
                    // do nothing
                    return;
                }

                // prevent possibly circular - needed?
                if(_currentValue == val)
                    return;

                _currentValue = val;

                // must reset ==> fire text change event
                ResetTextValue();
            }
        }

        #endregion

        #region Private

        void CheckValue(string value)
        {
            if (T.TryParse(value, null, out var val))
            {
                // check min & max range
                if(Min != null && Max != null && Min < Max && (val < Min || val > Max))
                {
                    // must reset
                }
                else
                {
                    // parse & range ok

                    // check valid format (if format spesified)
                    IsValidFormat = DoGetFormatted(val).Equals(value);

                    if (IsValidFormat)
                    {
                        // we got valid format, change current value & fire event
                        _currentValue = val;
                        ValueChanged?.Invoke(_currentValue);
                    }
                    else
                    {
                        InvalidFormat?.Invoke();
                    }

                    // parse & range ok - so we don't reset text value
                    return;
                }
            }

            // parse failed (invalid characters) --> must reset text box value
            ResetTextValue();
        }

        // resets text to current value
        void ResetTextValue()
        {
            ResetText(DoGetFormatted(_currentValue));
        }

        string DoGetFormatted(T val)
        {
            return FormatUtils.GetNumberFormat(val, _numericFormat);
        }

        #endregion
    }
}
