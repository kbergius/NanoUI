using NanoUI.Common;
using NanoUI.Fonts;
using NanoUI.Nvg.Data;
using NanoUI.Utils;
using System;
using System.IO;
using System.Numerics;

namespace NanoUI.Nvg
{
    public partial class NvgContext
    {
        #region Atlas

        /// <summary>
        /// Returns font atlas texture id.
        /// </summary>
        public int GetAtlasTextureId()
        {
            return Fontstash.GetAtlasTextureId();
        }

        /// <summary>
        /// Returns current font atlas texture size.
        /// </summary>
        public void GetAtlasSize(out int width, out int height)
        {
            Fontstash.GetAtlasSize(out width, out height);
        }

        /// <summary>
        /// Returns current font atlas texture data.
        /// Note: Data is in 8-bit format.
        /// </summary>
        public ReadOnlySpan<byte> GetAtlasData(out int width, out int height)
        {
            return Fontstash.GetTextureData(out width, out height);
        }

        /// <summary>
        /// Expands the font atlas texture size.
        /// </summary>
        public bool AtlasExpand(uint width, uint height)
        {
            return Fontstash.ExpandAtlas((int)width, (int)height);
        }

        /// <summary>
        /// Resets (clears) the whole font atlas data.
        /// </summary>
        public bool AtlasReset(uint width, uint height)
        {
            return Fontstash.ResetAtlas((int)width, (int)height);
        }

        #endregion

        #region Fonts

        /// <summary>
        /// Creates font by loading it from the disk from specified filename.
        /// Note: this checks, that your path is in "normal" filesystem (System.IO.File.Exists).
        /// If you use some other filesystem solution, pass byte array.
        /// </summary>
        /// <returns>Handle to the font.</returns>
        public int CreateFont(ReadOnlySpan<char> name, string path, GlyphBaking fontBaking = GlyphBaking.SDF, int fontCollectionIndex = 0)
        {
            if (!File.Exists(path))
            {
                // todo: shoulkd we throw error?
                return Globals.INVALID;
            }

            return CreateFont(name, File.ReadAllBytes(path), fontBaking, fontCollectionIndex);
        }

        /// <summary>
        /// Creates a font by loading it from the specified memory chunk.
        /// </summary>
        /// <returns>Handle to the font.</returns>
        public int CreateFont(ReadOnlySpan<char> name, ReadOnlySpan<byte> data, GlyphBaking fontBaking = GlyphBaking.SDF, int fontCollectionIndex = 0)
        {
            return Fontstash.AddFont(name, data, fontBaking, fontCollectionIndex);
        }

        /// <summary>
        /// Finds a loaded font from specified name.
        /// </summary>
        /// <returns>Handle to it, or -1 if the font is not found.</returns>
        public int GetFontByName(ReadOnlySpan<char> name)
        {
            if (name.IsEmpty)
            {
                return Globals.INVALID;
            }

            return Fontstash.GetFontByName(name);
        }

        #endregion

        #region RenderStyles

        /// <summary>
        /// Sets the font size of current text style.
        /// </summary>
        public void FontSize(float fontSize)
        {
            GetState().FontSize = fontSize;
        }

        /// <summary>
        /// Sets the char spacing of current text style.
        /// Note: this is additional spacing (default is 0)
        /// </summary>
        public void TextCharSpacing(float spacing)
        {
            GetState().TextCharSpacing = spacing;
        }

        /// <summary>
        /// Sets the proportinal line height of current text style. The line height is specified as multiple of font size.
        /// Note: this is PROPORTIONAL line height (default is 1)
        /// </summary>
        public void TextLineHeight(float lineHeight)
        {
            GetState().TextLineHeight = lineHeight;
        }

        public float TextHeight(float x, float y, ReadOnlySpan<char> text)
        {
            // todo: current alignment?
            TextAlign(TextAlignment.Left | TextAlignment.Top);
            TextBounds(x, y, text, out var bounds);

            return bounds.Height;
        }

