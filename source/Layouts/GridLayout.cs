using NanoUI.Common;
using NanoUI.Components;
using NanoUI.Nvg;
using NanoUI.Utils;
using System;
using System.Numerics;

namespace NanoUI.Layouts
{
    // Widgets are arranged in a grid that has a fixed column ot row count.
    // The orientation indicates the fixed dimension (columns/rows).
    // The spacing between widgets can be specified per axis.
    // The horizontal/vertical alignment can be specified per row and column.

    // todo: make Dictionary<Widget, stretch> -based stretching functionality
    // todo?: _alignments -> for columns & rows: Dictionary<row/colIndex, LayoutAlignment>
    // it would be easier to SetRowAlignment(rowIndex, LayoutAlignment) - NOW MUST SET ALL AT ONCE

    /// <summary>
    /// GridLayout.
    /// </summary>
    public class GridLayout : Layout
    {
        // The default alignments
        LayoutAlignment[] _defaultAlignments = new LayoutAlignment[2];
        
        // The actual alignments being used for each column/row
        LayoutAlignment[][] _alignments = new LayoutAlignment[2][];
        
        public GridLayout(Orientation orientation = Orientation.Horizontal, int columnOrRowCount = 2,
                   LayoutAlignment alignment = LayoutAlignment.Middle)
        {
            _orientation = orientation;
            _columnOrRowCount = columnOrRowCount;

            _defaultAlignments[0] = _defaultAlignments[1] = alignment;
        }

        #region Properties

        Orientation _orientation;

        // this is virtual since FlowLayout overrides (fixed orientation)
        public virtual Orientation Orientation
        {
            get => _orientation;
            set => _orientation = value;
        }

        // The number of rows or columns before starting a new one, depending on the orientation.
        int _columnOrRowCount;
        public virtual int ColumnOrRowCount
        {
            get => _columnOrRowCount;
            set => _columnOrRowCount = value;
        }

        public LayoutAlignment DefaultColumnAlignment
        {
            get => _defaultAlignments[0];
            set => _defaultAlignments[0] = value;
        }

        public LayoutAlignment DefaultRowAlignment
        {
            get => _defaultAlignments[1];
            set => _defaultAlignments[1] = value;
        }

        #endregion

        #region Methods

        // Sets the spacing for a specific axis (axis0 = X, axis1 = Y)
        public void SetSpacing(int axis, int spacing)
        {
            if(axis == 0)
            {
                Spacing = new Vector2(spacing, Spacing.Y);
            }
            else
            {
                Spacing = new Vector2(Spacing.X, spacing);
            }
        }

         // The alignment of the specified axis (row or column number, depending on
         // the orientation) at the specified index of that row or column.
        public LayoutAlignment GetAlignment(int axis, int item)
        {
            if (_alignments[axis] != null && item < _alignments[axis].Length)
                return _alignments[axis][item];
             else
                 return _defaultAlignments[axis];
        }

        // Use this to set alignments for columns
        public void SetColumnAlignments(LayoutAlignment[] value)
        {
            _alignments[0] = value;
        }

        // Use this to set alignments for rows
        public void SetRowAlignments(LayoutAlignment[] value)
        {
            _alignments[1] = value;
        }

        #endregion

        #region Layout

        /// <inheritdoc />
        public override Vector2 PreferredSize(NvgContext ctx, UIWidget parent)
        {
            var margin = parent.Margin;

            // Compute minimum row & column sizes
            int[][] grid = ComputeLayout(ctx, parent);

            // calculate size with children sizes & margins
            Vector2 size = new Vector2(
                2 * margin.Horizontal + MathUtils.Sum(grid[0])
                + Math.Max(grid[0].Length - 1, 0) * Spacing[0],
                2 * margin.Vertical + MathUtils.Sum(grid[1])
                + Math.Max(grid[1].Length - 1, 0) * Spacing[1]
            );

            return size;
        }

