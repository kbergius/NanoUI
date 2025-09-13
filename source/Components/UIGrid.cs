using NanoUI.Common;
using NanoUI.Layouts;
using NanoUI.Nvg;
using System.Collections.Generic;
using System.Numerics;

namespace NanoUI.Components
{
    // we use grid layout to do all layouting work
    // note: ChildrenLayout can't be set in this widget

    // todo: Column/Row header(s), sorting?, Scrollable
    // todo: paddings, margin?

    /// <summary>
    /// UIGrid.
    /// </summary>
    public class UIGrid : UIWidget
    {
        GridLayout _gridLayout;

        /// <inheritdoc />
        public UIGrid()
        {
            // Create a 2-column grid layout by default
            _gridLayout = new GridLayout(Orientation.Horizontal, 2, LayoutAlignment.Minimum);
        }

        /// <inheritdoc />
        public UIGrid(UIWidget parent)
            : base(parent)
        {
            // Create a 2-column grid layout by default
            _gridLayout = new GridLayout(Orientation.Horizontal, 2, LayoutAlignment.Minimum);
        }

        #region Properties

        /// <inheritdoc />
        public override Layout? ChildrenLayout
        {
            get => _gridLayout;
            set
            {
                // no - op: this is fixed to use grid layout
            }
        }

        public LayoutAlignment DefaultColumnAlignment
        {
            get => _gridLayout.DefaultColumnAlignment;
            set => _gridLayout.DefaultColumnAlignment = value;
        }

        public LayoutAlignment DefaultRowAlignment
        {
            get => _gridLayout.DefaultRowAlignment;
            set => _gridLayout.DefaultRowAlignment = value;
        }

        // Orientation decides which dimension is set (other overflows possibly)
        // default is horizontal
        public Orientation Orientation
        {
            get => _gridLayout.Orientation;
            set => _gridLayout.Orientation = value;
        }

        // The number of rows or columns (depending on the Orientation)
        // Orientation.Horizontal => column count, Orientation.Vertical => row count
        public int ColumnOrRowCount
        {
            get => _gridLayout.ColumnOrRowCount;
            set => _gridLayout.ColumnOrRowCount = value;
        }

        // The spacing used for each dimension
        public Vector2 Spacing
        {
            get => _gridLayout.Spacing;
            set => _gridLayout.Spacing = value;
        }

        #endregion

        #region Methods

        public void SetColumnAlignments(List<LayoutAlignment> value)
        {
            _gridLayout.SetColumnAlignments(value.ToArray());
        }

        public void SetRowAlignments(List<LayoutAlignment> value)
        {
            _gridLayout.SetRowAlignments(value.ToArray());
        }

        // The alignment of the specified axis (row or column number, depending on
        // the orientation) at the specified index of that row or column
        public LayoutAlignment GetAlignment(int axis, int item)
        {
            return _gridLayout.GetAlignment(axis, item);
        }

        #endregion

        #region Layout

        /// <inheritdoc />
        public override Vector2 PreferredSize(NvgContext ctx)
        {
            return _gridLayout.PreferredSize(ctx, this);
        }

        /// <inheritdoc />
        public override void PerformLayout(NvgContext ctx)
        {
            _gridLayout.PerformLayout(ctx, this);
        }

        #endregion
    }
}
