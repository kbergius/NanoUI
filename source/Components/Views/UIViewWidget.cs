using NanoUI.Common;
using NanoUI.Components.Scrolling;
using NanoUI.Nvg;
using System;
using System.Text.Json.Serialization;

namespace NanoUI.Components.Views
{
    // todo : could use scroll panel as to have both vertical & horizontal scrollbars?

    /// <summary>
    /// UIViewWidget<T> provides all basic functionality for views and
    /// is used in all views except dropdown & menu since they inherit from popup button.
    /// </summary>
    public abstract class UIViewWidget<T> : UIWidget
    {
        /// <summary>
        /// Fired when selection mode = item.
        /// </summary>
        public Action<T>? SelectedChanged;

        /// <summary>
        /// Fired when selection mode = cell. Int is column index.
        /// </summary>
        public Action<UIViewItemWidget<T>, int>? CellSelectedChanged;

        /// <inheritdoc />
        public UIViewWidget()
        {
            // set defaults to theme impl - prevents circular reference
            // this is only here to collect default properties
            _viewPanel = new UIViewPanel<T>();
        }

        /// <inheritdoc />
        public UIViewWidget(UIWidget parent)
            : base(parent)
        {
            _vscroll = new UIScrollPanel(this, ScrollbarType.Vertical);
            // create panel & wrap with scroll
            _viewPanel = new UIViewPanel<T>(_vscroll);
            // this is only used when selection mode is item
            _viewPanel.SelectedChanged += OnSelectedChanged;
            // this is only used when selection mode is cell
            _viewPanel.CellSelectedChanged += OnCellSelectedChanged;
        }

        #region Properties

        UIScrollPanel? _vscroll;

        UIViewPanel<T> _viewPanel;

        /// <summary>
        /// ViewPanel.
        /// </summary>
        public UIViewPanel<T> ViewPanel => _viewPanel;

        /// <summary>
        /// SelectedIndex.
        /// </summary>
        [JsonIgnore]
        public int SelectedIndex
        {
            get => _viewPanel.SelectedIndex;
            set => _viewPanel.SelectedIndex = value;
        }

        #endregion

        #region Methods

        /// <summary>
        /// ResetIndexes.
        /// </summary>
        public virtual void ResetIndexes()
        {
            _viewPanel.ResetIndexes();
        }

        /// <summary>
        /// ClearChildren.
        /// </summary>
        public virtual void ClearChildren()
        {
            _viewPanel.Children.Clear();
        }

        #endregion

        #region Events

        /// <summary>
        /// Virtual OnCellSelectedChanged.
        /// </summary>
        public virtual void OnCellSelectedChanged(
            UIViewItemWidget<T> widget, int columnIndex)
        {
            CellSelectedChanged?.Invoke(widget, columnIndex);
        }

        /// <summary>
        /// Virtual OnSelectedChanged.
        /// </summary>
        public virtual void OnSelectedChanged(UIViewItemWidget<T> widget)
        {
            if(widget.EventData != null)
            {
                OnSelectedChanged(widget.EventData);
            }
        }

        /// <summary>
        /// Virtual OnSelectedChanged.
        /// </summary>
        public virtual void OnSelectedChanged(T data)
        {
            SelectedChanged?.Invoke(data);
        }

        #endregion

        #region Layout

        /// <inheritdoc />
        public override void PerformLayout(NvgContext ctx)
        {
            // pass to scroll
            var preferred = PreferredSize(ctx);

            if(_vscroll != null)
            {
                _vscroll.FixedSize = preferred;
                _vscroll.Size = preferred;
            }
            
            base.PerformLayout(ctx);
        }

        #endregion
    }
}
