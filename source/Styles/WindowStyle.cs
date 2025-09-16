using NanoUI.Common;

namespace NanoUI.Styles
{
    /// <summary>
    /// Global window style has properties, that are same to all window instances.
    /// </summary>
    public struct WindowStyle
    {
        public Color BorderFocusedColor { get; set; }
        public Color BorderUnfocusedColor { get; set; }
        public Color BorderResizingColor { get; set; }

        /// <summary>
        /// Determines the area (border) width where we track if drag is happening.
        /// Default: 10,
        /// </summary>
        public uint DragAreaWidth { get; set; } = 10;

        /// <summary>
        /// ResizingTriangleSize. Default: 25,
        /// </summary>
        public float ResizingTriangleSize { get; set; } = 25f;

        public WindowStyle() { }
    }
}
