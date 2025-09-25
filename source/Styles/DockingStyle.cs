using NanoUI.Common;
using System.Numerics;

namespace NanoUI.Styles
{
    /// <summary>
    /// Global docking style is only used in docking.
    /// </summary>
    public struct DockingStyle
    {
        /// <summary>
        /// Hit area corner radius.
        /// </summary>
        public float HitAreaCornerRadius { get; set; }

        /// <summary>
        /// Hit area background color.
        /// </summary>
        public Color HitAreaBackgroundColor { get; set; }

        /// <summary>
        /// Hit area fill brush is for hit area visualization (left, top, right, bottom, center).
        /// </summary>
        public BrushBase? HitAreaFillBrush { get; set; }

        /// <summary>
        /// Docking overlay area color to show, where docking is going to be performed.
        /// </summary>
        public Color OverlayColor { get; set; }

        /// <summary>
        /// Title background focused brush.
        /// </summary>
        public BrushBase? TitleBackgroundFocused { get; set; }

        /// <summary>
        /// Title background unfocused brush.
        /// </summary>
        public BrushBase? TitleBackgroundUnfocused { get; set; }

        /// <summary>
        /// Title font type.
        /// </summary>
        public string? TitleFontType { get; set; }

        /// <summary>
        /// Title font size.
        /// </summary>
        public float TitleFontSize { get; set; }

        /// <summary>
        /// Title button size.
        /// </summary>
        public Vector2 TitleButtonSize { get; set; }
    }
}
