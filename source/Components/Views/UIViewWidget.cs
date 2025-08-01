﻿using NanoUI.Common;
using NanoUI.Components.Scrolling;
using NanoUI.Nvg;
using System;
using System.Text.Json.Serialization;

namespace NanoUI.Components.Views
{
    // This is kind of container (almost all functionality is is ViewPanel)

    // this is used by all views except dropdown & menu since they inherit from popup button
    // this provides all basic functionality for views
    // todo : could use scroll panel as to have both vertical & horizontal scrollbars?
    public abstract class UIViewWidget<T> : UIWidget
    {
        // when selection mode = item
        public Action<T>? SelectedChanged;

        // when selection mode = cell - int is column index
        public Action<UIViewItemWidget<T>, int>? CellSelectedChanged;

        // this is ctor for theme/layout generation (if you use this otherwise, set parent before using widget)
        public UIViewWidget()
        {
            // set defaults to theme impl - prevents circular reference
            // this is only here to collect default properties
            _viewPanel = new UIViewPanel<T>();
        }

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

        public UIViewPanel<T> ViewPanel => _viewPanel;
        
        [JsonIgnore]
        public int SelectedIndex
        {
            get => _viewPanel.SelectedIndex;
            set => _viewPanel.SelectedIndex = value;
        }

        #endregion

        #region Methods

        public virtual void ResetIndexes()
        {
            _viewPanel.ResetIndexes();
        }

        public virtual void ClearChildren()
        {
            _viewPanel.Children.Clear();
        }

        #endregion

        #region Events

        // these could be overridden
        public virtual void OnCellSelectedChanged(
            UIViewItemWidget<T> widget, int columnIndex)
        {
            CellSelectedChanged?.Invoke(widget, columnIndex);
        }

        public virtual void OnSelectedChanged(UIViewItemWidget<T> widget)
        {
            if(widget.EventData != null)
            {
                OnSelectedChanged(widget.EventData);
            }
        }

        public virtual void OnSelectedChanged(T data)
        {
            SelectedChanged?.Invoke(data);
        }

        #endregion

        #region Layout

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
