using NanoUI.Common;
using NanoUI.Components;
using NanoUI.Components.Buttons;
using NanoUI.Nvg;
using System.Numerics;
using System.Reflection;

namespace NanoUIDemos.Experimental.Components.Editors
{
    public interface INumericEditor : IPropertyEditor
    {
        IPropertyEditor Clone(UIWidget parent, NumericFormat numericFormat);
    }

    public class NumericEditor<T> : UINumericUpDown<T>, INumericEditor where T : INumber<T>
    {
        Func<PropertyInfo, object?> _getValue;
        Action<PropertyInfo, object?> _setValue;
        PropertyInfo _propertyInfo;

        T _currentValue;

        public NumericEditor(UIWidget parent, NumericFormat numericFormat = NumericFormat.NONE)
            : base(parent, default, numericFormat)
        {
           
        }

        // this is init like function we set value & wrap action
        public void InitEditor(
            PropertyInfo propertyInfo,
            Func<PropertyInfo, object?> getValue,
            Action<PropertyInfo, object?> setValue)
        {
            _getValue = getValue;
            _setValue = setValue;
            _propertyInfo = propertyInfo;

            // wrap action - thisaction comes from numeric updown
            ValueChanged += SetValue;
        }

        public override void Draw(NvgContext ctx)
        {
            // if used as combined field, func & property info are null
            if (_getValue != null && _propertyInfo != null)
            {
                SetValue(_getValue.Invoke(_propertyInfo));
            }

            base.Draw(ctx);
        }

        // call from Draw
        void SetValue(object? value)
        {
            if (value == null)
                return;

            // parse
            if (T.TryParse(value.ToString(), null, out T val))
            {
                SetValue(val);
            }
        }

        // call from Draw OR ValueChanged event
        new void SetValue(T value)
        {
            if (value == _currentValue)
                return;

            _currentValue = value;

            // set value to underlying up down (1. text, 2. value)
            base.SetValue(value);

            _setValue?.Invoke(_propertyInfo, value);
        }

        // we use prorotype editor when creating new editor
        public IPropertyEditor Clone(UIWidget parent)
        {
            return Clone(parent, NumericFormat.NONE);
        }

        // numeric editor specific cloning / create
        public IPropertyEditor Clone(UIWidget parent, NumericFormat numericFormat)
        {
            return new NumericEditor<T>(parent, numericFormat);
        }
    }
}
