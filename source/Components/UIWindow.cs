using NanoUI.Common;
using NanoUI.Components.Bars;
using NanoUI.Components.Scrolling;
using NanoUI.Nvg;
using NanoUI.Utils;
using System;
using System.Numerics;

namespace NanoUI.Components
{
    // note: in events we check scrolling first and after that window dragging
    // todo: statusbar (placement bottom)

    /// <summary>
    /// UIWindow.
    /// </summary>
    public class UIWindow : UIWidget, IScrollable
    {
        // private state params
        bool _layoutDirty = false;
        // when something happens in pointer events & scolling changes
        // we must recalculate scrolling
        bool _scrollingDirty = false;

        bool _isWindowDragging;
        DragMode _windowDragMode = DragMode.NONE;

        // this is calculated in PreferredSize
        float _layoutVerticalOffset;
        // titlebar height to support window dragging (change window position)
        float _titlebarHeight;

        // SCROLLING
        // this holds info about content (widgets) offset & size
        Rect _contentRect;
        Rect _vertSlider;
        Rect _horizontalSlider;
        ScrollbarType _scrollbars;
        // support for "highlighting"
        ScrollableDragMode _scrollableDragMode = ScrollableDragMode.None;

        UIScrollbar? _verticalScrollbar = null;
        UIScrollbar? _horizontalScrollbar = null;

        // this is ctor for theme/layout generation (if you use this otherwise, set parent before using widget)
        public UIWindow()
        {
            // set defaults to theme impl - prevents circular reference
            BorderSize = default;
            Draggable = default;
            DragResizable = default;
            ScrollbarDimension = default;
        }

        // if title is null, we don't create titlebar
        public UIWindow(UIWidget? parent, string? title, ScrollbarType scrollbars = ScrollbarType.Both)
            : base(parent)
        {
            if(title != null)
            {
                // create titlebar
                new UITitlebar(this, title);
            }
            
            Modal = false;

            // no hover tint
            DisablePointerFocus = true;

            // create scrollbars?
            _scrollbars = scrollbars;

            if (scrollbars == ScrollbarType.Horizontal || scrollbars == ScrollbarType.Both)
            {
                _horizontalScrollbar = new UIScrollbar(this, Orientation.Horizontal);
                _horizontalScrollbar.Visible = true;
                _horizontalScrollbar.Dimension = ScrollbarDimension;
            }

            if (scrollbars == ScrollbarType.Vertical || scrollbars == ScrollbarType.Both)
            {
                _verticalScrollbar = new UIScrollbar(this, Orientation.Vertical);
                _verticalScrollbar.Visible = true;
                _verticalScrollbar.Dimension = ScrollbarDimension;
            }

            ThemeType = typeof(UIWindow);
        }

        #region Properties

        // IScrollable support
        public Vector2 ScrollOffset => _contentRect.Position;

        uint? _borderSize;
        public uint BorderSize
        {
            get => _borderSize?? GetTheme().Window.BorderSize;
            set => _borderSize = value;
        }

        public string? Title
        {
            get
            {
                var bar = GetBar<UITitlebar>();

                if (bar == null)
                    return null;

                return bar.Title;
            }
            set
            {
                var bar = GetBar<UITitlebar>();

                if (bar == null || value == null)
                    return;

                bar.Title = value;
            }
        }

        bool _modal;
        public bool Modal
        {
            get => _modal;
            set
            {
                _modal = value;

                if (_modal)
                {
                    DragResizable = false;
                }
            }
        }

        uint? _scrollbarDimension;
        public uint ScrollbarDimension
        {
            get => _scrollbarDimension ?? GetTheme().Window.ScrollbarDimension;
            set => _scrollbarDimension = value;
        }

        bool? _draggable;
        public bool Draggable
        {
            get => _draggable?? GetTheme().Window.Draggable;
            set => _draggable = value;
        }

        bool? _dragResizable;
        public bool DragResizable
        {
            get => _dragResizable ?? GetTheme().Window.DragResizable;
            set => _dragResizable = value;
        }

        // flag to indicate is this is attachable; meaning that after dragging is finished (pointer up), screen
        // calls TryAttach. this is currently only used in docking / dock window.
        public bool Attachable { get; set; } = false;
                
