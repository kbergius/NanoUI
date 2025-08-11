using NanoUI.Common;
using NanoUI.Fonts.Data;
using NanoUI.Nvg;
using NanoUI.Nvg.Data;
using NanoUI.Utils;
using System;
using System.Numerics;

namespace NanoUI.Fonts
{
    internal static partial class Fontstash
    {
        #region _textIterInit, _textIterNext

        static FontTextIter _iter = new();

        // we use same iterator, so we must reset values
        // note: we should check validity before we come here
        // fonsTextIterInit
        static void _textIterInit(float x, float y, ReadOnlySpan<char> chars, FontGlyphBitmap bitmapOption)
        {
            // Horizontal align
            if ((_fontState.TextAlign & TextAlignment.Left) != 0)
            {
                // empty
            }
            else if ((_fontState.TextAlign & TextAlignment.Right) != 0)
            {
                float width = TextBounds(_fonts[_fontState.FontId], x, y, chars);
                x -= width;
            }
            else if ((_fontState.TextAlign & TextAlignment.Center) != 0)
            {
                float width = TextBounds(_fonts[_fontState.FontId], x, y, chars);
                x -= width * 0.5f;
            }

            y += _getVertAlign(_fonts[_fontState.FontId], _fontState.FontSize, _fontState.TextAlign);

            // reset with new values 
            _iter.Reset(x, y, bitmapOption);

            // set chars
            _textChars.Clear();
            _textChars.AddRange(chars);
        }

        // fonsTextIterNext
        static bool _textIterNext()
        {
            if (_iter.NextPos >= _textChars.Count)
            {
                return false;
            }

            _iter.CurrentPos = _iter.NextPos;

            if (char.IsHighSurrogate(_textChars[_iter.NextPos]) && _iter.NextPos + 1 < _textChars.Count)
            {
                // convert to Utf32 to support icons & non-ascii
                _iter.Codepoint = (uint)char.ConvertToUtf32(_textChars[_iter.NextPos], _textChars[_iter.NextPos + 1]);
                _iter.NextPos += 2;
            }
            else
            {
                _iter.Codepoint = _textChars[_iter.NextPos];
                _iter.NextPos++;
            }

            _iter.X = _iter.NextX;
            _iter.Y = _iter.NextY;

            // build quad for current char
            // note: with shapes we don't build glyph quad; just calculate iterators NextX & NextY 
            _buildGlyphQuad(_fontState.FontId);

            return true;
        }

        #endregion

        #region _getGlyph, _getGlyphIndex

        // note: we now have fonts & glyph collections in arrays, so fontId is index in fonts array &
        // glyphIndex is position in glyph collection
        // todo: if there will be possibilty to remove/insert fonts, then should be dictionaries
        static ref Glyph _getGlyph(int fontId, int glyphIndex)
        {
            return ref _glyphs[fontId][glyphIndex];
        }

