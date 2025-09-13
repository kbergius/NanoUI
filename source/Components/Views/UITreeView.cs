using NanoUI.Common;
using NanoUI.Components.Simple;
using NanoUI.Components.Views.Items;
using NanoUI.Nvg;
using System;
using System.Collections.Generic;
using System.Numerics;

namespace NanoUI.Components.Views
{
    #region UITreeItemWidget

    /// <summary>
    /// UITreeItemWidget<T>.
    /// </summary>
    public class UITreeItemWidget<T> : UIViewItemWidget<T>
    {
        // todo: configurable?
        const int LEVEL_INDENT = 24;

        /// <inheritdoc />
        public UITreeItemWidget(UIWidget parent, TreeItem<T> treeItem)
            : base(parent, treeItem)
        {
            StretchWidth = true;
            // todo: configurable?
            IconExtraScale = 1.2f;
        }

        #region Properties

        public int Level = 0;
        public bool Expanded = false;
        public bool IsGroup
        {
            get;
            internal set;
        }

        public UITreeItemWidget<T>? ParentItem;

        #endregion

        #region Drawing

        /// <inheritdoc />
        public override void Draw(NvgContext ctx)
        {
            base.Draw(ctx);

            // draw expanded / collapsed icon
            Vector2 center = Position + Size * 0.5f;

            // draw icon (expanded/collapsed)?
            if (IsGroup)
            {
                var icon = Expanded ?
                    GetTheme().Fonts.IconExpanded : GetTheme().Fonts.IconCollapsed;

                ctx.FontSize(FontSize * IconScale);
                ctx.FontFaceId(FontIconsId);
                ctx.FillColor(TextColor);
                ctx.TextAlign(TextAlignment.Left | TextAlignment.Middle);

                Vector2 iconPos = center;
                iconPos.Y -= 1;
                iconPos.X = Position.X + GetIconIndent(Level);

                ctx.Text(iconPos.X, iconPos.Y + 1, icon);
            }
        }

        #endregion

        #region Private

        static int GetIconIndent(int level)
        {
            return level * LEVEL_INDENT + 5;
        }

        #endregion
    }

    #endregion

    // treeview is just a list of widgets that has special padding/indent based on level &
    // expand/collapse actions changes Visible property

    // todo : combine methods AddGroup & AddItem -. Add?
    // todo: there should be methods for moving items &
    // dynamically adding/removing after tree is initially created
    // todo: refactor to use hierarcial system like other widgets, so it is easier to implement
    // all modifying

    /// <summary>
    /// UITreeView<T>.
    /// </summary>
    public class UITreeView<T> : UIViewWidget<T>
    {
        // we use root id as parent id if not found else
        const string ROOT_ID = "#Root";

        /// <inheritdoc />
        public UITreeView()
        {
            // set defaults to theme impl - prevents circular reference
        }

        /// <inheritdoc />
        public UITreeView(UIWidget parent, T rootData)
            : base(parent)
        {
            // note: we create view panel in base
            
            // add root Item
            var root = new UIText { Text = "Root" };
            
            CreateItem(new TreeItem<T>(new UIWidget[] { root }, rootData, ROOT_ID, null), true);
        }

        #region Methods

        // we clear previous tree & set root back

        /// <inheritdoc />
        public override void ClearChildren()
        {
            // Get root
            var root = ViewPanel.Children[0];

            // clear previous tree
            ViewPanel.Children.Clear();

            // add root back
            ViewPanel.Children.Add(root);
        }

        // todo : should we return widget so taht user can add directly to its children
        // Actions AddGroup/AddItem should then be in TreeViewItem self
        // (No need to find parent & mess with parent ids!)
        public void AddGroup(TreeItem<T> treeItem)
        {
            // check not root
            if (treeItem.Id == ROOT_ID)
            {
                // do not use same as root
                throw new InvalidOperationException($"Can't use id {ROOT_ID}. It is reserved.");
            }
            else if (string.IsNullOrEmpty(treeItem.Id))
            {
                // groups must have id
                throw new InvalidOperationException("Groups must have id");
            }

            // check not already exists
            // todo: use guid's?
            var widget = ViewPanel.Children.FindByName<UITreeItemWidget<T>>(treeItem.Id);

            if(widget != null)
            {
                // groups must have unique id
                throw new InvalidOperationException("Groups must have unique id");
            }

            // set to root
            if (string.IsNullOrEmpty(treeItem.ParentId))
            {
                treeItem.ParentId = ROOT_ID;
            }

            CreateItem(treeItem, true);
        }

