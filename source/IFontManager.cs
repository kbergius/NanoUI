using NanoUI.Common;
using NanoUI.Nvg;
using System;

namespace NanoUI
{
    /// <summary>
    /// IFontManager is an interface to font manager implementations.
    /// NanoUI has 2 built-in font managers based on StbTrueType (safe & unsafe),
    /// but you can also create your own font manager.
    /// Note: If you create your own, you must pass it to NvgContext when you create it.
    /// </summary>
    public interface IFontManager
    {
        /// <summary>
        /// Font manager implementation may not support all GlyphBaking methods.
        /// You can add here some fallback logic in this case.
        /// </summary>
        /// <param name="requestedBaking">Requested baking</param>
        /// <returns>GlyphBaking</returns>
        /// <remarks>NanoUI's built-in font managers support all glyph baking methods.</remarks>
        GlyphBaking GetGlyphBaking(GlyphBaking requestedBaking);

        /// <summary>
        /// Loads font from specified data.
        /// </summary>
        /// <param name="fontId">Font id</param>
        /// <param name="data">Data</param>
        /// <param name="fontCollectionIndex">Font collection index</param>
        /// <returns>Success</returns>
        /// <remarks>FontCollectionIndex is TTC font collection index, in TTF files it is 0.</remarks>
        bool Load(int fontId, ReadOnlySpan<byte> data, int fontCollectionIndex);

        /// <summary>
        /// Gets font's vertical metrics.
        /// </summary>
        /// <param name="fontId">Font id</param>
        /// <param name="ascent">Ascent</param>
        /// <param name="descent">Descent</param>
        /// <param name="lineGap">Line gap</param>
        void GetFontVMetrics(int fontId, out int ascent, out int descent, out int lineGap);

        /// <summary>
        /// Scale to convert font's internal units to pixels.
        /// </summary>
        /// <param name="fontId">Font id</param>
        /// <param name="size">Font size</param>
        /// <returns>Scale</returns>
        float GetPixelScale(int fontId, float size);

        /// <summary>
        /// Gets glyph index for specified codepoint.
        /// </summary>
        /// <param name="fontId">Font id</param>
        /// <param name="codepoint">Codepoint</param>
        /// <returns>Glyph index or 0, if codepoint is not found.</returns>
        int GetFontGlyphIndex(int fontId, uint codepoint);

        /// <summary>
        /// Gets kerning advance.
        /// </summary>
        /// <param name="fontId">Font id</param>
        /// <param name="glyph1">Glyph1</param>
        /// <param name="glyph2">Glyph2</param>
        /// <returns>Kerning advance</returns>
        int GetGlyphKernAdvance(int fontId, int glyph1, int glyph2);

        /// <summary>
        /// Builds glyph bitmap.
        /// </summary>
        /// <param name="fontId">Font id</param>
        /// <param name="glyph">Glyph</param>
        /// <param name="size">Font size</param>
        /// <param name="scale">Scale</param>
        /// <param name="advance">Advance</param>
        /// <param name="lsb">Lsb</param>
        /// <param name="x0">x0</param>
        /// <param name="y0">y0</param>
        /// <param name="x1">x1</param>
        /// <param name="y1">y1</param>
        /// <returns>Success</returns>
        bool BuildGlyphBitmap(int fontId, int glyph, float size, float scale, out int advance, out int lsb, out int x0, out int y0, out int x1, out int y1);

        /// <summary>
        /// Rasterizes glyph bitmap.
        /// </summary>
        /// <param name="fontId">Font id</param>
        /// <param name="texData">Texture data</param>
        /// <param name="startIndex">StartIndex</param>
        /// <param name="outWidth">OutWidth</param>
        /// <param name="outHeight">OutHeight</param>
        /// <param name="outStride">OutStride</param>
        /// <param name="scaleX">ScaleX</param>
        /// <param name="scaleY">ScaleY</param>
        /// <param name="glyph">Glyph</param>
        /// <param name="fontBaking">GlyphBaking</param>
        void RasterizeGlyphBitmap(int fontId, Span<byte> texData, int startIndex, int outWidth, int outHeight, int outStride, float scaleX, float scaleY, int glyph, GlyphBaking fontBaking);

        /// <summary>
        /// Gets glyph's shape (vectorized glyph).
        /// You can do any kind of transforms (scale etc) with these and glyphs are always sharp.
        /// Also you have opportunity to paint glyph shape with different paint methods
        /// (solid, gradients, image etc).
        /// </summary>
        /// <param name="fontId">Font id</param>
        /// <param name="fontGlyphIndex">FontGlyphIndex</param>
        /// <param name="shape">Shape</param>
        /// <returns>Success</returns>
        /// <remarks>This is only called, if font is initialized with GlyphBaking.Shapes.</remarks>
        bool GetGlyphShape(int fontId, int fontGlyphIndex, out ReadOnlySpan<GlyphShapeCommand> shape);

        /// <summary>
        /// When NanoUI tries to add glyph's bitmap to font atlas texture and
        /// there is no room left, this method is called and you must must expand atlas texture.
        /// </summary>
        /// <param name="ctx">NvgContext</param>
        /// <param name="atlasWidth">Atlas width</param>
        /// <param name="atlasHeight">Atlas height</param>
        void OnAtlasFull(NvgContext ctx, int atlasWidth, int atlasHeight);

        /// <summary>
        /// Dispose any unmanaged resources.
        /// </summary>
        void Dispose();   
    }
}
