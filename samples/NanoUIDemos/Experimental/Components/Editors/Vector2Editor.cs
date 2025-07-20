using NanoUI.Common;
using NanoUI.Components;
using NanoUI.Layouts;
using NanoUI.Nvg;
using System.Numerics;
using System.Reflection;

namespace NanoUIDemos.Experimental.Components.Editors
{
    public class Vector2Editor : UIWidget, IPropertyEditor
    {
        const float LABEL_WIDTH = 5f;

        //Label _labelX;
        //Label _labelY;
        NumericEditor<float> _floatX;
        NumericEditor<float> _floatY;

        Func<PropertyInfo, object?> _getValue;
        Action<PropertyInfo, object?> _setValue;
        PropertyInfo _propertyInfo;

        Vector2 _currentValue = Vector2.Zero;

        // this is ctor for theme/layout generation (if you use this otherwise, set parent before using widget)
        /*public Vector2Editor()
        {
            // set defaults to theme impl - prevents circular reference
        }*/

        // todo : formats of the X & Y
        public Vector2Editor(UIWidget parent)
            : base(parent)
        {
            var layout = new GridLayout(Orientation.Vertical, 2, LayoutAlignment.Middle);
            layout.SetColumnAlignments(new[] { LayoutAlignment.Minimum, LayoutAlignment.Fill });
            ChildrenLayout = layout;// new BoxLayout(Orientation.Vertical);

            // create widgets

            _floatX = new NumericEditor<float>(this);
            
            //_floatX.Nested = true;


            /*_labelX = new Label(_floatX, "X:");
            _labelX.FixedWidth1 = 5;// 25;
            _labelX.BackgroundColor = Color.Red;
            _labelX.BackgroundMode = BackgroundMode.Solid;
            //_labelX.Horizontal = TextHorizontalAlign.Center;
            //_labelX.Vertical = TextVerticalAlign.Middle;
            //_labelX.TextAlignment = TextAlignment.Center | TextAlignment.Middle;
            _labelX.TextHorizontalAlignment = TextHorizontalAlign.Center;
            _labelX.TextVerticalAlignment = TextVerticalAlign.Middle;
            _labelX.SendToBack();
            */
            _floatY = new NumericEditor<float>(this);
            
            //_floatY.Nested = true;

            /*_labelY = new Label(_floatY, "Y:");
            _labelY.FixedWidth1 = 5;// 25;
            _labelY.BackgroundColor = Color.Green;
            _labelY.BackgroundMode = BackgroundMode.Solid;
            //_labelY.Horizontal = TextHorizontalAlign.Center;
            //_labelY.Vertical = TextVerticalAlign.Middle;
            _labelY.TextHorizontalAlignment = TextHorizontalAlign.Center;
            _labelY.TextVerticalAlignment = TextVerticalAlign.Middle;
            _labelY.SendToBack();*/
        }

        // TODO
        public override void PerformLayout(NvgContext ctx)
        {
            // we set custom width because of the labels ("X, "Y")
            //_floatX.SetTextBoxWidth(Width1 - 2 * _floatX.TextBox.Height1);// - 25);// 25);
            //_floatY.SetTextBoxWidth(Width1 - 2 * _floatY.TextBox.Height1); // - 25);// 25);
            _floatX.SetTextBoxWidth((int)(Size.X - 2 * (int)GetTheme().UpDownButton.Dimension));// - 25);// 25);
            _floatY.SetTextBoxWidth((int)(Size.X - 2 * (int)GetTheme().UpDownButton.Dimension));
            base.PerformLayout(ctx);

            // TODO
            /*_labelX.FixedHeight1 = _floatX.Height1;
            _labelY.FixedHeight1 = _floatY.Height1;
            */
            //_floatX.PositionX1 -= 10;// _labelX.PositionX1 + _labelX.Width1 + 10;

            //_floatY.PositionX1 -= 10;// _labelY.PositionX1 + _labelY.Width1 + 10;
        }

        public void InitEditor(PropertyInfo propertyInfo,
            Func<PropertyInfo, object?> getValue,
            Action<PropertyInfo, object?> setValue)
        {
            _getValue = getValue;
            _setValue = setValue;
            _propertyInfo = propertyInfo;

            _floatX.ValueChanged += (val) =>
            {
                _currentValue.X = val;
                setValue?.Invoke(propertyInfo, _currentValue);
            };

            _floatY.ValueChanged += (val) =>
            {
                _currentValue.Y = val;
                setValue?.Invoke(propertyInfo, _currentValue);
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

            // todo : write "labels" "X:" & "Y:"?
            ctx.BeginPath();
            ctx.Rect(Position, new Vector2(LABEL_WIDTH, _floatX.Size.Y));
            ctx.FillColor(Color.Red);
            ctx.Fill();

            ctx.BeginPath();
            ctx.Rect(Position + new Vector2(0, _floatY.Size.Y), new Vector2(LABEL_WIDTH, _floatY.Size.Y));
            ctx.FillColor(Color.Green);
            ctx.Fill();
        }

        // call from Draw
        void SetValue(object? value)
        {
            if (value == null)
                return;

            Vector2 val = (Vector2)value;

            if (_currentValue == val)
                return;

            _currentValue = val;

            //_floatX.Value = val.X;
            //_floatX.TextBox.Text = val.X.ToString();
            _floatX.SetValue(val.X);
            //_floatY.Value = val.Y;
            //_floatY.TextBox.Text = val.Y.ToString();
            _floatY.SetValue(val.Y);
        }

        // we use prorotype editor when creating new editor
        public IPropertyEditor Clone(UIWidget parent)
        {
            return new Vector2Editor(parent);
        }
    }
}