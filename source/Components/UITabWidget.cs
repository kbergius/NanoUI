using NanoUI.Common;
using NanoUI.Nvg;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text.Json.Serialization;

namespace NanoUI.Components
{
    // note : user can't set visible state since we handle it in
    // UpdateVisibility()

    // todo: this needs lot of love!
    // todo : restrict ChildrenList only contain TabItem
    // todo: base tab item drawing functionality should be in tab item
    // todo: tabs positioning: Top, Bottom (Left & Right?)
    // todo: there should be Tabbar widget holding all tabs!!!!

    #region UITabItem

    public class UITabItem : UIWidget
    {
        public UITabItem(UITabWidget parent)
            : base(parent)
        {
            // defaults
            Caption = "Untitled";
            Closable = true;
            Draggable = true;
        }

        #region Properties

        public string Caption {  get; set; }
        public bool Closable { get; set; }
        public bool Draggable { get; set; }

        #endregion

        #region Layout

        public override void PerformLayout(NvgContext ctx)
        {
            // todo: check - we should have StrechLayout by default
            // if only 1 children and not layout set, strect content to fill all area
            if(Children.Count == 1 && ChildrenLayout == null)
            {
                Children[0].Position = Vector2.Zero;
                Children[0].FixedSize = Size;// - new Vector2(0, ((TabWidget)Parent).TabHeight);
            }

            base.PerformLayout(ctx);
        }

        #endregion
    }

    #endregion

    public class UITabWidget : UIWidget
    {
        // this is indicator that index is invalid
        const int INVALID = -1;

        List<int> _tabOffsets = new();

        int _closeWidth = 0;
        int _activeTab = 0;
        int _tabDragIndex = INVALID;
        int _tabDragMin = INVALID;
        int _tabDragMax = INVALID;
        int _tabDragStart = INVALID;
        int _tabDragEnd = INVALID;
        int _closeIndex = INVALID;
        int _closeIndexPushed = INVALID;
                
        public Action<UITabItem> TabChanged;

        // this is ctor for theme/layout generation (if you use this otherwise, set parent before using widget)
        public UITabWidget()
        {
            // set defaults to theme impl - prevents circular reference
            TabPaddingHorizontal = default;
            TabPaddingVertical = default;
            ContentPaddingHorizontal = default;
            ContentPaddingVertical = default;
            // todo : check (3)
            TabCornerRadius = default;
            TabDragColor = default;
            TabInactiveTop = default;
            TabInactiveBottom = default;
            CloseIcon = default;
        }

        public UITabWidget(UIWidget parent)
            : base(parent)
        {
            _tabOffsets.Add(0);

            // this is fixed
            TextVerticalAlignment = TextVerticalAlign.Top;

            // todo: configurable; close icon!
            IconExtraScale = 0.7f;

            // todo: this should be in tab item
            ThemeType = typeof(UILabel);
        }

        #region Properties

        int? _closeIcon;
        public int CloseIcon
        {
            get => _closeIcon?? GetTheme().Fonts.IconClose;
            set => _closeIcon = value;
        }

        float? _tabCornerRadius;
        public float TabCornerRadius
        {
            get => _tabCornerRadius?? GetTheme().TabWidget.TabCornerRadius;
            set => _tabCornerRadius = value;
        }

        uint? _contentPaddingHorizontal;
        public virtual uint ContentPaddingHorizontal
        {
            get => _contentPaddingHorizontal ?? GetTheme().TabWidget.ContentPaddingHorizontal;
            set => _contentPaddingHorizontal = value;
        }

        uint? _contentPaddingVertical;
        public virtual uint ContentPaddingVertical
        {
            get => _contentPaddingVertical?? GetTheme().TabWidget.ContentPaddingVertical;
            set => _contentPaddingVertical = value;
        }

        uint? _tabPaddingHorizontal;
        public virtual uint TabPaddingHorizontal
        {
            get => _tabPaddingHorizontal ?? GetTheme().TabWidget.TabPaddingHorizontal;
            set => _tabPaddingHorizontal = value;
        }

        uint? _tabPaddingVertical;
        public virtual uint TabPaddingVertical
        {
            get => _tabPaddingVertical?? GetTheme().TabWidget.TabPaddingVertical;
            set => _tabPaddingVertical = value;
        }

