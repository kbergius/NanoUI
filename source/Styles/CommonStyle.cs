using NanoUI.Common;

namespace NanoUI.Styles
{
    /// <summary>
    /// Global common style is used to set some widgets common properties
    /// in order to achieve some visual consistency.
    /// </summary>
    public struct CommonStyle
    {
        /// <summary>
        /// Accent color can be used to highlight some area.
        /// </summary>
        /// <remarks>Not all widgets use this.</remarks>
        public Color AccentColor { get; set; }

        /// <summary>
        /// Sunken background brush can be used to have some consistent, "sunken" look in widgets.
        /// </summary>
        public BrushBase? BackgroundSunken { get; set; }

        /// <summary>
        /// Disabled background brush.
        /// </summary>
        public BrushBase? BackgroundDisabled { get; set; }

        /// <summary>
        /// Invalid background brush is used to indicate invalid status
        /// (like invalid entry in UITextField).
        /// </summary>
        public BrushBase? BackgroundInvalid { get; set; }

        /// <summary>
        /// Background hover tint color.
        /// </summary>
        public Color? BackgroundHoverTint { get; set; }

        /// <summary>
        /// Frontend disabled color is used,
        /// when widget must draw widgets frontend (like text) disabled.
        /// </summary>
        public Color FrontendDisabledColor { get; set; }
    }
}
