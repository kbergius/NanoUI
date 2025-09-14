using NanoUI.Common;
using NanoUI.Components.Simple;
using NanoUI.Components.Views;
using NanoUI.Components.Views.Items;
using NanoUI.Nvg;
using System;

namespace NanoUI.Components
{
    /// <summary>
    /// UIComboBox<T> is just simple combo box with only 1 column (item text).
    /// </summary>
    public class UIComboBox<T> : UIDropDownView<T>
    {
        // these are used to calculate combo box popup min width
        const int ITEM_SAFETY_MARGIN = 18;

        string _longestCaption = string.Empty;

        /// <inheritdoc />
        public UIComboBox(UIWidget parent)
            : base(parent)
        {

        }

        #region Methods

        public UIViewItemWidget<T> AddItem(string caption, T eventData)
        {
            // get the longest caption so we can calculate min width of the display column / popup
            if(_longestCaption.Length < caption.Length)
            {
                _longestCaption = caption;
            }

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
            // calculate longest item width
            ctx.FontFaceId(FontFaceId);
            ctx.FontSize(FontSize);
            ctx.TextAlign(TextAlignment.Left | TextAlignment.Top);
            var longestItemWidth = ctx.TextBounds(0, 0, _longestCaption, out _);

            // there is only 1 column (combo item text)
            // get the max of width / longest item width
            Columns =
            [
                new ColumnDefinition((int)Math.Max((int)longestItemWidth + 
                    GetTheme().Scrollbars.ScrollbarDimension + ITEM_SAFETY_MARGIN, Size.X))
           ];

            base.PerformLayout(ctx);
        }

        #endregion
    }
}
