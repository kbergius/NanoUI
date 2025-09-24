using NanoUI.Common;
using Color = NanoUI.Common.Color;
using NanoUI.Layouts;
using NanoUI.Nvg;
using System;
using System.Numerics;
using System.Text.Json.Serialization;

namespace NanoUI.Components.Views
{
    #region Base

    // todo: we must override all properties that are themable

    /// <summary>
    /// UIViewPanel supports theming.
    /// </summary>
    public class UIViewPanel : UIWidget
    {
        /// <inheritdoc />
        public UIViewPanel()
        {
            // set defaults to theme impl - prevents circular reference
            HoverBorderColor = default;
            SelectedBorderColor = default;
            ItemBorderWidth = default;
            _itemSelectedBackgroundBrush = new();
            _itemHoverBackgroundBrush = new();

            RowHeight = default;
            _padding = new();
        }

        /// <inheritdoc />
        protected UIViewPanel(UIWidget parent)
            : base(parent)
        {
            ThemeType = typeof(UIViewPanel);
        }

        #region Properties

        Thickness? _padding;

        /// <summary>
        /// Padding.
        /// </summary>
        public virtual Thickness Padding
        {
            get => _padding?? GetTheme().ViewPanel.Padding;
            set => _padding = value;
        }

        
        // todo: this could also be dynamically calculated from default font size?

        int? _rowHeight;

        /// <summary>
        /// RowHeight is default value for all view rows.
        /// It is used in view item widget when creating,
        /// if IViewItem implementation doesn't define spesific row height.
        /// </summary>
        /// <remarks>This is not in theme by now since if it changes, all view items should be recreated.</remarks>
        public int RowHeight
        {
            get => _rowHeight?? GetTheme().ViewPanel.RowHeight;
            set =>_rowHeight = value;
        }

        ColumnDefinition[] _columns = Array.Empty<ColumnDefinition>();

        /// <summary>
        /// Column definitions.
        /// </summary>
        [JsonIgnore]
        public virtual ColumnDefinition[] Columns
        {
            get => _columns;
            set
            {
                // prevent null
                if (value == null)
                {
                    return;
                }

                _columns = value;

                RequestLayoutUpdate(this);
            }
        }

        /// <summary>
        /// ViewSelectionMode (item/cell). Default:  ViewSelectionMode.Item.
        /// </summary>
        public ViewSelectionMode ViewSelectionMode { get; set; } = ViewSelectionMode.Item;

        Color? _hoverBorderColor;

        /// <summary>
        /// HoverBorderColor is needed, when item/cell content hides totally background.
        /// </summary>
        public Color HoverBorderColor
        {
            get => _hoverBorderColor ?? GetTheme().ViewPanel.HoverBorderColor;
            set => _hoverBorderColor = value;
        }

        Color? _selectedBorderColor;

        /// <summary>
        /// SelectedBorderColor is needed, when item/cell content hides totally background.
        /// </summary>
        public Color SelectedBorderColor
        {
            get => _selectedBorderColor ?? GetTheme().ViewPanel.SelectedBorderColor;
            set => _selectedBorderColor = value;
        }

        int? _itemBorderWidth;

        /// <summary>
        /// ItemBorderWidth.
        /// </summary>
        public int ItemBorderWidth
        {
            get => _itemBorderWidth ?? GetTheme().ViewPanel.ItemBorderWidth;
            set => _itemBorderWidth = value;
        }

        BrushBase? _itemSelectedBackgroundBrush;

        /// <summary>
        /// ItemSelectedBackgroundBrush.
        /// </summary>
        public BrushBase ItemSelectedBackgroundBrush
        {
            get => _itemSelectedBackgroundBrush?? GetTheme().ViewPanel.ItemSelectedBackgroundBrush;
            set => _itemSelectedBackgroundBrush = value;
        }

        BrushBase? _itemHoverBackgroundBrush;

