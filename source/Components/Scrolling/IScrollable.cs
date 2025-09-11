using System.Numerics;

namespace NanoUI.Components.Scrolling
{
    // UIWindow & UIScrollpanel could be scrolled & popup does need to if its UIPopupButton is scrolled

    /// <summary>
    /// IScrollable.
    /// </summary>
    public interface IScrollable
    {
        // note: scroll offset is negative or Vectro2.Zero
        Vector2 ScrollOffset { get; }
    }
}