        protected DragMode WindowDragMode => _windowDragMode;

        // these are currently same to all windows
        float _resizingTriangleSize => GetTheme().Windows.ResizingTriangleSize;
        Color _borderResizingColor => GetTheme().Windows.BorderResizingColor;
        Color _borderFocusedColor => GetTheme().Windows.BorderFocusedColor;
        Color _borderUnfocusedColor => GetTheme().Windows.BorderUnfocusedColor;
        uint _dragAreaWidth => GetTheme().Windows.DragAreaWidth;

        #endregion

        #region Methods

        public bool ShowBar<T>(bool show) where T : UIWidgetbar
        {
            var bar = GetBar<T>();

            if( bar != null)
            {
                if(bar.Visible != show)
                {
                    bar.Visible = show;

                    RequestLayoutUpdate(this);
                }
                
                return true;
            }

            return false;
        }

        public T? GetBar<T>() where T : UIWidgetbar
        {
            // not initialized (in theme?)
            if(Children == null)
                return null;

            foreach (var child in Children.AsSpan())
            {
                if(child is T bar)
                {
                    return bar;
                }
            }

            return null;
        }

        // we get layout area for layout impementation (no bars)
        public override Rect GetLayoutArea()
        {
            // we set offset for titlebar
            Vector2 offset = new Vector2(0, _layoutVerticalOffset);

            return new Rect(
                    offset,
                    new Vector2
                    {
                        X = FixedSize.X > 0 ? FixedSize.X : Size.X,
                        Y = FixedSize.Y > 0 ? FixedSize.Y : Size.Y,
                    } - offset);
        }

        // Center the window in the current Screen
        public void Center()
        {
            if (Screen == null || Size == Vector2.Zero)
                return;

            // todo : Screen.Size < Size
            Position = (Screen.Size - Size) / 2;
        }

        #endregion

        #region Events

        // note: everytime we send pointer event to children (base.<OnPointerEvent>), we must recalculate
        // pointer position taking into account scrolling offset (GetChildrenPointerPos)
        // todo: if we are going to drag - we must clear possible previous pointer hovering (pointer focus)
        /// <inheritdoc />
        public override bool OnPointerMove(Vector2 p, Vector2 rel)
        {
            Vector2 localPos = p - Position;

            // check scrolling first
            if(_scrollbars != ScrollbarType.NONE)
            {
                // reset scrollbars focuses
                ResetScrollbarFocuses();

                // get the scrollbar where pointer is
                switch (GetPointerOnScrollbar(localPos))
                {
                    case ScrollbarType.Vertical:
                        _verticalScrollbar!.PointerFocus = true;
                        // pointer type can be other than default - so reset
                        Screen?.ResetPointerType();
                        return true;
                    case ScrollbarType.Horizontal:
                        _horizontalScrollbar!.PointerFocus = true;
                        // pointer type can be other than default - so reset
                        Screen?.ResetPointerType();
                        return true;
                }
            }

            // let the children do work first
            //var res = base.OnPointerMove(GetChildrenPointerPos(p), rel);
            
            bool res = false;

            // check if we change pointer type
            if (Focused && DragResizable)
            {
                uint dragW = _dragAreaWidth;

                if (localPos.X < dragW || localPos.X > Size.X - dragW)
                {
                    SetPointerType((int)PointerType.SizeWE);
                    res = true;
                }
                else if (localPos.Y < dragW || localPos.Y > Size.Y - dragW)
                {
                    SetPointerType((int)PointerType.SizeNS);
                    res = true;
                }
                else
                {
                    // check corner
                    float triangleSize = _resizingTriangleSize;
                    float ds = triangleSize / 2;

                    // Check if in right-bottom corner
                    bool inCorner = MathUtils.IsPointInsideTriangle(
                        localPos,
                        Size,
                        Size - new Vector2(ds, triangleSize),
                        Size - new Vector2(triangleSize, ds));

                    if (inCorner)
                    {
                        SetPointerType((int)PointerType.SizeNWSE);
                        res = true;
                    }
                }
            }

            if (!res)
            {
                res = base.OnPointerMove(GetChildrenPointerPos(p), rel);
            }
            else
            {
                // this closes all popups
                // note: less aggrssive way would be just Screen?.RequestPointerFocus that just
                // changes pointer focus
                //RequestFocus();
            }

            // we restrict pointer move to only topmost window, when pointer "inside" window
            // todo: should we make a restriction also in screen level?
            return true;
        }

