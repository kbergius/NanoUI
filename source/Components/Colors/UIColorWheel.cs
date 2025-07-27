using NanoUI.Common;
using NanoUI.Nvg;
using System;
using System.Numerics;

namespace NanoUI.Components.Colors
{
    // handles selection of color RGB values; A value should be handled separately
    // (this returns "full" alpha 255)
    public class UIColorWheel : UIWidget
    {
        // marker (circle rect & triangle circle) color
        static Color MARKER_COLOR = Color.White;

        // used to calculate triangle corners
        const float TRANGLE_VALUE = 120.0f / 180.0f * MathF.PI;

        enum Region
        {
            None = 0,
            InnerTriangle = 1,
            OuterCircle = 2,
            Both = 3
        }

        // The current region the pointer is interacting with.
        Region _dragRegion = Region.None;

        // The current Hue (H) in the HSV color model.
        float _hue;

        // The 'S' (saturation) of the HSV color model. Valid values are in the range [0, 1].
        float _black;

        // The 'V' (value) component of the HSV color model. Valid values are in the range [0, 1].
        float _white;

        public Action<Color>? ColorChanged;

        // this is ctor for theme/layout generation (if you use this otherwise, set parent before using widget)
        public UIColorWheel()
        {

        }

        public UIColorWheel(UIWidget parent)
            : this(parent, Color.Red)
        {
            // set defaults to theme impl - prevents circular reference
            // note: no color wheel extra properties - so nothing here
        }

        public UIColorWheel(UIWidget parent, Color color)
            : base(parent)
        {
            SetColor(color);
            // todo: there is no border drawing by now
            ThemeType = typeof(UIColorWheel);
        }

        #region Methods

        // The current Color this ColorWheel has selected (RGB; A is "full" - 255)
        public Color GetColor()
        {
            Color rgb = Hue2Rgb(_hue);

            Color black = Color.Black;
            Color white = Color.White;

            return rgb * (1 - _white - _black) + black * _black + white * _white;
        }

        // Sets the current Color (RGB - doesn't use A)
        public void SetColor(in Color rgb)
        {
            float r = rgb.R;
            float g = rgb.G;
            float b = rgb.B;

            float M = MathF.Max(r, MathF.Max(g, b));
            float m = MathF.Min(r, MathF.Min(g, b));

            if (M == m)
            {
                float l = 0.5f * (M + m);
                _hue = 0.0f;
                _black = 1.0f - l;
                _white = l;
            }
            else
            {
                float d = M - m, h;

                if (M == r)
                    h = (g - b) / d + (g < b ? 6 : 0);
                else if (M == g)
                    h = (b - r) / d + 2;
                else
                    h = (r - g) / d + 4;
                h /= 6;

                Color ch = Hue2Rgb(_hue);

                float M2 = MathF.Max(ch.R, MathF.Max(ch.G, ch.B));
                float m2 = MathF.Min(ch.R, MathF.Min(ch.G, ch.B));

                _white = (M * m2 - m * M2) / (m2 - M2);
                _black = (M + m2 + m * M2 - m - M * m2 - M2) / (m2 - M2);
                _hue = h;
            }

            ColorChanged?.Invoke(rgb);
        }

        #endregion

        #region Events

        public override bool OnPointerUpDown(Vector2 p, PointerButton button, bool down)
        {
            base.OnPointerUpDown(p, button, down);

            if (Disabled || button != PointerButton.Left)
                return false;

            if (down)
            {
                _dragRegion = AdjustPosition(p);

                // set drag widget
                Screen?.SetDragWidget(this);

                return _dragRegion != Region.None;
            }
            else
            {
                _dragRegion = Region.None;
                return true;
            }
        }

        // Handles pointer drag events
        public override bool OnPointerDrag(Vector2 p, Vector2 rel)
        {
            return AdjustPosition(p, _dragRegion) != Region.None;
        }

        #endregion

        #region Drawing

