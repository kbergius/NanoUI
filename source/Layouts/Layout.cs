using NanoUI.Components;
using NanoUI.Nvg;
using System.Numerics;

namespace NanoUI.Layouts
{
    // this is a base layout that all layout implementations should extend
    public abstract class Layout
    {
        // note: layout spacing may use only 1 dimension
        public Vector2 Spacing { get; set; }

        // calculates widget's children preferred size with margins
        // note: widget may add more into this size (like titlebar)
        public abstract Vector2 PreferredSize(NvgContext ctx, UIWidget parent);

        // sets widget's children positions and sizes & calls childs to perform layout
        // note: this method calls widget.GetLayoutArea(), so to get offset/topLeft & children area size
        public abstract void PerformLayout(NvgContext ctx, UIWidget parent);
    }
}