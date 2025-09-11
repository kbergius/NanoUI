using NanoUI.Common;
using NanoUI.Components.Views.Items;

namespace NanoUI.Components.Views
{
    // this is generic table view using cells
    // todo : draw column headers & provide sort by clicked column

    /// <summary>
    /// UITableView<T>.
    /// </summary>
    public class UITableView<T> : UIViewWidget<T>
    {
        // this is ctor for theme/layout generation (if you use this otherwise, set parent before using widget)
        public UITableView()
        {
            // set defaults to theme impl - prevents circular reference
        }

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
