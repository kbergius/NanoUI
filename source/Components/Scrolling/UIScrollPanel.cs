using NanoUI.Common;
using NanoUI.Layouts;
using NanoUI.Nvg;
using System;
using System.Numerics;

namespace NanoUI.Components.Scrolling
{
    // todo: handle Content == null
    // todo: make this behave like UIWindow scrolling & implement IScrollable

    /// <summary>
    /// UIScrollPanel.
    /// </summary>
    public class UIScrollPanel : UIWidget
    {
        // todo: margin for container <--> scrollbar
        const int MARGIN = 0;

        ScrollbarType _scrollableType;

        Rect _vertSlider;
        Rect _horizontalSlider;

        UIScrollbar? _horizontalScrollbar;
        UIScrollbar? _verticalScrollbar;

        // should we update scroll content?
        bool _updateLayout;

        /// <inheritdoc />
        public UIScrollPanel()
        {

        }

        /// <inheritdoc />
        public UIScrollPanel(UIWidget parent, ScrollbarType scrollbars = ScrollbarType.Both)
            : base(parent)
        {
            _scrollableType = scrollbars;

            if (scrollbars == ScrollbarType.Horizontal || scrollbars == ScrollbarType.Both)
            {
                _horizontalScrollbar = new UIScrollbar(this, Orientation.Horizontal);
            }

            if (scrollbars == ScrollbarType.Vertical || scrollbars == ScrollbarType.Both)
            {
                _verticalScrollbar = new UIScrollbar(this, Orientation.Vertical);
            }
        }

        #region Properties

        /// <summary>
        /// Horizontal scrollbar
        /// </summary>
        public UIScrollbar? HorizontalScrollbar => _horizontalScrollbar;

        /// <summary>
        /// Vertical scrollbar
        /// </summary>
        public UIScrollbar? VerticalScrollbar => _verticalScrollbar;

        /// <inheritdoc />
        public override Layout? ChildrenLayout
        {
            get => GetScrollContent().ChildrenLayout;
            set => GetScrollContent().ChildrenLayout = value;
        }

        // dragging
        /// <summary>
        /// ScrollableDragMode. Default: ScrollableDragMode.None.
        /// </summary>
        protected ScrollableDragMode ScrollableDragMode { get; set; } = ScrollableDragMode.None;

        Vector2 _contentPreferredSize;

        /// <summary>
        /// Content's preferred size
        /// </summary>
        public Vector2 ContentPreferredSize
        {
            get => _contentPreferredSize;
            private set => _contentPreferredSize = value;
        }

        #endregion

        #region Layout

        /// <inheritdoc />
        public override Vector2 PreferredSize(NvgContext ctx)
        {
            if (GetScrollContent() == null)
                return Vector2.Max(MinSize, Vector2.Zero);

            return base.PreferredSize(ctx);
        }

