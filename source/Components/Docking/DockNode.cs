using NanoUI.Common;
using NanoUI.Layouts;
using NanoUI.Nvg;
using System.Numerics;

namespace NanoUI.Components.Docking
{
    // note: hit & overlays operate in screen coordinates, since DrawDockAreas call comes directly from screen
    // with pointer position
    // todo: title + close action (in close, we must rearrange parent subnodes etc)
    public partial class DockNode : UIWidget
    {
        // todo: configueable?

        // proportion of hit area fill vs hit area size (must be < 1)
        const float HITAREA_FILL_PROPORTION = 0.3f;
        // default padding between hit areas
        const float HITAREA_PADDING = 5f;
        // hit area size
        const float DOCK_HITAREA_SIZE = 35f;

        // hit areas: used to check where to attach DockWindow
        // allowed areas: top, left, right, bottom, center
        DockHit[] _hitAreas = new DockHit[5];

        // these rects are (re)calculated, when recalculate flg is true
        bool _recalculateAreas = true;

        // this is active dock area where pointer is when DockWindow dragging
        DockArea _activeDockArea = DockArea.NONE;

        // we create split layout in case this is node + splitter
        SplitLayout _splitLayout;

        // these are invisible outside (only accessed in this class)
        UISplitter? _splitter;
        DockTabWidget _tabWidget;
        // titlebar only visible when has no subnodes
        DockTitlebar _titlebar;

        StackLayout _stackLayout;

        public DockNode(UIWidget parent)
            : base(parent)
        {
            // layout when we are drawing subnodes
            _splitLayout = new SplitLayout(Orientation.Horizontal);

            // default layout when we are drawing content
            _stackLayout = new StackLayout(Orientation.Vertical, LayoutAlignment.Fill);

            // title bar
            _titlebar = new DockTitlebar(this);

            // by default we create tabwidget
            // when this has child docknodes, we hide tabwidger(set it Visisble = false)
            _tabWidget = new DockTabWidget(this);
        }

        bool HasSubnodes => _splitter != null;

        #region Properties

        public string Title
        {
            get => _titlebar.Title;
            set => _titlebar.Title = value;
        }

        // this affects only if Children consists DockNodes & Splitter
        Orientation _orientation;
        public Orientation Orientation
        {
            get => _orientation;
            set
            {
                if (value == _orientation)
                    return;

                _orientation = value;

                RequestLayoutUpdate(this);
            }
        }

        public DockNode? FirstNode { get; private set; }
        public DockNode? SecondNode { get; private set; }

        public DockTabWidget TabWidget => _tabWidget;

        #endregion

        #region Methods

        // this is mainly called from titlebar (if there is close action mapped)
        public override void Close()
        {
            // first we must rearrange parent dock node structure
            
            // check if have right parent type
            if (Parent is DockNode parent)
            {
                // tofo: there could be totally different logic
                if (!parent.DeleteNode(this))
                {
                    return;
                }
            }

            // invokes dispose
            base.Close();
        }

        // if subnodes already exist - no need to recreate
        // todo: should we inform user that this has already subnodes - so they are not created?
        public void CreateSubNodes(Orientation orientation)
        {
            // set orientation - RequestLayoutUpdate there
            Orientation = orientation;

            // check we don' already has subnodes
            if (!HasSubnodes)
            {
                // create dock nodes & splitter
                FirstNode = new DockNode(this);
                _splitter = new UISplitter(this);
                SecondNode = new DockNode(this);
            }

            // hide tabwidget
            _tabWidget.Visible = false;
        }

        #endregion

        #region Events

        // - tabwidget calls this when tab item is dragged outside tabwidget's area &
        // - docktitlebar when dragged outside its area
        public override bool OnDetach(UIWidget child)
        {
            if(child is UITabItem tabItem)
            {
                Detach(tabItem);

                return true;
            }
            else if (child is DockTitlebar titlebar)
            {
                // we should detach this
                if(Parent is DockNode parent)
                {
                    parent.DetachNode(this);

                    return true;
                }
            }

            return false;
        }