        // gets glyph index or creates new glyph & rasterizes it
        // note: currently we create glyphs for all different font sizes
        // fons__getGlyph
        // todo: we could rasterize SDF glyphs only once for codepoint & all font sizes and
        // dynamically calculate glyph quad etc? Now rasterizes all font sizes fro same codepoint
        static unsafe bool _getGlyphIndex(int fontId, float fontSize, uint codepoint, GlyphBaking glyphBaking, FontGlyphBitmap bitmapOption, out int glyphIndex)
        {
            glyphIndex = Globals.INVALID;

            if (_fontManager == null)
                return false;

            int counter = 0;

            // these are counted only if baking is Normal
            int blur = 0;
            int dilate = 0;

            if(glyphBaking == GlyphBaking.Normal)
            {
                blur = (int)_fontState.Blur;
                dilate = (int)_fontState.Dilate;
            }

            // different logic based on glyphBaking method
            foreach (Glyph glyph in _glyphs[fontId].AsReadOnlySpan())
            {
                // these are common properties that we check match
                if (glyph.Codepoint == codepoint && glyph.FontSize == fontSize)
                {
                    // todo: with shapes we could use 1 glyph for all font sizes, but then we should
                    // "manually" calculate advanceX scale based on current font size / glyph created font stize
                    if(glyphBaking == GlyphBaking.Shapes)
                    {
                        glyphIndex = counter;

                        // we are not using bitmaps, so we can just return when we found matching glyph
                        return true;
                    }
                    else if (glyphBaking == GlyphBaking.SDF)
                    {
                        glyphIndex = counter;

                        if (bitmapOption == FontGlyphBitmap.Optional || (glyph.X0 >= 0 && glyph.Y0 >= 0))
                        {
                            return true;
                        }

                        break;
                    }
                    else
                    {
                        // Normal
                        if (glyph.Blur == blur && glyph.Dilate == dilate)
                        {
                            glyphIndex = counter;

                            if (bitmapOption == FontGlyphBitmap.Optional || (glyph.X0 >= 0 && glyph.Y0 >= 0))
                            {
                                return true;
                            }

                            break;
                        }
                    }
                }

                counter++;
            }

            // we check if font has codepoint defined
            int fontGlyphIndex = _fontManager.GetFontGlyphIndex(fontId, codepoint);

            if (fontGlyphIndex <= 0)
            {
                // not found glyph with codepoint

                // note: this happens also with normal fonts when codepoint is
                // "non-printable char" (line feed etc)
                glyphIndex = Globals.INVALID;
                return false;
            }

            // pixel scale
            float scale = _fontState.PixelScale;

            _fontManager.BuildGlyphBitmap(fontId, fontGlyphIndex, fontSize, scale, out int advance, out int lsb, out int x0, out int y0, out int x1, out int y1);

            const int antiAliasBonus = 2;
            int pad = blur + dilate + antiAliasBonus;
            int gx = 0;
            int gy = 0;
            int gw = x1 - x0 + pad * 2;
            int gh = y1 - y0 + pad * 2;

            // Not used with shapes - iterator bitmapOption == FontGlyphBitmap.Optional
            if (bitmapOption == FontGlyphBitmap.Required)
            {
                bool added = _atlasAddRect(gw, gh, ref gx, ref gy);

                if (!added)
                {
                    // invoke event so user can expand atlas
                    _fontManager.OnAtlasFull(NvgContext.Instance, _atlas.Width, _atlas.Height);

                    // try again
                    added = _atlasAddRect(gw, gh, ref gx, ref gy);

                    if (!added)
                    {
                        glyphIndex = Globals.INVALID;
                        return false;
                    }
                }
            }
            else
            {
                gx = Globals.INVALID;
                gy = Globals.INVALID;
            }

            // Create glyph?
            if (glyphIndex == Globals.INVALID)
            {
                // add
                ref Glyph glyph = ref _glyphs[fontId].Add();
                glyph.Codepoint = codepoint;
                glyph.FontSize = fontSize;
                glyph.Blur = blur;
                glyph.Dilate = dilate;

                glyphIndex = _glyphs[fontId].Count - 1;
            }

            ref Glyph newGlyph = ref _getGlyph(fontId, glyphIndex);
            newGlyph.FontGlyphIndex = fontGlyphIndex;
            newGlyph.X0 = gx;
            newGlyph.Y0 = gy;
            newGlyph.X1 = newGlyph.X0 + gw;
            newGlyph.Y1 = newGlyph.Y0 + gh;
            newGlyph.AdvX = scale * advance;
            newGlyph.OffX = x0 - pad;
            newGlyph.OffY = y0 - pad;

            // shapes
            if (glyphBaking == GlyphBaking.Shapes && !_glyphShapes.ContainsKey((fontId, codepoint)))
            {
                // todo: we could check if we really get shape
                _fontManager.GetGlyphShape(fontId, fontGlyphIndex, out ReadOnlySpan<GlyphShapeCommand> shape);

                // add to glyph shapes
                _glyphShapes.Add((fontId, codepoint), shape.ToArray());

                return true;
            }

            // when we use shapes we set iterator FontGlyphBitmap.Optional
            // so this exits when shapes & when we don't need bitmap
            if (bitmapOption == FontGlyphBitmap.Optional)
            {
                return true;
            }

            // Bitmap rasterizing

            // get atlas width
            int atlasWidth = _atlas.Width;

            // Rasterize start point
            int index = (newGlyph.X0 + pad) + (newGlyph.Y0 + pad) * atlasWidth;

            _fontManager.RasterizeGlyphBitmap(fontId, _atlasData, index, gw - (pad * 2), gh - (pad * 2), atlasWidth, scale, scale, fontGlyphIndex, glyphBaking);

            // Ensure border pixel
            index = newGlyph.X0 + (newGlyph.Y0 * atlasWidth);

            for (int y = 0; y < gh; y++)
            {
                _atlasData[index + (y * atlasWidth)] = 0;
                _atlasData[index + (gw - 1 + y * atlasWidth)] = 0;
            }

            for (int x = 0; x < gw; x++)
            {
                _atlasData[index + x] = 0;
                _atlasData[index + ((gh - 1) * atlasWidth)] = 0;
            }

            // SDF has not these options
            if (glyphBaking == GlyphBaking.Normal)
            {
                // Dilate (used to draw text outline)
                if (dilate > 0)
                {
                    fixed (byte* ptr = &_atlasData[newGlyph.X0 + newGlyph.Y0 * atlasWidth])
                    {
                        _dilate(ptr, gw, gh, atlasWidth, dilate);
                    }
                }

                // Blur
                if (blur > 0)
                {
                    fixed (byte* ptr = &_atlasData[newGlyph.X0 + newGlyph.Y0 * atlasWidth])
                    {
                        _blur(ptr, gw, gh, atlasWidth, blur);
                    }
                }
            }

            // reset dirty rect
            _atlasDirtyRect[0] = Math.Min(_atlasDirtyRect[0], newGlyph.X0);
            _atlasDirtyRect[1] = Math.Min(_atlasDirtyRect[1], newGlyph.Y0);
            _atlasDirtyRect[2] = Math.Max(_atlasDirtyRect[2], newGlyph.X1);
            _atlasDirtyRect[3] = Math.Max(_atlasDirtyRect[3], newGlyph.Y1);

            return true;
        }

