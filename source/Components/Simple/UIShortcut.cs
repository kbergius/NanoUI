using NanoUI.Common;
using NanoUI.Nvg;

namespace NanoUI.Components.Simple
{
    /// <summary>
    /// UIShortcut.
    /// </summary>
    public class UIShortcut : UIWidget
    {
        /// <inheritdoc />
        public UIShortcut()
        {

        }

        public UIShortcut(Shortcut shortcut)
            : this()
        {
            Shortcut = shortcut;
        }

        /// <inheritdoc />
        public UIShortcut(UIWidget parent)
            : base(parent)
        {

        }

        #region Properties

        float? _fontSize;

        /// <inheritdoc />
        public override float FontSize
        {
            get => _fontSize ?? GetTheme().Widget.FontSize;
            set => _fontSize = value;
        }

        TextAlignment? _alignment;

        /// <summary>
        /// Text alignment
        /// </summary>
        public TextAlignment Alignment
        {
            get => _alignment ?? TextAlignment.Left | TextAlignment.Middle;
            set => _alignment = value;
        }
        
        Color? _color;

        /// <summary>
        /// Color
        /// </summary>
        public virtual Color Color
        {
            get => _color ?? GetTheme().Widget.TextColor;
            set => _color = value;
        }

        /// <summary>
        /// Shortcut
        /// </summary>
        public Shortcut? Shortcut { get; set; }

        #endregion

        #region Drawing

        /// <inheritdoc />
        public override void Draw(NvgContext ctx)
        {
            if (!Shortcut.HasValue)
                return;

            ctx.FontSize(FontSize);
            ctx.FontFaceId(FontFaceId);

            ctx.TextAlign(Alignment);
            ctx.FillColor(Color);

            // note: top left
            ctx.Text(Position.X, Position.Y + Size.Y * 0.5f, Shortcut.Value.ToString());
        }

        #endregion
    }
}
