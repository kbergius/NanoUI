using NanoUI.Common;

namespace NanoUI.Fonts.Data
{
    internal struct Font
    {
        // needed to get glyphs & pass to font manager
        public int Id;
        // for search purposes
        public string Name;
        public float Ascender;
        public float Descender;
        public float LineHeight;
        public GlyphBaking FontBaking;
    }
}