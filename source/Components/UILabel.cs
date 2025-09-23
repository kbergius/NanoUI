using NanoUI.Common;
using NanoUI.Nvg;
using System;
using System.Numerics;

namespace NanoUI.Components
{
    /// <summary>
    /// UILabel.
    /// </summary>
    public class UILabel : UIWidget
    {
        // when caption or layout changes, we must recalculate lines
        bool _needRecalculate = true;

        string[] _lines = Array.Empty<string>();

        // this is used to supoort placing when WrapText = false (supports '\n' chars)
        float _offsetY = 0;

        TextAlignment _verticalAlign = TextAlignment.Middle;

        /// <inheritdoc />
        public UILabel()
        {
            // set defaults to theme impl - prevents circular reference

            // defaults
            _charSpacing = 0;
            _lineHeight = 1;
            _caption = string.Empty;
            DisablePointerFocus = true;
            Border = false;
        }

        /// <inheritdoc />
        public UILabel(UIWidget parent)
            : this(parent, string.Empty)
        {
        
        }

        /// <inheritdoc />
        public UILabel(UIWidget parent, string caption)
            : base(parent)
        {
            Caption = caption;
            // defaults
            DisablePointerFocus = true;
            Border = false;

            ThemeType = typeof(UILabel);
        }

        #region Properties

        string _caption = string.Empty;

        /// <summary>
        /// Caption.
        /// </summary>
        public string Caption
        {
            get => _caption;
            set
            {
                if(_caption == value)
                    return;
                
                _caption = value;
                _needRecalculate = true;
            }
        }

        bool _wrapText = false;

        /// <summary>
        /// Wrap text.
        /// </summary>
        public bool WrapText
        {
            get => _wrapText;
            set
            {
                if(_wrapText == value)
                    return;

                _wrapText = value;
                _verticalAlign = _wrapText ? TextAlignment.Top : TextAlignment.Middle;
            }
        }

        // used only when wrapping
        int? _maxTextRows;

        /// <summary>
        /// Max text rows
        /// </summary>
        public int MaxTextRows
        {
            get => _maxTextRows?? Globals.MAX_TEXT_ROWS;
            set => _maxTextRows = value;
        }

        Thickness? _padding;

        /// <summary>
        /// Padding
        /// </summary>
        public virtual Thickness Padding
        {
            get => _padding ?? GetTheme().Label.Padding;
            set => _padding = value;
        }

        float? _charSpacing;

        /// <summary>
        /// Additional spacing (default is 0).
        /// </summary>
        public float CharSpacing
        {
            get => _charSpacing ?? GetTheme().Label.CharSpacing;
            set => _charSpacing = value;
        }

        float? _lineHeight;

        /// <summary>
        /// Proportional line height (default is 1).
        /// </summary>
        public float LineHeight
        {
            get => _lineHeight ?? GetTheme().Label.LineHeight;
            set => _lineHeight = value;
        }

        #endregion

        #region Layout

        /// <inheritdoc />
        public override Vector2 PreferredSize(NvgContext ctx)
        {  
            if (string.IsNullOrEmpty(_caption))
                return Vector2.Max(MinSize, Vector2.Zero);

            ctx.FontFaceId(FontFaceId);
            ctx.FontSize(FontSize);
            ctx.TextCharSpacing(CharSpacing);
            ctx.TextLineHeight(LineHeight);
            // todo: check: we probably don't need to check horizontal alignment
            ctx.TextAlign(TextAlignment.Left | _verticalAlign);

            float x = FixedSize.X > 0 ? FixedSize.X : MathF.Max(MinSize.X, Size.X);

            if (WrapText)
            {
                ctx.TextBoxBounds(Position.X, Position.Y, x, _caption, out var rect);

                return Vector2.Max(MinSize, new Vector2(x, rect.Height) + 
                    new Vector2(Padding.Horizontal, Padding.Vertical));
            }
            else
            {
                // we do manual, forced wrapping with new line chars
                string[] split = Caption.Split(['\n'], StringSplitOptions.None);

                if (split.Length == 0)
                {
                    return Vector2.Max(MinSize, Vector2.Zero);
                }
                
                return Vector2.Max(MinSize, new Vector2(
                    ctx.TextBounds(0, 0, _caption, out _), split.Length * FontSize * LineHeight) +
                    new Vector2(Padding.Horizontal, Padding.Vertical));
            }
        }

        // todo: we could check if size really changed & set _needRecalculate flag accordingly

        /// <inheritdoc />
        public override void PerformLayout(NvgContext ctx)
        {
            base.PerformLayout(ctx);

            _needRecalculate = true;
        }

        #endregion

        #region Drawing

        /// <inheritdoc />
        public override void Draw(NvgContext ctx)
        {
            // background
            DrawBackgroundBrush(ctx);

            base.Draw(ctx);

            ctx.FontFaceId(FontFaceId);
            ctx.FontSize(FontSize);
            ctx.FillColor(TextColor);
            ctx.TextCharSpacing(CharSpacing);
            ctx.TextLineHeight(LineHeight);

            // recalculate lines if necessary
            RecalculateLines(ctx);

            // get the draw (text) position
            Vector2 drawPos = Position;
            drawPos.Y += _offsetY;

            float sizeX = FixedSize.X > 0? FixedSize.X : Size.X;

            switch (TextHorizontalAlignment)
            {
                case TextHorizontalAlign.Left:
                    ctx.TextAlign(TextAlignment.Left | _verticalAlign);
                    drawPos.X += Padding.Horizontal;
                    break;
                case TextHorizontalAlign.Right:
                    ctx.TextAlign(TextAlignment.Right | _verticalAlign);
                    drawPos.X += sizeX - Padding.Horizontal;
                    break;
                case TextHorizontalAlign.Center:
                    ctx.TextAlign(TextAlignment.Center | _verticalAlign);
                    drawPos.X += sizeX * 0.5f;
                    break;
            }

            for (int i = 0; i < _lines.Length; i++)
            {
                ctx.Text(drawPos, _lines[i]);
                drawPos.Y += FontSize * LineHeight;
            }
        }

        #endregion

        #region Private

        // me must recalculate lines
        void RecalculateLines(NvgContext ctx)
        {
            if (!_needRecalculate)
                return;

            _needRecalculate = false;

            if (!WrapText)
            {
                // we do manual, forced wrapping with new line chars
                string[] split = Caption.Split(['\n'], StringSplitOptions.None);

                if(split.Length < 2)
                {
                    _offsetY = Size.Y * 0.5f;

                    // calculate display caption with ellipsis (...) potentially
                    _lines = [this.GetText(ctx, Caption)];
                }
                else
                {
                    // we calculate top y pos offset from center
                    _offsetY = (Size.Y * 0.5f) - (split.Length * 0.5f * 0.5f * FontSize * LineHeight);

                    _lines = split;
                }
            }
            else
            {
                _offsetY = 0;

                ReadOnlySpan<char> textSpan = Caption;

                ctx.TextBreakLines(textSpan, FixedSize.X > 0 ? FixedSize.X : Size.X, MaxTextRows, out ReadOnlySpan<TextRow> rows);

                // note: ctx.Text handles new lines
                _lines = new string[rows.Length];

                for(int i = 0; i < rows.Length; i++)
                {
                    _lines[i] = textSpan.Slice(rows[i].StartPos, rows[i].TextLength).ToString();
                }
            }
        }

        #endregion
    }
}
