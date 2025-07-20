using NanoUI.Nvg;
using System.Numerics;

namespace NanoUI.Svg
{
    // this is helper class to create path commands
    static partial class SvgManager
    {
        static Vector2 _curPoint = Vector2.Zero;
        // we need this to smooth variants (bezier, quad)
        static Vector2 _prevPoint = Vector2.Zero;

        internal static void AddMoveTo(NvgContext ctx, float x, float y, bool absolute)
        {
            if (!absolute)
            {
                x += _curPoint[0];
                y += _curPoint[1];
            }
            
            ctx.MoveTo(x, y);

            _curPoint[0] = x;
            _curPoint[1] = y;

            _prevPoint = _curPoint;
        }

        internal static void AddLineTo(NvgContext ctx, float x, float y, bool absolute)
        {
            if (!absolute)
            {
                x += _curPoint[0];
                y += _curPoint[1];
            }

            ctx.LineTo(x, y);

            _curPoint[0] = x;
            _curPoint[1] = y;

            _prevPoint = _curPoint;
        }

        internal static void AddHorizontalLineTo(NvgContext ctx, float x, bool absolute)
        {
            if (!absolute)
            {
                x += _curPoint[0];
            }

            ctx.LineTo(x, _curPoint[1]);

            _curPoint[0] = x;

            _prevPoint = _curPoint;
        }

        internal static void AddVerticalLineTo(NvgContext ctx, float y, bool absolute)
        {
            if (!absolute)
            {
                y += _curPoint[1];
            }

            ctx.LineTo(_curPoint[0], y);

            _curPoint[1] = y;

            _prevPoint = _curPoint;
        }

        internal static void AddCurveTo(NvgContext ctx,
            float x1, float y1, float x2, float y2, float x, float y, bool absolute)
        {
            if (!absolute)
            {
                x1 += _curPoint[0];
                y1 += _curPoint[1];
                x2 += _curPoint[0];
                y2 += _curPoint[1];
                x += _curPoint[0];
                y += _curPoint[1];
            }

            ctx.BezierTo(x1, y1, x2, y2, x, y);

            _curPoint[0] = x;
            _curPoint[1] = y;

            _prevPoint = new Vector2(x2, y2);
        }

        internal static void AddSmoothCurveTo(NvgContext ctx,
            float x2, float y2, float x, float y, bool absolute)
        {
            if (!absolute)
            {
                x2 += _curPoint[0];
                y2 += _curPoint[1];
                x += _curPoint[0];
                y += _curPoint[1];
            }

            float x1 = 2 * _curPoint[0] - _prevPoint.X;
            float y1 = 2 * _curPoint[1] - _prevPoint.Y;

            ctx.BezierTo(x1, y1, x2, y2, x, y);

            _curPoint[0] = x;
            _curPoint[1] = y;

            _prevPoint = new Vector2(x2, y2);
        }

        internal static void AddQuadraticBezier(NvgContext ctx,
            float x1, float y1, float x, float y, bool absolute)
        {
            if (!absolute)
            {
                x1 += _curPoint[0];
                y1 += _curPoint[1];
                x += _curPoint[0];
                y += _curPoint[1];
            }

            ctx.QuadTo(x1, y1, x, y);

            _curPoint[0] = x;
            _curPoint[1] = y;

            _prevPoint = new Vector2(x1, y1);
        }

        internal static void AddSmoothQuadraticBezier(NvgContext ctx, float x, float y, bool absolute)
        {
            if (!absolute)
            {
                x += _curPoint[0];
                y += _curPoint[1];
            }

            float x1 = 2 * _curPoint[0] - _prevPoint.X;
            float y1 = 2 * _curPoint[1] - _prevPoint.Y;

            ctx.QuadTo(x1, y1, x, y);

            _curPoint[0] = x;
            _curPoint[1] = y;

            _prevPoint = new Vector2(x1, y1);
        }

        internal static void AddEllipticalArc(NvgContext ctx,
            float rx, float ry, float angle, float largeArcFlag, float sweepFlag, float x, float y, bool absolute)
        {
            if (!absolute)
            {
                x += _curPoint[0];
                y += _curPoint[1];
            }

            var newSegments = SvgArcUtils.ToCurve(_curPoint[0], _curPoint[1], x, y, largeArcFlag, sweepFlag, rx, ry, angle);
            
            if (newSegments.Length == 0)
            {
                ctx.LineTo(x, y);
            }
            else
            {
                foreach (var s in newSegments)
                {
                    // note: s[0], s[1] are start point
                    ctx.BezierTo(s[2], s[3], s[4], s[5], s[6], s[7]);
                }
            }

            _curPoint = new Vector2(x, y);

            _prevPoint = _curPoint;
        }
    }
}