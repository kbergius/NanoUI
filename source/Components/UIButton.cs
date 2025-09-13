using NanoUI.Common;
using NanoUI.Nvg;
using System.Collections.Generic;
using System;
using System.Numerics;

namespace NanoUI.Components
{
    /// <summary>
    /// UIButton.
    /// </summary>
    public class UIButton : UIWidget
    {
        // this makes pushed button text dimmer
        const float PUSHED_TEXT_MULTIPLIER = 0.6f;

        public Action? Clicked;

        // The callback issued for toggle & radio buttons.
        public Action<UIButton, bool>? StateChanged;

        /// <inheritdoc />
        public UIButton()
        {
            // set defaults to theme impl - prevents circular reference
            IconAlign = default;
            TextHorizontalAlignment = default;
            _padding = new();
            Caption = string.Empty;
        }

        /// <inheritdoc />
        public UIButton(UIWidget parent)
            :this(parent, "Untitled", -1)
        {
        }

        /// <inheritdoc />
        public UIButton(UIWidget parent, string caption = "Untitled", int icon = -1)
            : base(parent)
        {
            Caption = caption;
            Icon = icon;
            // this is fixed
            TextVerticalAlignment = TextVerticalAlign.Middle;

            ThemeType = typeof(UIButton);
        }

        #region Properties

        public string Caption { get; set; }

        Thickness? _padding;
        public virtual Thickness Padding
        {
            get => _padding?? GetTheme().Button.Padding;
            set => _padding = value;
        }

        /// <inheritdoc />
        public override bool Pushed
        {
            get => base.Pushed;
            set
            { 
                if(base.Pushed != value)
                {
                    base.Pushed = value;

                    if ((Flags & ButtonFlags.ToggleButton) != 0 ||
                        (Flags & ButtonFlags.RadioButton) != 0)
                    {
                        StateChanged?.Invoke(this, base.Pushed);
                    }
                }
            }
        }

        // -1 means no icon
        public virtual int Icon { get; set; }

        IconAlign? _iconAlign;
        public IconAlign IconAlign
        {
            get => _iconAlign?? GetTheme().Button.IconAlign;
            set => _iconAlign = value;
        }
        
        // The button group for radio buttons
        List<UIButton>? _buttonGroup;
        public List<UIButton>? ButtonGroup
        {
            get => _buttonGroup;
            set => _buttonGroup = value;
        }
        
        public virtual ButtonFlags Flags { get; set; } = ButtonFlags.NormalButton;

        #endregion

        #region Events

        /// <inheritdoc />
        public override bool OnPointerUpDown(Vector2 p, PointerButton button, bool down)
        {
            base.OnPointerUpDown(p, button, down);

            // handle if button changes
            UIButton self = this;

            if (button == PointerButton.Left && !Disabled)
            {
                if (down)
                {
                    if ((Flags & ButtonFlags.RadioButton) != 0)
                    {
                        if (ButtonGroup == null || ButtonGroup.Count == 0)
                        {
                            if(Parent != null)
                            {
                                foreach (var widget in Parent.Children.AsReadOnlySpan())
                                {
                                    if (widget is UIButton b)
                                    {
                                        if (b != this && b != null && (b.Flags & ButtonFlags.RadioButton) != 0 && b.Pushed)
                                        {
                                            b.Pushed = false;

                                            //b.StateChanged?.Invoke(this, _pushed);
                                        }
                                    }
                                }
                            }
                        }
                        else
                        {
                            foreach (var b in ButtonGroup)
                            {
                                if (b != this && (b.Flags & ButtonFlags.RadioButton) != 0 && b.Pushed)
                                {
                                    b.Pushed = false;

                                    //b.StateChanged?.Invoke(this, _pushed);
                                }
                            }
                        }
                    }
                    
                    if ((Flags & ButtonFlags.ToggleButton) != 0)
                        Pushed = !Pushed;
                    else
                        Pushed = true;
                }
                else if (Pushed)
                {
                    if (Contains(p))
                    {
                        Clicked?.Invoke();
                    }

                    if ((Flags & ButtonFlags.NormalButton) != 0)
                        Pushed = false;
                }

                return true;
            }

            return false;
        }

