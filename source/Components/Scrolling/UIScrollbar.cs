using NanoUI.Common;
using NanoUI.Nvg;
using System;
using System.Numerics;

namespace NanoUI.Components.Scrolling
{
    // note: we operate in same coordinate system as the owner (widget that created this and holds reference)
    // todo: could also extend Widget?
    public class UIScrollbar
    {
        // owner widget
        UIWidget _owner;

        public UIScrollbar(UIWidget owner, Orientation orientation)
        {
            _owner = owner;
            Orientation = orientation;
        }

        #region Properties

        // todo: these are like in Widget
        public bool Visible { get; set; }
        public bool PointerFocus { get; set; }
        public Vector2 Position { get; set; }

        Vector2 _size;
        public Vector2 Size
        {
            get => _size;
            set
            {
                if (_size == value)
                    return;

                _size = value;
            }
        }

        // current scroll values (0 - 1)
        float _scroll;
        public float Scroll
        {
            get => _scroll;
            set => _scroll = Math.Clamp(value, 0, 1);
        }

        public Orientation Orientation { get; private set; }

        uint? _dimension;
        public uint Dimension
        {
            get => _dimension ?? _owner.GetTheme().Scrollbars.ScrollbarDimension;
            set => _dimension = value;
        }

        BrushBase? _backgroundEnabled;
        public BrushBase? BackgroundEnabled
        {
            get => _backgroundEnabled ?? _owner.GetTheme().Scrollbars.BackgroundEnabled;
            set => _backgroundEnabled = value;
        }

        BrushBase? _backgroundDisabled;
        public BrushBase? BackgroundDisabled
        {
            get => _backgroundDisabled ?? _owner.GetTheme().Common.BackgroundDisabled;
            set => _backgroundDisabled = value;
        }

        BrushBase? _sliderBrush;
        public BrushBase? SliderBrush
        {
            get => _sliderBrush != null ? _sliderBrush : _owner.GetTheme().Scrollbars.SliderBrush;
            set => _sliderBrush = value;
        }

        Color? _sliderHoverTint;
        public Color SliderHoverTint
        {
            get => _sliderHoverTint ?? _owner.GetTheme().Scrollbars.SliderHoverTint;
            set => _sliderHoverTint = value;
        }

        #endregion

        #region Methods

        public virtual bool Contains(Vector2 position)
        {
            var d = position - Position;

            return d.X >= 0 && d.Y >= 0 &&
                   d.X < Size.X && d.Y < Size.Y;
        }

        #endregion

        #region Layout

        // after parent has set it size, call this to position scrollbar
        /// <inheritdoc />
        public void PerformLayout(NvgContext ctx, bool overflowX, bool overflowY)
        {
            var dimension = Dimension;

            if (Orientation == Orientation.Vertical)
            {
                Visible = overflowY;

                if (Visible)
                {
                    Position = new Vector2(_owner.Position.X + _owner.Size.X - dimension, _owner.Position.Y);
                    Size = new Vector2(dimension, _owner.Size.Y - dimension);
                }
            }
            else
            {
                Visible = overflowX;

                if (Visible)
                {
                    Position = new Vector2(_owner.Position.X, _owner.Position.Y + _owner.Size.Y - dimension);
                    Size = new Vector2(_owner.Size.X - dimension, dimension);
                }
            }
        }

        #endregion

        #region Drawing

        public void DrawBackground(NvgContext ctx, bool disabled)
        {
            if (!Visible)
                return;

            if (!disabled)
            {
                BackgroundEnabled?.Draw(ctx, Position, Size, null);
            }
            else
            {
                BackgroundDisabled?.Draw(ctx, Position, Size, null);
            }
        }

        public void DrawSlider(NvgContext ctx, Vector2 topLeft, Vector2 size, bool dragging)
        {
            if (!Visible)
                return;

            SliderBrush?.Draw(ctx, topLeft, size, dragging || PointerFocus ? SliderHoverTint : null);
        }

        #endregion
    }
}
