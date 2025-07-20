namespace NanoUI.Fonts.Data
{
    // this is "area" that glyph occupies in atlas texture
    // - x0, y0, s0, t0 = MIN pos & uv
    // - x1, y1, s1, t1 = MAX pos & uv
    internal struct GlyphQuad
    {
        public float x0, y0, s0, t0;
        public float x1, y1, s1, t1;
    }
}