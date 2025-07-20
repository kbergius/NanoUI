using NanoUI.Common;

namespace NanoUI.Fonts.Data
{
    // note: we can't use NvgState, since there is scaling applied in GetFontScale
    internal struct FontState
    {
        // this is just needed for glyphs
        public int FontId;
        public TextAlignment TextAlign;
        public float FontSize;
        public float CharSpacing;
        // this is a scale to convert font file's internal units to pixels
        public float PixelScale;
        public float Blur;
        public float Dilate;
        // there is different logic in some FontStash functions based on baking method
        public GlyphBaking GlyphBaking;

        // glyph shapes
        public Color? TextShapeOutline;
        public float TextShapeOutlineWidth;
        public Paint? TextShapeFill;
    }
}