        // this is by now called from screen, when attaching is in process
        // note: position is relative pointer position
        public override bool OnAttach(UIWidget widget, Vector2 position)
        {
            // we supoort by now only DockWindows
            if (widget is not DockWindow dockWindow)
                return false;

            DockArea dockArea = _activeDockArea;

            if (dockArea == DockArea.NONE)
            {
                return false;
            }

            if (dockArea == DockArea.Center)
            {
                return TryAttach(dockWindow, out _);
            }

            // if tabwidget has tabs, we must change their parent to correct subnode (tab node)
            DockNode? dockNode;
            DockNode? tabNode;

            if (dockArea == DockArea.Left || dockArea == DockArea.Right)
            {
                CreateSubNodes(Orientation.Horizontal);

                if (dockArea == DockArea.Left)
                {
                    dockNode = FirstNode;
                    tabNode = SecondNode;
                }
                else
                {
                    dockNode = SecondNode;
                    tabNode = FirstNode;
                }
            }
            else // DockArea.Top  && DockArea.Bottom
            {
                CreateSubNodes(Orientation.Vertical);

                if (dockArea == DockArea.Top)
                {
                    dockNode = FirstNode;
                    tabNode = SecondNode;
                }
                else
                {
                    dockNode = SecondNode;
                    tabNode = FirstNode;
                }
            }

            // move all tabs in this to tab node
            if(tabNode != null)
            {
                foreach (var child in _tabWidget.Children.AsSpan())
                {
                    if (child is UITabItem)
                    {
                        child.Parent = tabNode._tabWidget;
                    }
                }
            }
            
            // we have created new nodes - set titles to them
            if(dockNode != null)
            {
                dockNode.Title = dockWindow.Title?? "DockNode";
            }
            if (tabNode != null)
            {
                tabNode.Title = Title;
            }

            if (dockNode != null)
            {
                // attach
                return dockNode.TryAttach(dockWindow, out _);
            }

            return false;
        }

        #endregion

        #region Layout

        public override void PerformLayout(NvgContext ctx)
        {
            bool hasSubnodes = HasSubnodes;

            _titlebar.Visible = !hasSubnodes;

            // if parent is DockNode, we can set width or height fixed based on orientation
            if(Parent is DockNode node)
            {
                if(node.Orientation == Orientation.Horizontal)
                {
                    Height = node.Height;
                    FixedSize = new Vector2(FixedSize.X, Height);
                }
                else
                {
                    Width = node.Width;
                    FixedSize = new Vector2(Width, FixedSize.Y);
                }
            }

            // check if this is parent for dock nodes
            if (hasSubnodes)
            {
                // sync splitter & split layout orientation
                _splitLayout.Orientation = Orientation;
                if(_splitter != null)
                {
                    _splitter.Orientation = Orientation;
                }
                
                // perform splitter layout
                _splitLayout.PerformLayout(ctx, this);
            }
            else
            {
                _stackLayout.PerformLayout(ctx, this);

                Vector2 titleOffset = new Vector2(0, _titlebar.Size.Y);
                
                _tabWidget.Position = titleOffset;
                
                // stretch
                _tabWidget.Size = Size - titleOffset;

                _tabWidget.PerformLayout(ctx);
            }

            _recalculateAreas = true;
        }

        #endregion

        #region Drawing

        public override void Draw(NvgContext ctx)
        {
            // we check if this has subnodes (since all subnodes overdraws everything here)
            bool hasSubnodes = HasSubnodes;

            _titlebar.Visible = !hasSubnodes;

            if (!hasSubnodes)
                DrawBackgroundBrush(ctx);

            base.Draw(ctx);
        }

        // draws visual hints for possible drawing areas
        public override void PostDraw(NvgContext ctx, Vector2 pointerPosition)
        {
            if (_recalculateAreas)
            {
                CalculateDockAreas();

                _recalculateAreas = false;
            }

            // we calculate current hitarea where pointer is
            // note: we must store active hit area, because we do the attaching in DockWindow OnPointerUpDown - UP
            // if hit area set
            SetActiveHitArea(pointerPosition);

            // get hit area properties
            float cornerRadius = GetTheme().Docks.HitAreaCornerRadius;
            Color backgroundColor = GetTheme().Docks.HitAreaBackgroundColor;
            BrushBase? brush = GetTheme().Docks.HitAreaFillBrush;

            // draw hit areas
            if(brush != null)
            {
                for (int i = 0; i < _hitAreas.Length; i++)
                {
                    _hitAreas[i].DrawHitArea(ctx, cornerRadius, backgroundColor, brush);
                }
            }
            
            // Draw overlay if we have active dock area
            if (_activeDockArea != DockArea.NONE)
            {
                _hitAreas[(int)_activeDockArea].DrawOverlay(ctx, GetTheme().Docks.OverlayColor);
            }
        }

        #endregion
    }
}
