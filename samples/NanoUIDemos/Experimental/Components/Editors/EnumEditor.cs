using NanoUI.Components;
using NanoUI.Nvg;
using System.Reflection;

namespace NanoUIDemos.Experimental.Components.Editors
{
    public class EnumEditor<TEnum> : UIEnumDropDown<TEnum>, IPropertyEditor
        where TEnum :struct, Enum
    {
        Func<PropertyInfo, object?> _getValue;
        Action<PropertyInfo, object?> _setValue;
        PropertyInfo _propertyInfo;

        TEnum _currentValue = default;

        public EnumEditor(UIWidget parent)
            : base(parent)
        {

        }

        public void InitEditor(
            PropertyInfo propertyInfo,
            Func<PropertyInfo, object?> getValue,
            Action<PropertyInfo, object?> setValue)
        {
            _getValue = getValue;
            _setValue = setValue;
            _propertyInfo = propertyInfo;

            SelectedChanged += (val) =>
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

            TEnum val = (TEnum)value;

            if (_currentValue.Equals(val))
                return;

            _currentValue = val;

            SetSelected(val);
        }

        // we use prorotype editor when creating new editor
        public IPropertyEditor Clone(UIWidget parent)
        {
            return new EnumEditor<TEnum>(parent);
        }
    }
}