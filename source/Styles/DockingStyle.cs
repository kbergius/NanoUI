using NanoUI.Common;
using System.Numerics;

namespace NanoUI.Styles
{
    /// <summary>
    /// Global docking style is only used in docking.
    /// </summary>
    public struct DockingStyle
    {
        public float HitAreaCornerRadius { get; set; }
        public Color HitAreaBackgroundColor { get; set; }

        /// <summary>
        /// Hit area visualization (left, top, right, bottom, center).
        /// </summary>
        public BrushBase? HitAreaFillBrush { get; set; }

        /// <summary>
        /// Docking overlay area color to show, where docking is going to be performed.
        /// </summary>
        public Color OverlayColor { get; set; }

        // titlebar
        public BrushBase? TitleBackgroundFocused { get; set; }
        public BrushBase? TitleBackgroundUnfocused { get; set; }
        public string? TitleFontType { get; set; }
        public float TitleFontSize { get; set; }
        public Vector2 TitleButtonSize { get; set; }
    }
}
