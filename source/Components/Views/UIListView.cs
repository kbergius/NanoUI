using NanoUI.Components.Views.Items;

namespace NanoUI.Components.Views
{
    /// <summary>
    /// UIListView<T>.
    /// </summary>
    public class UIListView<T> : UIViewWidget<T>
    {
        /// <inheritdoc />
        public UIListView()
        {
            // set defaults to theme impl - prevents circular reference
        }

        /// <inheritdoc />
        public UIListView(UIWidget parent)
            :base(parent)
        {
            // note: we create view panel in base
        }

        #region Methods

        /// <summary>
        /// Adds item.
        /// Cells can have their own data OR row can have common data.
        /// </summary>
        /// <param name="listItem">RowItem<T></param>
        /// <returns>widget so you can set common properties.</returns>
        public UIViewItemWidget<T> Add(RowItem<T> listItem)
        {
            return new UIViewItemWidget<T>(ViewPanel, listItem) { StretchWidth = true };
        }

        #endregion
    }
}
