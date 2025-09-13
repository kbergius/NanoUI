using NanoUI.Common;
using NanoUI.Nvg;
using System.Numerics;
using System.Text.Json.Serialization;

namespace NanoUI.Components.Simple
{
    /// <summary>
    /// UIIcon.
    /// </summary>
    public class UIIcon : UIWidget
    {
        /// <inheritdoc />
        public UIIcon()
        {

        }

        public UIIcon(int icon)
        {
            Icon = icon;
        }

        /// <inheritdoc />
        public UIIcon(UIWidget parent)
            : base(parent)
        {

        }

        #region Properties

        // -1 means no icon
        public virtual int Icon { get; set; }

        float? _iconSize;
        public float IconSize
        {
            get => _iconSize?? GetTheme().Widget.FontSize;
            set => _iconSize = value;
        }

        // todo: in base Widget
        int? _fontIconsId;

        /// <inheritdoc />
        [JsonIgnore]
        public override int FontIconsId
        {
            get =>_fontIconsId?? GetTheme().Fonts.GetFontId(GetTheme().Fonts.GetDefaultIconType());
            set => _fontIconsId = value;
        }

        TextAlignment? _alignment;
        public TextAlignment Alignment
        {
            get => _alignment ?? TextAlignment.Left | TextAlignment.Middle;
            set => _alignment = value;
        }

        // todo: should this be in Widget?
        Color? _iconColor;
        public virtual Color IconColor
        {
            get => _iconColor?? GetTheme().Widget.TextColor;
            set => _iconColor = value;
        }

        #endregion

        #region Drawing

        /// <inheritdoc />
        public override void Draw(NvgContext ctx)
        {
            if (Icon <= 0)
                return;

            ctx.FontSize(IconSize);
            ctx.FontFaceId(FontIconsId);

            Vector2 center = Position + Size * 0.5f;

            ctx.FillColor(IconColor);

            ctx.TextAlign(Alignment);

            ctx.Text(Position.X, center.Y, Icon);
        }

        #endregion
    }
}
