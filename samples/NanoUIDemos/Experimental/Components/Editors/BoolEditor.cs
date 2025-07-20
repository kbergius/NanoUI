using NanoUI.Components;
using NanoUI.Nvg;
using System.Reflection;

namespace NanoUIDemos.Experimental.Components.Editors
{
    // todo: if we want to have BoolEditor configurable, we should not inherit from widget &
    // have CheckBox as child
    public class BoolEditor : UICheckBox, IPropertyEditor
    {
        Func<PropertyInfo, object?> _getValue;
        Action<PropertyInfo, object?> _setValue;
        PropertyInfo _propertyInfo;

        bool _currentValue;

        public BoolEditor(UIWidget parent)
            : base(parent, string.Empty)
        {
            // todo : in CheckBox
            IconExtraScale = 0.8f;
        }

        public void InitEditor(
            PropertyInfo propertyInfo,
            Func<PropertyInfo, object?> getValue,
            Action<PropertyInfo, object?> setValue)
        {
            _getValue = getValue;
            _setValue = setValue;
            _propertyInfo = propertyInfo;

            CheckedChanged += (val) =>
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

            // parse
            if (bool.TryParse(value.ToString(), out bool val))
            {
                if (_currentValue == val)
                    return;

                _currentValue = val;

                Checked = val;
            }
        }

        // we use prorotype editor when creating new editor
        public IPropertyEditor Clone(UIWidget parent)
        {
            return new BoolEditor(parent);
        }
    }
}
