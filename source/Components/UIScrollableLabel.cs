using NanoUI.Common;
using NanoUI.Components.Scrolling;
using NanoUI.Nvg;
using System.Numerics;

namespace NanoUI.Components
{
    // This is very simple widget that wraps label with vertical scrollbar

    /// <summary>
    /// UIScrollableLabel.
    /// </summary>
    public class UIScrollableLabel : UIWidget
    {
        UIScrollPanel _scroll;
        UILabel _label;

        /// <inheritdoc />
        public UIScrollableLabel()
        {
            _scroll = new();
            _label = new();
        }

        /// <inheritdoc />
        public UIScrollableLabel(UIWidget parent)
            : base(parent)
        {
            _scroll = new UIScrollPanel(this, ScrollbarType.Vertical);

            _label = new UILabel(_scroll);
            _label.WrapText = true;
            // todo:
            _label.FontType = base.FontType;
            _label.TextVerticalAlignment = TextVerticalAlign.Top;

            DisablePointerFocus = true;
        }

        #region Properties

        // we must override some properties, so we can pass them to Label

        /// <inheritdoc />
        public override bool Disabled
        {
            get => _label.Disabled;
            set
            {
                _label.Disabled = value;
                _scroll.Disabled = value;
            }
        }

        // todo: there could be other theme properties that should be synced!

        /// <inheritdoc />
        public override bool Border
        {
            get => base.Border;
            set
            {
                base.Border = value;
                // sync with scroll (needed in message boxes etc when hiding border)
                _scroll.Border = value;
            }
        }

        /// <inheritdoc />
        public override int FontFaceId
        {
            get => _label.FontFaceId;
            set => _label.FontFaceId = value;
        }

        /// <inheritdoc />
        public override float FontSize
        {
            get => _label.FontSize;
            set
            {
                _label.FontSize = value;

                // must recalculate layout, since label size changes
                RequestLayoutUpdate(_scroll);
            }
        }

        /// <inheritdoc />
        public override TextHorizontalAlign TextHorizontalAlignment
        {
            get => _label.TextHorizontalAlignment;
            set => _label.TextHorizontalAlignment = value;
        }

        /// <inheritdoc />
        public override TextVerticalAlign TextVerticalAlignment
        {
            get => _label.TextVerticalAlignment;
            set => _label.TextVerticalAlignment = value;
        }

        /// <inheritdoc />
        public override Color TextColor
        {
            get => _label.TextColor;
            set => _label.TextColor = value;
        }

        #endregion

        #region Methods

        // every time we set (new) text, we must update layout
        public void SetText(string text)
        {
            _label.Caption = text;

            RequestLayoutUpdate(_scroll);
        }

        #endregion

        #region Layout

        /// <inheritdoc />
        public override void PerformLayout(NvgContext ctx)
        {
            _scroll.FixedSize = Size;

            if(_scroll.VerticalScrollbar != null)
            {
                _label.FixedSize = new Vector2(Size.X - _scroll.VerticalScrollbar.Dimension, Size.Y);
            }
            
            base.PerformLayout(ctx);
        }

        #endregion

        #region Drawing

        /// <inheritdoc />
        public override void Draw(NvgContext ctx)
        {
            // background
            DrawBackgroundBrush(ctx);

            base.Draw(ctx);

            this.DrawBorder(ctx, true);
        }

        #endregion
    }
}