        public override void Draw(NvgContext ctx)
        {
            // todo : not needed?
            if (!Visible)
                return;

            // background
            DrawBackgroundBrush(ctx);

            // todo: there are no childs
            base.Draw(ctx);

            // circle & triangle variables
            float ax, ay, bx, by, r;
            
            // todo: not needed?
            ctx.SaveState(); // save 1

            // start position
            Vector2 pos = Position + (Size * 0.5f);

            float r1 = (Size.X < Size.Y ? Size.X : Size.Y) * 0.5f - 5.0f;
            float r0 = r1 * .75f;

            float aeps = 0.5f / r1;   // half a pixel arc length in radians (2pi cancels out).

            // outer circle
            for (int i = 0; i < 6; i++)
            {
                float a0 = (float)(i / 6.0f * MathF.PI * 2.0f - aeps);
                float a1 = (float)((i + 1.0f) / 6.0f * MathF.PI * 2.0f + aeps);

                ctx.BeginPath();
                ctx.Arc(pos.X, pos.Y, r0, a0, a1, Winding.Clockwise);
                ctx.Arc(pos.X, pos.Y, r1, a1, a0, Winding.CounterClockwise);
                ctx.ClosePath();

                ax = pos.X + MathF.Cos(a0) * (r0 + r1) * 0.5f;
                ay = pos.Y + MathF.Sin(a0) * (r0 + r1) * 0.5f;
                bx = pos.X + MathF.Cos(a1) * (r0 + r1) * 0.5f;
                by = pos.Y + MathF.Sin(a1) * (r0 + r1) * 0.5f;

                Paint circlePaint = Paint.LinearGradient(ax, ay, bx, by,
                                          Color.HSLA(a0 / (MathF.PI * 2), 1.0f, 0.55f, 255),
                                          Color.HSLA(a1 / (MathF.PI * 2), 1.0f, 0.55f, 255));
                ctx.FillPaint(circlePaint);
                ctx.Fill();
            }

            // outer circle borders
            ctx.BeginPath();
            ctx.Circle(pos.X, pos.Y, r0 - 0.5f);
            ctx.Circle(pos.X, pos.Y, r1 + 0.5f);
            ctx.StrokeColor(new Color(0, 0, 0, 64));
            ctx.StrokeWidth(1.0f);
            ctx.Stroke();

            // outer circle rect marker
            ctx.SaveState(); //save 2
            ctx.Translate(pos.X, pos.Y);
            ctx.Rotate(_hue * MathF.PI * 2);

            // todo : dynamic calculation of markers size (both outer circle & inner triangle)
            float u = Math.Max(r1 / 50, Size.X > 100? 2.0f: 1.5f);
            u = Math.Min(u, 4.0f);

            ctx.StrokeWidth(u);
            ctx.BeginPath();
            ctx.Rect(r0 - 1, -2 * u, r1 - r0 + 2, 4 * u);
            ctx.StrokeColor(MARKER_COLOR);
            ctx.Stroke();

            // Center triangle
            r = r0 - 6;

            // precalculate triangle corners
            ax = MathF.Cos(TRANGLE_VALUE) * r;
            ay = MathF.Sin(TRANGLE_VALUE) * r;
            bx = MathF.Cos(-TRANGLE_VALUE) * r;
            by = MathF.Sin(-TRANGLE_VALUE) * r;

            ctx.BeginPath();
            ctx.MoveTo(r, 0);
            ctx.LineTo(ax, ay);
            ctx.LineTo(bx, by);
            ctx.ClosePath();

            Paint paint = Paint.LinearGradient(r, 0, ax, ay, Color.HSLA(_hue, 1.0f, 0.5f, 255),
                                      new Color(255, 255, 255, 255));
            ctx.FillPaint(paint);
            ctx.Fill();

            // triangle overlay (darkens colors)
            paint = Paint.LinearGradient((r + ax) * 0.5f, (0 + ay) * 0.5f, bx, by,
                                              new Color(0, 0, 0, 0), new Color(0, 0, 0, 255));
            ctx.FillPaint(paint);
            ctx.Fill();
            ctx.StrokeColor(new Color(0, 0, 0, 64));
            ctx.Stroke();

            // inner triangle circle marker
            float sx = r * (1 - _white - _black) + ax * _white + bx * _black;
            float sy = ay * _white + by * _black;

            ctx.BeginPath();
            ctx.Circle(sx, sy, 2 * u);
            ctx.StrokeWidth(u);
            ctx.StrokeColor(MARKER_COLOR);
            ctx.Stroke();

            ctx.RestoreState(); // restore save 2

            ctx.RestoreState(); // restore save 1
        }