        /// <summary>
        /// ItemHoverBackgroundBrush.
        /// </summary>
        public BrushBase ItemHoverBackgroundBrush
        {
            get => _itemHoverBackgroundBrush?? GetTheme().ViewPanel.ItemHoverBackgroundBrush;
            set => _itemHoverBackgroundBrush = value;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Returns sum of the columns widths.
        /// </summary>
        public int GetColumnsWidth()
        {
            // calculate total columns width
            int columnsWidth = 0;

            for (int i = 0; i < _columns.Length; i++)
            {
                columnsWidth += _columns[i].Width;
            }

            return columnsWidth;
        }

        #endregion
    }

    #endregion

    // todo: keyboard navigation (up/down) fron selected index
    // todo: if RowHeight (or Columns) changes, we must update all ViewItemWidgets & perform layout

    /// <summary>
    /// UIViewPanel<T> provides most common operations for all view panels
    /// (extensions can override).
    /// </summary>
    public class UIViewPanel<T> : UIViewPanel
    {
        /// <summary>
        /// Selected changed action. ViewSelectionMode = item.
        /// </summary>
        public Action<UIViewItemWidget<T>>? SelectedChanged;

        // ViewSelectionMode = item - ViewItemWidget is row, int is column index
        // note: this provides also possibility to edit items dynamically. However there are
        // no helpers to do editing (needs cells to provide editing functionality - possibly use same
        // kind of editing as in propertygrid!)

        /// <summary>
        /// Cell selected changed action. Fired only when selection mode is Cell.
        /// </summary>
        public Action<UIViewItemWidget<T>, int>? CellSelectedChanged;

        // cell indexes
        int _cellSelectedIndex;
        int _cellHoveredIndex;

        /// <inheritdoc />
        public UIViewPanel()
        {
           
        }

        // must be public, since some implementation does not have own view panel implementation

        /// <inheritdoc />
        public UIViewPanel(UIWidget parent)
            : base(parent)
        {
            _selectedIndex = -1;
            _hoveredIndex = -1;

            ChildrenLayout = new StackLayout( Orientation.Vertical, LayoutAlignment.Minimum);
            ViewSelectionMode = ViewSelectionMode.Item;
        }

        #region Properties

        // todo : OnPointerUpDown doesn't call base.OnPointerUpDown.
        // Is there a better way?

        /// <summary>
        /// HasSubPopups is needed in menus to open submenus. Default: false.
        /// </summary>
        protected bool HasSubPopups { get; set; } = false;       

        int _selectedIndex;

        /// <summary>
        /// SelectedIndex. Only 1 can be selected at time.
        /// </summary>
        public int SelectedIndex
        {
            get => _selectedIndex;
            set
            {
                if (value <= -1 || value >= Children.Count)
                {
                    _selectedIndex = -1;
                    return;
                }

                if (Children[value] is UIViewItemWidget<T> viewItem)
                {
                    _selectedIndex = value;

                    // note: fires allways "row" selected (aslo when part selection mode)
                    SelectedChanged?.Invoke(viewItem);
                }
            }
        }

        int _hoveredIndex;

        /// <summary>
        /// HoveredIndex. Only 1 can be hovered at time.
        /// </summary>
        [JsonIgnore]
        public int HoveredIndex => _hoveredIndex;

        #endregion

        #region Methods

        /// <summary>
        /// Returns selected index or 0 (if there are childs). Otherwise returns -1.
        /// </summary>
        public int GetSelectedIndexOrDefault()
        {
            return _selectedIndex >= 0 ? _selectedIndex : Children.Count > 0 ? 0 : -1;
        }

        /// <summary>
        /// ResetIndexes.
        /// </summary>
        public void ResetIndexes()
        {
            _selectedIndex = -1;
            _hoveredIndex = -1;

            _cellSelectedIndex = -1;
            _cellHoveredIndex = -1;
        }

        #endregion

        #region Layout

        // todo: this does not calculate parts positions/sizes totally right!!!

