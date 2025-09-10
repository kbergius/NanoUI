namespace NanoUI.Common
{
    /// <summary>
    /// GlyphPosition.
    /// </summary>
    public struct GlyphPosition
    {
        /// <summary>
        /// Index of glyph in passed text
        /// </summary>
        public int Index;

        /// <summary>
        /// The X-coordinate of the logical glyph position
        /// </summary>
        public float X;

        /// <summary>
        /// The smallest X-bound of the glyph shape
        /// </summary>
        public float MinX;

        /// <summary>
        /// The largest X-bound of the glyph shape
        /// </summary>
        public float MaxX;
    }
}
