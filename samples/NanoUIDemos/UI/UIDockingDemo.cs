using NanoUI;
using NanoUI.Common;
using NanoUI.Components;
using NanoUI.Components.Docking;
using NanoUI.Layouts;

namespace NanoUIDemos.UI
{
    // todo : when window size changes must also reset layouts
    public class UIDockingDemo : DemoBase
    {
        public UIDockingDemo(UIScreen screen)
            : base(screen)
        {
            CreateLayout();
        }

        // todo: load docking specification from file & create docking hierarchy (call dock container)
        void CreateLayout()
        {
            if (_screen == null)
                return;

            var container = new DockContainer(_screen);
            container.RootOrientaton = Orientation.Horizontal;
            // note: we set container to occupy whole screen area
            container.Size = _screen.Size;

            // test child

            // set dockNodes & splitter
            if (container.FirstNode != null)
            {
                container.FirstNode.CreateSubNodes(Orientation.Vertical);

                // test child-child
                if (container.FirstNode.FirstNode != null)
                {
                    container.FirstNode.FirstNode.CreateSubNodes(Orientation.Horizontal);

                    // set titles
                    if (container.FirstNode.FirstNode.FirstNode != null)
                    {
                        container.FirstNode.FirstNode.FirstNode.Title = "First";
                    }
                    if (container.FirstNode.FirstNode.SecondNode != null)
                    {
                        container.FirstNode.FirstNode.SecondNode.Title = "Second";
                    }
                }

                if (container.FirstNode.SecondNode != null)
                {
                    container.FirstNode.SecondNode.CreateSubNodes(Orientation.Vertical);

                    // set titles
                    if (container.FirstNode.SecondNode.FirstNode != null)
                    {
                        container.FirstNode.SecondNode.FirstNode.Title = "Third";
                    }
                    if (container.FirstNode.SecondNode.FirstNode != null)
                    {
                        container.FirstNode.SecondNode.FirstNode.Title = "Fourth";
                    }
                }
            }

            // set dockNodes & splitter
            if (container.SecondNode != null && Screen != null)
            {
                // todo: there is problem with tabcontrol tabs background A != 255
                // this is window background
                container.SecondNode.BackgroundFocused = container.SecondNode.BackgroundUnfocused =
                    new SolidBrush(new Color(55, 55, 55, 230));
                container.SecondNode.Title = "Fifth";

                for (int i = 0; i < 3; i++)
                {
                    // temp create to screen
                    var dockWidget = new DockWindow(Screen);
                    dockWidget.Title = $"WINDOW-{i}";
                    dockWidget.TabCaption = $"TAB-{i}";
                    dockWidget.ChildrenLayout = new GroupLayout();

                    var label = new UILabel(dockWidget, $"LABEL-{i}");

                    // attach to tab -> changes parent
                    if (container.SecondNode.TryAttach(dockWidget, out UITabItem? tabItem))
                    {
                        if(tabItem != null)
                        {
                            tabItem.Closable = false;
                        }
                    }
                }
            }
        }
    }
}
