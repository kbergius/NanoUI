namespace NanoUI.Svg.Data
{
    /// <summary>
    /// SvgXmlPathCommandType
    /// </summary>
    public enum SvgXmlPathCommandType
    {
        /// <summary>
        ///  M = moveto (move from one point to another point).
        /// </summary>
        MoveTo,

        /// <summary>
        /// L = lineto (create a line).
        /// </summary>
        LineTo,

        /// <summary>
        /// H = horizontal lineto (create a horizontal line).
        /// </summary>
        HorizontalLineTo,

        /// <summary>
        /// V = vertical lineto (create a vertical line).
        /// </summary>
        VerticalLineTo,

        /// <summary>
        /// C = curveto (create a curve).
        /// </summary>
        CurveTo,

        /// <summary>
        /// S = smooth curveto (create a smooth curve).
        /// </summary>
        SmoothCurveTo,

        /// <summary>
        ///  Q = quadratic Bézier curve (create a quadratic Bézier curve).
        /// </summary>
        QuadraticBezier,

        /// <summary>
        /// T = smooth quadratic Bézier curveto (create a smooth quadratic Bézier curve).
        /// </summary>
        SmoothQuadraticBezier,

        /// <summary>
        /// A = elliptical Arc (create a elliptical arc).
        /// </summary>
        EllipticalArc,

        /// <summary>
        /// Z = closepath (close the path).
        /// </summary>
        ClosePath,

        /// <summary>
        /// None.
        /// </summary>
        NONE
    }
}
