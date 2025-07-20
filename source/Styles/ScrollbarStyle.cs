using NanoUI.Common;

namespace NanoUI.Styles
{
    // scrollbars + scrollbar sliders
    // note: UIScrollbar is not themable widget, so we must have global decorator
    public class ScrollbarStyle
    {
        public uint ScrollbarDimension { get; set; }
        public BrushBase? BackgroundEnabled { get; set; }
        public BrushBase? SliderBrush { get; set; }
        public Color SliderHoverTint { get; set; }
    }
}