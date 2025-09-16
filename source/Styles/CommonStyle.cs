using NanoUI.Common;

namespace NanoUI.Styles
{
    // note: we don't use base UIWidget as theme type, since it will "pollute" theme json file

    /// <summary>
    /// Global common style is used to set some widgets common properties
    /// in order to achieve some visual consistency.
    /// </summary>
    public struct CommonStyle
    {
        /// <summary>
        /// Can be used to "highlight" some area (not all widgets use this by now).
        /// </summary>
        public Color AccentColor { get; set; }

        /// <summary>
        /// Can be used to have some consistent, "sunken" look in widgets.
        /// </summary>
        public BrushBase? BackgroundSunken { get; set; }

        public BrushBase? BackgroundDisabled { get; set; }

        /// <summary>
        /// When using invalid background, some widgets may check invalid format (not all widgets support this).
        /// </summary>
        public BrushBase? BackgroundInvalid { get; set; }

        public Color? BackgroundHoverTint { get; set; }

        /// <summary>
        /// Used if widget must draw widgets frontend (like text) disabled.
        /// </summary>
        public Color FrontendDisabledColor { get; set; }
    }
}