        /// <inheritdoc />
        public override void PerformLayout(NvgContext ctx, UIWidget parent)
        {
            var margin = parent.Margin;

            Rect layoutArea = parent.GetLayoutArea();

            Vector2 containerSize = layoutArea.Size;

            // Compute minimum row & column sizes
            int[][] grid = ComputeLayout(ctx, parent);

            // dimensions
            int[] dim = [ grid[0].Length, grid[1].Length ];

            Vector2 extra = new Vector2(0, layoutArea.Y);

            // Strech to size provided by widget with axis
            for (int i = 0; i < 2; i++)
            {
                int gridSize = (int)(2 * margin[i] + extra[i]);

                foreach (int s in grid[i])
                {
                    gridSize += s;

                    if (i + 1 < dim[i])
                        gridSize += (int)Spacing[i];
                }

                if (gridSize < containerSize[i])
                {
                    // Re-distribute remaining space evenly
                    int gap = (int)containerSize[i] - gridSize;
                    int g = gap / dim[i];
                    int rest = gap - g * dim[i];

                    for (int j = 0; j < dim[i]; ++j)
                        grid[i][j] += g;

                    for (int j = 0; rest > 0 && j < dim[i]; --rest, ++j)
                        grid[i][j] += 1;
                }
            }

            int axis1 = (int)_orientation;
            int axis2 = (axis1 + 1) % 2;

            Vector2 start = new Vector2(margin.Horizontal, margin.Vertical) + extra;

            int numChildren = parent.Children.Count;
            int child = 0;

            // this is accumulated pos
            Vector2 pos = start;

            for (int i2 = 0; i2 < dim[axis2]; i2++)
            {
                pos[axis1] = start[axis1];

                for (int i1 = 0; i1 < dim[axis1]; i1++)
                {
                    UIWidget w;
                    do
                    {
                        if (child >= numChildren)
                            return;

                        w = parent.Children[child++];

                    } while (!w.Visible);

                    Vector2 ps = w.PreferredSize(ctx);
                    Vector2 fs = w.FixedSize;

                    Vector2 targetSize = new Vector2(
                        fs[0] > 0 ? fs[0] : ps[0],
                        fs[1] > 0 ? fs[1] : ps[1]
                    );

                    Vector2 itemPos = pos;

                    for (int j = 0; j < 2; j++)
                    {
                        int axis = (axis1 + j) % 2;
                        int item = j == 0 ? i1 : i2;

                        LayoutAlignment align = GetAlignment(axis, item);

                        switch (align)
                        {
                            case LayoutAlignment.Minimum:
                                break;
                            case LayoutAlignment.Middle:
                                itemPos[axis] += (grid[axis][item] - targetSize[axis]) / 2;
                                break;
                            case LayoutAlignment.Maximum:
                                itemPos[axis] += grid[axis][item] - targetSize[axis];
                                break;
                            case LayoutAlignment.Fill:
                                targetSize[axis] = fs[axis] > 0 ? fs[axis] : grid[axis][item];
                                break;
                        }
                    }

                    // set position taking into account also horizontal offset
                    w.Position = new Vector2(itemPos.X + layoutArea.X, itemPos.Y);
                    w.Size = new Vector2(targetSize.X, targetSize.Y);

                    w.PerformLayout(ctx);
                    
                    pos[axis1] += grid[axis1][i1] + Spacing[axis1];
                }

                pos[axis2] += grid[axis2][i2] + Spacing[axis2];
            }
        }

        #endregion

        #region Private

        // Compute the minimum column and row sizes
        int[][] ComputeLayout(NvgContext ctx, UIWidget parent)
        {
            int axis1 = (int)_orientation;
            int axis2 = (axis1 + 1) % 2;
            int numChildren = parent.Children.Count;
            int visibleChildren = 0;

            // visible childs
            foreach (var w in parent.Children.AsReadOnlySpan())
                visibleChildren += w.Visible ? 1 : 0;

            // dimension
            Vector2 dim = new();
            dim[axis1] = _columnOrRowCount;
            dim[axis2] = (visibleChildren + _columnOrRowCount - 1) / _columnOrRowCount;

            // init grid (column & row sizes)
            int[][] grid = new int[2][];

            Array.Resize(ref grid[axis1], (int)dim[axis1]);
            Array.Resize(ref grid[axis2], (int)dim[axis2]);

            int child = 0;

            for (int i2 = 0; i2 < dim[axis2]; i2++)
            {
                for (int i1 = 0; i1 < dim[axis1]; i1++)
                {
                    UIWidget w;

                    do
                    {
                        if (child >= numChildren)
                            return grid;

                        w = parent.Children[child++];

                    } while (!w.Visible);

                    // note: PreferredSize can be also calculated from child layouts,
                    // so we check fixed size
                    Vector2 ps = w.PreferredSize(ctx);
                    Vector2 fs = w.FixedSize;

                    Vector2 targetSize = new Vector2(
                        fs[0] > 0 ? fs[0] : ps[0],
                        fs[1] > 0 ? fs[1] : ps[1]
                    );

                    grid[axis1][i1] = (int)MathF.Max(grid[axis1][i1], targetSize[axis1]);
                    grid[axis2][i2] = (int)MathF.Max(grid[axis2][i2], targetSize[axis2]);
                }
            }

            return grid;
        }

        #endregion
    }
}