        #endregion

        #region _buildGlyphQuad, _setGlyphQuad

        // fons__getQuad
        // note: when using shapes, it is not necessary to really build glyph quad;
        // we are only intrested to set iterator's values
        static void _buildGlyphQuad(int fontId)
        {
            if (_getGlyphIndex(fontId, _fontState.FontSize, _iter.Codepoint, _fontState.GlyphBaking, _iter.BitmapOption, out int glyphIndex))
            {
                // set quad
                _setGlyphQuad(fontId, _fontState.FontSize, _iter.PrevGlyphIndex, glyphIndex,
                    _fontState.PixelScale, _fontState.CharSpacing,
                    ref _iter.NextX, ref _iter.NextY);
            }

            // store previous since we use it in advanceX
            _iter.PrevGlyphIndex = glyphIndex;
        }

        // note: if we draw shapes, we don't need to set glyph quad, since it is not used
        // todo: we don't really need ref in y
        static void _setGlyphQuad(int fontId, float fontSize, int prevGlyphIndex, int glyphIndex, float scale, float spacing,
            ref float x, ref float y)
        {
            ref Glyph glyph = ref _getGlyph(fontId, glyphIndex);

            if (_fontManager != null && prevGlyphIndex > Globals.INVALID)
            {
                // we must use fontGlyphIndexes
                ref Glyph prevGlyph = ref _getGlyph(fontId, prevGlyphIndex);

                float adv = _fontManager.GetGlyphKernAdvance(fontId, prevGlyph.FontGlyphIndex, glyph.FontGlyphIndex) * scale;
                x += (int)(adv + spacing + 0.5f);
            }

            // we don't need to calculate quad if we are using shapes
            if (_fontState.GlyphBaking != GlyphBaking.Shapes)
            {
                float xoff = glyph.OffX + 1;
                float yoff = glyph.OffY + 1;
                float x0 = glyph.X0 + 1;
                float y0 = glyph.Y0 + 1;
                float x1 = glyph.X1 - 1;
                float y1 = glyph.Y1 - 1;

                float rx = MathF.Floor(x + xoff);
                float ry = MathF.Floor(y + yoff);

                _quad.x0 = rx;
                _quad.y0 = ry;
                _quad.x1 = rx + x1 - x0;
                _quad.y1 = ry + y1 - y0;

                // atlas inverse sizes
                float inverseWidth = 1f / _atlas.Width;
                float inverseHeight = 1f / _atlas.Height;

                _quad.s0 = x0 * inverseWidth;
                _quad.t0 = y0 * inverseHeight;
                _quad.s1 = x1 * inverseWidth;
                _quad.t1 = y1 * inverseHeight;
            }

            x += (int)(glyph.AdvX + 0.5f);
        }

        #endregion

        #region _blur - unsafe

        // Based on Exponential blur, Jani Huhtanen, 2006
        const int APREC = 16;
        const int ZPREC = 7;

        // fons__blur
        static unsafe void _blur(byte* dst, int w, int h, int dstStride, int blur)
        {
            int alpha;
            float sigma;
            //(void)stash;

            if (blur < 1)
                return;
            // Calculate the alpha such that 90% of the kernel is within the radius. (Kernel extends to infinity)
            sigma = (float)blur * 0.57735f; // 1 / sqrt(3)
            //alpha = (int)((1 << APREC) * (1.0f - expf(-2.3f / (sigma + 1.0f))));
            alpha = (int)((1 << APREC) * (1.0f - MathF.Exp(-2.3f / (sigma + 1.0f))));

            _blurRows(dst, w, h, dstStride, alpha);
            _blurCols(dst, w, h, dstStride, alpha);
            _blurRows(dst, w, h, dstStride, alpha);
            _blurCols(dst, w, h, dstStride, alpha);
        }

