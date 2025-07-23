using NanoUI.Common;
using NanoUI.Fonts.Data;
using NanoUI.Nvg;
using NanoUI.Nvg.Data;
using NanoUI.Rendering;
using NanoUI.Utils;
using System;
using System.Numerics;

namespace NanoUI.Fonts
{
    internal static partial class Fontstash
    {
        // these functions are not "real" extensions. They are just moved from NvgContext class

        #region DrawText

        static Vector2[] _corners = new Vector2[4];

        // returns x position after text drawn
        // nvgText
        public static float DrawText(NvgContext ctx, ref NvgState state, in NvgParams nvgParams, Vector2 pos, ReadOnlySpan<char> chars)
        {
            if (chars.IsEmpty)
            {
                //return -1;
                return pos.X;
            }

            // this also inits/resets font state
            float scale = _getFontScale(state, nvgParams);

            // check if we use shapes
            if (_fontState.GlyphBaking == GlyphBaking.Shapes)// _useGlyphShapes())
            {
                float res = 0;

                // note: there is different winding & logic for fills & strokes (outline),
                // so we must draw text twice, if both of them are defined
                if (_fontState.TextShapeFill != null)
                {
                    res = DrawTextAsPaths(ctx, ref state, nvgParams, pos, chars, scale, true);
                }

                if (_fontState.TextShapeOutline != null && _fontState.TextShapeOutlineWidth > 0)
                {
                    res = DrawTextAsPaths(ctx, ref state, nvgParams, pos, chars, scale, false);
                }

                return res;
            }

            // collect info where vertices start
            int verticesOffset = DrawCache.VerticesCount;

            bool isFlipped = _isTransformFlipped(state.Transform);

            float invscale = 1.0f / scale;

            // reset text iterator
            _textIterInit(pos.X * scale, pos.Y * scale, chars, FontGlyphBitmap.Required);

            // loop chars
            // bote: this calculates also the quad (min/max pos & uv in atlas texture)
            while (_textIterNext())
            {
                if (isFlipped)
                {
                    MathUtils.Swap(ref _quad.y0, ref _quad.y1);
                    MathUtils.Swap(ref _quad.t0, ref _quad.t1);
                }

                // get glyph corners
                _corners[0] = Vector2.Transform(new Vector2(_quad.x0 * invscale, _quad.y0 * invscale), state.Transform);
                _corners[1] = Vector2.Transform(new Vector2(_quad.x1 * invscale, _quad.y0 * invscale), state.Transform);
                _corners[2] = Vector2.Transform(new Vector2(_quad.x1 * invscale, _quad.y1 * invscale), state.Transform);
                _corners[3] = Vector2.Transform(new Vector2(_quad.x0 * invscale, _quad.y1 * invscale), state.Transform);

                // quad - we make indices for this
                DrawCache.AddVertex(_corners[0], _quad.s0, _quad.t0);
                DrawCache.AddVertex(_corners[1], _quad.s1, _quad.t0);
                DrawCache.AddVertex(_corners[2], _quad.s1, _quad.t1);
                DrawCache.AddVertex(_corners[3], _quad.s0, _quad.t1);
            }

            // update atlas texture, if atlas is dirty
            _flushTextTexture();

            // create draw call
            // todo: send as a parameter _atlasTexture?
            state.Fill.Texture = _atlasTexture;

            DrawCache.CreateTextCommands(state, verticesOffset, nvgParams.FringeWidth, _fonts[state.FontId].FontBaking, state.FontSize);

            return _iter.NextX / scale;
        }

