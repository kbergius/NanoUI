namespace NanoUI.Svg.Data
{
    public enum SvgXmlPathCommandType
    {
        MoveTo,                 // M = moveto (move from one point to another point)
        LineTo,                 // L = lineto (create a line)
        HorizontalLineTo,       // H = horizontal lineto (create a horizontal line)
        VerticalLineTo,         // V = vertical lineto (create a vertical line)
        CurveTo,                // C = curveto (create a curve)
        SmoothCurveTo,          // S = smooth curveto (create a smooth curve)
        QuadraticBezier,        // Q = quadratic Bézier curve (create a quadratic Bézier curve)
        SmoothQuadraticBezier,  // T = smooth quadratic Bézier curveto (create a smooth quadratic Bézier curve)
        EllipticalArc,          // A = elliptical Arc (create a elliptical arc)
        ClosePath,               // Z = closepath (close the path)
        NONE
    }
}