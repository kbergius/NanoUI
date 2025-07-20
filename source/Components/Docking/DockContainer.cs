using NanoUI.Common;
using NanoUI.Nvg;
using System.Numerics;

namespace NanoUI.Components.Docking
{
    // note: screen can consist only 1 dock container
    // todo: load docking specification from file & create docking hierarchy
    public class DockContainer : UIWidget
    {
        DockNode _root;

        // this is the dock node where attaching is happening
        // supports for printing hit & overlay areas
        // note: we modify here screen's post draw list
        DockNode? _lastActiveNode;

        public DockContainer(UIScreen screen, Orientation orientation = Orientation.Horizontal)
            : base(screen)
        {
            // create default root node
            _root = new DockNode(this);
            _root.CreateSubNodes(orientation);
        }

        #region Properties

        public DockNode FirstNode => _root.FirstNode;
        public DockNode SecondNode => _root.SecondNode;

        public Orientation RootOrientaton
        {
            get => _root.Orientation;
            set
            {
                _root.Orientation = value;

                RequestLayoutUpdate(_root);
            }
        }

        #endregion

        #region Methods

        // dock window informs that it is dragging and wants to be attached
        // todo: could be internal?
        public void BeginDocking()
        {
            // now we must find DockNode where the pointer is & inform screen of this DockNode
            DockNode? pointerNode = FindLastNode(_root, Screen.PointerPosition); // - _root.Position);

            if(_lastActiveNode != null)
            {
                // remove last from screen's post draw list
                Screen?.PostDrawList.Remove(_lastActiveNode);
            }

            // set
            _lastActiveNode = pointerNode;

            if (pointerNode != null)
            {
                // add to screen's post draw list
                Screen?.PostDrawList.Add(pointerNode);
            }
        }

        // this is called from dock window, when pointer up event happens (it doesn't care if window
        // is reaaly docked or not)
        // todo: could be internal?
        public void EndDocking()
        {
            if (_lastActiveNode != null)
            {
                // remove from screen's post draw list
                Screen?.PostDrawList.Remove(_lastActiveNode);
            }

            _lastActiveNode = null;
        }

        #endregion

        #region Events

        public override void OnScreenResize(Vector2 size, NvgContext ctx)
        {
            Size = size;

            PerformLayout(ctx);
        }

        #endregion

        #region Layout

        public override void PerformLayout(NvgContext ctx)
        {
            // set root node to fill all area
            _root.Position = Vector2.Zero;
            _root.Size = Size;

            base.PerformLayout(ctx);
        }

        #endregion

        #region Drawing

        public override void Draw(NvgContext ctx)
        {
            base.Draw(ctx);
        }

        #endregion

        #region Private

        static DockNode? FindLastNode(DockNode parent, Vector2 position)
        {
            // get relative position
            var pos = position - parent.Position;

            foreach (var child in parent.Children.AsReadOnlySpan())
            {
                if (child is not DockNode dockNode)
                    continue;

                if (dockNode.FirstNode == null && dockNode.SecondNode == null && child.Contains(pos))
                {
                    return dockNode;
                }

                // do recursive search
                var found = FindLastNode(dockNode, pos);

                if (found != null)
                    return found;
            }

            return null;
        }


        #endregion
    }
}