        /// <inheritdoc />
        public override void PerformLayout(NvgContext ctx)
        {
            if (GetScrollContent() == null)
            {
                return;
            }

            // get the size - so this size adjust with parent width change
            // todo: height change?
            Vector2 size = new Vector2
            {
                X = FixedSize.X > 0 ? FixedSize.X : Size.X,
                Y = FixedSize.Y > 0 ? FixedSize.Y : Size.Y,
            };

            // perform layout (container)
            base.PerformLayout(ctx);

            ContentPreferredSize = GetScrollContent().PreferredSize(ctx);

            // set size back
            Size = size;

            UIWidget child = GetScrollContent();

            bool overflowX = ContentPreferredSize.X > Size.X;
            bool overflowY = ContentPreferredSize.Y > Size.Y;

            if (_scrollableType == ScrollbarType.Vertical)
            {
                if (_verticalScrollbar != null)
                {
                    if (overflowY)
                    {
                        child.Position = new Vector2(0, -_verticalScrollbar.Scroll * (ContentPreferredSize.Y - Size.Y));
                        // todo: should we set some margin
                        child.Size = new Vector2(Size.X - _verticalScrollbar.Dimension, ContentPreferredSize.Y);
                    }
                    else
                    {
                        child.Position = Vector2.Zero;
                        child.Size = Size;
                        _verticalScrollbar.Scroll = 0;
                    }
                }
            }
            else if (_scrollableType == ScrollbarType.Horizontal)
            {
                if (_horizontalScrollbar != null)
                {
                    if (overflowX)
                    {
                        child.Position = new Vector2(-_horizontalScrollbar.Scroll * (ContentPreferredSize.X - Size.X), 0);
                        // todo: should we set some margin
                        child.Size = new Vector2(ContentPreferredSize.X, Size.Y - _horizontalScrollbar.Dimension);
                    }
                    else
                    {
                        child.Position = Vector2.Zero;
                        child.Size = Size;
                        _horizontalScrollbar.Scroll = 0;
                    }
                }
            }
            else
            {
                if (_horizontalScrollbar != null && _verticalScrollbar != null)
                {
                    // both
                    if (overflowX || overflowY)
                    {
                        child.Position = new Vector2
                        {
                            X = overflowX ? -_horizontalScrollbar.Scroll * (ContentPreferredSize.X - Size.X) : 0,
                            Y = overflowY ? -_verticalScrollbar.Scroll * (ContentPreferredSize.Y - Size.Y) : 0
                        };

                        if (overflowX && overflowY)
                        {
                            child.Size = ContentPreferredSize;
                        }
                        else if (overflowX)
                        {
                            child.Size = new Vector2(ContentPreferredSize.X, Size.Y - _horizontalScrollbar.Dimension);
                        }
                        else if (overflowY)
                        {
                            child.Size = new Vector2(Size.X - _verticalScrollbar.Dimension, ContentPreferredSize.Y);
                        }
                    }
                    else
                    {
                        child.Position = Vector2.Zero;
                        child.Size = Size;
                        _horizontalScrollbar.Scroll = 0;
                        _verticalScrollbar.Scroll = 0;
                    }
                }
            }

            // perform layout in scrollbars
            _horizontalScrollbar?.PerformLayout(ctx, overflowX, overflowY);
            _verticalScrollbar?.PerformLayout(ctx, overflowX, overflowY);

            // perform layout in content (new size!)
            GetScrollContent().PerformLayout(ctx);

            CalculateSliderRects();
        }

        #endregion

        #region Events

        /// <inheritdoc />
        public override bool OnPointerUpDown(Vector2 p, PointerButton button, bool down)
        {
            if (Disabled)
                return false;

            // reset dragging
            ScrollableDragMode = ScrollableDragMode.None;

            if (down && button == (int)PointerButton.Left && GetScrollContent() != null)
            {
                // check if we are in scrollbar area
                if (ContentPreferredSize.Y > Size.Y && VerticalScrollbar != null && VerticalScrollbar.Contains(p))
                {
                    // vertical
                    int scrollH = (int)(Size.Y * MathF.Min(1.0f, Size.Y / ContentPreferredSize.Y));
                    int start = (int)(Position.Y + (Size.Y - scrollH) * VerticalScrollbar.Scroll);

                    float delta = 0.0f;

                    if (p.Y < start)
                        delta = -Size.Y / ContentPreferredSize.Y;
                    else if (p.Y > start + scrollH)
                        delta = Size.Y / ContentPreferredSize.Y;

                    VerticalScrollbar.Scroll = Math.Clamp(VerticalScrollbar.Scroll + delta, 0, 1);

                    _updateLayout = true;

                    // set dragging
                    ScrollableDragMode = ScrollableDragMode.Vertical;

                    // set this as drag widget -> call for OnPointerDrag
                    Screen?.SetDragWidget(this);

                    return true;
                }
                else if (ContentPreferredSize.X > Size.X && HorizontalScrollbar != null && HorizontalScrollbar.Contains(p))
                {
                    // horizontal
                    int scrollW = (int)(Size.X * MathF.Min(1.0f, Size.X / ContentPreferredSize.X));
                    int start = (int)(Position.X + (Size.X - scrollW) * HorizontalScrollbar.Scroll);

                    float delta = 0.0f;

                    if (p.X < start)
                        delta = -Size.X / ContentPreferredSize.X;
                    else if (p.X > start + scrollW)
                        delta = Size.X / ContentPreferredSize.X;

                    HorizontalScrollbar.Scroll = Math.Clamp(HorizontalScrollbar.Scroll + delta, 0, 1);

                    _updateLayout = true;

                    // set dragging
                    ScrollableDragMode = ScrollableDragMode.Horizontal;

                    // set this as drag widget -> call for OnPointerDrag
                    Screen?.SetDragWidget(this);

                    return true;
                }
            }

            // send to child
            return base.OnPointerUpDown(p, button, down);
        }

