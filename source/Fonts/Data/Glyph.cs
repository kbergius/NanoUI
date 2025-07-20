namespace NanoUI.Fonts.Data
{
    internal struct Glyph
    {
        public uint Codepoint;
        // this is glyphIndex in Font
        public int FontGlyphIndex;
        // this is the font size, when glyph was originally created
        // note: if we use same glyph with all font sizes, we must calculate scale/advanceX based on this
        public float FontSize;
        public int X0, Y0, X1, Y1; // min, max
        public int OffX, OffY;
        public float AdvX;
        public int Blur;
        public int Dilate;
    }
}