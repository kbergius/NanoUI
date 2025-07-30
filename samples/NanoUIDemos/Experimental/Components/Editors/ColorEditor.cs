using NanoUI.Common;
using NanoUI.Components;
using NanoUI.Components.Dialogs;
using NanoUI.Layouts;
using NanoUI.Nvg;
using System.Numerics;
using System.Reflection;

namespace NanoUIDemos.Experimental.Components.Editors
{
    // TODO: this is not really working (no color indicator rect) after changeing to brushes
    // use not label, just draw Rect
    public class ColorEditor : UIWidget, IPropertyEditor
    {
        UILabel _label;
        Color _currentColor;
        UIButton _button;

        Func<PropertyInfo, object?>? _getValue;
        Action<PropertyInfo, object?>? _setValue;
        PropertyInfo? _propertyInfo;

        public ColorEditor(UIWidget parent)
            : base(parent)
        {
            var layout = new StackLayout(Orientation.Horizontal);

            ChildrenLayout = layout;

            // create color indicator
            _label = new UILabel(this);
            //_label.BackgroundMode = BackgroundMode.Solid;
            _label.CornerRadius = new CornerRadius(0);
            _label.Border = true;
            _label.DisablePointerFocus = false;

            // color dialog opener
            _button = new UIButton(this, "...");
            _button.Clicked += OpenColorDialog;
        }

        public void InitEditor(
            PropertyInfo propertyInfo,
            Func<PropertyInfo, object?> getValue,
            Action<PropertyInfo, object?> setValue)
        {
            _getValue = getValue;
            _setValue = setValue;
            _propertyInfo = propertyInfo;
        }

        #region Events

        public override bool OnPointerUpDown(Vector2 p, PointerButton button, bool down)
        {
            // check if label clicked
            if(down && button == PointerButton.Left && _label.Contains(p - Position))
            {
                OpenColorDialog();
                return true;
            }

            return base.OnPointerUpDown(p, button, down);
        }

        #endregion

        // TODO
        public override void PerformLayout(NvgContext ctx)
        {
            base.PerformLayout(ctx);

            // TODO
            _button.Size = new Vector2(_button.Size.X, Size.Y);
            _label.FixedSize = new Vector2(35, _button.Size.Y);
            Position = new Vector2(Size.X - _button.Size.X, Position.Y);
        }

        public override void Draw(NvgContext ctx)
        {
            // if used as combined field, func & property info are null
            if (_getValue != null && _propertyInfo != null)
            {
                SetValue(_getValue.Invoke(_propertyInfo));
            }

            base.Draw(ctx);

            ctx.BeginPath();
            ctx.Rect(Position, new Vector2(Size.Y));
            ctx.FillColor(_currentColor);
            ctx.Fill();
        }

        // call from Draw
        void SetValue(object? value)
        {
            if (value == null)
                return;

            Color color = (Color)value;

            _currentColor = color;

            /*if (_label.BackgroundColor == color)
                return;

            _label.BackgroundColor = color;*/
        }

        void OpenColorDialog()
        {
            if(Screen == null)
                return;

            //var dlg = new ColorDialog(Screen);
            UIColorDialog? dlg = Screen.GetDialog<UIColorDialog>();

            if (dlg == null)
                return;

            // todo: get these strings sowewhere
            if (dlg.PickButton != null)
            {
                dlg.PickButton.Caption = "Pick";
            }
            if (dlg.CancelButton != null)
            {
                dlg.CancelButton.Caption = "Cancel";
            }

            // set initial color
            Color color = _currentColor != default ?
                _currentColor : GetTheme().Common.AccentColor;

            dlg.SetColor(color);

            /*dlg.ColorSelected += (value) =>
            {
                _label.BackgroundColor = value;
                _setValue?.Invoke(_propertyInfo, value);
            };*/
            dlg.SetCallback(this, ColorChanged);

            void ColorChanged(UIWidget widget, Color color)
            {
                _currentColor = color;
                if (_propertyInfo != null)
                {
                    _setValue?.Invoke(_propertyInfo, color);
                }
            }
        }

        // we use prorotype editor when creating new editor
        public IPropertyEditor Clone(UIWidget parent)
        {
            return new ColorEditor(parent);
        }
    }
}
