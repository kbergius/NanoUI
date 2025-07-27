using NanoUI.Common;
using NanoUI.Layouts;
using NanoUI.Nvg;
using System;
using System.Numerics;

namespace NanoUI.Components.Docking
{
    // todo: attach
    public partial class DockNode
    {
        #region Detach

        // this is called when tab dragging goes outside of the tabwidget 
        void Detach(UITabItem tabItem)
        {
            if (tabItem == null)
                return;

            // get the dock window from the tab
            DockWindow? dockWindow = null;

            foreach (var child in tabItem.Children.AsReadOnlySpan())
            {
                if (child is DockWindow dock)
                {
                    dockWindow = dock;
                    break;
                }
            }

            // check we got dock window
            if (dockWindow == null)
                return;

            // detach, state is not docked
            dockWindow.SetState(false, false);

            // remove tab item?
            if (tabItem.Children.Count == 0)
                _tabWidget.RemoveTab(tabItem);
        }

        // we "detach" whole dock node here meaning that we move all node tab content to floating dock window,
        // but don't delete node (it is just empty). Delete node is processes, when Close() is called
        void DetachNode(DockNode childNode)
        {
            // create window to screen
            var dockWidget = new DockWindow(Screen);
            // todo: should we set some mark that this was dock node
            dockWidget.Title = childNode.Title;
            dockWidget.ChildrenLayout = new GroupLayout();
            dockWidget.Size = new Vector2(300);
            dockWidget._savedBorderSize = 1;

            // create tabwidget
            UITabWidget tabWidget = new UITabWidget(dockWidget);

            // move tabs
            while(childNode.TabWidget.Children.Count > 0)
            {
                childNode.TabWidget.Children[0].Parent = tabWidget;
            }

            dockWidget.PerformLayout(NvgContext.Instance);

            // todo: set flag that this is dock node (different handling for tabs etc)
            // if dock window has tabwidget, we must move individually all tabs

            // detach, state is not docked
            dockWidget.SetState(false, true);
        }

        #endregion

        #region Attach

        // if this has subnodes, we can't attach since we don't know in which subnode tabs
        // should be attached
        public bool TryAttach(DockWindow dockWindow, out UITabItem? tabItem)
        {
            if (HasSubnodes)
            {
                tabItem = default;
                return false;
            }

            // check if we add single tab with dock window or all tabs in dock window (was really a dock node)
            if (!dockWindow.WasDockNode)
            {
                // just add tab
                tabItem = _tabWidget.AddTab(dockWindow.TabCaption != null? dockWindow.TabCaption : "Tab");
                dockWindow.Parent = tabItem;

                // note: PerformLayout called in AddTab method

                // attach, state is docked
                dockWindow.SetState(true, false);
            }
            else
            {
                // this was really a dock node - so move all tabs
                tabItem = default;

                var tabwidget = dockWindow.Children.FindFirst<UITabWidget>();

                // todo: if not really has tabwidget
                if (tabwidget != null)
                {
                    while (tabwidget.Children.Count > 0)
                    {
                        tabwidget.Children[0].Parent = _tabWidget;
                    }

                    // remove "temporary" dock window
                    dockWindow.Dispose();

                    // perform layout
                    RequestLayoutUpdate(Parent);
                }
            }

            return true;
        }

        #endregion

        #region DeleteNode

        // this is called from Close()
        bool DeleteNode(DockNode childNode)
        {
            // get the remaining node
            DockNode? remaining = FirstNode == childNode ? SecondNode : FirstNode;

            // todo: there could be totally different logic
            if (remaining == null || remaining.HasSubnodes)
                return false;

            // move tabs from remaining to this
            while (remaining.TabWidget.Children.Count > 0)
            {
                remaining.TabWidget.Children[0].Parent = _tabWidget;
            }

            // show tabwidget
            _tabWidget.Visible = true;

            // clear child dock nodes & splitter
            FirstNode.Dispose();
            SecondNode.Dispose();
            _splitter.Dispose();

            FirstNode = null;
            SecondNode = null;
            _splitter = null;

            PerformLayout(NvgContext.Instance);

            return true;
        }

        #endregion

        #region CalculateDockAreas

