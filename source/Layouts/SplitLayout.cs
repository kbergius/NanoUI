using NanoUI.Common;
using NanoUI.Components;
using NanoUI.Nvg;
using System.Collections.Generic;
using System.Numerics;

namespace NanoUI.Layouts
{
    // todo: does not use margin by now

    /// <summary>
    /// SplitLayout is needed when using Splitter.
    /// If you use this layout directly, you must create splitter panels & splitter(s) yourself.
    /// Note: there is a widget (UISplitPanel), that provides 2-panel functionality & is easier to use.
    /// </summary>
    public class SplitLayout : Layout
    {
        public SplitLayout(Orientation orientation)  
        {
            Orientation = orientation;
        }

        #region Properties

        public Orientation Orientation {  get; set; }

        #endregion

        #region Layout

        /// <inheritdoc />
        public override Vector2 PreferredSize(NvgContext ctx, UIWidget parent)
        {
            return parent.GetLayoutArea().Size;
        }

        /// <inheritdoc />
        public override void PerformLayout(NvgContext ctx, UIWidget parent)
        {
            // no margins!

            float layoutWidth = parent.FixedSize.X > 0 ?
                parent.FixedSize.X : parent.Size.X;

            float layoutHeight = parent.FixedSize.Y > 0 ?
                parent.FixedSize.Y : parent.Size.Y;

            // start values
            int position = 0;
            
            // collect all visible children
            List<UIWidget> visibleChildren = new();

            foreach (var w in parent.Children.AsReadOnlySpan())
            {
                if (!w.Visible)
                    continue;

                visibleChildren.Add(w);
            }

            if (visibleChildren.Count == 0)
                return;

            if (Orientation == Orientation.Horizontal)
            {
                while (visibleChildren.Count > 0)
                {
                    UIWidget child = visibleChildren[0];

                    // we remove so to get current count
                    visibleChildren.RemoveAt(0);

                    Vector2 size = new Vector2(
                        layoutWidth / (visibleChildren.Count + 1),
                        layoutHeight);

                    Vector2 fs = child.FixedSize;
                    Vector2 pos = new Vector2(position, 0);

                    if (fs.X > 0)
                        size.X = fs.X;

                    child.Position = pos;
                    child.Size = size;

                    child.PerformLayout(ctx);

                    position += (int)(size.X + Spacing.X);
                    layoutWidth -= size.X + Spacing.X;
                }
            }
            else
            {
                while (visibleChildren.Count > 0)
                {
                    UIWidget child = visibleChildren[0];
                    visibleChildren.RemoveAt(0);

                    Vector2 size = new Vector2(
                        layoutWidth,
                        layoutHeight / (visibleChildren.Count + 1));

                    Vector2 fs = child.FixedSize;
                    Vector2 pos = new Vector2(0, position);

                    if (fs.Y > 0)
                        size.Y = fs.Y;

                    child.Position = pos;
                    child.Size = size;

                    child.PerformLayout(ctx);

                    position += (int)(size.Y + Spacing.Y);
                    layoutHeight -= size.Y + Spacing.Y;
                }
            }
        }

        #endregion
    }
}
