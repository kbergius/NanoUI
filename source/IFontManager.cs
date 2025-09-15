using NanoUI.Common;
using NanoUI.Fonts;
using NanoUI.Nvg;
using System;

namespace NanoUI
{
    /// <summary>
    /// IFontManager.
    /// </summary>
    public interface IFontManager
    {
        // fontmanager implementation may not support all FontGlyphBaking methods
        // you can add here some fallback logic if your implementation doesn't support requested method
        // note: the default implementation supports all methods

        /// <summary>
        /// GetGlyphBaking.
        /// </summary>
        GlyphBaking GetGlyphBaking(GlyphBaking requestedBaking);

        // note: fontindex is TTC font collection index, in TTF files it is 0

        /// <summary>
        /// Load.
        /// </summary>
        bool Load(int fontId, ReadOnlySpan<byte> data, int fontCollectionIndex);

        /// <summary>
        /// GetFontVMetrics.
        /// </summary>
        void GetFontVMetrics(int fontId, out int ascent, out int descent, out int lineGap);

        // this is a scale to convert font file's internal units to pixels

        /// <summary>
        /// GetPixelScale.
        /// </summary>
        float GetPixelScale(int fontId, float size);

        // returns 0, if not found (could be -1)

        /// <summary>
        /// GetFontGlyphIndex.
        /// </summary>
        int GetFontGlyphIndex(int fontId, uint codepoint);

        /// <summary>
        /// GetGlyphKernAdvance.
        /// </summary>
        int GetGlyphKernAdvance(int fontId, int glyph1, int glyph2);

        /// <summary>
        /// BuildGlyphBitmap.
        /// </summary>
        bool BuildGlyphBitmap(int fontId, int glyph, float size, float scale, out int advance, out int lsb, out int x0, out int y0, out int x1, out int y1);

        /// <summary>
        /// RasterizeGlyphBitmap.
        /// </summary>
        void RasterizeGlyphBitmap(int fontId, Span<byte> texData, int startIndex, int outWidth, int outHeight, int outStride, float scaleX, float scaleY, int glyph, GlyphBaking fontBaking);

        // NEW Shapes
        // note: this is only called if we use for glyphs shapes (not bitmap & atlas)

        /// <summary>
        /// GetGlyphShape.
        /// </summary>
        bool GetGlyphShape(int fontId, int fontGlyphIndex, out ReadOnlySpan<GlyphShapeCommand> shape);

        // when trying to add glyph(s) & no room left in atlas. so user must expand atlas

        /// <summary>
        /// OnAtlasFull.
        /// </summary>
        void OnAtlasFull(NvgContext ctx, int atlasWidth, int atlasHeight);

        /// <summary>
        /// Dispose.
        /// </summary>
        void Dispose();   
    }
}
