using NanoUI.Components;
using NanoUI.Nvg;
using System.Numerics;

namespace NanoUI.Layouts
{
    /// <summary>
    /// Layout is an abstract layout class, that all layout implementations should extend.
    /// </summary>
    public abstract class Layout
    {
        /// <summary>
        /// Spacing between widgets.
        /// </summary>
        /// <remarks>Layouts my use spacing only in 1 dimension.</remarks>
        public Vector2 Spacing { get; set; }

        /// <summary>
        /// Calculates parent widget's children preferred size with margins.
        /// </summary>
        /// <param name="ctx">NvgContext</param>
        /// <param name="parent">Parent</param>
        /// <returns>Size</returns>
        public abstract Vector2 PreferredSize(NvgContext ctx, UIWidget parent);

        /// <summary>
        /// Sets widget's children positions and sizes & calls childs to perform layout,
        /// where you can tweak calculated values.
        /// </summary>
        /// <param name="ctx">NvgContext</param>
        /// <param name="parent">Parent</param>
        /// <remarks>Calls parent widget's GetLayoutArea method in order to get offset/topLeft & children area size.</remarks>
        public abstract void PerformLayout(NvgContext ctx, UIWidget parent);
    }
}
