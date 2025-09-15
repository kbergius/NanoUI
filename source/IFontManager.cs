using NanoUI.Common;
using NanoUI.Fonts;
using NanoUI.Nvg;
using System;

namespace NanoUI
{
    /// <summary>
    /// IFontManager is an interface to font manager implementations.
    /// NanoUI has 2 built-in font managers based on StbTrueType (safe & unsafe),
    /// but you can also create your own preferred font manager.
    /// Note: If you create your own, you must pass it to NvgContext when you create it.
    /// </summary>
    public interface IFontManager
    {
        /// <summary>
        /// Font manager implementation may not support all GlyphBaking methods.
        /// You can add here some fallback logic in this case.
        /// Note: NanoUI's built-in font managers support all glyph baking methods.
        /// </summary>
        GlyphBaking GetGlyphBaking(GlyphBaking requestedBaking);

        /// <summary>
        /// Loads font from specified data.
        /// Note: fontCollectionIndex is TTC font collection index, in TTF files it is 0.
        /// </summary>
        bool Load(int fontId, ReadOnlySpan<byte> data, int fontCollectionIndex);

        /// <summary>
        /// Gets font's vertical metrics.
        /// </summary>
        void GetFontVMetrics(int fontId, out int ascent, out int descent, out int lineGap);

        /// <summary>
        /// Scale to convert font's internal units to pixels.
        /// </summary>
        float GetPixelScale(int fontId, float size);

        /// <summary>
        /// Gets glyph index for specified codepoint.
        /// Returns 0, if codepoint is not found.
        /// </summary>
        int GetFontGlyphIndex(int fontId, uint codepoint);

        /// <summary>
        /// Gets kerning advance.
        /// </summary>
        int GetGlyphKernAdvance(int fontId, int glyph1, int glyph2);

        /// <summary>
        /// Builds glyph bitmap.
        /// </summary>
        bool BuildGlyphBitmap(int fontId, int glyph, float size, float scale, out int advance, out int lsb, out int x0, out int y0, out int x1, out int y1);

        /// <summary>
        /// Rasterizes glyph bitmap.
        /// </summary>
        void RasterizeGlyphBitmap(int fontId, Span<byte> texData, int startIndex, int outWidth, int outHeight, int outStride, float scaleX, float scaleY, int glyph, GlyphBaking fontBaking);

        /// <summary>
        /// Gets glyph's shape (vectorized glyph).
        /// You can do any kind of transforms (scale etc) with these and glyphs are always sharp.
        /// Also you have opportunity to paint glyph shape with different paint methods
        /// (solid, gradients, image etc).
        /// Note: this is only called, if font is initialized with GlyphBaking.Shapes.
        /// </summary>
        bool GetGlyphShape(int fontId, int fontGlyphIndex, out ReadOnlySpan<GlyphShapeCommand> shape);

        /// <summary>
        /// When NanoUI tries to add glyph's bitmap to font atlas texture and
        /// there is no room left, this method is called and you must must expand atlas texture.
        /// </summary>
        void OnAtlasFull(NvgContext ctx, int atlasWidth, int atlasHeight);

        /// <summary>
        /// Dispose any unmanaged resources.
        /// </summary>
        void Dispose();   
    }
}
