using NanoUI.Common;
using NanoUI.Nvg;
using System.Numerics;

namespace NanoUI.Components.Simple
{
    // todo: remove this - use UIlabel

    /// <summary>
    /// UIText.
    /// </summary>
    public class UIText : UIWidget
    {
        /// <inheritdoc />
        public UIText()
        {

        }

        /// <inheritdoc />
        public UIText(string text)
            : this()
        {
            Text = text;
        }

        public UIText(UIWidget parent)
            : base(parent)
        {

        }

        #region Properties

        /// <summary>
        /// Text
        /// </summary>
        public string? Text { get; set; }
        
        float? _fontSize;

        /// <inheritdoc />
        public override float FontSize
        {
            get => _fontSize?? GetTheme().Widget.FontSize;
            set => _fontSize = value;
        }

        float? _charSpacing;

        /// <summary>
        /// Char spacing is additional spacing (default is 0).
        /// </summary>
        public float CharSpacing
        {
            get => _charSpacing?? 0;
            set => _charSpacing = value;
        }

        /// <summary>
        /// LineHeight is proportional line height (default is 1).
        /// </summary>
        float? _lineHeight;
        public float LineHeight
        {
            get => _lineHeight ?? 1;
            set => _lineHeight = value;
        }

        TextAlignment? _alignment;

        /// <summary>
        /// Text alignment
        /// </summary>
        public TextAlignment Alignment
        {
            get => _alignment?? TextAlignment.Left | TextAlignment.Middle;
            set => _alignment = value;
        }

        Color? _textColor;

        /// <inheritdoc />
        public override Color TextColor
        {
            get => _textColor?? GetTheme().Widget.TextColor;
            set => _textColor = value;
        }

        #endregion

        #region Drawing

        // todo : calculate correct alignment

        /// <inheritdoc />
        public override void Draw(NvgContext ctx)
        {
            if (string.IsNullOrEmpty(Text))
                return;

            // todo: alignment etc
            Vector2 textPos = Position + new Vector2(0, Size.Y * 0.5f);

            ctx.FontSize(FontSize);
            ctx.FontFaceId(FontFaceId);
            ctx.TextCharSpacing(CharSpacing);
            ctx.TextLineHeight(LineHeight);

            // todo: align correctly
            ctx.TextAlign(Alignment);
            ctx.FillColor(TextColor);

            // draw
            ctx.Text(textPos, Text);
        }

        #endregion
    }
}
