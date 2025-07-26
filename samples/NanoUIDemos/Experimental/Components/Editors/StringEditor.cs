using NanoUI.Components;
using NanoUI.Nvg;
using System.Reflection;

namespace NanoUIDemos.Experimental.Components.Editors
{
    // todo: if we want to have StringEditor configurable, we should not inherit from widget &
    // have TextBox as child
    public class StringEditor : UITextField, IPropertyEditor
    {
        Func<PropertyInfo, object?> _getValue;
        Action<PropertyInfo, object?> _setValue;
        PropertyInfo _propertyInfo;

        string _currentValue = string.Empty;

        public StringEditor(UIWidget parent)
            : base(parent)
        {
            Editable = true;
        }

        public void InitEditor(
            PropertyInfo propertyInfo,
            Func<PropertyInfo, object?> getValue,
            Action<PropertyInfo, object?> setValue)
        {
            _getValue = getValue;
            _setValue = setValue;
            _propertyInfo = propertyInfo;

            // wrap change
            TextChanged += (val) =>
            {
                _currentValue = val;
                setValue?.Invoke(propertyInfo, val);
            };
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

            string? str = value.ToString();

            if (str == null || _currentValue == str)
                return;

            _currentValue = str;

            Text = str;
        }

        // we use prorotype editor when creating new editor
        public IPropertyEditor Clone(UIWidget parent)
        {
            return new StringEditor(parent);
        }
    }
}
