using NanoUI.Common;

namespace NanoUI.Styles
{
    /// <summary>
    /// Global scrollbar style.
    /// Note: UIScrollbar is not themable widget, so we must have global style.
    /// </summary>
    public struct ScrollbarStyle
    {
        /// <summary>
        /// Scrollbar dimension.
        /// </summary>
        public uint ScrollbarDimension { get; set; }

        /// <summary>
        /// Background enabled brush.
        /// </summary>
        public BrushBase? BackgroundEnabled { get; set; }

        /// <summary>
        /// Slider brush.
        /// </summary>
        public BrushBase? SliderBrush { get; set; }

        /// <summary>
        /// Slider hover tint color.
        /// </summary>
        public Color SliderHoverTint { get; set; }
    }
}
