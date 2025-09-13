using NanoUI.Common;
using NanoUI.Layouts;
using NanoUI.Nvg;
using System;
using System.Numerics;

namespace NanoUI.Components
{
    // todo: should there be vscroll in content?
    #region UICollapsablePanelContent

    // this is real content - show/hide

    /// <summary>
    /// UICollapsablePanelContent.
    /// </summary>
    public class UICollapsablePanelContent : UIWidget
    {
        /// <inheritdoc />
        public UICollapsablePanelContent()
        {
            // set defaults to theme impl - prevents circular reference
        }

        /// <inheritdoc />
        public UICollapsablePanelContent(UIWidget parent)
                : base(parent)
        {
            
        }

        #region Layout

        /// <inheritdoc />
        public override void PerformLayout(NvgContext ctx)
        {
            base.PerformLayout(ctx);

            if (Parent != null)
            {
                Size = new Vector2(Parent.Size.X, Size.Y);
            }
            
            Position = new Vector2(0, Position.Y);
        }

        #endregion

        #region Drawing

        /// <inheritdoc />
        public override void Draw(NvgContext ctx)
        {
            // background
            DrawBackgroundBrush(ctx);

            base.Draw(ctx);

            // border
            this.DrawBorder(ctx, true);
        }

        #endregion
    }

    #endregion

    #region UICollapsablePanelHeader

    // todo: should this extend titlebar?

    /// <summary>
    /// UICollapsablePanelHeader.
    /// </summary>
    public class UICollapsablePanelHeader : UIButton
    {
        // this is for title text padding (added to icon padding)
        // todo: should be calculated dynamically?
        // todo2: use button layout/draw code?

        public Action<bool>? CollapsedChanged;

        /// <inheritdoc />
        public UICollapsablePanelHeader()
        {
            // set defaults to theme impl - prevents circular reference
            // todo : icon padding different text padding
            Padding = new Thickness(8, 4);// Horizontal = default;
            TextHorizontalAlignment = default;
            IconAlign = default;
            CollapseIcon = default;
            ExpandIcon = default;
        }

        /// <inheritdoc />
        public UICollapsablePanelHeader(UIWidget parent)
                : base(parent)
        {
            TextHorizontalAlignment = TextHorizontalAlign.Left;
            // not needed, since button sets this already!
            TextVerticalAlignment = TextVerticalAlign.Middle;
            IconAlign = IconAlign.Left;
            Icon = _collapsed ? CollapseIcon : ExpandIcon;

            // we read all basic properties from UIButton
            ThemeType = typeof(UIButton);

            // wrap button clicked
            Clicked += () =>
            {
                Collapsed = !Collapsed;
            };
        }

        #region Properties

        Thickness? _padding;

        /// <inheritdoc />
        public override Thickness Padding
        {
            get => _padding?? GetTheme().UICollapsablePanelHeader.Padding;
            set => _padding = value;
        }

        bool _collapsed = false;
        public bool Collapsed
        {
            get => _collapsed;
            set
            {
                if(_collapsed != value)
                {
                    _collapsed = value;

                    // change icon
                    Icon = _collapsed ? CollapseIcon : ExpandIcon;

                    // fire event
                    CollapsedChanged?.Invoke(_collapsed);
                }
            }
        }

        int? _collapseIcon;
        public int CollapseIcon
        {
            get => _collapseIcon?? GetTheme().Fonts.IconCollapsed;
            set => _collapseIcon = value;
        }

        int? _expandIcon;
        public int ExpandIcon
        {
            get => _expandIcon ?? GetTheme().Fonts.IconExpanded;
            set => _expandIcon = value;
        }

        // we override button background
        BrushBase? _backgroundFocused;

        /// <inheritdoc />
        public override BrushBase? BackgroundFocused
        {
            get => _backgroundFocused != null ? _backgroundFocused : GetTheme().UICollapsablePanelHeader.BackgroundFocused;
            set => _backgroundFocused = value;
        }

        BrushBase? _backgroundUnfocused;

        /// <inheritdoc />
        public override BrushBase? BackgroundUnfocused
        {
            get => _backgroundUnfocused != null ? _backgroundUnfocused : GetTheme().UICollapsablePanelHeader.BackgroundUnfocused;
            set => _backgroundUnfocused = value;
        }

        #endregion

        #region Layout

        /// <inheritdoc />
        public override void PerformLayout(NvgContext ctx)
        {
            Position = Vector2.Zero;

            base.PerformLayout(ctx);

            if(Parent != null)
                Size = new Vector2(Parent.Size.X, Size.Y);
        }

        #endregion
    }

    #endregion

    // note: this is just container; set all properties to header & content widgets

    /// <summary>
    /// UICollapsablePanel.
    /// </summary>
    public class UICollapsablePanel : UIWidget
    {
        UICollapsablePanelHeader _header;
        UICollapsablePanelContent _content;

        /// <inheritdoc />
        public UICollapsablePanel(UIWidget parent)
            : base(parent)
        {
            ChildrenLayout = new StackLayout(Orientation.Vertical);

            _header = new UICollapsablePanelHeader(this);
            _header.CollapsedChanged += OnCollapseChanged;

            _content = new UICollapsablePanelContent(this);
        }

        #region Properties

        // all other properties should be set to header / content
        public UICollapsablePanelHeader Header => _header;

        // note: doesn't have any special properties
        public UICollapsablePanelContent Content => _content;

        #endregion

        #region Events

        public virtual void OnCollapseChanged(bool collapsed)
        {
            _content.Visible = !collapsed;

            RequestLayoutUpdate(Parent);
        }

        #endregion
    }
}
