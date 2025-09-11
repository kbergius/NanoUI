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
        public UIText()
        {

        }

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

        public string? Text { get; set; }
        
        float? _fontSize;
        public override float FontSize
        {
            get => _fontSize?? GetTheme().Widget.FontSize;
            set => _fontSize = value;
        }

        // note: this is additional spacing (default is 0)
        float? _charSpacing;
        public float CharSpacing
        {
            get => _charSpacing?? 0;
            set => _charSpacing = value;
        }

        // note: this is proportional line height (default is 1)
        float? _lineHeight;
        public float LineHeight
        {
            get => _lineHeight ?? 1;
            set => _lineHeight = value;
        }

        TextAlignment? _alignment;
        public TextAlignment Alignment
        {
            get => _alignment?? TextAlignment.Left | TextAlignment.Middle;
            set => _alignment = value;
        }

        Color? _textColor;
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
