using NanoUI.Components.Simple;
using NanoUI.Nvg;
using System.Numerics;

namespace NanoUI.Components.Views
{
    // most of the views / view panels use this as is
    // extensions: MenuItemWidget, UITreeItemWidget

    /// <summary>
    /// UIViewItemWidget<T>.
    /// </summary>
    public class UIViewItemWidget<T> : UIWidget
    {
        public UIViewItemWidget(UIWidget parent, IViewItem<T> viewRow)
            : base(parent)
        {
            // change/set item cells parent to this
            if(viewRow.Widgets != null)
            {
                foreach (var item in viewRow.Widgets)
                {
                    item.CreateParented(this);
                }
            }
            

            // set params
            Name = viewRow.Id?? "No name";
            ParentId = viewRow.ParentId?? "";
            EventData = viewRow.EventData;

            // if row height not set - use default
            Size = new Vector2(Size.X, viewRow.RowHeight.HasValue ? viewRow.RowHeight.Value : GetTheme().ViewPanel.RowHeight);

            // todo : should these be in theme?
            // note: these are overridden if focused/pointerFocused (uses theme colors)
            //BackgroundMode = BackgroundMode.NONE;
            Border = false;
        }

        #region Properties

        // this determines if we sync width width parent width
        // note: only flow & menu view currently doesn't use parent strech

        /// <summary>
        /// StretchWidth.
        /// </summary>
        public bool StretchWidth { get; set; } = false;

        // needed to place correctly into hierarcial structure

        /// <summary>
        /// ParentId.
        /// </summary>
        public string ParentId { get; }

        /// <summary>
        /// EventData.
        /// </summary>
        public T? EventData { get; set; }

        // there is special handling for separator (do not pointer click/focus etc)

        /// <summary>
        /// IsSeparator.
        /// </summary>
        public bool IsSeparator
        {
            get => Children.Count == 1 && Children[0] is UISeparator;
        }

        #endregion

        #region Layout

        /// <inheritdoc />
        public override void PerformLayout(NvgContext ctx)
        {
            base.PerformLayout(ctx);

            if (Parent != null && StretchWidth)
            {
                Size = new Vector2(Parent.Size.X, Size.Y);
            }
        }

        #endregion
    }
}
