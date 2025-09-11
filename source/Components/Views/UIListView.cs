using NanoUI.Components.Views.Items;

namespace NanoUI.Components.Views
{
    /// <summary>
    /// UIListView<T>.
    /// </summary>
    public class UIListView<T> : UIViewWidget<T>
    {
        // this is ctor for theme/layout generation (if you use this otherwise, set parent before using widget)
        public UIListView()
        {
            // set defaults to theme impl - prevents circular reference
        }

        public UIListView(UIWidget parent)
            :base(parent)
        {
            // note: we create view panel in base
        }

        #region Methods

        // We return widget so user can set common properties
        // todo: cells can have their own data OR row can have common data
        public UIViewItemWidget<T> Add(RowItem<T> listItem)
        {
            return new UIViewItemWidget<T>(ViewPanel, listItem) { StretchWidth = true };
        }

        #endregion
    }
}