        /// <summary>
        /// Sets the text align of current text style.
        /// </summary>
        public void TextAlign(TextHorizontalAlign horizontal, TextVerticalAlign vertical)
        {
            TextAlign(ConvertUtils.ConvertTextAlign(horizontal, vertical));
        }

        /// <summary>
        /// Sets the text align of current text style.
        /// </summary>
        public void TextAlign(TextAlignment align)
        {
            GetState().TextAlign = align;
        }

        /// <summary>
        /// Sets the font face based on specified id of current text style.
        /// </summary>
        public void FontFaceId(int font)
        {
            if (font == Globals.INVALID)
            {
                // todo : log error
                return;
            }

            GetState().FontId = font;
        }

        // Bitmaps

        /// <summary>
        /// Sets the text color of current text style.
        /// </summary>
        public void TextColor(in Color color)
        {
            GetState().Fill.Reset(color);
        }

        /// <summary>
        /// Sets the text's inner and outer color of current text style.
        /// This could be used with text effect outline (outer color = outline color).
        /// </summary>
        public void TextColor(in Color innerColor, in Color outerColor)
        {
            GetState().Fill.Reset(innerColor, outerColor);
        }

        /// <summary>
        /// Sets the blur of current text style. Only with GlypBaking.Normal.
        /// </summary>
        public void TextNormalBlur(int val)
        {
            GetState().TextBlur = Math.Clamp(val, 0, 20);
        }

        public void TextNormalDilate(int val)
        {
            GetState().TextDilate = Math.Clamp(val, 0, 20);
        }

        // SDF baking

        // note: there are currently no extra SDF params passed to the fragment/pixel shader to make
        // text effects, but they can be added here & passed to shader

        // Shapes baking

        /// <summary>
        /// Sets the text shape's inner and outer color.
        /// This could be used with text effect outline (outer color = outline color) with
        /// text shapes.
        /// Note: if you don't want outline, set color to null or outlineWidth <= 0.
        /// Default value is no outline color.
        /// </summary>
        public void TextShapeOutline(in Color? color, float outlineWidth = 1)
        {
            ref NvgState state = ref GetState();

            state.TextShapeOutline = color;
            state.TextShapeOutlineWidth = outlineWidth;
        }

        /// <summary>
        /// Sets the text shape's fiil paint.
        /// If you don't want fill, set paint to null. Default value is no fill.
        /// </summary>
        public void TextShapeFill(Paint? paint)
        {
            GetState().TextShapeFill = paint;
        }

        #endregion

        #region Text

        /// <summary>
        /// Draws icon at specified location (icons should be in TTF file).
        /// Note: Returns x position after icon drawn.
        /// </summary>
        public float Text(float x, float y, int icon)
            => Text(new Vector2(x, y), ConvertUtils.GetIconString(icon));

        /// <summary>
        /// Draws icon at specified location (icons should be in TTF file).
        /// Note: Returns x position after icon drawn.
        /// </summary>
        public float Text(Vector2 pos, int icon)
            => Text(pos, ConvertUtils.GetIconString(icon));

        /// <summary>
        /// Draws text string at specified location. Only the sub-string up to the end is drawn.
        /// Note: Returns x position after text drawn.
        /// </summary>
        public float Text(float x, float y, ReadOnlySpan<char> text)
            => Text(new Vector2(x, y), text);

        /// <summary>
        /// Draws text string at specified location. Only the sub-string up to the end is drawn.
        /// Note: Returns x position after text drawn.
        /// </summary>
        public float Text(Vector2 pos, ReadOnlySpan<char> text)
        {
            // note: we must pass NvgContext in case glyphs are "baked" in shapes and we
            // need access tos shapes API
            return Fontstash.DrawText(this, ref GetState(), _nvgParams, pos, text);
        }

        #endregion

        #region TextBounds