        // fons__blurCols
        static unsafe void _blurCols(byte* dst, int w, int h, int dstStride, int alpha)
        {
            int x, y;

            for (y = 0; y < h; y++)
            {
                int z = 0; // force zero border

                for (x = 1; x < w; x++)
                {
                    z += (alpha * (((int)(dst[x]) << ZPREC) - z)) >> APREC;
                    dst[x] = (byte)(z >> ZPREC);
                }

                dst[w - 1] = 0; // force zero border
                z = 0;

                for (x = w - 2; x >= 0; x--)
                {
                    z += (alpha * (((int)(dst[x]) << ZPREC) - z)) >> APREC;
                    dst[x] = (byte)(z >> ZPREC);
                }

                dst[0] = 0; // force zero border
                dst += dstStride;
            }
        }

        // fons__blurRows
        static unsafe void _blurRows(byte* dst, int w, int h, int dstStride, int alpha)
        {
            int x, y;

            for (x = 0; x < w; x++)
            {
                int z = 0; // force zero border

                for (y = dstStride; y < h * dstStride; y += dstStride)
                {
                    z += (alpha * (((int)(dst[y]) << ZPREC) - z)) >> APREC;
                    dst[y] = (byte)(z >> ZPREC);
                }

                dst[(h - 1) * dstStride] = 0; // force zero border
                z = 0;

                for (y = (h - 2) * dstStride; y >= 0; y -= dstStride)
                {
                    z += (alpha * (((int)(dst[y]) << ZPREC) - z)) >> APREC;
                    dst[y] = (byte)(z >> ZPREC);
                }

                dst[0] = 0; // force zero border
                dst++;
            }
        }

        #endregion

        #region _dilate - unsafe

        // Gray level morphological dilation approximated by convolving with a max stencil along
        // Diagonal convolution overlaps with horizontal & vertical, so we alternate between vertical & horizontal
        // and diagonal directions to prevent the dilation from being too large.
        // https://github.com/rgb2hsv/nanovg/tree/font_dilate
        // fons__dilate
        static unsafe void _dilate(byte* dst, int w, int h, int dstStride, int dilate)
        {
            for (int iter = 0; iter < dilate; iter++)
            {
                if (iter % 2 == 0)
                {
                    _dilateMaxRows(dst, w, h, dstStride);
                    _dilateMaxCols(dst, w, h, dstStride);
                }
                else
                {
                    _dilateMaxDiagUp(dst, w, h, dstStride);
                    _dilateMaxDiagDown(dst, w, h, dstStride);
                }
            }
        }

        // fons__maxRows
        static unsafe void _dilateMaxRows(byte* dst, int w, int h, int dstStride)
        {
            int x, y;
            byte prev, current;
            byte* ptr;

            for (x = 0; x < w; x++)
            {
                prev = dst[0];

                for (y = dstStride; y < h * dstStride; y += dstStride)
                {
                    ptr = &dst[y];
                    current = *ptr;

                    if (prev > current)
                    {
                        *ptr = prev;
                    }

                    prev = current;
                }

                for (y = (h - 2) * dstStride; y >= 0; y -= dstStride)
                {
                    ptr = &dst[y];
                    current = *ptr;

                    if (prev > current)
                    {
                        *ptr = prev;
                    }

                    prev = current;
                }

                dst++;
            }
        }

        // fons__maxCols
        static unsafe void _dilateMaxCols(byte* dst, int w, int h, int dstStride)
        {
            int x, y;
            byte prev, current;
            byte* ptr;

            for (y = 0; y < h; y++)
            {
                prev = dst[0];

                for (x = 1; x < w; x++)
                {
                    ptr = &dst[x];
                    current = *ptr;

                    if (prev > current)
                    {
                        *ptr = prev;
                    }

                    prev = current;
                }

                for (x = w - 2; x >= 0; x--)
                {
                    ptr = &dst[x];
                    current = *ptr;

                    if (prev > current)
                    {
                        *ptr = prev;
                    }

                    prev = current;
                }

                dst += dstStride;
            }
        }