        // Handle pointer events recursively and bring the current window to the top
        // Phases:
        // 0. check scrollbars
        // 1. check window drag resizing
        // 2. check window drag moving (header)
        // 3. forward to children
        /// <inheritdoc />
        public override bool OnPointerUpDown(Vector2 p, PointerButton button, bool down)
        {
            _isWindowDragging = false;
            // reset scroll dragging
            _scrollableDragMode = ScrollableDragMode.None;

            Vector2 localPos = p - Position;

            if (button == PointerButton.Left && !Disabled)
            {
                // check scrollbars first
                if (down && _scrollbars != ScrollbarType.NONE)
                {
                    // get scrollbar type where pointer is
                    var scrollbarType = GetPointerOnScrollbar(localPos);

                    if (scrollbarType == ScrollbarType.Vertical)
                    {
                        if (_verticalScrollbar != null)
                        {
                            // vertical
                            float contentHeight = _contentRect.Size.Y;
                            Vector2 size = _verticalScrollbar.Size;

                            int scrollH = (int)(size.Y * MathF.Min(1.0f, size.Y / contentHeight));
                            int start = (int)(_verticalScrollbar.Position.Y + (size.Y - scrollH) * _verticalScrollbar.Scroll);

                            float delta = 0.0f;

                            if (localPos.Y < start)
                                delta = -size.Y / contentHeight;
                            else if (localPos.Y > start + scrollH)
                                delta = size.Y / contentHeight;

                            _verticalScrollbar.Scroll = Math.Clamp(_verticalScrollbar.Scroll + delta, 0, 1);

                            _scrollingDirty = true;

                            // set dragging
                            _scrollableDragMode = ScrollableDragMode.Vertical;

                            // set this as drag widget -> call for OnPointerDrag
                            Screen?.SetDragWidget(this);
                        }

                        return true;
                    }
                    else if (scrollbarType == ScrollbarType.Horizontal)
                    {
                        if(_horizontalScrollbar != null)
                        {
                            // horizontal
                            float contentWidth = _contentRect.Size.X;
                            Vector2 size = _horizontalScrollbar.Size;

                            int scrollW = (int)(size.X * MathF.Min(1.0f, size.X / contentWidth));
                            int start = (int)(_horizontalScrollbar.Position.X + (size.X - scrollW) * _horizontalScrollbar.Scroll);

                            float delta = 0.0f;

                            if (localPos.X < start)
                                delta = -size.X / contentWidth;
                            else if (localPos.X > start + scrollW)
                                delta = size.X / contentWidth;

                            _horizontalScrollbar.Scroll = Math.Clamp(_horizontalScrollbar.Scroll + delta, 0, 1);

                            _scrollingDirty = true;

                            // set dragging
                            _scrollableDragMode = ScrollableDragMode.Horizontal;

                            // set this as drag widget -> call for OnPointerDrag
                            Screen?.SetDragWidget(this);
                        }

                        return true;
                    }
                }

                _isWindowDragging = down;

                // Check drag
                uint dragW = _dragAreaWidth;

                // Header?
                // todo: this is wrong : must also check left & right sides so we can resize from titlebar's sides!
                _windowDragMode = (down && localPos.Y < _titlebarHeight) ? DragMode.Header : DragMode.NONE;

                if (_windowDragMode == DragMode.NONE)
                {
                    // not in header - check borders
                    bool edgeResize = DragResizable;

                    if (edgeResize && localPos.X < dragW)
                        _windowDragMode = DragMode.Left;
                    else if (edgeResize && localPos.X > Size.X - dragW)
                        _windowDragMode = DragMode.Right;
                    else if (edgeResize && localPos.Y > Size.Y - dragW)
                        _windowDragMode = DragMode.Bottom;
                    else
                    {
                        // Check if in right-bottom corner
                        // note: this is focused
                        bool inCorner = Screen?.GetCurrentPointerType() == (int)PointerType.SizeNWSE;

                        if (down && inCorner)
                        {
                            _windowDragMode = DragMode.RightBottomCorner;
                        }
                    }
                }
                else if (DragResizable)
                {
                    // we are in header - check top (is needed?)
                    if (localPos.Y < dragW)
                        _windowDragMode = DragMode.Top;
                }

                // if we are really dragging - set this as drag widget
                if (_isWindowDragging && _windowDragMode != DragMode.NONE)
                {
                    // when window gets first time focus (maybe user clicked from header)
                    // we don't move to dragging mode
                    if (!Focused)
                    {
                        _isWindowDragging = false;
                    }

                    // supports attaching dock window after dragging is finished 
                    if(Attachable && _windowDragMode == DragMode.Header)
                    {
                        Screen?.SetDragWidget(this, true);
                    }
                    else
                    {
                        Screen?.SetDragWidget(this, false);
                    }

                    // window takes control - this closes also all possible popups
                    RequestFocus();

                    return true;
                }
            }

            // no window dragging - check children
            var ret = base.OnPointerUpDown(GetChildrenPointerPos(p), button, down);
            
            // if any child doesn't want to handle event - this (window) takes control
            // note: we use _isWindowDragging (instead of down)
            if (!ret && _isWindowDragging)
            {
                // window takes control - this closes also all possible popups
                RequestFocus();
            }

            // prevent propagating to other windows (like clicking other window's close button)
            return true;
        }

