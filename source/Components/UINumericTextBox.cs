using NanoUI.Common;
using NanoUI.Utils;
using System;
using System.Numerics;

namespace NanoUI.Components
{
    /// <summary>
    /// UINumericTextBox<T>.
    /// </summary>
    public class UINumericTextBox<T> : UITextField where T : INumber<T>
    {
        // needed when we parse failed --> reset
        T _currentValue = T.Zero;

        // numeric format (decimals)
        NumericFormat _numericFormat = NumericFormat.NONE;

        /// <summary>
        /// Value changed action
        /// </summary>
        public Action<T>? ValueChanged;

        /// <summary>
        /// Invalid format action
        /// </summary>
        public Action? InvalidFormat;

        /// <inheritdoc />
        public UINumericTextBox(UIWidget parent)
            : this(parent, T.Zero)
        {

        }

        /// <inheritdoc />
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

        /// <summary>
        /// Current value
        /// </summary>
        public T CurrentValue => _currentValue;

        /// <summary>
        /// Minimum allowed value.
        /// </summary>
        public T? Min { get; set; }

        /// <summary>
        /// Maximum allowed value.
        /// </summary>
        public T? Max { get; set; }

        #endregion

        #region Methods

        /// <summary>
        /// Get formatted
        /// </summary>
        /// <returns>string</returns>
        public string GetFormatted()
        {
            return DoGetFormatted(_currentValue);
        }
                
        /// <summary>
        /// Set change
        /// </summary>
        /// <param name="valueChange">Value change</param>
        public void SetChange(T valueChange)
        {
            // outside call like updown button

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
            return StringUtils.GetNumberFormat(val, _numericFormat);
        }

        #endregion
    }
}