        // fons__maxDiagUp
        static unsafe void _dilateMaxDiagUp(byte* dst, int w, int h, int dstStride)
        {
            int t, y;
            int a = dstStride - 1;
            int d = w + h;
            byte prev, current;
            byte* ptr;

            for (t = 0; t < d; t++)
            {
                int y_min = (t - w < 0) ? 0 : t - w;
                int y_max = (t < h - 1) ? t : h - 1;
                prev = dst[t + y_min * a];

                for (y = y_min; y <= y_max; y++)
                {
                    ptr = &dst[t + y * a];
                    current = *ptr;

                    if (prev > current)
                    {
                        *ptr = prev;
                    }

                    prev = current;
                }

                for (y = y_max - 1; y >= y_min; y--)
                {
                    ptr = &dst[t + y * a];
                    current = *ptr;

                    if (prev > current)
                    {
                        *ptr = prev;
                    }

                    prev = current;
                }
            }
        }

        // fons__maxDiagDown
        static unsafe void _dilateMaxDiagDown(byte* dst, int w, int h, int dstStride)
        {
            int t, y;
            int a = (h - 1) * dstStride;
            int b = dstStride + 1;
            int d = w + h;
            byte prev, current;
            byte* ptr;

            for (t = 0; t < d; t++)
            {
                int y_min = (t - w < 0) ? 0 : t - w;
                int y_max = (t < h - 1) ? t : h - 1;
                prev = dst[t - y_min * b + a];

                for (y = y_min; y <= y_max; y++)
                {
                    ptr = &dst[t - y * b + a];
                    current = *ptr;

                    if (prev > current)
                    {
                        *ptr = prev;
                    }

                    prev = current;
                }

                for (y = y_max - 1; y >= y_min; y--)
                {
                    ptr = &dst[t - y * b + a];
                    current = *ptr;

                    if (prev > current)
                    {
                        *ptr = prev;
                    }

                    prev = current;
                }
            }
        }

        #endregion

        #region Common

        // nvg__getFontScale
        // we calculate scale & store current font values to font state
        static float _getFontScale(in NvgState nvgState, in NvgParams nvgParams)
        {
            if (_fontManager == null || !_isValidFont(nvgState.FontId))
            {
                // todo : should we throw / debug assert / set default font?
                throw new Exception("Font not found: " + nvgState.FontId);
            }

            float scale = _getMatrixScale(nvgState) * nvgParams.DevicePxRatio;

            // apply scaled & other properties
            _fontState.FontId = nvgState.FontId;
            // we need to store, since different baking methods have different logic in some functions
            _fontState.GlyphBaking = _fontManager.GetGlyphBaking(_fonts[_fontState.FontId].FontBaking);
            _fontState.FontSize = nvgState.FontSize * scale;
            _fontState.CharSpacing = nvgState.TextCharSpacing * scale;
            _fontState.TextAlign = nvgState.TextAlign;
            _fontState.PixelScale = _fontManager.GetPixelScale(nvgState.FontId, _fontState.FontSize);
            _fontState.Blur = nvgState.TextBlur * scale;
            _fontState.Dilate = nvgState.TextDilate * scale;

            // glyph shapes
            _fontState.TextShapeOutline = nvgState.TextShapeOutline;
            _fontState.TextShapeOutlineWidth = nvgState.TextShapeOutlineWidth;
            _fontState.TextShapeFill = nvgState.TextShapeFill;

            return scale;
        }

        static bool _isValidFont(int fontId)
        {
            return fontId >= 0 && fontId < _fonts.Length;
        }

        static float _getMatrixScale(in NvgState nvgState)
        {
            return MathF.Min(_quantize(MathUtils.GetAverageScale(nvgState.Transform), 0.01f), 4.0f);
        }

        // nvg__quantize
        static float _quantize(float a, float d)
        {
            return (int)(a / d + 0.5f) * d;
        }

        // nvg__getAverageScale - in MathUtils

        // nvg__isTransformFlipped
        static bool _isTransformFlipped(Matrix3x2 m)
        {
            float det = m.M11 * m.M22 - m.M21 * m.M12;
            return det < 0.0f;
        }

        // note: atlas should be filled beginning from top left
        // fons__getVertAlign
        static float _getVertAlign(in Font font, float fontSize, TextAlignment align)
        {
            if ((align & TextAlignment.Top) != 0)
            {
                return font.Ascender * fontSize;
            }
            else if ((align & TextAlignment.Middle) != 0)
            {
                return (font.Ascender + font.Descender) / 2.0f * fontSize;
            }
            else if ((align & TextAlignment.Baseline) != 0)
            {
                return 0.0f;
            }
            else if ((align & TextAlignment.Bottom) != 0)
            {
                return font.Descender * fontSize;
            }

            return 0.0f;
        }

        #endregion
    }
}
