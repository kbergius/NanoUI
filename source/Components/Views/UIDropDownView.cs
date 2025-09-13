using NanoUI.Common;
using NanoUI.Components.Scrolling;
using NanoUI.Components.Views.Items;
using NanoUI.Nvg;
using System;
using System.Numerics;
using System.Text.Json.Serialization;

namespace NanoUI.Components.Views
{
    // todo : should we have a flag where overrides max popup height &
    // sets popup height to panel children total height?
    // todo: this should not be used for theming!!
    // note: if you change items in popup, you must call RequestLayoutUpdate(Popup)

    /// <summary>
    /// UIDropDownView<T>.
    /// </summary>
    public class UIDropDownView<T> : UIPopupButton
    {
        UIViewPanel<T> _viewPanel;
        UIScrollPanel? _scroll;

        // must be here (panel calls - popup button no)
        public Action<T>? SelectedChanged;

        // when popup closes OnSelectionChanged event, we must get focus back
        bool _requestFocus;

        /// <inheritdoc />
        public UIDropDownView()
        {
            // set defaults to theme impl - prevents circular reference

            // we must set dummy here for columns
            // todo: check how viewpanel properties can be configured!
            _viewPanel = new();
        }

        /// <inheritdoc />
        public UIDropDownView(UIWidget parent)
            :base(parent)
        {
            Popup.Visible = false;
            Popup.AnchorPos = Vector2.Zero;
            Popup.AnchorSize = 0;
            Popup.RelativePosition = PopupPosition.Bottom;

            // attach scroll to popup
            _scroll = new UIScrollPanel(Popup, ScrollbarType.Vertical);

            // create panel (container) & wrap with scroll
            _viewPanel = new UIViewPanel<T>(_scroll);
            _viewPanel.SelectedChanged+= OnSelectedChanged;
            _viewPanel.ViewSelectionMode = ViewSelectionMode.Item;

            // defaults
            Icon = GetTheme().Fonts.IconCaretDown;
        }

        #region Properties

        // note : view panel has global look in all derived widgets!
        [JsonIgnore]
        public UIViewPanel<T> ViewPanel => _viewPanel;

        // Pass (almost) everything to panel
        // todo: should we just force to use view pnel property?

        // todo: must call RequestLayoutUpdate if something changes?
        // todo: needed?
        // note: we don't need to save these since they are in view panel
        //[JsonIgnore]
        public ColumnDefinition[] Columns
        {
            get => _viewPanel.Columns;
            set => _viewPanel.Columns = value;
        }

        [JsonIgnore]
        public int SelectedIndex
        {
            get => _viewPanel.SelectedIndex;
            set => _viewPanel.SelectedIndex = value;
        }

        #endregion

        #region Methods

        public virtual void SetSelected(T value)
        {
            int index = 0;

            foreach (UIViewItemWidget<T> child in _viewPanel.Children.AsReadOnlySpan())
            {
                if (child.EventData != null && child.EventData.Equals(value))
                {
                    SelectedIndex = index;
                    break;
                }

                index++;
            }
        }

        // file folder drowpdown needs
        public void ClearSelection()
        {
            _viewPanel.SelectedIndex = -1;
        }

        public void ClearChilren()
        {
            _viewPanel.Children.Clear();
        }

        protected void RequestListUpdate()
        {
            // todo?
            if(_scroll != null && _scroll.VerticalScrollbar != null)
            {
                _scroll.VerticalScrollbar.Scroll = 0;
            }

            // if no children - disable
            Disabled = _viewPanel.Children.Count == 0;

            RequestLayoutUpdate(this);
        }

        // cells can have their own data OR row can have common data
        // WE search first cell data & if it is null . row data
        // We return widget so user can set common properties
        public UIViewItemWidget<T> Add(RowItem<T> listItem)
        {
            return new UIViewItemWidget<T>(_viewPanel, listItem) { StretchWidth = true };
        }

        #endregion

        #region Events

        public virtual void OnSelectedChanged(UIViewItemWidget<T> widget)
        {
            // close popup - set state
            Pushed = false;

            // when popup actually closes - get focus back
            _requestFocus = true;

            if(widget.EventData != null)
            {
                SelectedChanged?.Invoke(widget.EventData);
            }
        }

        #endregion

        #region Layout

        // todo: _viewpanel perform layout?

        /// <inheritdoc />
        public override void PerformLayout(NvgContext ctx)
        {
            // calculate total columns width
            // todo: read from view panel columns width
            int columnsWidth = 0;

            for(int i = 0; i< Columns.Length; i++)
            {
                columnsWidth += Columns[i].Width;
            }

            // popup width can be this width OR columns width if greater
            // so popup is not hiding any cells
            int width = (int)Math.Max(Size.X, columnsWidth);
            
            // todo : should we calculate actual childen heights?
            int height = (int)Math.Min(MaxPopupHeight, _viewPanel.Children.Count * Size.Y);
            
            // adjust popup size
            Popup.FixedSize = new Vector2(width, height);
            Popup.Size = Popup.FixedSize;

            // set scroll to fill all popup area
            if(_scroll != null)
            {
                _scroll.FixedSize = Popup.FixedSize;
                _scroll.Size = _scroll.FixedSize;
            }
            
            // perform layout in popup
            Popup.PerformLayout(ctx);
        }

        #endregion

        #region Drawing

        // note: we currently don't use popup button draw methods (rather we draw button manually here)

        /// <inheritdoc />
        public override void Draw(NvgContext ctx)
        {
            // todo?: we could also just call base.Draw, where is Popup.Show logic
            if (Disabled && Pushed)
                Pushed = false;

            // note: if popup visible, screen fires popup drawing
            Popup.Show(Pushed);

            // request focus (popup closed & popup window had focus) - get focus back
            if (_requestFocus)
            {
                _requestFocus = false;

                RequestFocus();
            }

            // Background
            DrawBackgroundBrush(ctx);

            // Draw selected
            DrawSelected(ctx);

            // Draw chevron overlay
            if (Icon > 0)
            {
                ctx.FontSize((FontSize < 0 ? GetTheme().Button.FontSize : FontSize) * IconScale);
                ctx.FontFaceId(FontIconsId);
                ctx.FillColor(!Disabled ? TextColor : TextDisabledColor);

                ctx.TextAlign(TextAlignment.Left | TextAlignment.Middle);

                float iw = ctx.TextBounds(0, 0, Icon, out _);

                Vector2 iconPos = new Vector2
                {
                    X = Position.X + Size.X - iw - Padding.Horizontal,
                    Y = Position.Y + Size.Y * 0.5f// - 1
                };

                ctx.Text(iconPos, Icon);
            }

            // border
            this.DrawBorder(ctx, false);
        }

        // some extensions may override this
        protected virtual void DrawSelected(NvgContext ctx)
        {
            // we get selected index (if no item is selected we use first, if there are childs)
            var seLectecIndex = ViewPanel.GetSelectedIndexOrDefault();

            if (seLectecIndex >= 0)
            {
                if(_viewPanel.Children[seLectecIndex] is UIViewItemWidget<T> viewItem)
                {
                    // get the position conversion
                    var pos = Position - viewItem.Position;

                    // make "correction" from selected item coordinate space to this
                    ctx.Translate(pos);

                    viewItem.Draw(ctx);

                    ctx.Translate(-pos);
                }
            }
        }

        #endregion
    }
}