        // note: this is kind of "hack":
        // - with strokes we don't create subpaths; instead we force to create separate paths
        // - with fills we force winding manual
        static float DrawTextAsPaths(NvgContext ctx, ref NvgState state, in NvgParams nvgParams,
            Vector2 pos, ReadOnlySpan<char> chars, float scale, bool fill)
        {
            float invscale = 1.0f / scale;

            // reset text iterator - we don't use bitmaps
            _textIterInit(pos.X * scale, pos.Y * scale, chars, FontGlyphBitmap.Optional);

            // we use this value to convert glyph commands values (font file internal units) to pixels
            float pixelScale = _fontState.PixelScale;

            int fontId = _fontState.FontId;

            // this is for the strokes (we call MoveTo after all LineTo, BezierTo, QuadTo commands)
            // It essentially creates separate paths (not subpaths)
            Vector2 lastPoint = Vector2.Zero;

            ctx.BeginPath();

            // loop chars
            // this sets x & y value & codepoint to iterator
            while (_textIterNext())
            {
                // now try to get glyph shape
                if (_glyphShapes.TryGetValue((fontId, _iter.Codepoint), out var shape))
                {
                    // get the "baseline" start point
                    Vector2 basePoint = new Vector2(_iter.X, _iter.Y);

                    // loop shape to create path commands
                    for (int i = 0; i < shape.Length; i++)
                    {
                        ref var command = ref shape[i];

                        // check command type
                        // note: position coordinates are in font file's units
                        switch (command.CommandType)
                        {
                            case GlyphShapeCommandType.MoveTo:
                                ctx._pathMoveTo(state, GetShapePoint(basePoint, command.P0, pixelScale, invscale, state.Transform));
                                
                                if (fill)
                                {
                                    // this is a hack to correctly handle many holes in glyph shape
                                    ctx._pathWinding(Winding.Manual);
                                }
                                break;
                            case GlyphShapeCommandType.LineTo:
                                // calculate last point (moveTo point)
                                lastPoint = GetShapePoint(basePoint, command.P0, pixelScale, invscale, state.Transform);
                                
                                ctx._pathLineTo(state, lastPoint);

                                if (!fill)
                                {
                                    ctx._pathMoveTo(state, lastPoint);
                                }
                                break;
                            case GlyphShapeCommandType.BezierTo:
                                // calculate last point (moveTo point)
                                lastPoint = GetShapePoint(basePoint, command.P2, pixelScale, invscale, state.Transform);
                                
                                ctx._pathBezierTo(state,
                                    GetShapePoint(basePoint, command.P0, pixelScale, invscale, state.Transform),
                                    GetShapePoint(basePoint, command.P1, pixelScale, invscale, state.Transform),
                                    lastPoint);

                                if (!fill)
                                {
                                    ctx._pathMoveTo(state, lastPoint);
                                }
                                break;
                            case GlyphShapeCommandType.QuadTo:
                                // calculate last point (moveTo point)
                                lastPoint = GetShapePoint(basePoint, command.P1, pixelScale, invscale, state.Transform);

                                ctx._pathQuadTo(state,
                                    GetShapePoint(basePoint, command.P0, pixelScale, invscale, state.Transform),
                                    lastPoint);

                                if (!fill)
                                {
                                    ctx._pathMoveTo(state, lastPoint);
                                }
                                break;
                            case GlyphShapeCommandType.Close:
                                ctx._pathClose();
                                break;
                            case GlyphShapeCommandType.Winding:
                                // this is not called from StbTrueTypeManager
                                ctx._pathWinding(command.Winding);
                                break;
                        }
                    }
                }
            }

            // now we have created path - so flush path drawing
            // note: we have already checked that values are not null, so "!"
            if (fill)
            {
                ctx.FillPaint(_fontState.TextShapeFill!.Value);
                ctx.Fill();
            }
            else
            {
                // outline
                ctx.StrokeColor(_fontState.TextShapeOutline!.Value);
                ctx.StrokeWidth(_fontState.TextShapeOutlineWidth);
                ctx.Stroke();
            }

            return _iter.NextX / scale;
        }

        // note: glyph shapes use mathematical convention (Y increases up). we use here convention
        // Y increases down. So we flip shape point's Y.
        static Vector2 GetShapePoint(Vector2 basePoint, Vector2 shapePoint,
            float pixelScale, float invscale, Matrix3x2 transform)
        {
            return Vector2.Transform((basePoint + (new Vector2(shapePoint.X, -shapePoint.Y) * pixelScale)) * invscale, transform);
        }

        #endregion

        #region TextBox

