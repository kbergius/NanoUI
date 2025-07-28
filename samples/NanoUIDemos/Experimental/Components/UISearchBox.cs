using NanoUI.Common;
using NanoUI.Components;
using NanoUI.Nvg;
using System.Numerics;

namespace NanoUIDemos.Experimental.Components
{
    // note: this uses text box as base theme
    public class UISearchBox : UITextField
    {
        Rect iconClearRect;
        bool _clearHovered;

        public Action<string>? Search;

        // this is ctor for theme/layout generation (if you use this otherwise, set parent before using widget)
        public UISearchBox()
        {
            // set defaults to theme impl - prevents circular reference
            CornerRadius = default;
        }

        public UISearchBox(UIWidget parent)
            : base(parent, string.Empty)
        {
            // defaults
            PlaceholderText = "Search";
            Editable = true;
            TextHorizontalAlignment = TextHorizontalAlign.Left;

            // note: this uses text box as base theme
            //ThemeType = typeof(SearchBox);
        }

        #region Properties

        CornerRadius? _cornerRadius;
        public override CornerRadius CornerRadius
        {
            get => _cornerRadius.HasValue? _cornerRadius.Value : new CornerRadius(Size.Y / 2 - 1);
            set => _cornerRadius = value;
        }

        #endregion

        #region Events

        public override bool OnKeyUpDown(Key key, bool down, KeyModifiers modifiers)
        {
            if(down && key == Key.Enter)
            {
                // fire search
                Search?.Invoke(Text);
            }

            return base.OnKeyUpDown(key, down, modifiers);
        }

        public override bool OnPointerMove(Vector2 p, Vector2 rel)
        {
            _clearHovered = iconClearRect.Contains(p - Position);

            return base.OnPointerMove(p, rel);
        }

        public override bool OnPointerUpDown(Vector2 p, PointerButton button, bool down)
        {
            if(down && iconClearRect.Contains(p - Position))
            {
                ResetText(string.Empty);
                //return true;
            }

            return base.OnPointerUpDown(p, button, down);
        }

        #endregion

        #region Layout

        public override void PerformLayout(NvgContext ctx)
        {
            base.PerformLayout(ctx);

            iconClearRect = new Rect(Size.X - Size.Y, 0, Size.Y, Size.Y);
        }

        #endregion

        #region Drawing

        // todo: magical numbers
        public override void Draw(NvgContext ctx)
        {
            // draw text box (must do this first since base draws background that can be opaque)
            base.Draw(ctx);

            // draw overlay icons
            // note : because padding is set after base draw, real text positioning is 1 frame behind

            // we set text offset (padding horizontal) based if we show search icon or not
            if (!Focused)
            {
                // make "room" for icon
                //PaddingHorizontal = (uint)(Size.Y * 1.1f);
                Padding = new Thickness(Size.Y * 1.1f, 4);

                // draw search icon
                ctx.FontSize(Size.Y * 0.6f);
                ctx.FontFaceId(FontIconsId);
                ctx.FillColor(!Disabled ? TextColor : TextDisabledColor);
                ctx.TextAlign(TextAlignment.Center | TextAlignment.Middle);
                ctx.Text(
                    Position.X + Size.Y * 0.55f,
                    Position.Y + Size.Y * 0.5f,
                    GetTheme().Fonts.IconSearch);
            }
            else
            {
                // edit mode
                Padding = new Thickness(Size.Y * 0.5f, 4);
            }            

            // draw "clear text" icon

            // slightly increase icon size if hovered
            ctx.FontSize(Size.Y * (!Disabled && _clearHovered ? 0.7f : 0.6f));
            ctx.FontFaceId(FontIconsId);
            ctx.FillColor(!Disabled? TextColor : TextDisabledColor);
            ctx.TextAlign(TextAlignment.Center | TextAlignment.Middle);
            ctx.Text(
                Position.X + (Size.X - Size.Y) + Size.Y * 0.55f,
                Position.Y + Size.Y * 0.5f,
                GetTheme().Fonts.IconCancel);
        }

        #endregion
    }
}