        /// <inheritdoc />
        public override bool OnPointerDrag(Vector2 p, Vector2 rel)
        {
            // check if we are dragging
            if (_verticalScrollbar != null && ScrollableDragMode == ScrollableDragMode.Vertical)
            {
                float scrollH = base.Size.Y * Math.Min(1.0f, base.Size.Y / GetScrollContent().Size.Y);

                _verticalScrollbar.Scroll = Math.Clamp(_verticalScrollbar.Scroll + rel.Y / (base.Size.Y - scrollH), 0, 1);

                _updateLayout = true;

                return true;
            }
            else if (_horizontalScrollbar != null && ScrollableDragMode == ScrollableDragMode.Horizontal)
            {
                float scrollW = base.Size.X * Math.Min(1.0f, base.Size.X / GetScrollContent().Size.X);

                _horizontalScrollbar.Scroll = Math.Clamp(_horizontalScrollbar.Scroll + rel.X / (base.Size.X - scrollW), 0, 1);

                _updateLayout = true;

                return true;
            }

            return base.OnPointerDrag(p, rel);
        }

        /// <inheritdoc />
        public override bool OnPointerScroll(Vector2 p, Vector2 scroll)
        {
            if (Disabled)
                return false;

            if (GetScrollContent() != null)
            {
                // check if we are in scrollbar area (or handle all area?)

                if (VerticalScrollbar != null && VerticalScrollbar.Contains(p) && ContentPreferredSize.Y > Size.Y)
                {
                    // vertical

                    // todo : scroll "intensity" with pointer scroll
                    // todo: magical numbers
                    float scrollAmount = scroll.Y * (Size.Y / 20.0f);
                    float scrollH = Size.Y * Math.Min(1.0f, Size.Y / ContentPreferredSize.Y);

                    VerticalScrollbar.Scroll = Math.Clamp(VerticalScrollbar.Scroll - scrollAmount / (Size.Y - scrollH), 0.0f, 1.0f);

                    _updateLayout = true;

                    return true;
                }
                else if (HorizontalScrollbar != null && HorizontalScrollbar.Contains(p) && ContentPreferredSize.X > Size.X)
                {
                    // horizontal
                    float scrollAmount = scroll.X * (Size.X / 20.0f);
                    float scrollW = Size.X * Math.Min(1.0f, Size.X / ContentPreferredSize.X);

                    HorizontalScrollbar.Scroll = Math.Clamp(HorizontalScrollbar.Scroll - scrollAmount / (Size.X - scrollW), 0.0f, 1.0f);

                    _updateLayout = true;

                    return true;
                }
            }

            return base.OnPointerScroll(p, scroll);
        }

        /// <inheritdoc />
        public override bool OnPointerMove(Vector2 p, Vector2 rel)
        {
            if (VerticalScrollbar != null)
                VerticalScrollbar.PointerFocus = false;

            if (HorizontalScrollbar != null)
                HorizontalScrollbar.PointerFocus = false;

            if (Disabled)
                return false;

            // todo: is this needed? we check first if this is in dragging mode
            //if (ScrollableDragMode != ScrollableDragMode.None)
            //    return true;

            // check if scrollbars has pointer focus
            if (VerticalScrollbar != null && VerticalScrollbar.Contains(p))
            {
                VerticalScrollbar.PointerFocus = true;
                return true;
            }
            else if (HorizontalScrollbar != null && HorizontalScrollbar.Contains(p))
            {
                HorizontalScrollbar.PointerFocus = true;
                return true;
            }

            return base.OnPointerMove(p, rel);
        }