        /// <summary>
        /// Measures the specified text string. Parameter bounds contains the bounds of the text.<br/>
        /// Measured values are returned in local coordinate space.
        /// </summary>
        /// <param name="bounds">Contains the bounds of the text when returned.</param>
        /// <returns>The horizontal advance of the measured text (i.e. where the next character should be drawn).</returns>
        public float TextBounds(float x, float y, int icon, out Rect bounds)
            => TextBounds(new Vector2(x, y), ConvertUtils.GetIconString(icon), out bounds);

        /// <summary>
        /// Measures the specified text string. Parameter bounds contains the bounds of the text.<br/>
        /// Measured values are returned in local coordinate space.
        /// </summary>
        /// <param name="bounds">Contains the bounds of the text when returned.</param>
        /// <returns>The horizontal advance of the measured text (i.e. where the next character should be drawn).</returns>
        public float TextBounds(Vector2 pos, int icon, out Rect bounds)
            => TextBounds(pos, ConvertUtils.GetIconString(icon), out bounds);

        /// <summary>
        /// Measures the specified text string. Parameter bounds contains the bounds of the text.<br/>
        /// Measured values are returned in local coordinate space.
        /// </summary>
        /// <param name="bounds">Contains the bounds of the text when returned.</param>
        /// <returns>The horizontal advance of the measured text (i.e. where the next character should be drawn).</returns>
        public float TextBounds(float x, float y, ReadOnlySpan<char> text, out Rect bounds)
            => TextBounds(new Vector2(x, y), text, out bounds);

        /// <summary>
        /// Measures the specified text string. Parameter bounds contains the bounds of the text.<br/>
        /// Measured values are returned in local coordinate space.
        /// </summary>
        /// <param name="bounds">Contains the bounds of the text when returned.</param>
        /// <returns>The horizontal advance of the measured text (i.e. where the next character should be drawn).</returns>
        public float TextBounds(Vector2 pos, ReadOnlySpan<char> text, out Rect bounds)
        {
            return Fontstash.TextBounds(GetState(), _nvgParams, pos, text, out bounds);
        }

        #endregion

        #region TextBox

        /// <summary>
        /// Draws multi-line text string at specified location wrapped at the specified width. Only the sub-string up to the end is drawn.
        /// White space is stripped at the beginning of the rows, the text is split at word boundries or when new-line characters are encountered.
        /// Words longer than the max width are slit at nearest character (i.e. no hyphenation).
        /// Note: it is not recommended to use this in production code, since this calculates text rows every frame.
        /// instead use TextBreakLines and cache TextRows.
        /// </summary>
        public bool TextBox(Vector2 position, float breakRowWidth, ReadOnlySpan<char> text, int maxRows = int.MaxValue)
        {
            return TextBox(position.X, position.Y, breakRowWidth, text, maxRows);
        }

        /// <summary>
        /// Draws multi-line text string at specified location wrapped at the specified width. Only the sub-string up to the end is drawn.
        /// White space is stripped at the beginning of the rows, the text is split at word boundries or when new-line characters are encountered.
        /// Words longer than the max width are slit at nearest character (i.e. no hyphenation).
        /// Note: it is not recommended to use this in production code, since this calculates text rows every frame.
        /// instead use TextBreakLines and cache TextRows.
        /// </summary>
        public bool TextBox(float x, float y, float breakRowWidth, ReadOnlySpan<char> text, int maxRows = int.MaxValue)
        {
            // note: we must pass NvgContext in case glyphs are "baked" in shapes and we
            // need access tos shapes API
            return Fontstash.TextBox(this, ref GetState(), _nvgParams, x, y, breakRowWidth, text, maxRows);
        }

        #endregion

        #region TextBoxBounds

        /// <summary>
        /// Measures the specified multi-text string.<br/>
        /// Measured values are returned in local space.
        /// </summary>
        /// <param name="bounds">Contains the bounds box of the multi-text when returned.</param>
        public void TextBoxBounds(float x, float y, float breakRowWidth, ReadOnlySpan<char> text, out Rect bounds)
            => TextBoxBounds(new Vector2(x, y), breakRowWidth, text, Globals.MAX_TEXT_ROWS, out bounds);

