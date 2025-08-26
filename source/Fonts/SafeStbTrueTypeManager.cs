using NanoUI.Common;
using NanoUI.Nvg;
using SafeStbTrueTypeSharp;
using System;
using System.Collections.Generic;
using System.Numerics;
using static SafeStbTrueTypeSharp.Common;

namespace NanoUI.Fonts
{
    // SafeStbTrueTypeSharp is a safe (managed) version of StbTrueTypeSharp 1.24
    // https://github.com/StbSharp/StbTrueTypeSharp
    // https://github.com/StbSharp/SafeStbTrueTypeSharp
    internal class SafeStbTrueTypeManager : IFontManager
    {
        // these affects sharpness & brightness when using FontBaking.SDF
        const float PIXEL_DIST = 200;// in sdf_test.c 64.0f
        const byte ONE_EDGE = 128;
        const int FONT_ATLAS_PADDING = 1; // we use 1 pixel padding

        Dictionary<int, FontInfo> _fonts = new();

        // we collect all current glyph commands temporarily here
        UnsafeBuffer<GlyphShapeCommand> _glyphCommands = new();

        // fontmanager implementation may not support all FontGlyphBaking methods
        // you can add here some fallback logic if your implementation doesn't support requested method
        // note: the default implementation supports all methods
        public GlyphBaking GetGlyphBaking(GlyphBaking requestedBaking) => requestedBaking;
        
        // note: fontindex is TTC font collection index, in TTF files it is 0
        public bool Load(int fontId, ReadOnlySpan<byte> data, int fontCollectionIndex)
        {
            int stbError;
            int offset = stbtt_GetFontOffsetForIndex(data.ToArray(), fontCollectionIndex);
            
            if (offset == -1)
            {
                stbError = 0;
            }
            else
            {
                var font = new FontInfo();
                stbError = font.stbtt_InitFont(data.ToArray(), offset);

                if (stbError != 0)
                {
                    // add to dictionary
                    _fonts[fontId] = font;
                }
                else
                {
                    // todo: throw error?
                }
            }

            return stbError != 0;
        }

        public void GetFontVMetrics(int fontId, out int ascent, out int descent, out int lineGap)
        {
            if (!_fonts.TryGetValue(fontId, out var font))
            {
                ascent = descent = lineGap = 0;
                return;
            }

            font.stbtt_GetFontVMetrics(out ascent, out descent, out lineGap);
        }

        // this is a scale to convert font file's internal units to pixels
        public float GetPixelScale(int fontId, float size)
        {
            if (!_fonts.TryGetValue(fontId, out var font))
            {
                return 0;
            }

            return font.stbtt_ScaleForMappingEmToPixels(size);
        }

        // returns 0, if not found (could be -1)
        public int GetFontGlyphIndex(int fontId, uint codepoint)
        {
            if (!_fonts.TryGetValue(fontId, out var font))
            {
                return 0;
            }

            return font.stbtt_FindGlyphIndex((int)codepoint);
        }

        public int GetGlyphKernAdvance(int fontId, int glyph1, int glyph2)
        {
            if (!_fonts.TryGetValue(fontId, out var font))
            {
                return 0;
            }

            return font.stbtt_GetGlyphKernAdvance(glyph1, glyph2);
        }

        public bool BuildGlyphBitmap(int fontId, int glyph, float size, float scale, out int advance, out int lsb, out int x0, out int y0, out int x1, out int y1)
        {
            if (!_fonts.TryGetValue(fontId, out var font))
            {
                advance = lsb = x0 = y0 = x1 = y1 = 0;
                return false;
            }

            int advanceWidth = 0;
            int leftSideBearing = 0;
            int ix0 = 0;
            int iy0 = 0;
            int ix1 = 0;
            int iy1 = 0;

            font.stbtt_GetGlyphHMetrics(glyph, ref advanceWidth, ref leftSideBearing);
            font.stbtt_GetGlyphBitmapBox(glyph, scale, scale, ref ix0, ref iy0, ref ix1, ref iy1);

            advance = advanceWidth;
            lsb = leftSideBearing;
            x0 = ix0;
            y0 = iy0;
            x1 = ix1;
            y1 = iy1;

            return true;
        }