        /// <inheritdoc />
        public override void PerformLayout(NvgContext ctx)
        {
            // Phases: 
            // - let the current layout system set widget positions & sizes
            // - loop view item widgets & set items positions & sizes

            // layout widgets
            base.PerformLayout(ctx);

            // we check if we are going to strech/diminish 1 column
            if (Columns.Length > 0)
            {
                // calculate delta (can be positive or negative)
                var delta = Size.X - GetColumnsWidth() - Padding.Horizontal;

                if (delta != 0)
                {
                    // find first strechable column
                    for (int i = 0; i < Columns.Length; i++)
                    {
                        ref var column = ref Columns[i];

                        if (column.Stretch)
                        {
                            column.Width += (int)delta;

                            break;
                        }
                    }
                }

                // loop view item widget and set parts position & size
                foreach (var child in Children.AsReadOnlySpan())
                {
                    if(!child.Visible)
                        continue;

                    // todo: should this code be in view item widget
                    if (child is UIViewItemWidget<T> itemWidget)
                    {
                        float xOffset = Padding.Horizontal;

                        // todo: ViewItemWidget has indent?
                        if (itemWidget is UITreeItemWidget<T> treeItem)
                        {
                            const int LEVEL_INDENT = 24;
                            xOffset = treeItem.Level * LEVEL_INDENT;
                            //if (treeItem.IsGroup)
                            {
                                xOffset += 24;
                            }
                        }

                        // Separator part for example (only 1 part)
                        if(itemWidget.IsSeparator)
                        {
                            // todo : paddings
                            itemWidget.Children[0].Position = new Vector2(xOffset, 0);
                            itemWidget.Children[0].Size = itemWidget.Size;
                        }
                        else
                        {
                            for (int i = 0; i < itemWidget.Children.Count; i++)
                            {
                                if (i >= Columns.Length)
                                    break;

                                itemWidget.Children[i].Position = new Vector2(xOffset, 0);
                                itemWidget.Children[i].Size = new Vector2(Columns[i].Width, itemWidget.Size.Y);

                                xOffset += Columns[i].Width;
                            }
                        }
                    }
                }
            }
        }

        #endregion

        #region Events

        /// <inheritdoc />
        public override bool OnPointerMove(Vector2 p, Vector2 rel)
        {
            // todo: should this be in base ViewPanel?

            // get hover indexes (do not fire selected actions)
            GetIndexes(p, false, out _hoveredIndex, out _cellHoveredIndex);

            // view panel handles all hovering - so no need to go further
            // we use pointer focus flag - so that we don't need to call
            // RequestPointerFocus again & again
            if (!PointerFocus)
            {
                Screen?.RequestPointerFocus(this);
                PointerFocus = true;

                
            }
            
            return true;
        }

        /// <inheritdoc />
        public override void OnPointerEnter(bool enter)
        {
            if (!enter)
            {
                _hoveredIndex = -1;
                _cellHoveredIndex = -1;
                PointerFocus = false;

                // todo: there could be also clearance of selected indices in some cases?
            }
        }

        /// <inheritdoc />
        public override bool OnPointerUpDown(Vector2 p, PointerButton button, bool down)
        {
            // todo: should this be in base ViewPanel?

            if (down && button == PointerButton.Left)
            {
                // get selected indexes & fire correct action if found
                GetIndexes(p, true, out _selectedIndex, out _cellSelectedIndex);

                // we can't call base.OnPointerUpDown since there is RequestFocus
                if (HasSubPopups)
                {
                    return true;
                }
            }

            // handle focus & context menu
            return base.OnPointerUpDown(p, button, down);
        }