        // note: it is not recommended to use this in production code, since this calculates text rows every frame.
        // instead use TextBreakLines and cache TextRows.
        // nvgTextBox
        public static bool TextBox(NvgContext ctx, ref NvgState state, in NvgParams nvgParams, float x, float y, float breakRowWidth, ReadOnlySpan<char> text, int maxRows = int.MaxValue)
        {
            if (!_isValidFont(state.FontId) || text.Length == 0)
                return false;

            // store old aligns
            TextAlignment oldAlign = state.TextAlign;
            TextAlignment halign = state.TextAlign & (TextAlignment.Left | TextAlignment.Center | TextAlignment.Right);
            TextAlignment valign = state.TextAlign & (TextAlignment.Top | TextAlignment.Middle | TextAlignment.Bottom | TextAlignment.Baseline);
            
            float lineh = 0;

            TextMetrics(state, nvgParams, out _, out _, out lineh);

            // set temp align
            state.TextAlign = TextAlignment.Left | valign;

            TextBreakLines(state, nvgParams, text, breakRowWidth, maxRows, out var rows);
            {
                foreach (var row in rows)
                {
                    if ((halign & TextAlignment.Left) != 0)
                        DrawText(ctx, ref state, nvgParams, new Vector2(x, y), text.Slice(row.StartPos, row.TextLength));
                    else if ((halign & TextAlignment.Center) != 0)
                        DrawText(ctx, ref state, nvgParams, new Vector2(x + breakRowWidth * 0.5f - row.Width * 0.5f, y), text.Slice(row.StartPos, row.TextLength));
                    else if ((halign & TextAlignment.Right) != 0)
                        DrawText(ctx, ref state, nvgParams, new Vector2(x + breakRowWidth - row.Width, y), text.Slice(row.StartPos, row.TextLength));

                    y += lineh * state.TextLineHeight;
                }
            }

            // restore
            state.TextAlign = oldAlign;

            return true;
        }

        #endregion

        #region TextBounds

        // nvgTextBounds
        public static float TextBounds(in NvgState state, in NvgParams nvgParams, Vector2 pos, ReadOnlySpan<char> chars,
            out Rect bounds)
        {
            if (chars.IsEmpty)
            {
                bounds = default;
                return 0;
            }

            float scale = _getFontScale(state, nvgParams);
            float invscale = 1.0f / scale;

            // we set _textBoxBounds
            float width = TextBounds(_fonts[_fontState.FontId], pos.X * scale, pos.Y * scale, chars);

            // check width & height
            if (_textBoxBounds[2] > _textBoxBounds[0] && _textBoxBounds[3] > _textBoxBounds[1])
            {
                LineBounds(_fonts[state.FontId], pos.Y * scale, out _textBoxBounds[1], out _textBoxBounds[3]);

                bounds = new Rect()
                {
                    Position = new Vector2(_textBoxBounds[0] * invscale, _textBoxBounds[1] * invscale),
                    Size = new Vector2((_textBoxBounds[2] - _textBoxBounds[0]) * invscale, (_textBoxBounds[3] - _textBoxBounds[1]) * invscale)
                };
            }
            else
            {
                bounds = default;
            }

            return width * invscale;
        }

        #endregion

        #region TextBoxBounds