        public void AddItem(TreeItem<T> treeItem)
        {
            // set to root
            if (string.IsNullOrEmpty(treeItem.ParentId))
            {
                treeItem.ParentId = ROOT_ID;
            }

            CreateItem(treeItem, false);
        }

        void CreateItem(TreeItem<T> treeItem, bool isGroup)
        {
            UITreeItemWidget<T>? parent = null;

            if(treeItem.ParentId != null)
            {
                // not root - find parent
                parent = ViewPanel.Children.FindByName<UITreeItemWidget<T>>(treeItem.ParentId);

                if (parent == null)
                {
                    // todo: log?
                    treeItem.ParentId = ROOT_ID;
                    parent = ViewPanel.Children[0] as UITreeItemWidget<T>;
                }
            }

            var item = new UITreeItemWidget<T>(ViewPanel, treeItem);
            item.Name = treeItem.Id?? "No name";
            
            // ROOT IS INVISIBLE - all under root gets 0
            item.Level = parent != null ? parent.Level + 1 : -1;
            
            item.ParentItem = parent;
            // THIS DETERMINES IF EXPAND/COLLAPSE ICON SHOWED
            item.IsGroup = isGroup;
            // MUST SET FIXED HEIGHT SO WE CAN FIND WIDGET
            //item.FixedHeight1 = _panel.RowHeight;

            // SET VISIBILITY - Expanded = false;
            if(parent == null)
            {
                // this is root
                item.Visible = false;
                item.Expanded = true;
                
                // todo - root visible? - doesn't show expand/collapse icon
                //item.Visible = true;
                //item.Expanded = false;
            }
            else
            {
                // if user adds to already created tree -. visibility . parent expanded value
                // normally all but root items are hidden
                item.Visible = parent.Expanded;
                // groups by default - not expanded
                item.Expanded = false;
            }
        }

        #endregion

        #region Events

        /// <inheritdoc />
        public override void OnSelectedChanged(UIViewItemWidget<T> widget)
        {
            // we must handle expand/collapse (change visibility of widgets)
            HandleExpandCollapse(widget);

            if(widget.EventData != null)
            {
                base.OnSelectedChanged(widget.EventData);
            }
        }

        #endregion

        #region Private

        Stack<UITreeItemWidget<T>> _tempStack = new();

        // we must handle expand & collapse
        void HandleExpandCollapse(UIViewItemWidget<T> widget)
        {
            if (widget is UITreeItemWidget<T> item)
            {
                // we can handle take care of groups, since they only have expand/collapse functionality
                if (!item.IsGroup)
                    return;

                // switch group item status
                item.Expanded = !item.Expanded;

                // Set all childs visible false/true
                ChangeVisibility(item, item.Expanded);

                // we must do recursive - to hide childs if collapsed and show if expanded
                while (_tempStack.Count > 0)
                {
                    ChangeVisibility(_tempStack.Pop(), item.Expanded);
                }

                // we must update layout since there is visiblity changes
                RequestLayoutUpdate(this);
            }
        }

        void ChangeVisibility(UITreeItemWidget<T> groupItem, bool visible)
        {
            foreach(var item in ViewPanel.Children.AsReadOnlySpan())
            {
                if (item is UITreeItemWidget<T> child)
                {
                    if (child.ParentItem == groupItem)
                    {
                        child.Visible = visible && groupItem.Expanded && groupItem.Visible;

                        if (child.IsGroup)
                        {
                            _tempStack.Push(child);
                        }
                    }
                }
            }
        }

        #endregion
    }
}