        void GetIndexes(Vector2 p, bool selected, out int itemIndex, out int cellIndex)
        {
            itemIndex = -1;
            cellIndex = -1;

            var found = Children.Find(p - Position, out itemIndex);

            if (found != null)
            {
                // try cast
                UIViewItemWidget<T>? viewItem = found is UIViewItemWidget<T> item ? item : null;
                
                // fire selected action?
                if (selected && ViewSelectionMode == ViewSelectionMode.Item && viewItem != null)
                {
                    SelectedChanged?.Invoke(viewItem);
                    return;
                }

                // check cell selection mode
                if (ViewSelectionMode == ViewSelectionMode.Cell)
                {
                    // convert to children coordinate space
                    var pos = p - Position - found.Position;

                    int i = 0;

                    foreach (var child in found.Children.AsReadOnlySpan())
                    {
                        if (child.Contains(pos))
                        {
                            // found
                            cellIndex = i;

                            // fire selected action?
                            if (selected && viewItem != null)
                            {
                                CellSelectedChanged?.Invoke(viewItem, cellIndex);
                            }

                            break;
                        }

                        i++;
                    }
                }
            }
        }

        #endregion

        #region Drawing

        // todo: should we use standard widget drawing - so drawing functions are in viewItemWidget

        /// <inheritdoc />
        public override void Draw(NvgContext ctx)
        {
            // draw hovered or selected index widget background
            if (SelectedIndex >= 0)
            {
                DrawBackground(ctx, Children[SelectedIndex], ItemSelectedBackgroundBrush, false);
            }

            if (HoveredIndex >= 0 && (SelectedIndex != HoveredIndex || ViewSelectionMode == ViewSelectionMode.Cell))
            {
                DrawBackground(ctx, Children[HoveredIndex], ItemHoverBackgroundBrush, true);
            }
            
            // just draw widgets
            base.Draw(ctx);

            if(ItemBorderWidth > 0)
            {
                if (SelectedIndex >= 0 && SelectedBorderColor.A > 0)
                {
                    DrawBorder(ctx, Children[SelectedIndex], SelectedBorderColor, ItemBorderWidth, false);
                }

                if (HoveredIndex >= 0 && HoverBorderColor.A > 0 && (SelectedIndex != HoveredIndex || ViewSelectionMode == ViewSelectionMode.Cell))
                {
                    DrawBorder(ctx, Children[HoveredIndex], HoverBorderColor, ItemBorderWidth, true);
                }
            }
        }

        // selected or hovered (other wise none)
        // todo: should we draw hover tint when selected
        void DrawBackground(NvgContext ctx, UIWidget widget, BrushBase brush, bool hovered)
        {
            Vector2 itemPosition;
            Vector2 itemSize;

            // _cellSelectedIndex
            if(ViewSelectionMode == ViewSelectionMode.Item)
            {
                itemPosition = widget.Position;
                itemSize = widget.Size;
            }
            else
            {
                // get right cell index
                int cellIndex = hovered? _cellHoveredIndex : _cellSelectedIndex;

                // check valid index
                if(cellIndex < 0 || cellIndex >= widget.Children.Count)
                    return;

                itemPosition = widget.Position + widget.Children[cellIndex].Position;
                itemSize = widget.Children[cellIndex].Size;
            }

            brush?.Draw(ctx, Position + itemPosition, itemSize, null);
        }

        void DrawBorder(NvgContext ctx, UIWidget widget, in Color color, int borderWidth, bool hovered)
        {
            Vector2 itemPosition;
            Vector2 itemSize;

            // _cellSelectedIndex
            if (ViewSelectionMode == ViewSelectionMode.Item)
            {
                itemPosition = widget.Position;
                itemSize = widget.Size;
            }
            else
            {
                // get right cell index
                int cellIndex = hovered ? _cellHoveredIndex : _cellSelectedIndex;

                // check valid index
                if (cellIndex < 0 || cellIndex >= widget.Children.Count)
                    return;

                itemPosition = widget.Position + widget.Children[cellIndex].Position;
                itemSize = widget.Children[cellIndex].Size;
            }

            this.DrawBorder(ctx, Position + itemPosition, itemSize, borderWidth, color);
        }

        #endregion
    }
}
