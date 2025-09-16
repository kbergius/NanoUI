using NanoUI.Common;
using NanoUI.Fonts.Data;
using System;
using System.Collections.Generic;

namespace NanoUI.Fonts
{
    // note: this is internal, because this has some unsafe structs/functions.
    // so functions in this class are only accessible through NvgContext
    // todo: there is no functions to remove fonts & glyphs
    // todo2: "fallback" fonts - if glyph not found in current font, try other font (possibility to
    // mix chars & icons in same text)
    // todo3: we could always use font0 as fallback font
    internal static partial class Fontstash
    {
        // Fonts & Glyphs
        // todo: should these be dictionaries so we can remove fonts?
        // note: problematic, since we would also need to clear atlas texture
        static Font[] _fonts = Array.Empty<Font>();
        static UnsafeBuffer<Glyph>[] _glyphs = Array.Empty<UnsafeBuffer<Glyph>>();

        // collect values in same frame (there can be many times these are used)
        static UnsafeBuffer<TextRow> _textRows = new(50);
        static UnsafeBuffer<GlyphPosition> _glyphPositions = new(300);

        // buffer for current text
        static UnsafeBuffer<char> _textChars = new(512);

        // this is the state
        static FontState _fontState = new();

        // these is only one used at time
        static GlyphQuad _quad;

        static IFontManager? _fontManager;
        static INvgRenderer? _nvgRenderer;

        // prevent multiple inits
        static bool _inited = false;

        // todo: unmanaged buffer/dictionary
        // <fontId, codepoint> - fontId point to _fonts
        
        // note: we don't need to store glyph commands with glyph index, since commands
        // distinct each other only by codepoint (not font size)
        static Dictionary<(int, uint), GlyphShapeCommand[]> _glyphShapes = new();

        public static void Init(INvgRenderer nvgRenderer, IFontManager fontManager)
        {
            if (_inited)
                return;

            _inited = true;

            _nvgRenderer = nvgRenderer;
            _fontManager = fontManager;

            // note: we could get these also as params
            int width = Globals.FONT_TEXTURE_WIDTH;
            int height = Globals.FONT_TEXTURE_HEIGHT;

            // Init atlas
            _allocAtlas(width, height);
        }

        public static void Clear()
        {
            _textRows.Clear();
            _glyphPositions.Clear();
        }

        #region Fonts

        // get individual font ids in case fonts are removed
        static int _fontCounter;

        public static int AddFont(ReadOnlySpan<char> name, ReadOnlySpan<byte> data, GlyphBaking fontBaking, int fontCollectionIndex)
        {
            if (_fontManager != null && _fontManager.Load(_fontCounter, data, fontCollectionIndex))
            {
                int fontId = _fontCounter;

                // get metrics
                _fontManager.GetFontVMetrics(fontId, out int asc, out int desc, out int lineG);

                asc += lineG;

                float fh = asc - desc;

                // create font
                Font font = new();
                font.Id = fontId;
                font.Name = name.ToString();
                font.FontBaking = fontBaking;

                font.Ascender = asc / fh;
                font.Descender = desc / fh;
                font.LineHeight = font.Ascender - font.Descender;
                //font.LineGap = lineG;

                // add font
                Array.Resize(ref _fonts, _fonts.Length + 1);
                _fonts[fontId] = font;

                // create glyphs
                Array.Resize(ref _glyphs, _glyphs.Length + 1);
                _glyphs[fontId] = new(256);

                // increase counter
                _fontCounter++;

                // font id
                return fontId;
            }

            return Globals.INVALID;
        }

        public static int GetFontByName(ReadOnlySpan<char> name)
        {
            for (int i = 0; i < _fonts.Length; i++)
            {
                if (_fonts[i].Name == name)
                    return i;
            }

            return Globals.INVALID;
        }

        #endregion

        #region Measure text

        static float[] _textBoxBounds = new float[4];
        static float TextBounds(in Font font, float x, float y, ReadOnlySpan<char> chars)
        {
            if (_fontManager == null || chars.IsEmpty)
            {
                _textBoxBounds[0] = 0;
                _textBoxBounds[1] = 0;
                _textBoxBounds[2] = 0;
                _textBoxBounds[3] = 0;
                //return 0;
                return x;
            }

            uint codepoint;
            int glyphIndex;
            int prevGlyphIndex = Globals.INVALID;

            // todo: this value is already calculated to fontstate
            float scale = _fontManager.GetPixelScale(font.Id, _fontState.FontSize);

            y += _getVertAlign(font, _fontState.FontSize, _fontState.TextAlign);

            float minx = x, maxx = x;
            float miny = y, maxy = y;
            float startx = x;

            // get the most used values in loop
            float fontSize = _fontState.FontSize;
            float charSpacing = _fontState.CharSpacing;
            GlyphBaking glyphBaking = _fontState.GlyphBaking;

            for (int i = 0; i < chars.Length; i++)
            {
                if (char.IsHighSurrogate(chars[i]) && i + 1 < _textChars.Count)
                {
                    // convert to Utf32 to support icons & non-ascii
                    codepoint = (uint)char.ConvertToUtf32(chars[i], chars[i + 1]);
                    i++;
                }
                else
                {
                    codepoint = chars[i];
                }

                if (_getGlyphIndex(font.Id, fontSize, codepoint, glyphBaking, FontGlyphBitmap.Optional, out glyphIndex))
                {
                    // set quad field
                    _setGlyphQuad(font.Id, fontSize, prevGlyphIndex, glyphIndex, scale, charSpacing, ref x, ref y);

                    if (_quad.x0 < minx)
                        minx = _quad.x0;

                    if (_quad.x1 > maxx)
                        maxx = _quad.x1;

                    if (_quad.y0 < miny)
                        miny = _quad.y0;

                    if (_quad.y1 > maxy)
                        maxy = _quad.y1;
                }

                prevGlyphIndex = glyphIndex;
            }

            float advance = x - startx;

            if ((_fontState.TextAlign & TextAlignment.Left) != 0)
            {
                // empty
            }
            else if ((_fontState.TextAlign & TextAlignment.Right) != 0)
            {
                minx -= advance;
                maxx -= advance;
            }
            else if ((_fontState.TextAlign & TextAlignment.Center) != 0)
            {
                minx -= advance * 0.5f;
                maxx -= advance * 0.5f;
            }

            _textBoxBounds[0] = minx;
            _textBoxBounds[1] = miny;
            _textBoxBounds[2] = maxx;
            _textBoxBounds[3] = maxy;

            return advance;
        }

        static void LineBounds(in Font font, float y, out float minY, out float maxY)
        {
            y += _getVertAlign(font, _fontState.FontSize, _fontState.TextAlign);

            minY = y - font.Ascender * _fontState.FontSize;
            maxY = minY + font.LineHeight * _fontState.FontSize;
        }

        static void VertMetrics(in Font font, out float ascender, out float descender, out float lineh)
        {
            ascender = font.Ascender * _fontState.FontSize;
            descender = font.Descender * _fontState.FontSize;
            lineh = font.LineHeight * _fontState.FontSize;
        }

        #endregion

        public static void Dispose()
        {
            // dispose fonts
            _fontManager?.Dispose();

            if(_atlasTexture != Globals.INVALID)
            {
                _nvgRenderer?.DeleteTexture(_atlasTexture);
            }
            
            // unmanaged buffers
            for (int i = 0; i < _glyphs.Length; i++)
            {
                _glyphs[i].Dispose();
            }

            _textRows.Dispose();
            _glyphPositions.Dispose();
            _textChars.Dispose();
            _atlasNodes.Dispose();
        }
    }
}
