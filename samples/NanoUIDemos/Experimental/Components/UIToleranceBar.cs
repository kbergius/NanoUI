using NanoUI.Common;
using NanoUI.Components;
using NanoUI.Nvg;
using System.Numerics;

namespace NanoUIDemos.Experimental.Components
{
    // todo : make vertical, up/down
    // todo : in draw path creation is about the same
    // todo: write percentage in the middle? (numeric text box / label)
    // todo: nvgcontext build triangle path
    // todo: more 2.5D style
    public class UIToleranceBar : UIWidget
    {
        public Action<float> ValueChanged;

        // this is ctor for theme/layout generation (if you use this otherwise, set parent before using widget)
        public UIToleranceBar()
        {
            // set defaults to theme impl - prevents circular reference
            ValueColor = default;
        }

        public UIToleranceBar(UIWidget parent)
            : base(parent)
        {
            ThemeType = typeof(UIToleranceBar);
        }

        #region Properties

        float _value;
        public float Value
        {
            get => _value;
            set
            { 
                _value = Math.Clamp(value, Range.Min, Range.Max);
            }
        }

        public MinMax Range { get; set; } = new MinMax(0, 100);

        // for pointer scrolling
        public float Step { get; set; } = 1;

        Color? _valueColor;
        public Color ValueColor
        {
            get => _valueColor ?? ((ThemeEXT)GetTheme()).ToleranceBar.ValueColor;
            set => _valueColor = value;
        }

        #endregion

        #region Events

        public override bool OnPointerUpDown(Vector2 p, PointerButton button, bool down)
        {
            if(down && button == PointerButton.Left)
            {
                UpdateValue(ValueForPosition(p));

                // set drag widget
                Screen?.SetDragWidget(this);
            }

            return base.OnPointerUpDown(p, button, down);
        }

        public override bool OnPointerDrag(Vector2 p, Vector2 rel)
        {
            UpdateValue(ValueForPosition(p));

            return base.OnPointerDrag(p, rel);
        }

        public override bool OnPointerScroll(Vector2 p, Vector2 scroll)
        {
            float step = scroll.Y < 0 ? -Step : Step;
            UpdateValue(Value + step);

            //return true;
            return base.OnPointerScroll(p, scroll);
        }

        #endregion

        #region Drawing

        public override void Draw(NvgContext ctx)
        {
            // background
            DrawBackground(ctx, Position, Size);
            
            // value
            DrawContent(ctx, Position, Size);

            // border
            // note : we must draw border last, since content partially overlaps "border area"?
            DrawBorder(ctx, Position, Size);

            base.Draw(ctx);
        }

        #endregion

        #region Private

        void DrawBackground(NvgContext ctx, Vector2 position, Vector2 size)
        {
            ctx.BeginPath();
            ctx.MoveTo(position.X, position.Y + size.Y);
            ctx.LineTo(position.X + size.X, position.Y);
            ctx.LineTo(position.X + size.X, position.Y + size.Y);
            ctx.ClosePath();

            // TODO theme.Widgets => BrushBase? BackgroundSunken
            var PassiveBackgroundColor = new Color(0, 0, 0, 224);
            var PassiveBackgroundColor2 = new Color(0, 0, 0, 128);

            ctx.FillColor(PassiveBackgroundColor);
            ctx.Fill();
        }

        void DrawContent(NvgContext ctx, Vector2 position, Vector2 size)
        {
            // Prevent divide with 0
            var val = Math.Max(Value, 1);

            ctx.BeginPath();
            ctx.MoveTo(position.X, position.Y + size.Y);
            ctx.LineTo(position.X + size.X / Range.Max * val, position.Y + size.Y - size.Y / Range.Max * val);
            ctx.LineTo(position.X + size.X / Range.Max * val, position.Y + size.Y);
            ctx.ClosePath();

            ctx.FillColor(ValueColor);
            ctx.Fill();
        }

        void DrawBorder(NvgContext ctx, Vector2 position, Vector2 size)
        {
            if (!Border || Disabled)
                return;

            if (PointerFocus)
            {
                ctx.BeginPath();
                ctx.MoveTo(position.X, position.Y + size.Y);
                ctx.LineTo(position.X + size.X, position.Y);
                ctx.LineTo(position.X + size.X, position.Y + size.Y);
                ctx.ClosePath();

                ctx.StrokeColor(GetTheme().Borders.PointerFocus);
                ctx.StrokeWidth(1);
                ctx.Stroke();
            }
            else
            {
                // todo : check
                ctx.BeginPath();
                ctx.MoveTo(position.X + 0.5f, position.Y + size.Y + 0.5f);
                ctx.LineTo(position.X + 0.5f + size.X, position.Y + 0.5f);
                ctx.LineTo(position.X + 0.5f + size.X, position.Y + size.Y + 0.5f);
                ctx.ClosePath();

                ctx.StrokeColor(GetTheme().Borders.Light);
                ctx.StrokeWidth(1);
                ctx.Stroke();

                ctx.BeginPath();
                ctx.MoveTo(position.X, position.Y + size.Y);
                ctx.LineTo(position.X + size.X, position.Y);
                ctx.LineTo(position.X + size.X, position.Y + size.Y);
                ctx.ClosePath();

                ctx.StrokeColor(GetTheme().Borders.Dark);
                ctx.StrokeWidth(1);
                ctx.Stroke();
            }
        }

        void UpdateValue(float value)
        {
            Value = Math.Clamp(value, Range.Min, Range.Max);

            ValueChanged?.Invoke(Value);
        }

        float ValueForPosition(Vector2 pos)
        {
            float value = ((pos - Position).X * Range.Max / Size.X);

            return Math.Clamp(value, Range.Min, Range.Max);
        }

        #endregion
    }
}