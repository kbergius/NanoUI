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
        /// <returns>font atlas texture id</returns>
        public int GetAtlasTextureId()
        {
            return Fontstash.GetAtlasTextureId();
        }

        /// <summary>
        /// Gets font atlas size.
        /// </summary>
        /// <param name="width">Width</param>
        /// <param name="height">Height</param>
        public void GetAtlasSize(out int width, out int height)
        {
            Fontstash.GetAtlasSize(out width, out height);
        }

        /// <summary>
        /// Returns font atlas texture data.
        /// </summary>
        /// <param name="width">Width</param>
        /// <param name="height">Height</param>
        /// <returns>Atlas data</returns>
        /// <remarks>Data is in 8-bit format.</remarks>
        public ReadOnlySpan<byte> GetAtlasData(out int width, out int height)
        {
            return Fontstash.GetTextureData(out width, out height);
        }

        /// <summary>
        /// Expands font atlas texture size.
        /// </summary>
        /// <param name="width">Width</param>
        /// <param name="height">Height</param>
        public bool AtlasExpand(uint width, uint height)
        {
            return Fontstash.ExpandAtlas((int)width, (int)height);
        }

        /// <summary>
        /// Resets (clears) the whole font atlas data.
        /// </summary>
        /// <param name="width">Width</param>
        /// <param name="height">Height</param>
        public bool AtlasReset(uint width, uint height)
        {
            return Fontstash.ResetAtlas((int)width, (int)height);
        }

        #endregion

        #region Fonts

        /// <summary>
        /// Creates font by loading it from the disk from specified filename.
        /// </summary>
        /// <param name="name">Name</param>
        /// <param name="path">Path</param>
        /// <param name="fontBaking">GlyphBaking</param>
        /// <param name="fontCollectionIndex">Font collection index</param>
        /// <returns>Id of the font.</returns>
        /// <remarks>
        /// This checks, that your path is in normal filesystem (System.IO.File.Exists).
        /// If you use some other filesystem solution, pass byte array.
        /// </remarks>
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
        /// <param name="name">Name</param>
        /// <param name="data">Data</param>
        /// <param name="fontBaking">GlyphBaking</param>
        /// <param name="fontCollectionIndex">Font collection index</param>
        /// <returns>Id of the font.</returns>
        public int CreateFont(ReadOnlySpan<char> name, ReadOnlySpan<byte> data, GlyphBaking fontBaking = GlyphBaking.SDF, int fontCollectionIndex = 0)
        {
            return Fontstash.AddFont(name, data, fontBaking, fontCollectionIndex);
        }

        /// <summary>
        /// Finds a loaded font from specified name.
        /// </summary>
        /// <param name="name">Name</param>
        /// <returns>Id to it, or -1 if the font is not found.</returns>
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
        /// <param name="fontSize">Font size</param>
        public void FontSize(float fontSize)
        {
            GetState().FontSize = fontSize;
        }

        /// <summary>
        /// Sets the char spacing of current text style.
        /// </summary>
        /// <param name="spacing">Text char spacing</param>
        /// <remarks>This is additional spacing (default is 0)</remarks>
        public void TextCharSpacing(float spacing)
        {
            GetState().TextCharSpacing = spacing;
        }

        /// <summary>
        /// Sets the proportinal line height of current text style.
        /// The line height is specified as multiple of font size.
        /// </summary>
        /// <param name="lineHeight">Text line height</param>
        /// <remarks>This is PROPORTIONAL line height (default is 1)</remarks>
        public void TextLineHeight(float lineHeight)
        {
            GetState().TextLineHeight = lineHeight;
        }

        /// <summary>
        /// Sets the text align of current text style.
        /// </summary>
        /// <param name="horizontal">Text horizontal alignment</param>
        /// <param name="vertical">Text vertical alignment</param>
        public void TextAlign(TextHorizontalAlign horizontal, TextVerticalAlign vertical)
        {
            TextAlign(ConvertUtils.ConvertTextAlign(horizontal, vertical));
        }

        /// <summary>
        /// Sets the text align of current text style.
        /// </summary>
        /// <param name="align">Text horizontal & vertical alignment</param>
        public void TextAlign(TextAlignment align)
        {
            GetState().TextAlign = align;
        }

        /// <summary>
        /// Sets the font face based on specified id of current text style.
        /// </summary>
        /// <param name="font">Font id</param>
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
        /// <param name="color">Text color</param>
        public void TextColor(in Color color)
        {
            GetState().Fill.Reset(color);
        }

        /// <summary>
        /// Resets the text's inner and outer color of current text style.
        /// </summary>
        /// <param name="innerColor">Text innerColor</param>
        /// <param name="outerColor">Text outerColor</param>
        public void TextColor(in Color innerColor, in Color outerColor)
        {
            GetState().Fill.Reset(innerColor, outerColor);
        }

        /// <summary>
        /// Sets the blur of current text style. Only with GlypBaking.Normal.
        /// </summary>
        /// <param name="val">Text blur value</param>
        /// <remarks>Value must be in range [0 .. 20]</remarks>
        public void TextNormalBlur(int val)
        {
            GetState().TextBlur = Math.Clamp(val, 0, 20);
        }

        /// <summary>
        /// Sets the text outer color's dimension (outer color = outline color).
        /// Only with GlypBaking.Normal.
        /// </summary>
        /// <param name="val">Text dilate value</param>
        /// <remarks>When you want to set normal text fill color, reset dilate value to 0.</remarks>
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
        /// </summary>
        /// <param name="color">Text shape outline color</param>
        /// <param name="outlineWidth">Text outline width</param>
        /// <remarks>
        /// If you don't want outline, set color to null or outlineWidth <= 0.
        /// Default value is no outline color.
        /// </remarks>
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
        /// <param name="color">Text shape fill paint</param>
        public void TextShapeFill(Paint? paint)
        {
            GetState().TextShapeFill = paint;
        }

        #endregion

        #region Text

        /// <summary>
        /// Draws icon at specified location (icons should be in TTF file).
        /// </summary>
        /// <param name="x">Left position</param>
        /// <param name="y">Top position</param>
        /// <param name="icon">Icon id</param>
        /// <returns>X position after icon is drawn.</returns>
        public float Text(float x, float y, int icon)
            => Text(new Vector2(x, y), ConvertUtils.GetIconString(icon));

        /// <summary>
        /// Draws icon at specified location (icons should be in TTF file).
        /// </summary>
        /// <param name="pos">TopLeft position</param>
        /// <param name="icon">Icon id</param>
        /// <returns>X position after icon is drawn.</returns>
        public float Text(Vector2 pos, int icon)
            => Text(pos, ConvertUtils.GetIconString(icon));

        /// <summary>
        /// Draws text at specified location.
        /// </summary>
        /// <param name="x">Left position</param>
        /// <param name="y">Top position</param>
        /// <param name="text">Text</param>
        /// <returns>X position after text is drawn.</returns>
        public float Text(float x, float y, ReadOnlySpan<char> text)
            => Text(new Vector2(x, y), text);

        /// <summary>
        /// Draws text at specified location.
        /// </summary>
        /// <param name="pos">TopLeft position</param>
        /// <param name="text">Text</param>
        /// <returns>X position after text is drawn.</returns>
        public float Text(Vector2 pos, ReadOnlySpan<char> text)
        {
            // note: we must pass NvgContext in case glyphs are "baked" in shapes and we
            // need access tos shapes API
            return Fontstash.DrawText(this, ref GetState(), _nvgParams, pos, text);
        }

        #endregion

        #region TextBounds

        /// <summary>
        /// Measures the specified icon. Parameter bounds contains the bounds of the icon.<br/>
        /// Measured values are returned in local coordinate space.
        /// </summary>
        /// <param name="x">Left position</param>
        /// <param name="y">Top position</param>
        /// <param name="icon">Icon id</param>
        /// <param name="bounds">Contains the bounds of the icon when returned.</param>
        /// <returns>The horizontal advance of the measured icon (i.e. where the next character should be drawn).</returns>
        public float TextBounds(float x, float y, int icon, out Rect bounds)
            => TextBounds(new Vector2(x, y), ConvertUtils.GetIconString(icon), out bounds);

        /// <summary>
        /// Measures the specified icon. Parameter bounds contains the bounds of the icon.<br/>
        /// Measured values are returned in local coordinate space.
        /// </summary>
        /// <param name="pos">TopLeft position</param>
        /// <param name="icon">Icon id</param>
        /// <param name="bounds">Contains the bounds of the icon when returned.</param>
        /// <returns>The horizontal advance of the measured icon (i.e. where the next character should be drawn).</returns>
        public float TextBounds(Vector2 pos, int icon, out Rect bounds)
            => TextBounds(pos, ConvertUtils.GetIconString(icon), out bounds);

        /// <summary>
        /// Measures the specified text. Parameter bounds contains the bounds of the text.<br/>
        /// Measured values are returned in local coordinate space.
        /// </summary>
        /// <param name="x">Left position</param>
        /// <param name="y">Top position</param>
        /// <param name="text">Text</param>
        /// <param name="bounds">Contains the bounds of the text when returned.</param>
        /// <returns>The horizontal advance of the measured text (i.e. where the next character should be drawn).</returns>
        public float TextBounds(float x, float y, ReadOnlySpan<char> text, out Rect bounds)
            => TextBounds(new Vector2(x, y), text, out bounds);

        /// <summary>
        /// Measures the specified text. Parameter bounds contains the bounds of the text.<br/>
        /// Measured values are returned in local coordinate space.
        /// </summary>
        /// <param name="pos">TopLeft position</param>
        /// <param name="text">Text</param>
        /// <param name="bounds">Contains the bounds of the text when returned.</param>
        /// <returns>The horizontal advance of the measured text (i.e. where the next character should be drawn).</returns>
        public float TextBounds(Vector2 pos, ReadOnlySpan<char> text, out Rect bounds)
        {
            return Fontstash.TextBounds(GetState(), _nvgParams, pos, text, out bounds);
        }

        #endregion

        #region TextBox

        /// <summary>
        /// Draws multi-line text at specified location wrapped at the specified width.
        /// White space is stripped at the beginning of the rows, the text is split at word boundries or when new-line characters are encountered.
        /// Words longer than the max width are split at nearest character (i.e. no hyphenation).
        /// </summary>
        /// <param name="position">TopLeft position</param>
        /// <param name="breakRowWidth">Break row width</param>
        /// <param name="text">Text</param>
        /// <param name="maxRows">MaxRows</param>
        /// <returns>Success</returns>
        /// <remarks>
        /// It is not recommended to use this in production code, since this calculates text rows every frame.
        /// instead use TextBreakLines and cache TextRows.
        /// </remarks>
        public bool TextBox(Vector2 position, float breakRowWidth, ReadOnlySpan<char> text, int maxRows = int.MaxValue)
        {
            return TextBox(position.X, position.Y, breakRowWidth, text, maxRows);
        }

        /// <summary>
        /// Draws multi-line text at specified location wrapped at the specified width.
        /// White space is stripped at the beginning of the rows, the text is split at word boundries or when new-line characters are encountered.
        /// Words longer than the max width are split at nearest character (i.e. no hyphenation).
        /// </summary>
        /// <param name="x">Left position</param>
        /// <param name="y">Top position</param>
        /// <param name="breakRowWidth">Break row width</param>
        /// <param name="text">Text</param>
        /// <param name="maxRows">MaxRows</param>
        /// <returns>Success</returns>
        /// <remarks>
        /// It is not recommended to use this in production code, since this calculates text rows every frame.
        /// instead use TextBreakLines and cache TextRows.
        /// </remarks>
        public bool TextBox(float x, float y, float breakRowWidth, ReadOnlySpan<char> text, int maxRows = int.MaxValue)
        {
            // note: we must pass NvgContext in case glyphs are "baked" in shapes and we
            // need access tos shapes API
            return Fontstash.TextBox(this, ref GetState(), _nvgParams, x, y, breakRowWidth, text, maxRows);
        }

        #endregion

        #region TextBoxBounds

        /// <summary>
        /// Measures the specified multi-text.<br/>
        /// Measured values are returned in local space.
        /// </summary>
        /// <param name="x">Left position</param>
        /// <param name="y">Top position</param>
        /// <param name="breakRowWidth">Break row width</param>
        /// <param name="text">Text</param>
        /// <param name="bounds">Contains the bounds box of the multi-text when returned.</param>
        public void TextBoxBounds(float x, float y, float breakRowWidth, ReadOnlySpan<char> text, out Rect bounds)
            => TextBoxBounds(new Vector2(x, y), breakRowWidth, text, Globals.MAX_TEXT_ROWS, out bounds);

        /// <summary>
        /// Measures the specified multi-text.<br/>
        /// Measured values are returned in local space.
        /// </summary>
        /// <param name="x">Left position</param>
        /// <param name="y">Top position</param>
        /// <param name="breakRowWidth">Break row width</param>
        /// <param name="text">Text</param>
        /// <param name="maxRows">MaxRows</param>
        /// <param name="bounds">Contains the bounds box of the multi-text when returned.</param>
        public void TextBoxBounds(float x, float y, float breakRowWidth, ReadOnlySpan<char> text, int maxRows, out Rect bounds)
            => TextBoxBounds(new Vector2(x, y), breakRowWidth, text, maxRows, out bounds);

        /// <summary>
        /// Measures the specified multi-text.<br/>
        /// Measured values are returned in local space.
        /// </summary>
        /// <param name="pos">TopLeft position</param>
        /// <param name="breakRowWidth">Break row width</param>
        /// <param name="text">Text</param>
        /// <param name="bounds">Contains the bounds box of the multi-text when returned.</param>
        public void TextBoxBounds(Vector2 pos, float breakRowWidth, ReadOnlySpan<char> text, out Rect bounds)
        {
            TextBoxBounds(pos, breakRowWidth, text, Globals.MAX_TEXT_ROWS, out bounds);
        }

        /// <summary>
        /// Measures the specified multi-text.<br/>
        /// Measured values are returned in local space.
        /// </summary>
        /// <param name="pos">TopLeft position</param>
        /// <param name="breakRowWidth">Break row width</param>
        /// <param name="text">Text</param>
        /// <param name="maxRows">MaxRows</param>
        /// <param name="bounds">Contains the bounds box of the multi-text when returned.</param>
        public void TextBoxBounds(Vector2 pos, float breakRowWidth, ReadOnlySpan<char> text, int maxRows, out Rect bounds)
        {
            Fontstash.TextBoxBounds(ref GetState(), _nvgParams, pos, breakRowWidth, text, maxRows, out bounds);
        }

        #endregion

        #region TextGlyphPositions

        /// <summary>
        /// Calculates the glyph x positions of the specified text.<br/>
        /// Measures values are returned in local coordinate space.
        /// </summary>
        /// <param name="x">Left position</param>
        /// <param name="y">Top position</param>
        /// <param name="text">Text</param>
        /// <param name="maxGlyphs">MaxGlyphs</param>
        /// <param name="positions">Glyph positions</param>
        public void TextGlyphPositions(float x, float y, ReadOnlySpan<char> text, int maxGlyphs,
            out ReadOnlySpan<GlyphPosition> positions)
           => TextGlyphPositions(new Vector2(x, y), text, maxGlyphs, out positions);

        /// <summary>
        /// Calculates the glyph x positions of the specified text.<br/>
        /// Measures values are returned in local coordinate space.
        /// </summary>
        /// <param name="pos">TopLeft position</param>
        /// <param name="text">Text</param>
        /// <param name="maxGlyphs">MaxGlyphs</param>
        /// <param name="positions">Glyph positions</param>
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
        /// <param name="ascender">Ascender</param>
        /// <param name="descender">Descender</param>
        /// <param name="lineh">Line height</param>
        public void TextMetrics(out float ascender, out float descender, out float lineh)
        {
            Fontstash.TextMetrics(GetState(), _nvgParams, out ascender, out descender, out lineh);
        }

        #endregion

        #region TextBreakLines

        /// <summary>
        /// Breaks the specified text into lines.<br/>
        /// White space is stripped at the beginning of the rows, the text is split at word boundaries or when new-line characters are encountered.<br/>
        /// Words longer than the max width are slit at nearest character (i.e. no hyphenation).
        /// </summary>
        /// <param name="text">Text</param>
        /// <param name="breakRowWidth">Break row width</param>
        /// <param name="rows">Text rows</param>
        public void TextBreakLines(ReadOnlySpan<char> text, float breakRowWidth, out ReadOnlySpan<TextRow> rows)
        {
            TextBreakLines(text, breakRowWidth, Globals.MAX_TEXT_ROWS, out rows);
        }

        /// <summary>
        /// Breaks the specified text into lines.<br/>
        /// White space is stripped at the beginning of the rows, the text is split at word boundaries or when new-line characters are encountered.<br/>
        /// Words longer than the max width are slit at nearest character (i.e. no hyphenation).
        /// </summary>
        /// <param name="text">Text</param>
        /// <param name="breakRowWidth">Break row width</param>
        /// <param name="maxRows">MaxRows</param>
        /// <param name="rows">Text rows</param>
        public void TextBreakLines(ReadOnlySpan<char> text, float breakRowWidth, int maxRows, out ReadOnlySpan<TextRow> rows)
        {
            Fontstash.TextBreakLines(GetState(), _nvgParams, text, breakRowWidth, maxRows, out rows);
        }

        #endregion
    }
}