        // Rules:
        // - hit areas form a cross
        // - overlay areas are formed as part of the content area
        // - center overlay area covers whole content area
        void CalculateDockAreas()
        {
            // this maybe is called when size is not set 
            if (Size == Vector2.Zero)
                return;

            // default position & size
            // note: we operate in screen coordinates, since the call to draw hit & overlay areas
            // comes directly from screen

            Vector2 titleOffset = new Vector2(0, _titlebar.Size.Y);
            // note: we could also get widget tree position since Docknodes & DockContainer doesn't
            // implement IScrollable
            Vector2 pos = this.GetDisplayPosition() + titleOffset;
            Vector2 size = Size - titleOffset;

            // hit areas

            // marker rect size
            Vector2 markerSize = new Vector2(DOCK_HITAREA_SIZE);
            // marker padding
            float hitAreaPadding = HITAREA_PADDING;

            // dynamically resize marker size / padding, if not all markers fit into the content are
            if (MathF.Min(size.X, size.Y) < DOCK_HITAREA_SIZE * 3 + hitAreaPadding * 2)
            {
                markerSize = new Vector2(MathF.Min(size.X, size.Y) / 3.5f);
                hitAreaPadding = 1f;
            }

            // half marker
            Vector2 halfMarkerSize = markerSize / 2;

            // get center
            Vector2 center = pos + size / 2;

            // calculate rects for hit areas

            // center
            Rect centerArea = new Rect(center - halfMarkerSize, markerSize);
            _hitAreas[(int)DockArea.Center].Area = centerArea; // todo : not needed since it is overwritten
            _hitAreas[(int)DockArea.Center].Fill = centerArea;

            // left
            Rect area = new Rect(
                center.X - markerSize.X * 1.5f - hitAreaPadding,
                center.Y - halfMarkerSize.Y,
                markerSize);
            _hitAreas[(int)DockArea.Left].Area = area;
            _hitAreas[(int)DockArea.Left].Fill = new Rect(
                area.Position,
                new Vector2(area.Size.X * HITAREA_FILL_PROPORTION, area.Size.Y));

            // right
            area = new Rect(
                center.X + markerSize.X * 0.5f + hitAreaPadding,
                center.Y - halfMarkerSize.Y,
                markerSize);
            _hitAreas[(int)DockArea.Right].Area = area;
            _hitAreas[(int)DockArea.Right].Fill = new Rect(
                area.Position + new Vector2(area.Size.X - area.Size.X * HITAREA_FILL_PROPORTION, 0),
                new Vector2(area.Size.X * HITAREA_FILL_PROPORTION, area.Size.Y));

            // top
            area = new Rect(
                center.X - halfMarkerSize.X,
                center.Y - markerSize.Y * 1.5f - hitAreaPadding,
                markerSize);
            _hitAreas[(int)DockArea.Top].Area = area;
            _hitAreas[(int)DockArea.Top].Fill = new Rect(
                area.Position,
                new Vector2(area.Size.X, area.Size.Y * HITAREA_FILL_PROPORTION));

            // bottom
            area = new Rect(
                center.X - halfMarkerSize.X,
                center.Y + markerSize.Y * 0.5f + hitAreaPadding,
                markerSize);
            _hitAreas[(int)DockArea.Bottom].Area = area;
            _hitAreas[(int)DockArea.Bottom].Fill = new Rect(
                area.Position + new Vector2(0, area.Size.Y - area.Size.Y * HITAREA_FILL_PROPORTION),
                new Vector2(area.Size.X, area.Size.Y * HITAREA_FILL_PROPORTION));

            // overlay areas

            // center
            _hitAreas[(int)DockArea.Center].Overlay = new Rect(pos, size); // whole
            // left
            _hitAreas[(int)DockArea.Left].Overlay = new Rect(
                pos, new Vector2(centerArea.X - pos.X, size.Y));
            // right
            _hitAreas[(int)DockArea.Right].Overlay = new Rect(
                new Vector2(centerArea.X + centerArea.Size.X, pos.Y),
                new Vector2(centerArea.X - pos.X, size.Y));
            // top
            _hitAreas[(int)DockArea.Top].Overlay = new Rect(
                pos, new Vector2(size.X, centerArea.Y - pos.Y));
            // bottom
            _hitAreas[(int)DockArea.Bottom].Overlay = new Rect(
                new Vector2(pos.X, centerArea.Y + centerArea.Size.Y),
                new Vector2(size.X, centerArea.Y - pos.Y));
        }

        #endregion

        #region SetActiveHitArea

        // set active area so we know where we are docking & what operations we should do
        void SetActiveHitArea(Vector2 position)
        {
            for (int i = 0; i < _hitAreas.Length; i++)
            {
                if (_hitAreas[i].Contains(position))
                {
                    _activeDockArea = (DockArea)i;
                    return;
                }
            }

            _activeDockArea = DockArea.NONE;
        }

        #endregion
    }
}
