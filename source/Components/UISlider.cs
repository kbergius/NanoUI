using NanoUI.Common;
using NanoUI.Nvg;
using System;
using System.ComponentModel;
using System.Numerics;

namespace NanoUI.Components
{
    // todo: vertical

    /// <summary>
    /// UISlider has not normal background/border drawing.
    /// </summary>
    public class UISlider : UIWidget
    {
        /// <summary>
        /// Value changed action
        /// </summary>
        public Action<float>? ValueChanged;

        /// <summary>
        /// Final value action
        /// </summary>
        public Action<float>? FinalValue;

        /// <inheritdoc />
        public UISlider()
        {
            // set defaults to theme impl - prevents circular reference
            ValueColor = default;
        }

        /// <inheritdoc />
        public UISlider(UIWidget parent)
            : base(parent)
        {
            ThemeType = typeof(UISlider);
        }

        #region Properties

        float _value;

        /// <summary>
        /// Current value
        /// </summary>
        [Browsable(false)]
        public float Value
        {
            get => _value;
            set
            { 
                _value = Math.Clamp(value, Range.Min, Range.Max);
            }
        }

        // note : there is no editor for this - so this property is not shown in
        // property grid

        /// <summary>
        /// Range
        /// </summary>
        public MinMax Range { get; set; } = new MinMax(0.0f, 1.0f);

        Color? _valueColor;

        /// <summary>
        /// Value color
        /// </summary>
        public Color ValueColor
        {
            get => _valueColor?? GetTheme().Slider.ValueColor;
            set => _valueColor = value;
        }

        #endregion

        #region Events

        /// <inheritdoc />
        public override bool OnPointerDrag(Vector2 p, Vector2 rel)
        {
            // todo : same calculation as in pointer down

            if (Disabled)
                return false;

            float kr = (int)(Size.Y * 0.4f);
            float kshadow = 3;
            float startX = kr + kshadow + Position.X - 1;
            float widthX = Size.X - 2 * (kr + kshadow);

            float value = (p.X - startX) / widthX;
            value = value * (Range.Max - Range.Min) + Range.Min;
            
            _value = Math.Clamp(value, Range.Min, Range.Max);

            ValueChanged?.Invoke(_value);

            return true;
        }

        /// <inheritdoc />
        public override bool OnPointerUpDown(Vector2 p, PointerButton button, bool down)
        {
            if (Disabled)
                return false;

            float kr = (int)(Size.Y * 0.4f);
            float kshadow = 3;
            float startX = kr + kshadow + Position.X - 1;
            float widthX = Size.X - 2 * (kr + kshadow);

            float value = (p.X - startX) / widthX;
            value = value * (Range.Max - Range.Min) + Range.Min;
            Value = MathF.Min(MathF.Max(value, Range.Min), Range.Max);

            ValueChanged?.Invoke(Value);

            if (down)
            {
                // set drag widget
                Screen?.SetDragWidget(this);
            }
            else
            {
                FinalValue?.Invoke(Value);
            }

            return true;
        }

        #endregion

        #region Drawing

        /// <inheritdoc />
        public override void Draw(NvgContext ctx)
        {
            // todo: magical numbers
            Vector2 center = Position + Size * 0.5f;
            float kr = (int)(Size.Y * 0.4f);
            float kshadow = 3;

            float startX = kr + kshadow + Position.X;
            float widthX = Size.X - 2 * (kr + kshadow);

            // background
            DrawBackgroundBrush(ctx, new Vector2(startX, center.Y - 3 + 1), new Vector2(widthX, 6));

            Vector2 knobPos = new Vector2(
                startX + (Value - Range.Min) / (Range.Max - Range.Min) * widthX,
                center.Y + 0.5f);

            // background value color
            if (!Disabled)
            {
                ctx.BeginPath();
                ctx.RoundedRect(startX, center.Y - kshadow + 1,
                                    knobPos.X - startX, kshadow * 2, 2);
                ctx.FillColor(ValueColor);
                ctx.Fill();
            }

            // border drawing
            this.DrawBorder(ctx, new Vector2(startX, center.Y - 3 + 1), new Vector2(widthX, 6), true);

            // knob
            Paint knob = Paint.LinearGradient(
                Position.X, center.Y - kr, Position.X, center.Y + kr,
                GetTheme().Borders.Light,
                GetTheme().Borders.Medium);
            
            Paint knobReverse = Paint.LinearGradient(
                Position.X, center.Y - kr, Position.X, center.Y + kr,
                GetTheme().Borders.Medium,
                GetTheme().Borders.Light);

            // outer
            ctx.BeginPath();
            ctx.Circle(knobPos.X, knobPos.Y, kr);
            ctx.StrokeColor(GetTheme().Borders.Dark);
            ctx.FillPaint(knob);
            ctx.Stroke();
            ctx.Fill();

            // inner
            ctx.BeginPath();
            ctx.Circle(knobPos.X, knobPos.Y, kr / 2);
            ctx.FillColor(!Disabled ? ValueColor : GetTheme().Common.FrontendDisabledColor);

            ctx.StrokePaint(knobReverse);
            ctx.Stroke();
            ctx.Fill();

            // note : should not be children - but call draw anyways
            base.Draw(ctx);
        }

        #endregion
    }
}
