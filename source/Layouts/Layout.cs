using NanoUI.Components;
using NanoUI.Nvg;
using System.Numerics;

namespace NanoUI.Layouts
{
    /// <summary>
    /// Layout is a base, abstract layout class, that all layout implementations should extend.
    /// </summary>
    public abstract class Layout
    {
        /// <summary>
        /// Spacing between widgets.
        /// Note: layout spacing may use only 1 dimension.
        /// </summary>
        public Vector2 Spacing { get; set; }

        /// <summary>
        /// Calculates widget's children preferred size with margins.
        /// </summary>
        public abstract Vector2 PreferredSize(NvgContext ctx, UIWidget parent);

        /// <summary>
        /// Sets widget's children positions and sizes & calls childs to perform layout,
        /// where you can tweak calculated values.
        /// Note: calls parent widget's GetLayoutArea method in order to get offset/topLeft & children area size.
        /// </summary>
        public abstract void PerformLayout(NvgContext ctx, UIWidget parent);
    }
}
