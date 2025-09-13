using NanoUI.Common;
using NanoUI.Nvg;
using NanoUI.Utils;
using System;
using System.Collections.Generic;
using System.Numerics;

namespace NanoUI.Components
{
    // note: all widgets in grid view should normally have their cell set (or invisible), because
    // base drawing function draws all visible children in widget list at their specified position & size
    // and this has no override draw function.

    // todo: Column/Row header(s), Scrollable, use paddings (instead or margin), spacing inside cells
    // (determined in row & column definitions) - cell has not alignment (they are specified
    // in row & column definitions)

    /// <summary>
    /// UIGridView.
    /// </summary>
    public class UIGridView : UIWidget
    {
        #region Cell

        public struct Cell
        {
            public uint[] Position = new uint[2];   // (columnIndex, rowIndex)
            public uint[] Span = new uint[2];       // (columnSpan, rowSpan)
            public LayoutAlignment[] Align = new LayoutAlignment[2]; // (horizontal, vertical)

            public Cell() { }

            public Cell(uint columnIndex, uint rowIndex, LayoutAlignment horizontal = LayoutAlignment.Fill,
                  LayoutAlignment vertical = LayoutAlignment.Fill)
            {
                Position[0] = columnIndex;
                Position[1] = rowIndex;
                Span[0] = Span[1] = 1;
                Align[0] = horizontal;
                Align[1] = vertical;
            }

            public Cell(uint columnIndex, uint rowIndex, uint columnSpan, uint rowSpan,
                  LayoutAlignment horizontal = LayoutAlignment.Fill,
                  LayoutAlignment vertical = LayoutAlignment.Fill)
            {
                Position[0] = columnIndex;
                Position[1] = rowIndex;
                Span[0] = columnSpan;
                Span[1] = rowSpan;
                Align[0] = horizontal;
                Align[1] = vertical;
            }
        }

        #endregion

        List<int> _columns;
        List<int> _rows;

        float[] _columnStretches;
        float[] _rowStreches;

        // we use widget id (guid) as key, so we don't have strong reference to widget
        // note: widgets are stored in widget list (this is just a mapping)
        Dictionary<Guid, Cell> _widgetAnchors = new();

        /// <inheritdoc />
        public UIGridView()
        {
            _columns = new();
            _rows = new();
            _columnStretches = Array.Empty<float>();
            _rowStreches = Array.Empty<float>();
        }

        /// <inheritdoc />
        public UIGridView(UIWidget parent)
            : this(parent, new List<int>(), new List<int>())
        {
        }

        /// <inheritdoc />
        public UIGridView(UIWidget parent, List<int> columns, List<int> rows, int gridPadding = 0)
            : base(parent)
        {
            _columns = columns;
            _rows = rows;
            GridPadding = gridPadding;

            // sync
            Array.Resize(ref _columnStretches, _columns.Count);
            Array.Resize(ref _rowStreches, _rows.Count);
        }

        #region Properties

        public int GridPadding { get; set; }
        public List<int> Columns => _columns;
        public List<int> Rows => _rows;
        public int ColumnCount =>_columns.Count;
        public int RowCount => _rows.Count;

        #endregion

        #region Methods

        public void AppendRow(uint height, float stretch = 0.0f)
        {
            _rows.Add((int)height);

            // sync
            Array.Resize(ref _rowStreches, _rows.Count);
            _rowStreches[_rowStreches.Length - 1] = stretch;
        }

        public void AppendColumn(uint width, float stretch = 0.0f)
        {
            _columns.Add((int)width);
            
            // sync
            Array.Resize(ref _columnStretches, _columns.Count);
            _columnStretches[_columnStretches.Length - 1] = stretch;
        }

        public void SetRowStretch(uint rowIndex, float stretch)
        {
            // todo: validate index
            _rowStreches[rowIndex] = stretch;
        }

        public void SetColumnStretch(int columnIndex, float stretch)
        {
            // todo: validate index
            _columnStretches[columnIndex] = stretch;
        }

        public void SetCell(UIWidget widget, Cell anchor)
        {
            _widgetAnchors[widget.Id] = anchor;
        }

        // note: this returns a copy of cell. So if you modify it, you should call SetCell.
        public bool TryGetCell(UIWidget widget, out Cell cell)
        {
            if (_widgetAnchors.TryGetValue(widget.Id, out cell))
            {
                return true;
            }

            return false;
        }

        #endregion

        #region Layout

        /// <inheritdoc />
        public override Vector2 PreferredSize(NvgContext ctx)
        {
            // Compute minimum row / column sizes
            List<int>[] grid = ComputeLayout(ctx);

            Vector2 size = new Vector2(
                MathUtils.Sum(grid[0]),
                MathUtils.Sum(grid[1]));

            Vector2 extra = new Vector2(2 * GridPadding);
            
            return size + extra;
        }

