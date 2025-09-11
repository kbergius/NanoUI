using NanoUI.Common;
using System.Numerics;

namespace NanoUI.Styles
{
    // Docking - used only used when drawing docking layout screen
    // basically determines docking "texture" (hit areas) params (hit areas: top, left, bottom, right, center)

    /// <summary>
    /// DockingStyle.
    /// </summary>
    public struct DockingStyle
    {
        public float HitAreaCornerRadius { get; set; }
        public Color HitAreaBackgroundColor { get; set; }

        // draw hit area visualization (left, top, right, bottom, center)
        public BrushBase? HitAreaFillBrush { get; set; }

        // docking overlay area color to show where docking is going to performed
        public Color OverlayColor { get; set; }

        // TITLEBAR
        public BrushBase? TitleBackgroundFocused { get; set; }
        public BrushBase? TitleBackgroundUnfocused { get; set; }
        public string? TitleFontType { get; set; }
        public float TitleFontSize { get; set; }
        public Vector2 TitleButtonSize { get; set; }
    }
}
