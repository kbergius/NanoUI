using NanoUI.Common;
using NanoUI.Components.Simple;
using NanoUI.Components.Views;
using NanoUI.Components.Views.Items;
using NanoUI.Nvg;

namespace NanoUI.Components
{
    // this is simple listbox that displays strings & returns T when item selected

    /// <summary>
    /// UIListBox<T>.
    /// </summary>
    public class UIListBox<T> : UIListView<T>
    {
        public UIListBox(UIWidget parent)
            : base(parent)
        {
            
        }

        #region Methods

        public UIViewItemWidget<T> AddItem(string caption, T eventData)
        {
            UIWidget[] cells =
            [
                new UIText { Text = caption },
            ];

            return Add(new RowItem<T>(cells, eventData));
        }

        #endregion

        #region Layout

        /// <inheritdoc />
        public override void PerformLayout(NvgContext ctx)
        {
            // there is only 1 column (list item text)
            ViewPanel.Columns =
            [
                new ColumnDefinition((int)Size.X)
            ];

            base.PerformLayout(ctx);
        }

        #endregion
    }
}