        #endregion

        #region Layout

        /// <inheritdoc />
        public override Vector2 PreferredSize(NvgContext ctx)
        {
            ctx.FontSize(FontSize);
            ctx.FontFaceId(FontFaceId);

            // get caption size
            float tw = ctx.TextBounds(0, 0, Caption, out _);
            float iw = 0.0f;
            float ih = FontSize;

            // check if we have icon
            if (Icon > 0)
            {
                ih *= IconScale;
                ctx.FontFaceId(FontIconsId);
                ctx.FontSize(ih);
                iw = ctx.TextBounds(0, 0, Icon, out _) + Size.Y * 0.15f;
            }

            // todo : calculate vertical - from text bounds
            return Vector2.Max(MinSize, new Vector2(
                (int)(tw + iw) + 20 + (Padding.Horizontal * 2),
                FontSize + 10));
        }

        #endregion

        #region Drawing

        /// <inheritdoc />
        public override void Draw(NvgContext ctx)
        {
            // Background
            DrawBackgroundBrush(ctx);

            ctx.FontSize(FontSize);
            ctx.FontFaceId(FontFaceId);

            // caption size for alignment
            // todo : could be cached?
            ctx.TextBounds(0, 0, Caption, out Rect bounds);
            
            Vector2 textPos = Vector2.Zero;

            // todo : more padding

            // horizontal
            switch (TextHorizontalAlignment)
            {
                case TextHorizontalAlign.Left:
                    // todo . more like this
                    textPos.X += Padding.Horizontal;
                    break;
                case TextHorizontalAlign.Center:
                    textPos.X = (Size.X - bounds.Width) / 2;
                    break;
                case TextHorizontalAlign.Right:
                    textPos.X = Size.X - bounds.Width;
                    break;
            }

            // Vertical
            // todo : vertical is fixed to middle
            textPos.Y = (Size.Y - bounds.Height) / 2;

            // get textcolor based on button state
            // color is same for icon & text
            ctx.FillColor(GetTextColor());

            // icon?
            if (Icon > 0)
            {
                ctx.FontSize(FontSize * IconScale);
                ctx.FontFaceId(FontIconsId);

                float iw = ctx.TextBounds(0, 0, Icon, out _);

                if (!string.IsNullOrEmpty(Caption))
                    iw += Size.Y * 0.15f;

                ctx.TextAlign(TextAlignment.Left | TextAlignment.Middle);

                Vector2 iconPos = Position + Size / 2;
                iconPos.Y -= 1;

                if (IconAlign == IconAlign.LeftCentered)
                {
                    iconPos.X -= (bounds.Width + iw) * 0.5f;
                    textPos.X += iw * 0.5f;
                }
                else if (IconAlign == IconAlign.RightCentered)
                {
                    textPos.X -= iw * 0.5f;
                    iconPos.X += bounds.Width * 0.5f;
                }
                else if (IconAlign == IconAlign.Left)
                {
                    iconPos.X = Position.X + 8;
                }
                else if (IconAlign == IconAlign.Right)
                {
                    iconPos.X = Position.X + Size.X - iw - 8;
                }

                ctx.Text(iconPos + new Vector2(0, 1), Icon);
            }

            // Caption
            if(!string.IsNullOrEmpty(Caption))
            {
                ctx.FontFaceId(FontFaceId);
                ctx.FontSize(FontSize);
                ctx.TextAlign(TextAlignment.Left | TextAlignment.Top);
                ctx.Text(Position + textPos, Caption);
            }

            // shouldn't be any children!
            base.Draw(ctx);

            // Border
            this.DrawBorder(ctx, Pushed);
        }

        #endregion

        #region Private

        // called from drawing
        Color GetTextColor()
        {
            if (Disabled)
            {
                return GetTheme().Button.TextDisabledColor;
            }
            else if (Pushed)
            {
                // get current text color
                var textColor = TextColor;

                // make text dimmer
                return new Color(textColor, textColor.A * PUSHED_TEXT_MULTIPLIER / 255);
            }

            return TextColor;
        }

        #endregion
    }
}