        // note: manipulates state!
        // nvgTextBoxBounds
        public static void TextBoxBounds(ref NvgState state, in NvgParams nvgParams, Vector2 pos, float breakRowWidth,
            ReadOnlySpan<char> chars, int maxRows, out Rect bounds)
        {
            if (chars.IsEmpty)
            {
                bounds = default;
                return;
            }

            TextAlignment oldAlign = state.TextAlign;
            TextAlignment hAlign = state.TextAlign & (TextAlignment.Left | TextAlignment.Center | TextAlignment.Right);
            TextAlignment vAlign = state.TextAlign & (TextAlignment.Top | TextAlignment.Middle | TextAlignment.Bottom | TextAlignment.Baseline);

            TextMetrics(state, nvgParams, out _, out _, out float lineh);

            state.TextAlign = TextAlignment.Left | vAlign;

            float minX = pos.X, maxX = pos.X;
            float minY = pos.Y, maxY = pos.Y;

            float scale = _getFontScale(state, nvgParams);
            float invscale = 1.0f / scale;

            LineBounds(_fonts[state.FontId], 0, out float rMinY, out float rMaxY);

            rMinY *= invscale;
            rMaxY *= invscale;

            // gets rows (limited to max rows)
            TextBreakLines(state, nvgParams, chars, breakRowWidth, maxRows, out ReadOnlySpan<TextRow> rows);

            float rMinX, rMaxX;
            float dx;

            for (int i = 0; i < rows.Length; i++)
            {
                if ((hAlign & TextAlignment.Left) != 0)
                {
                    dx = 0.0f;
                }
                else if ((hAlign & TextAlignment.Center) != 0)
                {
                    dx = (breakRowWidth - rows[i].Width) * 0.5f;
                }
                else if ((hAlign & TextAlignment.Right) != 0)
                {
                    dx = breakRowWidth - rows[i].Width;
                }
                else
                {
                    dx = 0.0f;
                }

                rMinX = pos.X + rows[i].MinX + dx;
                rMaxX = pos.X + rows[i].MaxX + dx;

                minX = MathF.Min(minX, rMinX);
                maxX = MathF.Max(maxX, rMaxX);

                minY = MathF.Min(minY, pos.Y + rMinY);
                maxY = MathF.Max(maxY, pos.Y + rMaxY);

                pos.Y += lineh * state.TextLineHeight;
            }

            state.TextAlign = oldAlign;

            bounds = new Rect(
                new Vector2(minX, minY),
                new Vector2(maxX, maxY) - new Vector2(minX, minY));
        }

        #endregion

        #region TextGlyphPositions

        // nvgTextGlyphPositions
        public static void TextGlyphPositions(in NvgState state, in NvgParams nvgParams, Vector2 pos,
            ReadOnlySpan<char> chars, int maxGlyphs, out ReadOnlySpan<GlyphPosition> posSpan)
        {
            if (chars.IsEmpty)
            {
                posSpan = default;
                return;
            }

            // where glyph positions start
            int posOffset = _glyphPositions.Count;

            int numPos = 0;

            float scale = _getFontScale(state, nvgParams);
            float invscale = 1.0f / scale;

            // reset text iterator
            _textIterInit(pos.X * scale, pos.Y * scale, chars, FontGlyphBitmap.Optional);

            while (_textIterNext())
            {
                numPos++;

                ref GlyphPosition glyphPos = ref _glyphPositions.Add();
                glyphPos.Index = _iter.CurrentPos;
                glyphPos.X = _iter.X * invscale;
                glyphPos.MinX = MathF.Min(_iter.X, _quad.x0) * invscale;
                glyphPos.MaxX = MathF.Max(_iter.NextX, _quad.x1) * invscale;

                if (numPos >= maxGlyphs)
                {
                    posSpan = _glyphPositions.AsReadOnlySpan(posOffset, numPos);
                    return;
                }
            }

            posSpan = _glyphPositions.AsReadOnlySpan(posOffset, numPos);
        }

        #endregion

        #region TextMetrics

        // nvgTextMetrics
        public static void TextMetrics(in NvgState state, in NvgParams nvgParams, out float ascender, out float descender, out float lineHeight)
        {
            float scale = _getFontScale(state, nvgParams);
            float invscale = 1.0f / scale;

            VertMetrics(_fonts[state.FontId], out ascender, out descender, out lineHeight);

            ascender *= invscale;
            descender *= invscale;
            lineHeight *= invscale;
        }

        #endregion

        #region TextBreakLines

