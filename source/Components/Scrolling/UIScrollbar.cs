using NanoUI.Common;
using NanoUI.Nvg;
using System;
using System.Numerics;

namespace NanoUI.Components.Scrolling
{
    // todo: could also extend Widget?

    /// <summary>
    /// UIScrollbar.
    /// Note: UIScrollbar operates in the same coordinate system as the owner
    /// (widget that created this and holds reference).
    /// </summary>
    public class UIScrollbar
    {
        // owner widget
        UIWidget _owner;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="owner">UIWidget</param>
        /// <param name="orientation">Orientation</param>
        public UIScrollbar(UIWidget owner, Orientation orientation)
        {
            _owner = owner;
            Orientation = orientation;
        }

        #region Properties

        // todo: these are like in Widget!

        /// <summary>
        /// Visible
        /// </summary>
        public bool Visible { get; set; }

        /// <summary>
        /// PointerFocus
        /// </summary>
        public bool PointerFocus { get; set; }

        /// <summary>
        /// Position
        /// </summary>
        public Vector2 Position { get; set; }

        Vector2 _size;

        /// <summary>
        /// Size
        /// </summary>
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

        float _scroll;

        /// <summary>
        /// Scroll value is in range [0..1].
        /// </summary>
        public float Scroll
        {
            get => _scroll;
            set => _scroll = Math.Clamp(value, 0, 1);
        }

        /// <summary>
        /// Orientation
        /// </summary>
        public Orientation Orientation { get; private set; }

        uint? _dimension;

        /// <summary>
        /// Dimension
        /// </summary>
        public uint Dimension
        {
            get => _dimension ?? _owner.GetTheme().Scrollbars.ScrollbarDimension;
            set => _dimension = value;
        }

        BrushBase? _backgroundEnabled;

        /// <summary>
        /// Background enabled brush
        /// </summary>
        public BrushBase? BackgroundEnabled
        {
            get => _backgroundEnabled ?? _owner.GetTheme().Scrollbars.BackgroundEnabled;
            set => _backgroundEnabled = value;
        }

        BrushBase? _backgroundDisabled;

        /// <summary>
        /// Background disabled brush
        /// </summary>
        public BrushBase? BackgroundDisabled
        {
            get => _backgroundDisabled ?? _owner.GetTheme().Common.BackgroundDisabled;
            set => _backgroundDisabled = value;
        }

        BrushBase? _sliderBrush;

        /// <summary>
        /// Slider brush
        /// </summary>
        public BrushBase? SliderBrush
        {
            get => _sliderBrush != null ? _sliderBrush : _owner.GetTheme().Scrollbars.SliderBrush;
            set => _sliderBrush = value;
        }

        Color? _sliderHoverTint;

        /// <summary>
        /// Slider hover tint color
        /// </summary>
        public Color SliderHoverTint
        {
            get => _sliderHoverTint ?? _owner.GetTheme().Scrollbars.SliderHoverTint;
            set => _sliderHoverTint = value;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Contains
        /// </summary>
        /// <param name="position">Position</param>
        /// <returns>Success</returns>
        public virtual bool Contains(Vector2 position)
        {
            var d = position - Position;

            return d.X >= 0 && d.Y >= 0 &&
                   d.X < Size.X && d.Y < Size.Y;
        }

        #endregion

        #region Layout

        /// <summary>
        /// After parent has set it size, call this to position scrollbar.
        /// </summary>
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
