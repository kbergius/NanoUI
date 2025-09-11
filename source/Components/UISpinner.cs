using NanoUI.Common;
using NanoUI.Nvg;
using System;
using System.Numerics;

namespace NanoUI.Components
{
    public class UISpinner : UIWidget
    {
        // todo?
        const double FACTOR = 6;
        Vector2 _center;

        // this is ctor for theme/layout generation (if you use this otherwise, set parent before using widget)
        public UISpinner()
        {
            // set defaults to theme impl - prevents circular reference
            Speed = default;
            Radius = default;
            InnerRadius = default;
            StartColor = default;
            EndColor = default;

            // defaults
            DisablePointerFocus = true;
            Border = false;
        }

        public UISpinner(UIWidget parent)
            : base(parent)
        {
            // defaults
            DisablePointerFocus = true;
            Border = false;

            // use UILabel as base theme type
            ThemeType = typeof(UILabel);
        }

        #region Properties

        // determines if we draw whole/half circle (default = true)
        public bool HalfCircle { get; set; }
        
        // note: when speed set negative rotates counter clockwise
        float? _speed;
        public float Speed
        {
            get => _speed?? GetTheme().Spinner.Speed;
            set => _speed = value;
        }

        // this is relative to spinner size
        float? _radius;
        public float Radius
        {
            get => _radius?? GetTheme().Spinner.Radius;
            set => _radius = value;
        }

        // this is relative to calculated radius (determines spinner "width")
        float? _innerRadius;
        public float InnerRadius
        {
            get => _innerRadius ?? GetTheme().Spinner.InnerRadius;
            set => _innerRadius = value;
        }

        Color? _startColor;
        public Color StartColor
        {
            get => _startColor?? GetTheme().Spinner.StartColor;
            set => _startColor = value;
        }

        Color? _endColor;
        public Color EndColor
        {
            get => _endColor ?? GetTheme().Spinner.EndColor;
            set => _endColor = value;
        }

        #endregion

        #region Layout

        /// <inheritdoc />
        public override void PerformLayout(NvgContext ctx)
        {
            base.PerformLayout(ctx);

            _center = Position + Size / 2;
        }

        #endregion

        #region Drawing

        double _elapsed;

        /// <inheritdoc />
        public override void Draw(NvgContext ctx)
        {
            // todo: better way, clear elapsed?
            if(Screen != null)
                _elapsed += Screen.DeltaSeconds;

            double t = _elapsed * Speed;

            float a0 = (float)((!HalfCircle ? 180 : 0.0) + t * FACTOR);
            float a1 = (float)(Math.PI + t * FACTOR);
            float r = MathF.Min(Size.X, Size.Y) / 2 * Radius;
            float r0 = r;
            float r1 = r * InnerRadius;

            // background
            DrawBackgroundBrush(ctx);

            // spinner
            ctx.BeginPath();
            ctx.Arc(_center.X, _center.Y, r0, a0, a1, Winding.Clockwise);
            ctx.Arc(_center.X, _center.Y, r1, a1, a0, Winding.CounterClockwise);
            ctx.ClosePath();

            float ax = _center.X + MathF.Cos(a0) * (r0 + r1) * 0.5f;
            float ay = _center.Y + MathF.Sin(a0) * (r0 + r1) * 0.5f;
            float bx = _center.X + MathF.Cos(a1) * (r0 + r1) * 0.5f;
            float by = _center.Y + MathF.Sin(a1) * (r0 + r1) * 0.5f;

            if (Speed < 0)
            {
                // counter clockwise
                ctx.FillPaint(Paint.LinearGradient(bx, by, ax, ay, StartColor, EndColor));
            }
            else
            {
                // clockwise
                ctx.FillPaint(Paint.LinearGradient(ax, ay, bx, by, StartColor, EndColor));
            }

            ctx.Fill();

            // note: no border drawing
        }

        #endregion
    }
}