        public void RasterizeGlyphBitmap(int fontId, Span<byte> texData, int startIndex, int outWidth, int outHeight, int outStride, float scaleX, float scaleY, int glyph, GlyphBaking fontBaking)
        {
            if (!_fonts.TryGetValue(fontId, out var font))
            {
                return;
            }

            if (fontBaking == GlyphBaking.Normal)
            {
                // normal bitmap with real font size
                FakePtr<byte> output = new FakePtr<byte>(texData.ToArray(), startIndex);
                font.stbtt_MakeGlyphBitmap(output, outWidth, outHeight, outStride, scaleX, scaleY, glyph);
            }
            else
            {
                // Signed Distance Field (1 bitmap for all font sizes) - uses font scaling factor
                int xoff = 0;
                int yoff = 0;
                int w = 0;
                int h = 0;

                byte[]? bitmap = font.stbtt_GetGlyphSDF(Math.Max(scaleX, scaleY), glyph, FONT_ATLAS_PADDING,
                    ONE_EDGE, PIXEL_DIST, ref w, ref h, ref xoff, ref yoff);

                if (bitmap == null || bitmap.Length == 0)
                {
                    return;
                }

                if (w < outWidth)
                    outWidth = w;

                if (h < outHeight)
                    outHeight = h;

                for (int x = 0; x < w; ++x)
                {
                    for (int y = 0; y < h; ++y)
                    {
                        // calculate position in atlas texture
                        texData[y * outStride + x + startIndex] = bitmap[y * w + x];
                    }
                }
            }
        }

        // note: this is only called if we use for glyphs shapes (not bitmap & atlas)
        public bool GetGlyphShape(int fontId, int fontGlyphIndex, out ReadOnlySpan<GlyphShapeCommand> shape)
        {
            if (!_fonts.TryGetValue(fontId, out var font))
            {
                shape = default;
                return false;
            }

            // clear temp buffer
            _glyphCommands.Clear();

            // get glyph vertices
            int verticesCount = font.stbtt_GetGlyphShape(fontGlyphIndex, out stbtt_vertex[]? vertices);

            if (vertices == null)
            {
                shape = default;
                return false;
            }

            // we must store last position since we are going to convert QuadTo --> BezierTo
            Vector2 lastPos = Vector2.Zero;

            // loop glyph vertices & create path commands
            for (int i = 0; i < verticesCount; i++)
            {
                if (vertices[i].type == STBTT_vmove)
                {
                    ref GlyphShapeCommand command1 = ref _glyphCommands.Add();
                    command1.CommandType = GlyphShapeCommandType.MoveTo;
                    command1.P0 = new Vector2(vertices[i].x, vertices[i].y);
                    command1.P1 = Vector2.Zero;
                    command1.P2 = Vector2.Zero;
                }
                else if (vertices[i].type == STBTT_vline)
                {
                    ref GlyphShapeCommand command2 = ref _glyphCommands.Add();
                    command2.CommandType = GlyphShapeCommandType.LineTo;
                    command2.P0 = new Vector2(vertices[i].x, vertices[i].y);
                    command2.P1 = Vector2.Zero;
                    command2.P2 = Vector2.Zero;
                }
                else if (vertices[i].type == STBTT_vcurve)
                {
                    // we convert QuadTo -> BezierTo since it is little bit faster in path creation
                    // because QuadTo does this conversion anyway
                    Vector2 cp = new Vector2(vertices[i].cx, vertices[i].cy);
                    Vector2 p = new Vector2(vertices[i].x, vertices[i].y);

                    ref GlyphShapeCommand command4 = ref _glyphCommands.Add();
                    command4.CommandType = GlyphShapeCommandType.BezierTo;
                    command4.P0 = lastPos + 2.0f / 3.0f * (cp - lastPos);
                    command4.P1 = p + 2.0f / 3.0f * (cp - p);
                    command4.P2 = p;
                }
                else if (vertices[i].type == STBTT_vcubic)
                {
                    ref GlyphShapeCommand command6 = ref _glyphCommands.Add();
                    command6.CommandType = GlyphShapeCommandType.BezierTo;
                    command6.P0 = new Vector2(vertices[i].cx, vertices[i].cy);
                    command6.P1 = new Vector2(vertices[i].cx1, vertices[i].cy1);
                    command6.P2 = new Vector2(vertices[i].x, vertices[i].y);
                }

                // store last point so we can convert QuadTo -> BezierTo
                lastPos = new Vector2(vertices[i].x, vertices[i].y);
            }

            shape = _glyphCommands.AsReadOnlySpan();

            return _glyphCommands.Count > 0;
        }

        // when trying to add glyph(s) & no room left in atlas. So user must expand atlas
        public void OnAtlasFull(NvgContext ctx, int atlasWidth, int atlasHeight)
        {
            ctx.AtlasExpand((uint)atlasWidth * 2, (uint)atlasHeight * 2);
        }

        public void Dispose()
        {
            _fonts.Clear();

            // dispose temp shapes
            _glyphCommands.Dispose();
        }
    }
}