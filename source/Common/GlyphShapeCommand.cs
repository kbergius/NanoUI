using System.Numerics;

namespace NanoUI.Common
{
    /// <summary>
    /// GlyphShapeCommand is used when glyphs are rendered with shapes.
    /// </summary>
    public struct GlyphShapeCommand
    {
        // note: in drawText converts this to NvgPathCommand
        // note2:
        // - MoveTo uses P0
        // - LineTo uses P0
        // - BezierTo uses P0 & P1 & P2 (P0 & P1 are control points, P2 is end point)
        // - QuadTo uses P0 as control point, P1 as endpoint
        // note3: this is nearly identical to NvgPathCommand, but we don't use it since it is internal

        /// <summary>
        /// GlyphShapeCommandType
        /// </summary>
        public GlyphShapeCommandType CommandType;

        /// <summary>
        /// Position0
        /// </summary>
        public Vector2 P0;

        /// <summary>
        /// Position1
        /// </summary>
        public Vector2 P1;

        /// <summary>
        /// Position2
        /// </summary>
        public Vector2 P2;

        /// <summary>
        /// Winding
        /// </summary>
        public Winding Winding;
    }
}
