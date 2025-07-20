using NanoUI.Common;
using NanoUI.Layouts;
using NanoUI.Components.Views.Items;
using System.Numerics;

namespace NanoUI.Components.Views
{
    // todo : Show name under Texture/Icon (truncated/wrapped if not fits to thumbsize) - CUSTOM CELL?
    // todo : Show tooltip (description) under Texture/Icon if spesified?
    public class UIFlowView<T> : UIViewWidget<T>
    {
        // this is ctor for theme/layout generation (if you use this otherwise, set parent before using widget)
        public UIFlowView()
        {
            // set defaults to theme impl - prevents circular reference
            // note: user can set default part size
            // PartSize = new Vector2(100, 100);
        }

        public UIFlowView(UIWidget parent)
            : base(parent)
        {
            // note: we create view panel in base

            // override default
            ViewPanel.ChildrenLayout = new FlowLayout(LayoutAlignment.Minimum) { Spacing = new Vector2(10) };
            ViewPanel.ViewSelectionMode = ViewSelectionMode.Cell;
        }

        #region Properties

        // note: this is just a helper
        // todo: If user changes this, we must update panel widgets Fixed size & panel layout

        // note: since Columns (ViewColumn[]) is not stored with theme (has owner widget)
        // todo: we don't store param here, since it produces sync problem, if user changes columns/rowheight directly
        Vector2 _partSize;
        public Vector2 PartSize
        {
            get => _partSize;
            set
            {
                _partSize = value;

                ViewPanel.Columns = [new ColumnDefinition((int)_partSize.X)];
                ViewPanel.RowHeight = (int)_partSize.Y;

                // todo: sync all view panel widgets & perform layout if this is changed at runtime
                // (must sync also vscrollpanel)
            }
        }
        
        #endregion

        #region Methods

        // this is for forcing to use flow item
        public UIViewItemWidget<T> Add(FlowItem<T> flowItem)
        {
            return new UIViewItemWidget<T>(ViewPanel, flowItem)
            {
                StretchWidth = true,
                FixedSize = PartSize,
                CornerRadius = new CornerRadius(0)
            };
        }

        #endregion
    }
}