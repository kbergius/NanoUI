using NanoUI.Common;
using NanoUI.Nvg;
using System;
using System.ComponentModel;
using System.Numerics;

namespace NanoUI.Components
{
    // this is simple text tooltip. In order to use custom tooltip:
    // - extend this
    // - override draw method
    // - set your customeized tooltip into screen

    // note : this is not visible by default, so that this doesn't effect any layout method
    // todo : should this derive from label - have label as child
    // todo : make anchor position / size / side & text paddings configurable (like in popup button/popup)
    // now sets tooltip always below widget
    // todo: cleanup code
    // todo: adjust size - when maxrows limits
    public class UITooltip : UIWidget
    {
        // this is ctor for theme/layout generation (if you use this otherwise, set parent before using widget)
        public UITooltip()
        {
            // set defaults to theme impl - prevents circular reference
            // todo : should this be dynamic based on widget's size?
            MaxWidth = default;
            
            // this is by default set to screen & screen calls draw method
            Visible = false;
            DisablePointerFocus = true;
            // no default border drawing?
            Border = false;
        }

        public UITooltip(UIWidget parent)
            : base(parent)
        {
            // this is by default set to screen & screen calls draw method
            Visible = false;
            DisablePointerFocus = true;
            // no default border drawing?
            Border = false;

            ThemeType = typeof(UITooltip);
        }

        #region Properties

        // note: we use this to draw both tooltip & anchor background color
        Color? _backgroundColor;
        [Category(Globals.CATEGORY_APPEARANCE)]
        public virtual Color BackgroundColor
        {
            get => _backgroundColor?? GetTheme().Tooltip.BackgroundColor;
            set => _backgroundColor = value;
        }

        // this is used if text wraps many lines
        int? _maxWidth;
        public int MaxWidth
        {
            get => _maxWidth?? GetTheme().Tooltip.MaxWidth;
            set => _maxWidth = value;
        }

        int? _maxTextRows;
        public int MaxTextRows
        {
            get => _maxTextRows ?? GetTheme().Tooltip.MaxTextRows;
            set => _maxTextRows = value;
        }

        #endregion

        #region Layout

        public override void PerformLayout(NvgContext ctx)
        {
            // do nothing - when drawn position & size is dynamically calculated from the widget
        }

        #endregion

        #region Drawing

        Rect _bounds;
        string _toolTiptext;
        string[] _lines = Array.Empty<string>();
        bool _needRecalculate;

        // this is called every frame if show tooltips flag is true
        // so this should handle if this draws anything or not
        public virtual void Draw(NvgContext ctx, UIWidget widget)
        {
            // check shall we draw or not
            if (widget == null || string.IsNullOrEmpty(widget.Tooltip))
                return;

            _needRecalculate = false;

            if (_toolTiptext != widget.Tooltip)
            {
                _toolTiptext = widget.Tooltip;
                _needRecalculate = true;
            }

            // calculate text box dimensions with max width
            ctx.FontFaceId(FontFaceId);
            ctx.FontSize(FontSize);
            ctx.TextAlign(TextAlignment.Left | TextAlignment.Top);
            ctx.TextLineHeight(1.1f);

            // note: we draw directly to "screen" - so we use absolute position
            Vector2 absPos = widget.GetDisplayPosition() +
                           new Vector2(widget.Size.X / 2, widget.Size.Y + 10);

            ctx.TextBoxBounds(absPos.X, absPos.Y, MaxWidth, _toolTiptext, out _bounds);

            float halfWidth = _bounds.Width / 2;
            
            // todo: keep tooltip in screen (check all screen dimensions)

            ctx.BeginPath();
            ctx.FillColor(BackgroundColor);
            ctx.RoundedRect(_bounds.X - 4 - halfWidth, _bounds.Y - 4,
                           (int)_bounds.Width + 8,
                           (int)_bounds.Height + 8, 3);

            int px = (int)((_bounds.Width + _bounds.X + _bounds.X) / 2) - (int)halfWidth;

            ctx.MoveTo(px, _bounds.Y - 10);
            ctx.LineTo(px + 7, _bounds.Y + 1);
            ctx.LineTo(px - 7, _bounds.Y + 1);
            ctx.Fill();

            ctx.FillColor(TextColor);

            if (_needRecalculate)
            {
                RecalculateLines(ctx);
            }

            // loop through cached tooltip lines

            var pos = new Vector2(absPos.X - halfWidth, absPos.Y);

            for (int i = 0; i < _lines.Length; i++)
            {
                ctx.Text(pos.X, pos.Y, _lines[i]);
                // todo : we could make row height configurable (FontSize * fACTOR)
                pos.Y += FontSize;
            }
        }

        #endregion

        #region Private

        // me must recalculate lines
        // note : this is same as in Label
        void RecalculateLines(NvgContext ctx)
        {
            ReadOnlySpan<char> tooltipSpan = _toolTiptext;
            
            ctx.TextBreakLines(tooltipSpan, _bounds.Size.X, MaxTextRows, out ReadOnlySpan<TextRow> rows);

            // note: ctx.Text handles new lines
            _lines = new string[rows.Length];

            for (int i = 0; i < rows.Length; i++)
            {
                _lines[i] = tooltipSpan.Slice(rows[i].StartPos, rows[i].TextLength).ToString();
            }
        }

        #endregion
    }
}