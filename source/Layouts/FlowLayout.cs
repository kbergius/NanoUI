using NanoUI.Common;
using NanoUI.Components.Scrolling;
using NanoUI.Nvg;
using System.Numerics;
using System;
using NanoUI.Components;

namespace NanoUI.Layouts
{
    /// <summary>
    /// FlowLayout is an extension of the grid layout, where widgets are positioned
    /// in horizontal order in a way, that they always use all the parent's width.
    /// Layout dynamically calculates column count and lets then grid layout do the real work.
    /// </summary>
    public class FlowLayout : GridLayout
    {
        /// <summary>
        /// Ctor. Supports only horizontal orientation by now (layout based on columns).
        /// </summary>
        public FlowLayout(LayoutAlignment defaultAlignments = LayoutAlignment.Middle)
            :base(Orientation.Horizontal, 1, defaultAlignments)
        {
            
        }

        #region Properties

        /// <summary>
        /// Supports only horizontal orientation by now (layout based on columns),
        /// so the setter does nothing.
        /// </summary>
        public override Orientation Orientation
        {
            get => base.Orientation;
            set
            {
                // no - op in flow layout
            }
        }

        /// <summary>
        /// Column count is calculated dynamically, so the setter does nothing.
        /// </summary>
        public override int ColumnOrRowCount
        {
            get => base.ColumnOrRowCount;
            set
            {
                // no - op in flow layout
            }
        }

        #endregion

        #region Layout

        /// <inheritdoc />
        public override Vector2 PreferredSize(NvgContext ctx, UIWidget parent)
        {
            // 1. calculate column count
            CalculateColumnCount(ctx, parent);

            // 2. let the grid layout calculate rest
            return base.PreferredSize(ctx, parent);
        }

        /// <inheritdoc />
        public override void PerformLayout(NvgContext ctx, UIWidget parent)
        {
            // 1. calculate column count
            CalculateColumnCount(ctx, parent);

            // 2. let the grid layout do rest
            base.PerformLayout(ctx, parent);
        }

        #endregion

        #region Private

        // calculate column count - rows are calculated from it
        void CalculateColumnCount(NvgContext ctx, UIWidget parent)
        {
            // we care here only for width (columns fit)
            float cellWidth = 0;

            foreach (var child in parent.Children.AsReadOnlySpan())
            {
                if (!child.Visible)
                    continue;

                // we prefer widgets own dimensions
                var width = child.FixedSize.X > 0 ? child.FixedSize.X : MathF.Max(child.Size.X, child.MinSize.X);
                
                // if we didn't get any value and there is children layout, try to get value there
                if(width == 0 && child.ChildrenLayout != null)
                    width = child.PreferredSize(ctx).X;
                
                cellWidth = Math.Max(cellWidth, width);
            }

            // calculate columns fit -> rows are determined from it
            var parentWidth = parent.GetLayoutArea().Width;

            if (parent.Parent is UIScrollPanel scrollPanel)
            {
                parentWidth = scrollPanel.GetLayoutArea().Width;
            }

            var columns = 1 + Math.Max(0,
                (parentWidth - parent.Margin.Horizontal * 2 - cellWidth) /
                (cellWidth + Spacing.X));

            // we round down to prevent horizontal overflow
            base.ColumnOrRowCount = (int)columns;
        }

        #endregion
    }
}
