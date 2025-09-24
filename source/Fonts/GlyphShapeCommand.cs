﻿using NanoUI.Common;
using System.Numerics;

namespace NanoUI.Fonts
{
    /// <summary>
    /// GlyphShapeCommandType is converted to NvgPathCommandType
    /// </summary>
    public enum GlyphShapeCommandType
    {
        MoveTo,
        LineTo,
        BezierTo,
        QuadTo,
        Close,
        Winding,
    }
    
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

        public GlyphShapeCommandType CommandType;
        public Vector2 P0;
        public Vector2 P1;
        public Vector2 P2;
        public Winding Winding;
    }
}
