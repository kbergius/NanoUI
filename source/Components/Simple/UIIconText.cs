using NanoUI.Common;
using NanoUI.Nvg;
using System.Numerics;
using System.Text.Json.Serialization;

namespace NanoUI.Components.Simple
{
    // todo : make more generic, this uses some magical numbers & works only with values Vector2(100, 90);
    
    /// <summary>
    /// UIIconText is a combination of icon & text.
    /// Note: this is used by now in FileFolderFlow.
    /// </summary>
    public class UIIconText : UIWidget
    {
        /// <inheritdoc />
        public UIIconText()
        {

        }

        /// <inheritdoc />
        public UIIconText(UIWidget parent)
            : base(parent)
        {

        }

        #region Properties

        public int Icon { get; set; }
        
        // todo: in base Widget
        int? _fontIconsId;

        /// <inheritdoc />
        [JsonIgnore]
        public override int FontIconsId
        {
            get => _fontIconsId?? GetTheme().Fonts.GetFontId(GetTheme().Fonts.GetDefaultIconType());
            set => _fontIconsId = value;
        }

        TextAlignment? _iconAlignment;
        public TextAlignment IconAlignment
        {
            get => _iconAlignment?? TextAlignment.Left | TextAlignment.Middle;
            set => _iconAlignment = value;
        }

        // todo: should this be in Widget?
        Color? _iconColor;
        public virtual Color IconColor
        {
            get => _iconColor ?? GetTheme().Widget.TextColor;
            set => _iconColor = value;
        }
        public string? Text { get; set; }

        float? _fontSize;

        /// <inheritdoc />
        public override float FontSize
        {
            get => _fontSize?? GetTheme().Widget.FontSize;
            set => _fontSize = value;
        }
        
        TextAlignment? _alignment;
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

        /// <inheritdoc />
        public override void Draw(NvgContext ctx)
        {
            if (Icon > 0)
            {
                // icon size
                // todo:
                float ih = Size.X * 0.5f;

                ctx.FontSize(ih);
                ctx.FontFaceId(FontIconsId);

                // todo: make this configurable
                Vector2 iconPos = Position + new Vector2(10, 30);

                ctx.FillColor(IconColor);

                ctx.TextAlign(IconAlignment);

                ctx.Text(iconPos.X, iconPos.Y, Icon);
            }

            // Text
            if (string.IsNullOrEmpty(Text))
                return;

            // left padding & y from bottom
            // todo: magical numbers!
            Vector2 textPos = Position + new Vector2(10, Size.Y - FontSize * 1.2f);

            ctx.FontSize(FontSize);
            ctx.FontFaceId(FontFaceId);
            ctx.TextAlign(Alignment);
            ctx.FillColor(TextColor);

            ctx.Text(textPos.X, textPos.Y + 1, Text);
        }

        #endregion
    }
}
