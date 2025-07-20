using NanoUI.Common;
using NanoUI.Fonts;
using NanoUI.Nvg;
using System;

namespace NanoUI
{
    public interface IFontManager
    {
        // fontmanager implementation may not support all FontGlyphBaking methods
        // you can add here some fallback logic if your implementation doesn't support requested method
        // note: the default implementation supports all methods
        GlyphBaking GetGlyphBaking(GlyphBaking requestedBaking);

        // note: fontindex is TTC font collection index, in TTF files it is 0
        bool Load(int fontId, ReadOnlySpan<byte> data, int fontCollectionIndex);
        void GetFontVMetrics(int fontId, out int ascent, out int descent, out int lineGap);

        // this is a scale to convert font file's internal units to pixels
        float GetPixelScale(int fontId, float size);
        // returns 0, if not found (could be -1)
        int GetFontGlyphIndex(int fontId, uint codepoint);
        int GetGlyphKernAdvance(int fontId, int glyph1, int glyph2);
        bool BuildGlyphBitmap(int fontId, int glyph, float size, float scale, out int advance, out int lsb, out int x0, out int y0, out int x1, out int y1);
        void RasterizeGlyphBitmap(int fontId, Span<byte> texData, int startIndex, int outWidth, int outHeight, int outStride, float scaleX, float scaleY, int glyph, GlyphBaking fontBaking);
        
        // when trying to add glyp(s) & no room left in atlas. so user must expand atlas
        void OnAtlasFull(NvgContext ctx, int atlasWidth, int atlasHeight);
        void Dispose();

        // NEW Shapes
        // note: this is only called if we use for glyphs shapes (not bitmap & atlas)
        bool GetGlyphShape(int fontId, int fontGlyphIndex, out ReadOnlySpan<GlyphShapeCommand> shape);
    }
}