using NanoUI.Common;
using NanoUI.Nvg;
using NanoUI.Utils;
using System;
using System.Numerics;

namespace NanoUI.Components
{
    // todo : should we have configurable corner radius?
    public class UICheckBox : UIWidget
    {
        public Action<bool>? CheckedChanged;

        // this is used if not wrapping caption
        string _displayCaption = string.Empty;

        // this is ctor for theme/layout generation (if you use this otherwise, set parent before using widget)
        public UICheckBox()
        {
            // set defaults to theme impl - prevents circular reference
            CheckedIcon = default;
            _boxBackgroundBrush = new();
            _caption = string.Empty;
        }

        public UICheckBox(UIWidget parent)
            :this(parent, string.Empty, false)
        {
            
        }

        public UICheckBox(UIWidget parent, string caption, bool @checked = false)
            : base(parent)
        {
            Caption = caption;
            _checked = @checked;

            // we read all basic properties from widget and extra properties from checkbox
            ThemeType = typeof(UIWidget);
        }

        #region Properties

        string _caption;
        public string Caption
        {
            get => _caption;
            set
            {
                _caption = value;
                // must call RequestLayoutUpdate to update display caption
                RequestLayoutUpdate(this);
            }
        }

        Thickness? _padding;
        public virtual Thickness Padding
        {
            get => _padding?? GetTheme().CheckBox.Padding;
            set => _padding = value;
        }

        bool _checked;
        public bool Checked
        {
            get => _checked;
            set
            {
                if(_checked != value)
                {
                    _checked = value;
                    CheckedChanged?.Invoke(_checked);
                }
            }
        }

        // todo : uint O means no icon
        int? _checkedIcon;
        public int CheckedIcon
        {
            get => _checkedIcon?? GetTheme().Fonts.IconChecked;
            set => _checkedIcon = value;
        }

        BrushBase? _boxBackgroundBrush;
        public virtual BrushBase? BoxBackgroundBrush
        {
            get => _boxBackgroundBrush ?? GetTheme().CheckBox.BoxBackgroundBrush;
            set => _boxBackgroundBrush = value;
        }

        #endregion

        #region Events

        // Pointer button event processing for this check box
        public override bool OnPointerUpDown(Vector2 p, PointerButton button, bool down)
        {
            if (down && button == PointerButton.Left)
            {
                // pointer down
                Checked = !Checked;
            }

            return base.OnPointerUpDown(p, button, down);
        }

        public override bool OnKeyUpDown(Key key, bool down, KeyModifiers modifiers)
        {
            if (!Focused)
                return false;

            if (down)
            {
                if (key == Key.Space || key == Key.Enter)
                {
                    Checked = !Checked;
                    return true;
                }
            }

            return base.OnKeyUpDown(key, down, modifiers);
        }

        #endregion

        #region Layout

        // The preferred size of this CheckBox.
        public override Vector2 PreferredSize(NvgContext ctx)
        {
            ctx.FontSize(FontSize);
            ctx.FontFaceId(FontFaceId);

            // todo : magic numbers
            Vector2 prefSize = new Vector2(ctx.TextBounds(0, 0, Caption, out _) + 1.8f * FontSize,
                       FontSize * 1.3f);

            //prefSize += new Vector2(PaddingVertical);
            prefSize += new Vector2(Padding.Horizontal, Padding.Vertical);
            prefSize = FixedSize.ReplaceZero(prefSize);

            return Vector2.Max(MinSize, prefSize);
        }

        public override void PerformLayout(NvgContext ctx)
        {
            base.PerformLayout(ctx);

            // calculate display caption with ellipsis (...) potentially
            // todo: text width calculation
            _displayCaption = this.GetText(ctx, Caption, Size.X - (0.6f * FontSize + Size.Y + Padding.Horizontal));
        }

        #endregion

        #region Drawing

        public override void Draw(NvgContext ctx)
        {
            base.Draw(ctx);

            // text
            ctx.FontSize(FontSize);
            ctx.FontFaceId(FontFaceId);

            ctx.FillColor(!Disabled ? TextColor : TextDisabledColor);
            ctx.TextAlign(TextAlignment.Left | TextAlignment.Middle);
            ctx.Text(Position + new Vector2(0.6f * FontSize + Size.Y + Padding.Horizontal,
                                          Size.Y / 2), _displayCaption);

            // check background
            // todo: more modes?
            if (Disabled)
            {
                GetTheme().Common.BackgroundDisabled?.Draw(ctx, Position + new Vector2(1), new Vector2(Size.Y) - new Vector2(2), null);
            }
            else
            {
                BoxBackgroundBrush?.Draw(ctx, Position + new Vector2(1), new Vector2(Size.Y) - new Vector2(2), null);
            }

            // check mark?
            if (Checked && CheckedIcon > 0)
            {
                // calculates icon scale & icon extrascale
                ctx.FontSize((int)(Size.Y * IconScale));
                ctx.FontFaceId(FontIconsId);

                ctx.FillColor(!Disabled ? TextColor : TextDisabledColor);

                // place to center of check background
                ctx.TextAlign(TextAlignment.Center | TextAlignment.Middle);
                ctx.Text(Position + new Vector2(Size.Y) / 2, CheckedIcon);
            }

            // checkbox border
            this.DrawBorder(ctx, Position, new Vector2(Size.Y), true);
        }

        #endregion
    }
}
