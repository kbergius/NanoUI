using NanoUI.Common;
using NanoUI.Layouts;
using NanoUI.Components.Views.Items;
using System.Numerics;

namespace NanoUI.Components.Views
{
    // todo : Show name under Texture/Icon (truncated/wrapped if not fits to thumbsize) - CUSTOM CELL?
    // todo : Show tooltip (description) under Texture/Icon if spesified?

    /// <summary>
    /// UIFlowView<T>.
    /// </summary>
    public class UIFlowView<T> : UIViewWidget<T>
    {
        /// <inheritdoc />
        public UIFlowView()
        {
            // set defaults to theme impl - prevents circular reference
            // note: user can set default part size
            // PartSize = new Vector2(100, 100);
        }

        /// <inheritdoc />
        public UIFlowView(UIWidget parent)
            : base(parent)
        {
            // note: we create view panel in base

            // override default
            ViewPanel.ChildrenLayout = new FlowLayout(LayoutAlignment.Minimum) { Spacing = new Vector2(10) };
            ViewPanel.ViewSelectionMode = ViewSelectionMode.Cell;
        }

        #region Properties

        // todo: If user changes this, we must update panel widgets Fixed size & panel layout
        // todo: we don't store param here, since it produces sync problem, if user changes columns/rowheight directly
        Vector2 _partSize;

        /// <summary>
        /// PartSize is just a helper.
        /// </summary>
        /// <remarks>Since Columns (ViewColumn[]) is not stored with theme (has owner widget).</remarks>
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

        /// <summary>
        /// Adds item.
        /// </summary>
        /// <param name="flowItem">FlowItem<T></param>
        /// <returns>UIViewItemWidget<T></returns>
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