        // this is needed in docking by now
        // todo: JsonIgnore?
        // todo: GetLayoutArea()
        public Rect ContentArea { get; private set; }

        // Return the total number of tabs
        [JsonIgnore]
        public int TabsCount => Children.Count;

        // todo : needed in docking solution by now
        public int TabHeight { get; private set; }
                
        // Return the index of the currently active tab
        public int SelectedIndex
        {
            get => _activeTab;
            set
            {
                // should we throw error?
                if (value >= Children.Count)
                    return;

                _activeTab = value;

                UpdateVisibility();
            }
        }

        Color? _tabDragColor;
        public Color TabDragColor
        {
            get => _tabDragColor ?? GetTheme().TabWidget.TabDragColor;
            set => _tabDragColor = value;
        }

        Color? _tabInactiveTop;
        public Color TabInactiveTop
        {
            get => _tabInactiveTop?? GetTheme().TabWidget.TabInactiveTop;
            set => _tabInactiveTop = value;
        }
        Color? _tabInactiveBottom;
        public Color TabInactiveBottom
        {
            get => _tabInactiveBottom ?? GetTheme().TabWidget.TabInactiveBottom;
            set => _tabInactiveBottom = value;
        }

        #endregion

        #region Methods

        // Add a new tab
        public UITabItem AddTab(string caption)
        {
            var res = new UITabItem(this) { Caption = caption };

            int index = Children.Count -1;

            // fires also update visibility
            RequestLayoutUpdate(this);

            if (index < _activeTab)
                _activeTab++;

            if (Children.Count == 1)
            {
                _activeTab = 0;

                if (TabChanged != null && Children.TryGet(index, out UITabItem? selectedTab))
                {
                    if(selectedTab != null)
                    {
                        TabChanged.Invoke(selectedTab);
                    }
                }
            }

            return res;
        }

        // Remove a tab
        public void RemoveTab(UITabItem tab)
        {
            RemoveTab(Children.IndexOf(tab));
        }

        // Removes a tab with the specified index
        public void RemoveTab(int index)
        {
            if (Children.TryGet(index, out UITabItem tab))
            {
                bool closeActive = index == _activeTab;

                Children.RemoveAt(index);

                if (index <= _activeTab)
                    _activeTab = Math.Max(0, _activeTab - 1);

                // fires also update visibility
                RequestLayoutUpdate(this);

                if (closeActive)
                {
                    if (TabChanged != null && Children.TryGet(SelectedIndex, out UITabItem selectedTab))
                    {
                        TabChanged.Invoke(selectedTab);
                    }
                }
            }
        }

        #endregion

        #region Layout

        // we must recalculate tab offsets & content position & size
        public override void PerformLayout(NvgContext ctx)
        {
            _tabOffsets.Clear();

            ctx.FontFaceId(FontFaceId);
            ctx.FontSize(FontSize);
            ctx.TextAlign(TextAlignment.Left | TextAlignment.Top);

            int width = 0;

            // calculate tabs offsets & widths (= "position")
            foreach (UITabItem tab in Children.AsReadOnlySpan())
            {
                int labelWidth = (int)ctx.TextBounds(0, 0, tab.Caption, out _);
                _tabOffsets.Add(width);

                tab.Size = new Vector2(labelWidth + 2 * (int)TabPaddingHorizontal, tab.Size.Y);

                if (tab.Closable)
                    tab.Size += new Vector2(_closeWidth, 0);

                width += labelWidth + 2 * (int)TabPaddingHorizontal;

                if (tab.Closable)
                    width += _closeWidth;
            }

            _tabOffsets.Add(width);

            TabHeight = (int)FontSize + 2 * (int)TabPaddingVertical;

            bool first = true;

            // calculate tab items positions & sizes
            foreach (UITabItem tabItem in Children.AsReadOnlySpan())
            {
                // we set all tab items at same position (only active is drawn)
                tabItem.Position = new Vector2(
                    ContentPaddingHorizontal,
                    ContentPaddingVertical + TabHeight + 1);

                // we set item size according to tabWidget size - tabs area size
                tabItem.Size = Size - new Vector2(2 * ContentPaddingHorizontal, 2 * ContentPaddingVertical + TabHeight + 1);

                // this is needed in docking by now
                if (first)
                {
                    first = false;
                    ContentArea = new Rect(tabItem.Position, tabItem.Size);
                }

                tabItem.PerformLayout(ctx);
            }

            UpdateVisibility();
        }

