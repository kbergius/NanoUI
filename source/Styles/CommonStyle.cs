using NanoUI.Common;

namespace NanoUI.Styles
{
    // this is used to set all widgets base properties
    // note: we don't use base Widget as theme type, since it will "pollute" theme json file
    public struct CommonStyle
    {
        // this can be used to "highlight" some area (not all widgets use this by now)
        public Color AccentColor { get; set; }

        // this is used to have some consistent, "sunken" look in several widgets
        // (CheckBox, Progressbar, Slider, SwithBox, TextBox)
        public BrushBase? BackgroundSunken { get; set; }
        public BrushBase? BackgroundDisabled { get; set; }

        // when using invalid background - some widgets may check invalid format (not all widgets support this)
        // so they should override DrawBackgroundBrush method (first check it and then use method here)
        public BrushBase? BackgroundInvalid { get; set; }
        public Color? BackgroundHoverTint { get; set; }
        // this is used if widget must draw widgets parts (not background) disabled
        public Color FrontendDisabledColor { get; set; }
    }
}
