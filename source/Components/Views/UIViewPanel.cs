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

    // this is for supporting theming
    // todo: we must override all properties that are themable
    public class UIViewPanel : UIWidget
    {
        // this is ctor for theme/layout generation (if you use this otherwise, set parent before using widget)
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

        protected UIViewPanel(UIWidget parent)
            : base(parent)
        {
            ThemeType = typeof(UIViewPanel);
        }

        #region Properties

        Thickness? _padding;
        public virtual Thickness Padding
        {
            get => _padding?? GetTheme().ViewPanel.Padding;
            set => _padding = value;
        }

        // this is default value for all view rows. It is used in view item widget when creating,
        // if IViewItem implementation doesn't define spesific row height
        // note: this is not in theme by now since if changed, all view items should be recreated
        // this could also be dynamically calculated from default font size
        int? _rowHeight;
        public int RowHeight
        {
            get => _rowHeight?? GetTheme().ViewPanel.RowHeight;
            set =>_rowHeight = value;
        }

        ColumnDefinition[] _columns = Array.Empty<ColumnDefinition>();
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

        // select item/cell
        public ViewSelectionMode ViewSelectionMode { get; set; } = ViewSelectionMode.Item;

        // note: this color is needed, when item/cell content hides totally background
        Color? _hoverBorderColor;
        public Color HoverBorderColor
        {
            get => _hoverBorderColor ?? GetTheme().ViewPanel.HoverBorderColor;
            set => _hoverBorderColor = value;
        }

        // note: this color is needed, when item/cell content hides totally background
        Color? _selectedBorderColor;
        public Color SelectedBorderColor
        {
            get => _selectedBorderColor ?? GetTheme().ViewPanel.SelectedBorderColor;
            set => _selectedBorderColor = value;
        }

        int? _itemBorderWidth;
        public int ItemBorderWidth
        {
            get => _itemBorderWidth ?? GetTheme().ViewPanel.ItemBorderWidth;
            set => _itemBorderWidth = value;
        }

        BrushBase? _itemSelectedBackgroundBrush;
        public BrushBase ItemSelectedBackgroundBrush
        {
            get => _itemSelectedBackgroundBrush?? GetTheme().ViewPanel.ItemSelectedBackgroundBrush;
            set => _itemSelectedBackgroundBrush = value;
        }

        BrushBase? _itemHoverBackgroundBrush;
        public BrushBase ItemHoverBackgroundBrush
        {
            get => _itemHoverBackgroundBrush?? GetTheme().ViewPanel.ItemHoverBackgroundBrush;
            set => _itemHoverBackgroundBrush = value;
        }

        #endregion

        #region Methods

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

    // Provides most common operations for all view panels (extensions can override)
    // todo: keyboard navigation (up/down) fron selected index
    // todo: if RowHeight (or Columns) changes, we must update all ViewItemWidgets & perform layout
    public class UIViewPanel<T> : UIViewPanel
    {
        // ViewSelectionMode = item
        public Action<UIViewItemWidget<T>> SelectedChanged;

        // ViewSelectionMode = part - ViewItemWidget is row, int is column index
        // note: this provides also possibility to edit parts dynamically. however there are
        // no helpers to do editing (needs cells to provide editing functionality - possibly use same
        // kind of editing as in propertygrid!)
        // note: this is fired only when selection mode is Cell
        public Action<UIViewItemWidget<T>, int> CellSelectedChanged;

        // cell indexes
        int _cellSelectedIndex;
        int _cellHoveredIndex;

        // this is ctor for theme/layout generation (if you use this otherwise, set parent before using widget)
        public UIViewPanel()
        {
           
        }

        // must be public, since some implementaion does not have own view panel implementation
        public UIViewPanel(UIWidget parent)
            : base(parent)
        {
            _selectedIndex = -1;
            _hoveredIndex = -1;

            ChildrenLayout = new StackLayout( Orientation.Vertical, LayoutAlignment.Minimum);
            ViewSelectionMode = ViewSelectionMode.Item;
        }

        #region Properties

        // this is needed for menuview to open submenus (OnPointerUpDown doesn't call base.OnPointerUpDown)
        // todo : is there a better way?
        protected bool HasSubPopups { get; set; } = false;       

        // note : only 1 can be hovered/focused at time
        int _selectedIndex;
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
        [JsonIgnore]
        public int HoveredIndex => _hoveredIndex;

        #endregion

        #region Methods

        // returns selected index or 0, if there is childs
        public int GetSelectedIndexOrDefault()
        {
            return _selectedIndex >= 0 ? _selectedIndex : Children.Count > 0 ? 0 : -1;
        }

        public void ResetIndexes()
        {
            _selectedIndex = -1;
            _hoveredIndex = -1;

            _cellSelectedIndex = -1;
            _cellHoveredIndex = -1;
        }

        #endregion

        #region Layout

        // Phases: 
        // - let the current layout system set widget psoitions & sizes
        // - loop view item widgets & set parts positions & sizes

        // todo: this does not calculate parts positions/sizes totally right!!!
        public override void PerformLayout(NvgContext ctx)
        {
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

                        // Todo: ViewItemWidget has indent?
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

        // todo: should this be in base ViewPanel?
        public override bool OnPointerMove(Vector2 p, Vector2 rel)
        {
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

        // todo: should this be in base ViewPanel?
        public override bool OnPointerUpDown(Vector2 p, PointerButton button, bool down)
        {
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