        // nvgTextBreakLines
        public static void TextBreakLines(in NvgState state, in NvgParams nvgParams, ReadOnlySpan<char> chars,
            float breakRowWidth, int maxRows, out ReadOnlySpan<TextRow> rowsSpan)
        {
            if (maxRows <= 0 || chars.IsEmpty)
            {
                rowsSpan = default;
                return;
            }

            // text rows offset
            // todo: we could just clear textrows, so then there is no need to get offset
            int textRowsOffset = _textRows.Count;

            // variables
            int nrows = 0;
            float rowStartX = 0.0f;
            float rowWidth = 0.0f;
            float rowMinX = 0.0f;
            float rowMaxX = 0.0f;
            float wordStartX = 0.0f;
            float wordMinX = 0.0f;
            float breakWidth = 0.0f;
            float breakMaxX = 0.0f;
            CodepointType type = CodepointType.Space;
            CodepointType pType = CodepointType.Space;
            uint pCodepoint = 0;
            // positions in string
            int rowStart = -1;
            int rowEnd = -1;
            int wordStart = -1;
            int breakEnd = -1;

            float scale = _getFontScale(state, nvgParams);
            float invscale = 1.0f / scale;

            breakRowWidth *= scale;

            // reset text iterator
            _textIterInit(0, 0, chars, FontGlyphBitmap.Optional);

            // todo: should we use char in text instead of codepoint?
            while (_textIterNext())
            {
                switch (_iter.Codepoint)
                {
                    case 9: // \t
                    case 11: // \v
                    case 12: // \f
                    case 32: // \space
                    case 0x00a0: // NBSP
                        type = CodepointType.Space;
                        break;
                    case 10: // \n
                        type = pCodepoint == 13 ? CodepointType.Space : CodepointType.Newline;
                        break;
                    case 13: // \r
                        type = pCodepoint == 10 ? CodepointType.Space : CodepointType.Newline;
                        break;
                    case 0x0085: // New line
                        type = CodepointType.Newline;
                        break;
                    default:
                        if ((_iter.Codepoint >= 0x4E00 && _iter.Codepoint <= 0x9FFF) ||
                            (_iter.Codepoint >= 0x3000 && _iter.Codepoint <= 0x30FF) ||
                            (_iter.Codepoint >= 0xFF00 && _iter.Codepoint <= 0xFFEF) ||
                            (_iter.Codepoint >= 0x1100 && _iter.Codepoint <= 0x11FF) ||
                            (_iter.Codepoint >= 0x3130 && _iter.Codepoint <= 0x318F) ||
                            (_iter.Codepoint >= 0xAC00 && _iter.Codepoint <= 0xD7AF))
                        {
                            type = CodepointType.CJKChar;
                        }
                        else
                        {
                            type = CodepointType.Char;
                        }
                        break;
                }

                if (type == CodepointType.Newline)
                {
                    nrows++;

                    int start = rowStart >= 0 ? rowStart : _iter.CurrentPos;
                    int end = rowEnd >= 0 ? rowEnd : _iter.CurrentPos;

                    // add text row
                    ref TextRow textRow = ref _textRows.Add();

                    textRow.StartPos = start;
                    textRow.TextLength = end - start;
                    textRow.Width = rowWidth * invscale;
                    textRow.MinX = rowMinX * invscale;
                    textRow.MaxX = rowMaxX * invscale;

                    if (nrows >= maxRows)
                    {
                        rowsSpan = _textRows.AsReadOnlySpan(textRowsOffset, nrows);
                        return;
                    }

                    breakEnd = rowStart;
                    breakWidth = 0.0f;
                    breakMaxX = 0.0f;

                    rowStart = -1;
                    rowEnd = -1;
                    rowWidth = 0.0f;
                    rowMinX = rowMaxX = 0.0f;
                }
                else
                {
                    if (rowStart == -1)
                    {
                        if (type == CodepointType.Char || type == CodepointType.CJKChar)
                        {
                            rowStartX = _iter.X;
                            rowStart = _iter.CurrentPos;
                            rowEnd = _iter.NextPos;
                            rowWidth = _iter.NextX - rowStartX;
                            rowMinX = _quad.x0 - rowStartX;
                            rowMaxX = _quad.x1 - rowStartX;
                            wordStart = _iter.CurrentPos;
                            wordStartX = _iter.X;
                            wordMinX = _quad.x0 - rowStartX;

                            breakEnd = rowStart;
                            breakWidth = 0.0f;
                            breakMaxX = 0.0f;
                        }
                    }
                    else
                    {
                        float nextWidth = _iter.NextX - rowStartX;

                        if (type == CodepointType.Char || type == CodepointType.CJKChar)
                        {
                            rowEnd = _iter.NextPos;
                            rowWidth = _iter.NextX - rowStartX;
                            rowMaxX = _quad.x1 - rowStartX;
                        }

                        if (((pType == CodepointType.Char || pType == CodepointType.CJKChar) && type == CodepointType.Space) || type == CodepointType.CJKChar)
                        {
                            breakEnd = _iter.CurrentPos;
                            breakWidth = rowWidth;
                            breakMaxX = rowMaxX;
                        }

                        if ((pType == CodepointType.Space && (type == CodepointType.Char || type == CodepointType.CJKChar)) || type == CodepointType.CJKChar)
                        {
                            wordStart = _iter.CurrentPos;
                            wordStartX = _iter.X;
                            wordMinX = _quad.x0;
                        }

                        if ((type == CodepointType.Char || type == CodepointType.CJKChar) && nextWidth > breakRowWidth)
                        {
                            if (breakEnd == rowStart)
                            {
                                nrows++;

                                int start = rowStart;
                                int end = _iter.CurrentPos;

                                // add text row
                                ref TextRow textRow = ref _textRows.Add();

                                textRow.StartPos = start;
                                textRow.TextLength = end - start;
                                textRow.Width = rowWidth * invscale;
                                textRow.MinX = rowMinX * invscale;
                                textRow.MaxX = rowMaxX * invscale;

                                if (nrows >= maxRows)
                                {
                                    rowsSpan = _textRows.AsReadOnlySpan(textRowsOffset, nrows);
                                    return;
                                }

                                rowStartX = _iter.X;
                                rowStart = _iter.CurrentPos;
                                rowEnd = _iter.NextPos;
                                rowWidth = _iter.NextX - rowStartX;
                                rowMinX = _quad.x0 - rowStartX;
                                rowMaxX = _quad.x1 - rowStartX;
                                wordStart = _iter.CurrentPos;
                                wordStartX = _iter.X;
                                wordMinX = _quad.x0 - rowStartX;
                            }
                            else
                            {
                                nrows++;

                                int start = rowStart;
                                int end = breakEnd;

                                // add text row
                                ref TextRow textRow = ref _textRows.Add();

                                textRow.StartPos = start;
                                textRow.TextLength = end - start;
                                textRow.Width = breakWidth * invscale;
                                textRow.MinX = rowMinX * invscale;
                                textRow.MaxX = breakMaxX * invscale;

                                if (nrows >= maxRows)
                                {
                                    rowsSpan = _textRows.AsReadOnlySpan(textRowsOffset, nrows);
                                    return;
                                }

                                rowStartX = wordStartX;
                                rowStart = wordStart;
                                rowEnd = _iter.NextPos;
                                rowWidth = _iter.NextX - rowStartX;
                                rowMinX = wordMinX - rowStartX;
                                rowMaxX = _quad.x1 - rowStartX;
                            }

                            breakEnd = rowStart;
                            breakWidth = 0.0f;
                            breakMaxX = 0.0f;
                        }
                    }
                }

                pCodepoint = _iter.Codepoint;
                pType = type;
            }

            if (rowStart != -1)
            {
                nrows++;

                int start = rowStart;
                int end = rowEnd;

                // add text row
                ref TextRow textRow = ref _textRows.Add();

                textRow.StartPos = start;
                textRow.TextLength = end - start;
                textRow.Width = rowWidth * invscale;
                textRow.MinX = rowMinX * invscale;
                textRow.MaxX = rowMaxX * invscale;
            }

            rowsSpan = _textRows.AsReadOnlySpan(textRowsOffset, nrows);
        }

        #endregion

        #region _flushTextTexture

        // nvg__flushTextTexture
        static void _flushTextTexture()
        {
            if (_validateAtlasTexture())
            {
                if (_atlasTexture != Globals.INVALID)
                {
                    int x = _updateTextureRect[0];
                    int y = _updateTextureRect[1];
                    int w = _updateTextureRect[2] - _updateTextureRect[0];
                    int h = _updateTextureRect[3] - _updateTextureRect[1];

                    // todo: slice data, so we can only update dirty rect data area
                    _nvgRenderer.UpdateTextureRegion(_atlasTexture, new Vector4(x, y, w, h), _atlasData);
                }
            }
        }

        #endregion
    }
}
