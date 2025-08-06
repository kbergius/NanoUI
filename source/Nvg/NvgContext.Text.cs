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

        public int GetAtlasTextureId()
        {
            return Fontstash.GetAtlasTextureId();
        }

        // Returns current font atlas size.
        public void GetAtlasSize(out int width, out int height)
        {
            Fontstash.GetAtlasSize(out width, out height);
        }

        public ReadOnlySpan<byte> GetAtlasData(out int width, out int height)
        {
            return Fontstash.GetTextureData(out width, out height);
        }

        // Expands the atlas size
        public bool AtlasExpand(uint width, uint height)
        {
            return Fontstash.ExpandAtlas((int)width, (int)height);
        }

        // Resets the whole stash
        public bool AtlasReset(uint width, uint height)
        {
            return Fontstash.ResetAtlas((int)width, (int)height);
        }

        #endregion

        #region Fonts

        // nvgCreateFont
        // note: this checks, that your path is in "normal" filesystem (System.IO.File.Exists).
        // If you use some other filesystem solution, pass byte array.
        public int CreateFont(ReadOnlySpan<char> name, string path, GlyphBaking fontBaking = GlyphBaking.SDF, int fontCollectionIndex = 0)
        {
            if (!File.Exists(path))
            {
                // todo: shoulkd we throw error?
                return Globals.INVALID;
            }

            return CreateFont(name, File.ReadAllBytes(path), fontBaking, fontCollectionIndex);
        }

        // nvgCreateFontMem
        public int CreateFont(ReadOnlySpan<char> name, ReadOnlySpan<byte> data, GlyphBaking fontBaking = GlyphBaking.SDF, int fontCollectionIndex = 0)
        {
            return Fontstash.AddFont(name, data, fontBaking, fontCollectionIndex);
        }

        // nvgFindFont
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

        public void FontSize(float fontSize)
        {
            GetState().FontSize = fontSize;
        }

        // note: this is additional spacing (default is 0)
        public void TextCharSpacing(float spacing)
        {
            GetState().TextCharSpacing = spacing;
        }

        // note: this is proportional line height (default is 1)
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

        public void TextAlign(TextHorizontalAlign horizontal, TextVerticalAlign vertical)
        {
            TextAlign(ConvertUtils.ConvertTextAlign(horizontal, vertical));
        }

        public void TextAlign(TextAlignment align)
        {
            GetState().TextAlign = align;
        }

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

        // this is same as FillColor
        public void TextColor(in Color color)
        {
            GetState().Fill.Reset(color);
        }

        // this is used with text effect outline (outer color = outline color)
        public void TextColor(in Color innerColor, in Color outerColor)
        {
            GetState().Fill.Reset(innerColor, outerColor);
        }

        // Normal baking
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

        // note: if you don't want outline, set color to null or outlineWidth <= 0.
        // Default value is no outline color
        public void TextShapeOutline(in Color? color, float outlineWidth = 1)
        {
            ref NvgState state = ref GetState();

            state.TextShapeOutline = color;
            state.TextShapeOutlineWidth = outlineWidth;
        }

        // note: if you don't want fill, set paint to null. Default value is no fill
        public void TextShapeFill(Paint? paint)
        {
            GetState().TextShapeFill = paint;
        }

        #endregion

        #region Text

        // returns x position after text drawn
        // nvgText
        public float Text(float x, float y, int icon)
            => Text(new Vector2(x, y), ConvertUtils.GetIconString(icon));
        public float Text(Vector2 pos, int icon)
            => Text(pos, ConvertUtils.GetIconString(icon));

        public float Text(float x, float y, ReadOnlySpan<char> text)
            => Text(new Vector2(x, y), text);
        public float Text(Vector2 pos, ReadOnlySpan<char> text)
        {
            // note: we must pass NvgContext in case glyphs are "baked" in shapes and we
            // need access tos shapes API
            return Fontstash.DrawText(this, ref GetState(), _nvgParams, pos, text);
        }

        #endregion

        #region TextBounds

        public float TextBounds(float x, float y, int icon, out Rect bounds)
            => TextBounds(new Vector2(x, y), ConvertUtils.GetIconString(icon), out bounds);
        public float TextBounds(Vector2 pos, int icon, out Rect bounds)
            => TextBounds(pos, ConvertUtils.GetIconString(icon), out bounds);

        public float TextBounds(float x, float y, ReadOnlySpan<char> text, out Rect bounds)
            => TextBounds(new Vector2(x, y), text, out bounds);
        public float TextBounds(Vector2 pos, ReadOnlySpan<char> text, out Rect bounds)
        {
            return Fontstash.TextBounds(GetState(), _nvgParams, pos, text, out bounds);
        }

        #endregion

        #region TextBox

        // note: it is not recommended to use this in production code, since this calculates text rows every frame.
        // instead use TextBreakLines and cache TextRows.
        public bool TextBox(Vector2 position, float breakRowWidth, ReadOnlySpan<char> text, int maxRows = int.MaxValue)
        {
            return TextBox(position.X, position.Y, breakRowWidth, text, maxRows);
        }

        // note: it is not recommended to use this in production code, since this calculates text rows every frame.
        // instead use TextBreakLines and cache TextRows.
        public bool TextBox(float x, float y, float breakRowWidth, ReadOnlySpan<char> text, int maxRows = int.MaxValue)
        {
            // note: we must pass NvgContext in case glyphs are "baked" in shapes and we
            // need access tos shapes API
            return Fontstash.TextBox(this, ref GetState(), _nvgParams, x, y, breakRowWidth, text, maxRows);
        }

        #endregion

        #region TextBoxBounds

        public void TextBoxBounds(float x, float y, float breakRowWidth, ReadOnlySpan<char> text, out Rect bounds)
            => TextBoxBounds(new Vector2(x, y), breakRowWidth, text, Globals.MAX_TEXT_ROWS, out bounds);

        public void TextBoxBounds(float x, float y, float breakRowWidth, ReadOnlySpan<char> text, int maxRows, out Rect bounds)
            => TextBoxBounds(new Vector2(x, y), breakRowWidth, text, maxRows, out bounds);

        public void TextBoxBounds(Vector2 pos, float breakRowWidth, ReadOnlySpan<char> text, out Rect bounds)
        {
            TextBoxBounds(pos, breakRowWidth, text, Globals.MAX_TEXT_ROWS, out bounds);
        }

        public void TextBoxBounds(Vector2 pos, float breakRowWidth, ReadOnlySpan<char> text, int maxRows, out Rect bounds)
        {
            Fontstash.TextBoxBounds(ref GetState(), _nvgParams, pos, breakRowWidth, text, maxRows, out bounds);
        }

        #endregion

        #region TextGlyphPositions

        public void TextGlyphPositions(float x, float y, ReadOnlySpan<char> text, int maxGlyphs,
            out ReadOnlySpan<GlyphPosition> positions)
           => TextGlyphPositions(new Vector2(x, y), text, maxGlyphs, out positions);

        public void TextGlyphPositions(Vector2 pos, ReadOnlySpan<char> text, int maxGlyphs,
            out ReadOnlySpan<GlyphPosition> positions)
        {
            Fontstash.TextGlyphPositions(GetState(), _nvgParams, pos, text, maxGlyphs, out positions);
        }

        #endregion

        #region TextMetrics

        public void TextMetrics(out float ascender, out float descender, out float lineh)
        {
            Fontstash.TextMetrics(GetState(), _nvgParams, out ascender, out descender, out lineh);
        }

        #endregion

        #region TextBreakLines

        // Breaks the specified text into lines.<br/>
        // White space is stripped at the beginning of the rows, the text is split at word boundaries or when new-line characters are encountered.<br/>
        // Words longer than the max width are slit at nearest character (i.e. no hyphenation).
        public void TextBreakLines(ReadOnlySpan<char> text, float breakRowWidth, out ReadOnlySpan<TextRow> rows)
        {
            TextBreakLines(text, breakRowWidth, Globals.MAX_TEXT_ROWS, out rows);
        }

        public void TextBreakLines(ReadOnlySpan<char> text, float breakRowWidth, int maxRows, out ReadOnlySpan<TextRow> rows)
        {
            Fontstash.TextBreakLines(GetState(), _nvgParams, text, breakRowWidth, maxRows, out rows);
        }

        #endregion
    }
}
