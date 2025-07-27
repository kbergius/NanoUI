using NanoUI.Common;

namespace NanoUI.Styles
{
    // These are properties that are same to all window instances
    public struct WindowStyle
    {
        public Color BorderFocusedColor { get; set; }
        public Color BorderUnfocusedColor { get; set; }
        public Color BorderResizingColor { get; set; }

        // Determines the area (border) width where we track if drag is happening
        // todo: this could be same as margin?
        public uint DragAreaWidth { get; set; } = 10;
        public float ResizingTriangleSize { get; set; } = 25f;

        public WindowStyle() { }
    }
}