        /// <inheritdoc />
        public override bool OnPointerDrag(Vector2 p, Vector2 rel)
        {
            // check scrollbars first
            if (_scrollbars != ScrollbarType.NONE)
            {
                // check if we are dragging
                if (_verticalScrollbar != null && _scrollableDragMode == ScrollableDragMode.Vertical)
                {
                    Vector2 size = _verticalScrollbar.Size;

                    float scrollH = size.Y * Math.Min(1.0f, size.Y / _contentRect.Size.Y);

                    _verticalScrollbar.Scroll = Math.Clamp(_verticalScrollbar.Scroll + rel.Y / (size.Y - scrollH), 0, 1);

                    _scrollingDirty = true;

                    return true;
                }
                else if (_horizontalScrollbar != null && _scrollableDragMode == ScrollableDragMode.Horizontal)
                {
                    Vector2 size = _horizontalScrollbar.Size;

                    float scrollW = size.X * Math.Min(1.0f, size.X / _contentRect.Size.X);

                    _horizontalScrollbar.Scroll = Math.Clamp(_horizontalScrollbar.Scroll + rel.X / (size.X - scrollW), 0, 1);

                    _scrollingDirty = true;

                    return true;
                }
            }

            if (!Draggable)
                return false;

            if (_windowDragMode == DragMode.Header)
            {
                Position += rel;
                // prevents to move outside screen boundaries
                // todo : take into account mainmenubar, maintoolbars & mainstatusbar
                // (= get screen client size)
                Position = Vector2.Max(Position, Vector2.Zero);
                if(Parent != null)
                    Position = Vector2.Min(Position, Parent.Size - Size);

                return true;
            }
            else if (DragResizable)
            {
                // todo : Must also test limits (width, height, positionX, positionY > 0)
                switch (_windowDragMode)
                {
                    case DragMode.Top:
                        {
                            Position += new Vector2(0, rel.Y);

                            Size -= new Vector2(0, rel.Y);

                            _layoutDirty = true;

                            return true;
                        }
                    case DragMode.Left:
                        {
                            Position += new Vector2(rel.X, 0);

                            Size -= new Vector2(rel.X, 0);

                            _layoutDirty = true;

                            return true;
                        }
                    case DragMode.Right:
                        {
                            Size += new Vector2(rel.X, 0);
                            _layoutDirty = true;

                            return true;
                        }
                    case DragMode.Bottom:
                        {
                            Size += new Vector2(0, rel.Y);

                            _layoutDirty = true;

                            return true;
                        }
                    case DragMode.RightBottomCorner:
                        {
                            Size += rel;
                            Size = Vector2.Max(Size, new Vector2(15, _layoutVerticalOffset));

                            _layoutDirty = true;

                            return true;
                        }
                }
            }

            // todo: send to children?

            return false;
        }

