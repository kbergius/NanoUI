using NanoUI.Common;

namespace NanoUI.Styles
{
    /// <summary>
    /// Global scrollbar style.
    /// Note: UIScrollbar is not themable widget, so we must have global style.
    /// </summary>
    public struct ScrollbarStyle
    {
        public uint ScrollbarDimension { get; set; }
        public BrushBase? BackgroundEnabled { get; set; }
        public BrushBase? SliderBrush { get; set; }
        public Color SliderHoverTint { get; set; }
    }
}