        public override Vector2 PreferredSize(NvgContext ctx)
        {
            // calculate icon first
            ctx.FontFaceId(FontIconsId);
            ctx.FontSize(FontSize);
            ctx.TextAlign(TextAlignment.Left | TextAlignment.Top);

            _closeWidth =
                (int)ctx.TextBounds(0, 0, CloseIcon, out _);

            // calculate text
            ctx.FontFaceId(FontFaceId);

            int width = 0;

            foreach (UITabItem tab in Children.AsReadOnlySpan())
            {
                int labelWidth = (int)ctx.TextBounds(0, 0, tab.Caption, out _);
                labelWidth += 2 * (int)TabPaddingHorizontal;

                if (tab.Closable)
                    labelWidth += _closeWidth;

                // set tab width
                tab.Size = new Vector2(labelWidth, tab.Size.Y);

                width += labelWidth;
            }

            Vector2 baseSize = new Vector2(width + 1,
                           FontSize + 2 * TabPaddingVertical + 2 * ContentPaddingVertical);

            Vector2 contentSize = Vector2.Zero;

            // get max content size in all tabs
            foreach (UITabItem tab in Children.AsReadOnlySpan())
            {
                contentSize = Vector2.Max(contentSize, tab.PreferredSize(ctx));
            }

            return Vector2.Max(MinSize, new Vector2(
                (int)MathF.Max(baseSize.X, contentSize.X + 2 * ContentPaddingHorizontal),
                baseSize.Y + contentSize.Y + 2 * ContentPaddingVertical));
        }

        #endregion

        #region Events

        public override bool OnPointerUpDown(Vector2 p, PointerButton button, bool down)
        {
            bool handled = false;

            if (button == PointerButton.Left)
            {
                (int index, bool close) = GetTabAtPosition(p);

                if (index >= 0)
                {
                    if (close && _tabDragIndex == INVALID)
                    {
                        if (down)
                        {
                            _closeIndexPushed = index;
                        }
                        else if (_closeIndex == _closeIndexPushed)
                        {
                            RemoveTab(index);

                            OnPointerMove(p, new Vector2(0));
                        }
                    }
                    else
                    {
                        if (down)
                        {
                            bool tabChanged = _activeTab != index;
                            _activeTab = index;
                            
                            if(Children.TryGet(index, out UITabItem tab))
                            {
                                _tabDragIndex = tab.Draggable ? index : INVALID;
                            }
                                                        
                            _tabDragStart = _tabDragEnd = (int)p.X;
                            _tabDragMin = _tabOffsets[index];
                            _tabDragMax = _tabOffsets[index + 1];

                            _closeIndexPushed = INVALID;

                            if (tabChanged)
                            {
                                if (TabChanged != null && Children.TryGet(SelectedIndex, out UITabItem selectedTab))
                                {
                                    TabChanged.Invoke(selectedTab);
                                }

                                UpdateVisibility();
                            }

                            // register drag widget
                            Screen?.SetDragWidget(this);
                        }
                        else if (_tabDragIndex != INVALID)
                        {
                            _tabDragIndex = INVALID;
                            
                            OnPointerMove(p, new Vector2(0));
                        }
                    }
                    handled = true;
                }

                if (!down)
                {
                    handled = _closeIndexPushed != INVALID || _tabDragIndex != INVALID;
                    _closeIndexPushed = INVALID;
                    _tabDragIndex = INVALID;
                }

                handled = true;
            }

            handled |= base.OnPointerUpDown(p, button, down);

            return handled;
        }

        // we check if tab dragged outside
        public override bool OnPointerDrag(Vector2 p, Vector2 rel)
        {
            if (!Contains(p))
            {
                if(_tabDragIndex >= 0)
                {
                    // signal parent that tab item wants to be detached
                    if (Parent.OnDetach(Children[_tabDragIndex]))
                    {
                        
                    }

                    // todo: we should only clear indices, if request accepted
                    _tabDragIndex = INVALID;
                    _closeIndex = INVALID;
                    _closeIndexPushed = INVALID;
                }

                return true;
            }

            return base.OnPointerDrag(p, rel);
        }

