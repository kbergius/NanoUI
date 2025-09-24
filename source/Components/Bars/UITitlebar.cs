using NanoUI.Common;
using Color = NanoUI.Common.Color;
using NanoUI.Nvg;
using System;
using System.Numerics;
using System.Text.Json.Serialization;

namespace NanoUI.Components.Bars
{
    // todo: layouting works somehow, but there could be better algorithm

    /// <summary>
    /// UITitlebar.
    /// </summary>
    public class UITitlebar : UIWidgetbar
    {
        UILabel _title;

        bool _recreateButtons = true;

        /// <summary>
        /// This is static since you probably want all titlebars do the same with button clicked &
        /// this way you can only wrap action once.
        /// Widget is UITitlebar instance & int is icon id.
        /// </summary>
        public static Action<UIWidget, int>? ButtonClicked;

        /// <inheritdoc />
        public UITitlebar()
        {
            // set defaults to theme impl - prevents circular reference
            _title = new();
            ButtonSize = default;
            ButtonFontIconsId = default;
            ButtonIcons = Array.Empty<int>();
            FontSize = default;
        }

        /// <inheritdoc />
        public UITitlebar(UIWidget parent, string title = "")
            : base(parent)
        {
            // add label for title
            _title = new UILabel(this, title);
        }

        #region Properties

        /// <summary>
        /// Title
        /// </summary>
        public string Title
        {
            get => _title.Caption;
            set => _title.Caption = value;
        }

        Vector2? _buttonSize;

        /// <summary>
        /// Buttons' size
        /// </summary>
        public virtual Vector2 ButtonSize
        {
            get => _buttonSize?? GetTheme().Titlebar.ButtonSize;
            set => _buttonSize = value;
        }

        int? _buttonFontIconsId;

        /// <summary>
        /// Buttons' font icons id
        /// </summary>
        [JsonIgnore]
        public virtual int ButtonFontIconsId
        {
            get => _buttonFontIconsId?? GetTheme().Fonts.GetFontId(GetTheme().Fonts.GetDefaultIconType());
            set => _buttonFontIconsId = value;
        }

        int[]? _buttonIcons;

        /// <summary>
        /// Button icons
        /// </summary>
        public virtual int[] ButtonIcons
        {
            get => _buttonIcons?? GetTheme().Titlebar.ButtonIcons;
            set
            {
                if (value == null)
                {
                    _buttonIcons = Array.Empty<int>();
                }
                else
                {
                    _buttonIcons = value;
                }

                _recreateButtons = true;
            }
        }

        #endregion

        #region Events

        /// <inheritdoc />
        public override bool OnPointerUpDown(Vector2 p, PointerButton button, bool down)
        {
            if (button == PointerButton.Left && down && !Focused)
            {
                RequestFocus();
            }

            // check buttons first (omit label)
            // note: need todo, since there are no actions wired on buttons & we want to send click event
            Vector2 childPos = p - Position;

            for (int i = 1; i < Children.Count; i++)
            {
                UIWidget child = Children[i];

                if (child.Contains(childPos))
                {
                    // forward event to button
                    var res = child.OnPointerUpDown(childPos, button, down);

                    if (!down && child is UIButton b)
                    {
                        // todo: works only with buttons
                        ButtonClicked?.Invoke(this, b.Icon);
                    }

                    return res;
                }
            }

            // note: we don't support any other widgets by now
            return false;
        }

        #endregion

        #region Layout

        /// <inheritdoc />
        public override Vector2 PreferredSize(NvgContext ctx)
        {
            if (!Visible)
                return Vector2.Max(MinSize, Vector2.Zero);

            // set title params
            _title.FontSize = FontSize;
            _title.FontFaceId = FontFaceId;

            var titleSize = _title.PreferredSize(ctx);
            return Vector2.Max(MinSize, new Vector2(titleSize.X + ButtonSize.X, MathF.Max(titleSize.Y, ButtonSize.Y) + Margin.Vertical * 2));
        }

        /// <inheritdoc />
        public override void PerformLayout(NvgContext ctx)
        {
            if(Parent == null || ChildrenLayout == null)
                return;

            // stretch to parent width
            Size = new Vector2(Parent.Size.X, PreferredSize(ctx).Y);
            
            if (_recreateButtons)
            {
                CreateButtons();

                _recreateButtons = false;
            }

            // let the layout do layouting
            base.PerformLayout(ctx);

            // todo: parent border?
            Position = Vector2.Zero;

            // todo: this is a hack
            _title.Position = new Vector2(Margin.Horizontal, Margin.Vertical);

            var start = Size.X - Margin.Horizontal;
            var spacing = ChildrenLayout.Spacing.X;

            for (int i = Children.Count - 1; i > 0; i--)
            {
                Children[i].Position = new Vector2(start - ButtonSize.X, Children[i].Position.Y);

                start -= spacing + ButtonSize.X;
            }

            _title.Size = new Vector2(start - Margin.Horizontal, _title.Size.Y);
        }

        #endregion

        #region Drawing

        /// <inheritdoc />
        public override void Draw(NvgContext ctx)
        {
            if (!Visible)
                return;

            // background
            if (Parent != null && Parent.Focused)
            {
                BackgroundFocused?.Draw(ctx, Position, Size, null);
            }
            else
            {
                BackgroundUnfocused?.Draw(ctx, Position, Size, null);
            }

            // draw children
            base.Draw(ctx);

            // no border!
        }

        #endregion

        #region Private

        void CreateButtons()
        {
            // set buttons fixed width (no label)
            for (int i = 0; i < ButtonIcons.Length; i++)
            {
                // create button if there is none
                if (i + 1 >= Children.Count)
                {
                    new UIButton(this, string.Empty, ButtonIcons[i]);
                }

                if (Children[i + 1] is UIButton button)
                {
                    button.FixedSize = ButtonSize;
                    button.FontIconsId = ButtonFontIconsId;
                    button.BackgroundFocused = button.BackgroundUnfocused = new SolidBrush(Color.Transparent);
                    button.Border = false;
                }
            }
        }

        #endregion
    }
}
