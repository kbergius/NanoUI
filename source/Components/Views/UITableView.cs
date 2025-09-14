using NanoUI.Common;
using NanoUI.Components.Views.Items;

namespace NanoUI.Components.Views
{
    // todo : draw column headers & provide sort by clicked column

    /// <summary>
    /// UITableView<T> is a generic table view using cells.
    /// </summary>
    public class UITableView<T> : UIViewWidget<T>
    {
        /// <inheritdoc />
        public UITableView()
        {
            // set defaults to theme impl - prevents circular reference
        }

        /// <inheritdoc />
        public UITableView(UIWidget parent)
            :base(parent)
        {
            // note: we create view panel in base

            // override default
            ViewPanel.ViewSelectionMode = ViewSelectionMode.Cell;
        }

        #region Methods

        public UIViewItemWidget<T> Add(RowItem<T> viewItem)
        {
            return new UIViewItemWidget<T>(ViewPanel, viewItem) { StretchWidth = true };
        }

        #endregion
    }
}