        /// <inheritdoc />
        public override bool OnPointerScroll(Vector2 p, Vector2 scroll)
        {
            // todo: should we handle scrollbar scrolling like in UIScrollPanel?

            base.OnPointerScroll(GetChildrenPointerPos(p), scroll);
            
            // this restrict event executing only in topmost window
            // can be modal or not
            return true;
        }

        /// <inheritdoc />
        public override bool OnPointerDoubleClick(Vector2 p, PointerButton button)
        {
            return base.OnPointerDoubleClick(GetChildrenPointerPos(p), button);
        }

        // check also key events - shall we response to them
        // note: there is special case for menubar, we pass event to it if found. else passes event to base,
        // that by default returns false
        /// <inheritdoc />
        public override bool OnKeyUpDown(Key key, bool down, KeyModifiers modifiers)
        {
            foreach (var child in Children.AsReadOnlySpan())
            {
                if (!child.Visible || child.Disabled)
                    continue;

                if(child is UIMenubar)
                {
                    if(child.OnKeyUpDown(key, down, modifiers))
                    {
                        return true;
                    }
                }
            }

            return base.OnKeyUpDown(key, down, modifiers);
        }

        /// <inheritdoc />
        public override bool OnKeyChar(char c)
        {
            return base.OnKeyChar(c);
        }

        /// <inheritdoc />
        public override bool OnFocusChanged(bool focused)
        {
            ResetScrollbarFocuses();

            return base.OnFocusChanged(focused);
        }

        #endregion

        #region Layout

        // todo: this could be optimized?
        /// <inheritdoc />
        public override void PerformLayout(NvgContext ctx)
        {
            // calculate offset area Y (all bars)
            _layoutVerticalOffset = 0;
            _titlebarHeight = 0;

            // collect first bars info
            int[] bars = Array.Empty<int>();
            Vector2[] sizeY = Array.Empty<Vector2>();
            int counter = 0;

            foreach (var child in Children.AsSpan())
            {
                if (child.Visible && child is UIWidgetbar)
                {
                    Array.Resize(ref bars, bars.Length + 1);
                    Array.Resize(ref sizeY, sizeY.Length + 1);

                    bars[bars.Length - 1] = counter;

                    // increase offset
                    var prefSize = child.PreferredSize(ctx);
                    sizeY[sizeY.Length - 1] = prefSize;

                    _layoutVerticalOffset += prefSize.Y;

                    if(child is UITitlebar)
                    {
                        // if titlebar speacial handling OnPointerUpDown
                        _titlebarHeight = prefSize.Y;
                    }

                    // temporarily hide
                    child.Visible = false;
                }

                counter++;
            }

            // perform layout with normal widgets
            base.PerformLayout(ctx);

            // perform layout with bars?
            if(bars.Length > 0)
            {
                float posY = 0;

                for (int i = 0; i < bars.Length; i++)
                {
                    // get the bar widget
                    ref var bar = ref Children[bars[i]];

                    // bring back bar
                    bar.Visible = true;
                    // manually set pos
                    bar.Position = new Vector2(0, posY);
                    // set size
                    bar.Size = sizeY[i];
                    // perform layout in bar self
                    bar.PerformLayout(ctx);

                    // increase pos
                    posY += bar.Size.Y;
                }
            }

            // if this is called somewhere else, reset flag since we have done layouting
            _layoutDirty = false;

            if(_scrollbars != ScrollbarType.NONE)
            {
                // layout recalculated - set scollbars position & size
                uint dimension = GetTheme().Scrollbars.ScrollbarDimension;

                var borderOffset = BorderSize;

                if (_verticalScrollbar != null)
                {
                    _verticalScrollbar.Position = new Vector2(Size.X - dimension - borderOffset, _layoutVerticalOffset);
                    _verticalScrollbar.Size = new Vector2(dimension, Size.Y - _layoutVerticalOffset - borderOffset * 2);
                }

                if (_horizontalScrollbar != null)
                {
                    _horizontalScrollbar.Position = new Vector2(borderOffset, Size.Y - dimension - borderOffset);
                    _horizontalScrollbar.Size = new Vector2(Size.X - dimension - borderOffset * 2, dimension);
                }

                // calculate max bounds of child widgets
                Vector2 max = new Vector2(float.MinValue);

                foreach (var child in Children.AsSpan())
                {
                    if (child is UITitlebar || !child.Visible)
                        continue;

                    max = Vector2.Max(max, child.Position + child.Size);
                }

                // reduce bars height
                _contentRect.Size = max - new Vector2(0, _layoutVerticalOffset);

                // must recalculate offset & slider rects
                CalculateSliderRects();
            }
        }