        /// <inheritdoc />
        public override void PerformLayout(NvgContext ctx)
        {
            // Compute minimum row / column sizes
            List<int>[] grid = ComputeLayout(ctx);

            grid[0].Insert(0, GridPadding);
            grid[1].Insert(0, GridPadding);

            for (int axis = 0; axis < 2; axis++)
            {
                for (int i = 1; i < grid[axis].Count; i++)
                    grid[axis][i] += grid[axis][i - 1];

                foreach (UIWidget w in Children.AsSpan())
                {
                    if (!w.Visible)
                        continue;
                     
                    if(!TryGetCell(w, out Cell cell))
                        continue;

                    int itemPos = grid[axis][(int)cell.Position[axis]];
                    int cellSize = grid[axis][(int)cell.Position[axis] + (int)cell.Span[axis]] - itemPos;

                    int ps = (int)w.PreferredSize(ctx)[axis];
                    int fs = (int)w.FixedSize[axis];
                    int targetSize = fs > 0 ? fs : ps;

                    switch (cell.Align[axis])
                    {
                        case LayoutAlignment.Minimum:
                            break;
                        case LayoutAlignment.Middle:
                            itemPos += (cellSize - targetSize) / 2;
                            break;
                        case LayoutAlignment.Maximum:
                            itemPos += cellSize - targetSize;
                            break;
                        case LayoutAlignment.Fill:
                            targetSize = fs > 0 ? fs : cellSize;
                            break;
                    }

                    Vector2 pos = w.Position;
                    Vector2 size = w.Size;
                    pos[axis] = itemPos;
                    size[axis] = targetSize;

                    w.Position = pos;
                    w.Size = size;

                    w.PerformLayout(ctx);
                }
            }
        }

        #endregion

        #region Private

        // Compute the minimum row and column sizes
        List<int>[] ComputeLayout(NvgContext ctx)
        {
            int[][] result = { _columns.ToArray(), _rows.ToArray()};

            Vector2 fixedSize = FixedSize;
            Vector2 containerSize = new Vector2(
                fixedSize[0] > 0 ? fixedSize[0] : MathF.Max(MinSize.X, Size.X),
                fixedSize[1] > 0 ? fixedSize[1] : MathF.Max(MinSize.Y, Size.Y)
            );

            Vector2 extra = new Vector2(2 * GridPadding);
            
            containerSize -= extra;

            for (int axis = 0; axis < 2; axis++)
            {
                int[] sizes = axis == 0 ? _columns.ToArray() : _rows.ToArray();
                float[] stretches = axis == 0 ? _columnStretches : _rowStreches;
                // we collect values in grid
                int[] grid = sizes;

                for (int phase = 0; phase < 2; phase++)
                {
                    foreach (var kvp in _widgetAnchors)
                    {
                        // note: we don't do recursive search, sinve all widgets are in this children list 
                        UIWidget? w = Children.FindById(kvp.Key, false);

                        if (w == null || !w.Visible)
                            continue;

                        Cell cell = kvp.Value;
                        
                        if ((cell.Span[axis] == 1) != (phase == 0))
                            continue;

                        int ps = (int)w.PreferredSize(ctx)[axis];
                        int fs = (int)w.FixedSize[axis];
                        int targetSize = fs > 0 ? fs : ps;

                        if (cell.Position[axis] + cell.Span[axis] > grid.Length)
                            throw new Exception($"Grid: widget is out of bounds: {cell}.");

                        int currentSizeTemp = 0;
                        float totalStretchTemp = 0;
                        
                        for (int i = (int)cell.Position[axis]; i < cell.Position[axis] + cell.Span[axis]; i++)
                        {
                            if (sizes[i] == 0 && cell.Span[axis] == 1)
                                grid[i] = Math.Max(grid[i], targetSize);

                            currentSizeTemp += grid[i];
                            totalStretchTemp += stretches[i];
                        }

                        if (targetSize <= currentSizeTemp)
                            continue;

                        if (totalStretchTemp == 0)
                            throw new Exception($"Grid: no space to place widget: { cell }.");

                        float amountTemp = (targetSize - currentSizeTemp) / totalStretchTemp;
                        
                        for (int i = (int)cell.Position[axis]; i < cell.Position[axis] + cell.Span[axis]; i++)
                            grid[i] += (int)Math.Round(amountTemp * stretches[i]);
                    }
                }

                int currentSize = MathUtils.Sum(grid);
                float totalStretch = MathUtils.Sum(stretches);

                if (currentSize >= containerSize[axis] || totalStretch == 0)
                    continue;
                
                float amount = (containerSize[axis] - currentSize) / totalStretch;
                
                for (int i = 0; i < grid.Length; i++)
                    grid[i] += (int)Math.Round(amount * stretches[i]);

                // after axis set - store
                result[axis] = grid;
            }

            return new List<int>[]{ new List<int>(result[0]), new List<int>(result[1]) };
        }

        #endregion
    }
}