        public override bool OnPointerMove(Vector2 p, Vector2 rel)
        {
            (int index, bool close) = GetTabAtPosition(p, false);

            if (_tabDragIndex != INVALID)
            {
                _tabDragEnd = (int)p.X;

                if (index != INVALID && _tabDragIndex != index)
                {
                    int i0 = Math.Min(_tabDragIndex, index);

                    int mid = (_tabOffsets[i0] + _tabOffsets[i0 + 1]) / 2;

                    if ((_tabDragIndex < index && p.X - Position.Y > mid) ||
                        (_tabDragIndex > index && p.X - Position.Y < mid))
                    {
                        // swap
                        if(Children.Swap(index, _tabDragIndex))
                        {
                            // must recalculate offsets
                            RequestLayoutUpdate(this);

                            _tabDragIndex = index;
                            _activeTab = index;
                        }
                    }
                }

                return true;
            }

            if (!close)
                index = INVALID;

            if (index != _closeIndex)
            {
                _closeIndex = index;
                _closeIndexPushed = INVALID;

                return true;
            }

            return base.OnPointerMove(p, rel);
        }

        #endregion

        #region Drawing

        public override void Draw(NvgContext ctx)
        {
            // if there is no tabs, we don't draw anything
            if(Children.Count == 0)
                return;

            // Must run PerformLayout() after adding/removing tabs;
            // todo : in concrete functions?
            if (_tabOffsets.Count != Children.Count + 1)
                PerformLayout(ctx);

            int tabHeight = (int)FontSize + 2 * (int)TabPaddingVertical;

            // Background
            DrawBackgroundBrush(ctx);
                        
            // draw active / visible tab item (its childs)
            base.Draw(ctx);

            // Draw tab area

            // Tabs background
            Paint tabBackgroundColor = Paint.LinearGradient(
                Position.X, Position.Y + 1, Position.X, Position.Y + tabHeight,
                TabInactiveTop,
                TabInactiveBottom);

            ctx.SaveState();
            ctx.IntersectScissor(Position.X, Position.Y, Size.X, tabHeight);
            
            ctx.FontSize(FontSize);
            // todo
            //ctx.TextAlign(TextHorizontalAlignment, TextVerticalAlignment);
            ctx.TextAlign(TextHorizontalAlignment, TextVerticalAlign.Middle);

            int counter = 0;

            // loop tab items
            foreach (var child in Children.AsReadOnlySpan())
            {
                if(child is UITabItem childTab)
                {
                    int xPos = (int)Position.X + _tabOffsets[counter];
                    // todo : magical number
                    int yPos = (int)Position.Y + 6;
                    int width = _tabOffsets[counter + 1] - _tabOffsets[counter];

                    // active tab
                    if (counter == _activeTab)
                    {
                        ctx.BeginPath();
                        ctx.RoundedRect(xPos + 0.5f, yPos + 1.5f, width,
                                       tabHeight + 4, TabCornerRadius);

                        ctx.StrokeColor(GetTheme().Borders.Light);
                        ctx.Stroke();

                        ctx.BeginPath();
                        ctx.RoundedRect(xPos + 0.5f, yPos + 0.5f, width,
                                       tabHeight + 4, TabCornerRadius);

                        ctx.StrokeColor(GetTheme().Borders.Dark);
                        ctx.Stroke();
                    }
                    else
                    {
                        // inactive tab
                        ctx.BeginPath();
                        ctx.RoundedRect(xPos + 0.5f, yPos + 1.5f, width,
                                       tabHeight + 4, TabCornerRadius);

                        ctx.FillPaint(tabBackgroundColor);
                        ctx.Fill();

                        ctx.StrokeColor(GetTheme().Borders.Dark);
                        ctx.Stroke();
                    }

                    xPos += (int)TabPaddingHorizontal;
                    yPos += (int)TabPaddingVertical + 1;

                    ctx.FillColor(TextColor);
                    ctx.FontFaceId(FontFaceId);
                    ctx.Text(xPos, yPos, childTab.Caption);

                    // close icon
                    if (childTab.Closable)
                    {
                        xPos = (int)Position.X + _tabOffsets[counter + 1] -
                                (int)TabPaddingHorizontal - _closeWidth + 5;

                        ctx.FontFaceId(FontIconsId);

                        // when pushing close icon - change color slightly
                        bool highlight = _closeIndex == counter;

                        // we set icon sligthly dimmer when not pushed/pointer focused
                        // todo : magical number; close icon active color / other way to mark active state?
                        ctx.FillColor(counter == _closeIndexPushed || highlight ?
                            TextColor : TextColor * 0.82f);

                        ctx.FontSize(FontSize * IconScale);

                        ctx.Text(xPos, yPos, CloseIcon);

                        // set font size back
                        ctx.FontSize(FontSize);
                    }

                    counter++;
                }
            }

            // Dragging
            if (_tabDragIndex != INVALID && _tabDragStart != _tabDragEnd)
            {
                int xPos = (int)Position.X + _tabDragMin + _tabDragEnd - _tabDragStart;

                ctx.BeginPath();
                ctx.RoundedRect(xPos + 0.5f, Position.Y + 1.5f, _tabDragMax - _tabDragMin,
                               tabHeight + 4, TabCornerRadius);
                
                ctx.FillColor(TabDragColor);
                ctx.Fill();
            }

            ctx.RestoreState();

            int x0 = _tabOffsets[_activeTab];
            int x1 = _tabOffsets[_tabOffsets.Count > 1 ? _activeTab + 1 : 0];

            for (int i = 1; i >= 0; --i)
            {
                // Top border
                ctx.BeginPath();
                ctx.MoveTo(Position.X + .5f, Position.Y + tabHeight + i + .5f);
                ctx.LineTo(Position.X + x0 + 1.0f, Position.Y + tabHeight + i + .5f);
                ctx.MoveTo(Position.X + x1, Position.Y + tabHeight + i + .5f);
                ctx.LineTo(Position.X + Size.X + .5f, Position.Y + tabHeight + i + .5f);
                
                ctx.StrokeWidth(1.0f);
                ctx.StrokeColor((i == 0) ? GetTheme().Borders.Dark : GetTheme().Borders.Light);
                ctx.Stroke();

                // Bottom + side borders
                ctx.SaveState();
                ctx.IntersectScissor(Position.X, Position.Y + tabHeight, Size.X, Size.Y);
                
                ctx.BeginPath();
                ctx.RoundedRect(Position.X + .5f, Position.Y + i + .5f, Size.X - 1,
                               Size.Y - 2, TabCornerRadius);
                ctx.Stroke();
                ctx.RestoreState();
            }

            // border
            this.DrawBorder(ctx, true);
        }