        #endregion

        #region Drawing

        // todo: this could b simplified?
        /// <inheritdoc />
        public override void Draw(NvgContext ctx)
        {
            // Check if we need perform layout (this originates from resizing event - OnPointerDrag)
            // or scrolling
            if (_layoutDirty)
            {
                // perform layout resets flag
                PerformLayout(ctx);
            }
            else if (_scrollingDirty && _scrollbars != ScrollbarType.NONE)
            {
                // layout not changed - no need to calculate content size
                CalculateSliderRects();
            }

            // Background
            DrawBackgroundBrush(ctx);

            // check if we have some content offset specified
            if (_scrollbars == ScrollbarType.NONE || _contentRect.Position == Vector2.Zero)
            {
                base.Draw(ctx);
            }
            else
            {
                // delayed bars drawing - collect all bars indexes so they are not offsetted &
                // we can overdraw them to other child widgets
                int[] bars = Array.Empty<int>();
                int counter = 0;

                foreach (var child in Children.AsSpan())
                {
                    if (child.Visible && child is UIWidgetbar)
                    {
                        Array.Resize(ref bars, bars.Length + 1);

                        // temporarily hide
                        child.Visible = false;

                        bars[bars.Length - 1] = counter;
                    }

                    counter++;
                }

                // draw normal widgets with content rect offset
                ctx.SaveState();
                ctx.Translate(_contentRect.Position);

                base.Draw(ctx);

                ctx.RestoreState();

                // bars overdrawing?
                if(bars.Length > 0)
                {
                    ctx.SaveState();
                    ctx.Translate(Position);

                    for (int i = 0; i < bars.Length; i++)
                    {
                        // get the bar widget
                        ref var bar = ref Children[bars[i]];

                        bar.Visible = true;
                        bar.Draw(ctx);
                    }
                    
                    ctx.RestoreState();
                }
            }

            // Scrollbars
            if (_scrollbars != ScrollbarType.NONE)
            {
                ctx.SaveState();
                ctx.Translate(Position);

                // vertical
                _verticalScrollbar?.DrawBackground(ctx, Disabled);
                _verticalScrollbar?.DrawSlider(ctx, _vertSlider.Position, _vertSlider.Size, _scrollableDragMode == ScrollableDragMode.Vertical);

                // horizontal
                _horizontalScrollbar?.DrawBackground(ctx, Disabled);
                _horizontalScrollbar?.DrawSlider(ctx, _horizontalSlider.Position, _horizontalSlider.Size, _scrollableDragMode == ScrollableDragMode.Horizontal);

                ctx.RestoreState();
            }

            // corner triangle
            // todo : check if FixedSize set =>
            // restrict dragging in horizontal/vertical/none

            // todo : should there be ResetScissor, if there is some ill-behaving
            // widget that doesn't reset its state (scissor) after drawing?
            // (could be in widget childen drawing code?)

            // todo: magical numbers

            if (DragResizable)
            {
                float triangleSize = _resizingTriangleSize;

                // check if pointer is in corner
                bool inCorner = Focused && Screen?.GetCurrentPointerType() == (int)PointerType.SizeNWSE;

                ctx.BeginPath();

                ctx.MoveTo(Position.X + Size.X - triangleSize, Position.Y + Size.Y - 2);
                ctx.LineTo(Position.X + Size.X - 2, Position.Y + Size.Y - triangleSize);
                ctx.LineTo(Position.X + Size.X - 2, Position.Y + Size.Y - 5);
                ctx.LineTo(Position.X + Size.X - 5, Position.Y + Size.Y - 2);

                ctx.ClosePath();

                ctx.FillColor(inCorner ? _borderResizingColor : Focused ? _borderFocusedColor : _borderUnfocusedColor);
                ctx.Fill();
            }

            // draw border
            if(_isWindowDragging && _windowDragMode != DragMode.NONE)
            {
                // resizing (should we have factor configurable?)
                this.DrawBorder(ctx, BorderSize * 2.5f, _borderResizingColor);
            }
            else if (BorderSize > 0)
            {
                // draw normal border
                this.DrawBorder(ctx, BorderSize,
                    Focused ? _borderFocusedColor : _borderUnfocusedColor);
            }
        }

