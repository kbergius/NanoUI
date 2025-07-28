using NanoUI.Common;
using NanoUI.Components;
using NanoUI.Nvg;
using System.Numerics;

namespace NanoUIDemos.Experimental.Components
{
    // todo : make knob inner radius configurable?
    // todo: use Size values instead of magic numbers
    // todo: "knob" colots like in dial
    public class UISwitchBox : UIWidget
    {
        // calculated in PerformLayout
        float _knobRadius;

        Vector2 _start;
        Vector2 _size;
        Vector2 _center;

        public Action<bool>? CheckedChanged;

        // this is ctor for theme/layout generation (if you use this otherwise, set parent before using widget)
        public UISwitchBox()
        {
            // set defaults to theme impl - prevents circular reference
            CheckedColor = default;
            CheckedColor2 = default;
            KnobColor = default;
            KnobColor2 = default;
        }

        public UISwitchBox(UIWidget parent)
            : base(parent)
        {
           ThemeType = typeof(UISwitchBox);
        }

        #region Properties

        public bool Checked { get; set; }

        Color? _checkedColor;
        public Color CheckedColor
        {
            get => _checkedColor ?? ((ThemeEXT)GetTheme()).SwitchBox.CheckedColor;
            set => _checkedColor = value;
        }

        Color? _checkedColor2;
        public Color CheckedColor2
        {
            get => _checkedColor2 ?? ((ThemeEXT)GetTheme()).SwitchBox.CheckedColor2;
            set => _checkedColor2 = value;
        }

        Color? _knobColor;
        public Color KnobColor
        {
            get => _knobColor ?? ((ThemeEXT)GetTheme()).SwitchBox.KnobColor;
            set => _knobColor = value;
        }

        Color? _knobColor2;
        public Color KnobColor2
        {
            get => _knobColor2 ?? ((ThemeEXT)GetTheme()).SwitchBox.KnobColor2;
            set => _knobColor2 = value;
        }

        public Orientation Orientation { get; set; } = Orientation.Horizontal;

        #endregion

        #region Events

        public override bool OnPointerUpDown(Vector2 p, PointerButton button, bool down)
        {
            base.OnPointerUpDown(p, button, down);

            if (Disabled)
                return false;

            if (down && button == PointerButton.Left)
            {
                Checked = !Checked;
                CheckedChanged?.Invoke(Checked);

                return true;
            }

            return false;
        }

        public override bool OnKeyUpDown(Key key, bool down, KeyModifiers modifiers)
        {
            if (!Focused)
                return false;

            if (down)
            {
                if (key == Key.Space || key == Key.Enter)
                {
                    Checked = !Checked;
                    CheckedChanged?.Invoke(Checked);

                    return true;
                }
            }

            return base.OnKeyUpDown(key, down, modifiers);
        }

        #endregion

        #region Layout

        public override void PerformLayout(NvgContext ctx)
        {
            base.PerformLayout(ctx);

            // calculate basic params
            if (Orientation == Orientation.Horizontal)
            {
                _knobRadius = (int)(Size.Y * 0.4f);

                _size = new Vector2
                {
                    X = Size.Y * 1.5f,
                    Y = Size.Y * 0.8f
                };

                _start = new Vector2
                {
                    X = Position.X + Size.Y * 0.1f,
                    Y = Position.Y + (Size.Y - _size.Y) / 2 + 1
                };
            }
            else
            {
                _knobRadius = (int)(Size.Y * 0.2f);

                _size = new Vector2
                {
                    X = Size.Y * 0.4f,
                    Y = Size.Y * 0.8f
                };

                _start = new Vector2
                { 
                    X = Position.X + Size.Y * 0.05f + 1,
                    Y = Position.Y + (Size.Y - _size.Y) / 2
                };
            }

            _center = Position + Size * 0.5f;

            // needed to draw border
            CornerRadius = new CornerRadius(_knobRadius);
        }

        #endregion

        #region Drawing

        public override void Draw(NvgContext ctx)
        {
            // calculate positions
            Vector2 knobPos;
            
            float knobMultiplier = Checked ? 1 : 0;

            if (Orientation == Orientation.Horizontal)
            {
                knobPos = new Vector2(_start.X + _knobRadius + knobMultiplier * (_size.X - 2 * _knobRadius), _center.Y + 0.5f);
            }
            else
            {
                knobPos = new Vector2(_start.X + _knobRadius, _start.Y + knobMultiplier * (_size.Y - 2 * _knobRadius) + _knobRadius);
            }

            // switch box background
            Color bgTopColor;
            Color bgBtColor;

            if (Disabled)
            {
                bgTopColor = GetTheme().Common.FrontendDisabledColor;
                bgBtColor = GetTheme().Common.FrontendDisabledColor;
            }
            else if (Checked)
            {
                bgTopColor = CheckedColor;
                bgBtColor = CheckedColor2;
            }
            else
            {
                // TODO theme.Widgets => BrushBase? BackgroundSunken
                var PassiveBackgroundColor = new Color(0, 0, 0, 224);
                var PassiveBackgroundColor2 = new Color(0, 0, 0, 128);

                bgTopColor = PassiveBackgroundColor;
                bgBtColor = PassiveBackgroundColor2;
            }

            Paint bg = Paint.BoxGradient(_start, _size, 3, 3, bgTopColor, bgBtColor);

            ctx.BeginPath();
            ctx.RoundedRect(_start, _size, _knobRadius);
            ctx.FillPaint(bg);
            ctx.Fill();

            // knob
            Paint knob = Paint.LinearGradient(Position.X, _center.Y - _knobRadius, Position.X, _center.Y + _knobRadius,
              GetTheme().Borders.Light, GetTheme().Borders.Medium);

            Paint knobReverse = Paint.LinearGradient(Position.X, _center.Y - _knobRadius, Position.X, _center.Y + _knobRadius,
              GetTheme().Borders.Medium, GetTheme().Borders.Light);

            // outer
            ctx.BeginPath();
            ctx.Circle(knobPos.X, knobPos.Y, _knobRadius * 0.9f);
            ctx.StrokeColor(KnobColor2);
            ctx.FillPaint(knob);
            ctx.Stroke();
            ctx.Fill();

            // inner
            ctx.BeginPath();
            ctx.Circle(knobPos.X, knobPos.Y, _knobRadius * 0.7f);
            ctx.FillColor(!Disabled ? KnobColor : GetTheme().Common.FrontendDisabledColor);
            ctx.StrokePaint(knobReverse);
            ctx.Stroke();
            ctx.Fill();

            // border
            this.DrawBorder(ctx, _start, _size, true);
        }

        #endregion
    }
}
