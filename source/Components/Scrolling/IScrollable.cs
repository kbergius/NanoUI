using System.Numerics;

namespace NanoUI.Components.Scrolling
{
    /// <summary>
    /// IScrollable.
    /// Note: UIWindow & UIScrollpanel could be scrolled & popup does need to if its UIPopupButton is scrolled.
    /// </summary>
    public interface IScrollable
    {
        /// <summary>
        /// Scroll offset is negative or Vectro2.Zero
        /// </summary>
        Vector2 ScrollOffset { get; }
    }
}