        #endregion

        #region Private

        // when we send pointer event to children (base.<OnPointerEvent>), we must recalculate
        // pointer position taking into account scrolling offset
        Vector2 GetChildrenPointerPos(Vector2 pointerPos)
        {
            // no scrolling offset set or bars
            if(_contentRect.Position == Vector2.Zero || pointerPos.Y - Position.Y <= _layoutVerticalOffset)
            {
                return pointerPos;
            }

            // content rect position is negative, but we want to add to get right position
            return pointerPos - _contentRect.Position;
        }

        void ResetScrollbarFocuses()
        {
            if (_verticalScrollbar != null)
                _verticalScrollbar.PointerFocus = false;

            if (_horizontalScrollbar != null)
                _horizontalScrollbar.PointerFocus = false;
        }

        // check is pointer is in scrollbar
        // note: we use slider rects to check this so there is "room" for border resizing
        ScrollbarType GetPointerOnScrollbar(Vector2 localPosition)
        {
            // we check vertical first since it is more common
            if (_verticalScrollbar != null && _verticalScrollbar.Visible &&
                _vertSlider.Contains(localPosition))
            {
                return ScrollbarType.Vertical;
            }
            else if (_horizontalScrollbar != null && _horizontalScrollbar.Visible &&
                _horizontalSlider.Contains(localPosition))
            {
                return ScrollbarType.Horizontal;
            }

            return ScrollbarType.NONE;
        }

        // when scrolling or layout changes, we must recalculate slider rects & content offset
        void CalculateSliderRects()
        {
            _scrollingDirty = false;

            if (_scrollbars == ScrollbarType.NONE)
            {
                // no need to calculate scrolling values
                return;
            }

            // content offset
            float offsetX = 0;
            float offsetY = 0;

            if (_horizontalScrollbar != null)
            {
                // set visibility (if not visible no need to calculate slider rect)
                _horizontalScrollbar.Visible = Size.X < _contentRect.Size.X;

                if (_horizontalScrollbar.Visible)
                {
                    Vector2 size = _horizontalScrollbar.Size;
                    float scrollW = size.X * Math.Min(1.0f, size.X / _contentRect.Size.X);

                    _horizontalSlider = new Rect(
                        (size.X - scrollW) * _horizontalScrollbar.Scroll,
                        _horizontalScrollbar.Position.Y,
                        scrollW,
                        _horizontalScrollbar.Dimension);

                    // calculate offset
                    offsetX = -_horizontalScrollbar.Scroll * (_contentRect.Size.X - size.X);
                }
            }

            if (_verticalScrollbar != null)
            {
                // set visibility (if not visible no need to calculate slider rect)
                _verticalScrollbar.Visible = Size.Y < _contentRect.Size.Y + _layoutVerticalOffset;

                if (_verticalScrollbar.Visible)
                {
                    Vector2 size = _verticalScrollbar.Size;
                    float scrollH = size.Y * Math.Min(1.0f, size.Y / _contentRect.Size.Y);

                    // offset Y position
                    _vertSlider = new Rect(
                        _verticalScrollbar.Position.X,
                        (size.Y - scrollH) * _verticalScrollbar.Scroll + _verticalScrollbar.Position.Y,
                        _verticalScrollbar.Dimension,
                        scrollH);

                    // calculate offset
                    offsetY = -_verticalScrollbar.Scroll * (_contentRect.Size.Y - size.Y);
                }
            }

            // set offset
            _contentRect.Position = new Vector2(offsetX, offsetY);
        }

        #endregion
    }
}