        #endregion

        #region Private

        void UpdateVisibility()
        {
            if (Children.Count == 0)
                return;

            // set all children visible state
            int counter = 0;

            foreach (UIWidget child in Children.AsReadOnlySpan())
            {
                child.Visible = counter == SelectedIndex? true : false;

                counter++;
            }
        }

        (int, bool) GetTabAtPosition(Vector2 p, bool testVertical = true)
        {
            int tabHeight = (int)FontSize + 2 * (int)TabPaddingVertical;

            if (testVertical && (p.Y <= Position.Y || p.Y > Position.Y + tabHeight))
                return (INVALID, false);

            int x = (int)p.X - (int)Position.X;

            for (int i = 0; i < _tabOffsets.Count - 1; ++i)
            {
                if (x >= _tabOffsets[i] && x < _tabOffsets[i + 1])
                {
                    int r = _tabOffsets[i + 1] - x;

                    bool closable = false;
                    if(i < Children.Count)
                    {
                        if(Children.TryGet(i, out UITabItem childTab))
                        {
                            closable = childTab.Closable;
                        }
                    }

                    return (
                            i, closable &&
                            r < TabPaddingHorizontal + _closeWidth - 4 &&
                            r > TabPaddingHorizontal - 4 &&
                            p.Y - Position.Y > TabPaddingVertical &&
                            p.Y - Position.Y <= tabHeight - TabPaddingVertical
                    );
                }
            }

            return (INVALID, false);
        }

        #endregion
    }
}
