using NanoUI.Common;
using NanoUI.Components;
using NanoUI.Nvg;
using NanoUI.Utils;
using System.Numerics;

namespace NanoUIDemos.Experimental.Components
{
    public class UIDial : UIWidget
    {
        float _knobRadius;
        // todo: make configurable
        float _kshadow = 2;

        public Action<float> ValueChanged;

        public Action<float> FinalValue;

        // this is ctor for theme/layout generation (if you use this otherwise, set parent before using widget)
        public UIDial()
        {
            // set defaults to theme impl - prevents circular reference
            HighlightColor = default;
            FaceColor = default;
            FaceColor2 = default;
            MarkerColor = default;
            // no normal border drawing
            Border = false;
        }

        public UIDial(UIWidget parent)
            : base(parent)
        {
            // todo!!!
            Size = new Vector2(40);
            // no normal border drawing
            Border = false;

            ThemeType = typeof(UIDial);
        }

        #region Properties

        MinMax _range = new MinMax(0.0f, 1.0f);
        
        public MinMax Range
        {
            get => _range;
            set => _range = value;
        }

        public MinMax HighlightRange { get; set; }
        
        Color? _highlightColor;
        public Color HighlightColor
        {
            get => _highlightColor ?? ((ThemeEXT)GetTheme()).Dial.HighlightColor;
            set => _highlightColor = value;
        }

        Color? _faceColor;
        public Color FaceColor
        {
            get => _faceColor ?? ((ThemeEXT)GetTheme()).Dial.FaceColor;
            set => _faceColor = value;
        }

        Color? _faceColor2;
        public Color FaceColor2
        {
            get => _faceColor2 ?? ((ThemeEXT)GetTheme()).Dial.FaceColor2;
            set => _faceColor2 = value;
        }

        Color? _markerColor;
        public Color MarkerColor
        {
            get => _markerColor ?? ((ThemeEXT)GetTheme()).Dial.MarkerColor;
            set => _markerColor = value;
        }

        float _value;
        public float Value
        {
            get => _value;
            set => _value = value;
        }

        #endregion

        #region Events

        public override bool OnPointerDrag(Vector2 p, Vector2 rel)
        {
            if (Disabled)
                return false;

            // todo: magical numbers
            Vector2 pos = p - Position - Size / 2;
            float value = 0.5f + 0.5f * MathF.Atan2(pos.X, -pos.Y) / MathF.PI;
            value = -0.1f + 1.2f * value;

            value = value * (Range.Max - Range.Min) + Range.Min;
            Value = Math.Min(Math.Max(value, Range.Min), Range.Max);

            ValueChanged?.Invoke(Value);

            return true;
        }

        public override bool OnPointerUpDown(Vector2 p, PointerButton button, bool down)
        {
            if (Disabled)
                return false;

            if (down)
            {
                // todo: magical numbers
                Vector2 pos = p - Position - Size / 2;
                float kr = 0.5f * (Size.Y * 0.4f);

                if (pos.SquaredNorm() >= kr * kr)
                {
                    float value = 0.5f + 0.5f * MathF.Atan2(pos.X, -pos.Y) / MathF.PI;
                    value = -0.1f + 1.2f * value;

                    value = value * (Range.Max - Range.Min) + Range.Min;
                    Value = Math.Min(Math.Max(value, Range.Min), Range.Max);
                }

                ValueChanged?.Invoke(Value);

                // set drag widget
                Screen?.SetDragWidget(this);
            }
            else
            {
                FinalValue?.Invoke(Value);
            }

            return base.OnPointerUpDown(p, button, down);
        }

        #endregion

        #region Layout

        public override void PerformLayout(NvgContext ctx)
        {
            base.PerformLayout(ctx);

            // todo: magical numbers
            // calculate knob radius by height
            _knobRadius = Size.Y * 0.4f;
        }

        #endregion

        #region Drawing

        public override void Draw(NvgContext ctx)
        {
            // todo: magical numbers
            Vector2 center = Position + Size * 0.5f;           

            Vector2 dialPos = new Vector2(center.X, center.Y + 0.5f);

            Paint dial = Paint.LinearGradient(
                Position.X, center.Y - _knobRadius, Position.X, center.Y + _knobRadius,
                GetTheme().Borders.Light,
                GetTheme().Borders.Dark);

            Paint dialReverse = Paint.LinearGradient(
                Position.X, center.Y - _knobRadius, Position.X, center.Y + _knobRadius,
                GetTheme().Borders.Dark,
                GetTheme().Borders.Light);

            Paint dialFace = Paint.RadialGradient(
                dialPos.X, dialPos.Y, _knobRadius - _kshadow,
                _knobRadius + _kshadow,
                FaceColor,
                FaceColor2);

            // highlight
            if (HighlightRange.Max > HighlightRange.Min)
            {
                float a0 = 0.5f * MathF.PI + 2.0f * MathF.PI * (0.1f + 0.8f * HighlightRange.Min);
                float a1 = 0.5f * MathF.PI + 2.0f * MathF.PI * (0.1f + 0.8f * HighlightRange.Max);

                ctx.BeginPath();
                ctx.Arc(dialPos, _knobRadius, a0, a1, Winding.Clockwise);
                ctx.Arc(dialPos, _knobRadius + 2 * _kshadow, a1, a0, Winding.CounterClockwise);
                ctx.FillColor(HighlightColor);
                ctx.Fill();
            }

            // notch
            ctx.BeginPath();
            ctx.Circle(dialPos, _knobRadius);
            ctx.StrokeColor(GetTheme().Borders.Dark);
            ctx.FillPaint(dial);
            ctx.Stroke();
            ctx.Fill();

            ctx.BeginPath();
            ctx.Circle(dialPos, _knobRadius - _kshadow);
            ctx.FillPaint(dialFace);
            ctx.StrokePaint(dialReverse);
            ctx.Stroke();
            ctx.Fill();

            Vector2 notchPos = new Vector2(0.0f, 0.8f * (_knobRadius - 1.5f * _kshadow));
            float value = (Value - Range.Min) / (Range.Max - Range.Min);
            float theta = 2.0f * MathF.PI * (0.1f + 0.8f * value);

            notchPos = notchPos.RotateAroundPoint(theta, Vector2.Zero);
            notchPos += dialPos + Position;

            ctx.BeginPath();
            ctx.Circle(notchPos, 0.15f * _knobRadius);
            ctx.FillColor(!Disabled? MarkerColor : TextDisabledColor);
            ctx.StrokePaint(dial);
            ctx.Stroke();
            ctx.Fill();
        }

        #endregion
    }
}