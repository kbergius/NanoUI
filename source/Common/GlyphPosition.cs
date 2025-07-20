namespace NanoUI.Common
{
    public struct GlyphPosition
    {
        // index of glyph in passed text
        public int Index;

        // The X-coordinate of the logical glyph position.
        public float X;

        // The smallest X-bound of the glyph shape.
        public float MinX;

        // The largest X-bound of the glyph shape.
        public float MaxX;
    }
}