using NanoUI.Common;
using NanoUI.Components.Bars;
using System.Numerics;

namespace NanoUI.Components.Docking
{
    // this is Window implementation that supports docking in dock container / node
    public class DockWindow : UIWindow
    {
        // flag indicating if this is docked or floating
        bool _docked = false;

        // this parent is normally either TabItem in Tabwidget (docked) OR Screen (floating)
        public DockWindow(UIWidget parent)
            : base(parent, string.Empty, ScrollbarType.Both)
        {
            Attachable = true;
        }

        #region Properties

        string? _tabCaption;
        public string TabCaption
        {
            get => _tabCaption ?? Title;
            set => _tabCaption = value;
        }

        // we have different attaching style if is dock node or not
        public bool WasDockNode {  get; private set; }

        #endregion

        #region Methods

        // we must store some window properties so we can use them when detaching (undocking)

        // todo: there is possibility that user changes these properties when docked, whick means
        // these values are out of sync (could be special flag "docked" in window that prevents changing
        // these values)?
        // todo2: more values/properties?
        CornerRadius _savedCornerRadius;
        internal uint _savedBorderSize;

        public virtual void SetState(bool docked, bool wasDockNode)
        {
            // set docked flag
            _docked = docked;
            WasDockNode = wasDockNode;

            // change basic window properties
            DragResizable = docked? false : true;
            Draggable = docked ? false : true;
            // shiw/hide titlebar
            ShowBar<UITitlebar>(docked ? false : true);

            if (docked)
            {
                // store window properties
                _savedCornerRadius = CornerRadius;
                _savedBorderSize = BorderSize;

                // clear values
                CornerRadius = default;
                BorderSize = default;

                Screen?.SetDragWidget(null);
            }
            else
            {
                // set floating
                Parent = Screen;

                // set position & size
                Position = Screen.PointerPosition;
                
                // todo: this shouldn't be here?
                // should we store value in docked?
                FixedSize = Vector2.Zero;

                // reset values
                CornerRadius = _savedCornerRadius;
                BorderSize = _savedBorderSize;

                RequestFocus();
            }
        }

        #endregion

        #region Events

        public override bool OnPointerUpDown(Vector2 p, PointerButton button, bool down)
        {
            // note: this happens before real attaching / docking
            if (!down && !_docked)
            {
                // now we must inform dock container (& screen) that we are ending attaching process
                // this works in both cases: attached / not attached
                DockContainer? container = Screen?.Children.FindFirst<DockContainer>();

                container?.EndDocking();
            }

            return base.OnPointerUpDown(p, button, down);
        }

        // this is here to check if we are beginning attaching proscess
        public override bool OnPointerDrag(Vector2 p, Vector2 rel)
        {
            // get the result from underlying Window
            var res = base.OnPointerDrag(p, rel);

            // check this is floating & dragging is done from header (title)
            // note: if resizing we don't do anything
            if (!_docked && WindowDragMode == DragMode.Header)
            {
                // now we must inform dock container (& screen) that we are in attaching process
                DockContainer? container = Screen.Children.FindFirst<DockContainer>();

                container?.BeginDocking();
            }

            return res;
        }

        #endregion
    }
}