        /// <summary>
        /// Measures the specified multi-text string.<br/>
        /// Measured values are returned in local space.
        /// </summary>
        /// <param name="bounds">Contains the bounds box of the multi-text when returned.</param>
        public void TextBoxBounds(float x, float y, float breakRowWidth, ReadOnlySpan<char> text, int maxRows, out Rect bounds)
            => TextBoxBounds(new Vector2(x, y), breakRowWidth, text, maxRows, out bounds);

        /// <summary>
        /// Measures the specified multi-text string.<br/>
        /// Measured values are returned in local space.
        /// </summary>
        /// <param name="bounds">Contains the bounds box of the multi-text when returned.</param>
        public void TextBoxBounds(Vector2 pos, float breakRowWidth, ReadOnlySpan<char> text, out Rect bounds)
        {
            TextBoxBounds(pos, breakRowWidth, text, Globals.MAX_TEXT_ROWS, out bounds);
        }

        /// <summary>
        /// Measures the specified multi-text string.<br/>
        /// Measured values are returned in local space.
        /// </summary>
        /// <param name="bounds">Contains the bounds box of the multi-text when returned.</param>
        public void TextBoxBounds(Vector2 pos, float breakRowWidth, ReadOnlySpan<char> text, int maxRows, out Rect bounds)
        {
            Fontstash.TextBoxBounds(ref GetState(), _nvgParams, pos, breakRowWidth, text, maxRows, out bounds);
        }

        #endregion

        #region TextGlyphPositions

        /// <summary>
        /// Calculates the glyph x positions of the specified text. Only the sub-string will be used.<br/>
        /// Measures values are returned in local coordinate space.
        /// </summary>
        public void TextGlyphPositions(float x, float y, ReadOnlySpan<char> text, int maxGlyphs,
            out ReadOnlySpan<GlyphPosition> positions)
           => TextGlyphPositions(new Vector2(x, y), text, maxGlyphs, out positions);

        /// <summary>
        /// Calculates the glyph x positions of the specified text. Only the sub-string will be used.<br/>
        /// Measures values are returned in local coordinate space.
        /// </summary>
        public void TextGlyphPositions(Vector2 pos, ReadOnlySpan<char> text, int maxGlyphs,
            out ReadOnlySpan<GlyphPosition> positions)
        {
            Fontstash.TextGlyphPositions(GetState(), _nvgParams, pos, text, maxGlyphs, out positions);
        }

        #endregion

        #region TextMetrics

        /// <summary>
        /// Returns the vertical metrics based on the current text style.<br/>
        /// Measured values are returned in local coordinate space.
        /// </summary>
        public void TextMetrics(out float ascender, out float descender, out float lineh)
        {
            Fontstash.TextMetrics(GetState(), _nvgParams, out ascender, out descender, out lineh);
        }

        #endregion

        #region TextBreakLines

        /// <summary>
        /// Breaks the specified text into lines. Only the sub-string will be used.<br/>
        /// White space is stripped at the beginning of the rows, the text is split at word boundaries or when new-line characters are encountered.<br/>
        /// Words longer than the max width are slit at nearest character (i.e. no hyphenation).
        /// </summary>
        public void TextBreakLines(ReadOnlySpan<char> text, float breakRowWidth, out ReadOnlySpan<TextRow> rows)
        {
            TextBreakLines(text, breakRowWidth, Globals.MAX_TEXT_ROWS, out rows);
        }

        /// <summary>
        /// Breaks the specified text into lines. Only the sub-string will be used.<br/>
        /// White space is stripped at the beginning of the rows, the text is split at word boundaries or when new-line characters are encountered.<br/>
        /// Words longer than the max width are slit at nearest character (i.e. no hyphenation).
        /// </summary>
        public void TextBreakLines(ReadOnlySpan<char> text, float breakRowWidth, int maxRows, out ReadOnlySpan<TextRow> rows)
        {
            Fontstash.TextBreakLines(GetState(), _nvgParams, text, breakRowWidth, maxRows, out rows);
        }

        #endregion
    }
}