        /// <inheritdoc />
        public override void OnPointerEnter(bool enter)
        {
            base.OnPointerEnter(enter);

            if (!enter)
            {
                ScrollableDragMode = ScrollableDragMode.None;
            }
        }

        #endregion

        #region Drawing

        /// <inheritdoc />
        public override void Draw(NvgContext ctx)
        {
            if (GetScrollContent() == null)
                return;

            if (_updateLayout)
            {
                if(VerticalScrollbar != null)
                {
                    GetScrollContent().Position = new Vector2(GetScrollContent().Position.X, -VerticalScrollbar.Scroll * (ContentPreferredSize.Y - Size.Y));
                }

                if (HorizontalScrollbar != null)
                {
                    GetScrollContent().Position = new Vector2(-HorizontalScrollbar.Scroll * (ContentPreferredSize.X - Size.X), GetScrollContent().Position.Y);
                }

                CalculateSliderRects();

                GetScrollContent().PerformLayout(ctx);
                _updateLayout = false;
            }

            ctx.SaveState();
            ctx.Translate(Position);

            // Create scissor
            var sizeX = _verticalScrollbar != null && _verticalScrollbar.Visible ?
                Size.X - _verticalScrollbar.Dimension - MARGIN : Size.X;

            var sizeY = _horizontalScrollbar != null && _horizontalScrollbar.Visible ?
                Size.Y - _horizontalScrollbar.Dimension - MARGIN : Size.Y;

            ctx.IntersectScissor(0, 0, sizeX, sizeY);

            if (GetScrollContent().Visible)
                GetScrollContent().Draw(ctx);

            ctx.RestoreState();

            // draw scrollbars over if necessary

            // vertical
            _verticalScrollbar?.DrawBackground(ctx, Disabled);
            //_verticalScrollbar?.DrawSlider(ctx, ScrollableDragMode == ScrollableDragMode.Vertical);
            _verticalScrollbar?.DrawSlider(ctx, _vertSlider.Position, _vertSlider.Size, ScrollableDragMode == ScrollableDragMode.Vertical);

            // horizontal
            _horizontalScrollbar?.DrawBackground(ctx, Disabled);
            //_horizontalScrollbar?.DrawSlider(ctx, ScrollableDragMode == ScrollableDragMode.Horizontal);
            _horizontalScrollbar?.DrawSlider(ctx, _horizontalSlider.Position, _horizontalSlider.Size, ScrollableDragMode == ScrollableDragMode.Horizontal);

            // border
            this.DrawBorder(ctx, true);
        }

        #endregion

        #region Private

        // container where user can put widgets
        UIWidget GetScrollContent()
        {
            // we create content it not exists
            if (Children.Count == 0)
            {
                new UIWidget(this);
            }

            return Children[0];
        }

        // when scrolling or layout changes, we must recalculate slider rects
        void CalculateSliderRects()
        {
            if (_horizontalScrollbar != null)
            {
                // horizontal
                float scrollW = Size.X * Math.Min(1.0f, Size.X / ContentPreferredSize.X);

                _horizontalSlider = new Rect(
                    Position.X + (Size.X - scrollW) * _horizontalScrollbar.Scroll,
                    Position.Y + Size.Y - _horizontalScrollbar.Dimension,
                    scrollW,
                    _horizontalScrollbar.Dimension);
            }
            
            if (_verticalScrollbar != null)
            {
                float scrollH = Size.Y * Math.Min(1.0f, Size.Y / ContentPreferredSize.Y);

                _vertSlider = new Rect(
                    Position.X + Size.X - _verticalScrollbar.Dimension,
                    Position.Y + (Size.Y - scrollH) * _verticalScrollbar.Scroll,
                    _verticalScrollbar.Dimension,
                    scrollH);
            }
        }

        #endregion
    }
}
