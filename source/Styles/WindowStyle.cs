using NanoUI.Common;

namespace NanoUI.Styles
{
    /// <summary>
    /// Global window style has properties, that are same to all window instances.
    /// </summary>
    public struct WindowStyle
    {
        /// <summary>
        /// Border focused color.
        /// </summary>
        public Color BorderFocusedColor { get; set; }

        /// <summary>
        /// Border unfocused color.
        /// </summary>
        public Color BorderUnfocusedColor { get; set; }

        /// <summary>
        /// Border resizing color.
        /// </summary>
        public Color BorderResizingColor { get; set; }

        /// <summary>
        /// DragAreaWidth determines the area (border) width where we track if drag is happening.
        /// Default: 10,
        /// </summary>
        public uint DragAreaWidth { get; set; } = 10;

        /// <summary>
        /// Resizing triangle size in the UIWindows' bottom-right corner. Default: 25,
        /// </summary>
        public float ResizingTriangleSize { get; set; } = 25f;

        /// <summary>
        /// Constructor.
        /// </summary>
        public WindowStyle() { }
    }
}