        #endregion

        #region Private

        // Converts a specified hue (with saturation = value = 1) to RGB space.
        Color Hue2Rgb(float h)
        {
            float s = 1.0f;
            float v = 1.0f;

            if (h < 0)
                h += 1;

            int i = (int)(h * 6);
            float f = h * 6 - i;
            float p = v * (1 - s);
            float q = v * (1 - f * s);
            float t = v * (1 - (1 - f) * s);

            float r = 0, g = 0, b = 0;

            switch (i % 6)
            {
                case 0:
                    r = v; g = t; b = p;
                    break;
                case 1:
                    r = q; g = v; b = p;
                    break;
                case 2:
                    r = p; g = v; b = t;
                    break;
                case 3:
                    r = p; g = q; b = v;
                    break;
                case 4:
                    r = t; g = p; b = v;
                    break;
                case 5:
                    r = v; g = p; b = q;
                    break;
            }

            return new Color(r, g, b, 1.0f);
        }

        // Manipulates the positioning of the different regions
        // (outer circle / inner triangle / none)
        Region AdjustPosition(Vector2 p, Region consideredRegions = Region.Both)
        {
            // calculate position in this widget
            Vector2 pos = (p - Position) - (Size * 0.5f);       

            float r1 = (Size.X < Size.Y ? Size.X : Size.Y) * 0.5f - 5.0f;
            float r0 = r1 * .75f;

            float mr = MathF.Sqrt(pos.X * pos.X + pos.Y * pos.Y);

            if (mr >= r0 && mr <= r1 || consideredRegions == Region.OuterCircle)
            {
                // outer circle
                if ((consideredRegions & Region.OuterCircle) != Region.OuterCircle)
                    return Region.None;

                _hue = MathF.Atan(pos.Y / pos.X);

                if (pos.X < 0)
                    _hue += MathF.PI;

                _hue /= 2 * MathF.PI;

                ColorChanged?.Invoke(GetColor());

                return Region.OuterCircle;
            }

            float a = -_hue * 2 * MathF.PI;
            float sinA = MathF.Sin(a);
            float cosA = MathF.Cos(a);

            Vector2 xy = new Vector2(
                cosA * pos.X - sinA * pos.Y,
                sinA * pos.X + cosA * pos.Y);

            float r = r0 - 6;
            float l0 = (r - xy.X + MathF.Sqrt(3) * xy.Y) / (3 * r);
            float l1 = (r - xy.X - MathF.Sqrt(3) * xy.Y) / (3 * r);

            float l2 = 1 - l0 - l1;
            bool triangleTest = l0 >= 0 && l0 <= 1.0f && l1 >= 0.0f && l1 <= 1.0f &&
                                 l2 >= 0.0f && l2 <= 1.0f;

            if (triangleTest || consideredRegions == Region.InnerTriangle)
            {
                // inner triangle
                if ((consideredRegions & Region.InnerTriangle) != Region.InnerTriangle)
                    return Region.None;

                l0 = MathF.Min(MathF.Max(0.0f, l0), 1.0f);
                l1 = MathF.Min(MathF.Max(0.0f, l1), 1.0f);
                l2 = MathF.Min(MathF.Max(0.0f, l2), 1.0f);
                float sum = l0 + l1 + l2;
                l0 /= sum;
                l1 /= sum;
                _white = l0;
                _black = l1;

                ColorChanged?.Invoke(GetColor());

                return Region.InnerTriangle;
            }

            // no outer circle or inner triangle
            return Region.None;
        }

        #endregion
    }
}
