using NanoUI.Common;
using NanoUI.Layouts;
using NanoUI.Nvg;
using System.Numerics;

namespace NanoUI.Components
{
    // If you use this, don't set any child widgets on this. Instead set child widgets to Panel1 & Panel2
    // todo: should we check that there is exactly 3 child widgets (Panel 1 & 2, Splitter)

    /// <summary>
    /// UISplitPanel.
    /// </summary>
    public class UISplitPanel : UIWidget
    {
        // we use splitter layout to handle all layouting work
        SplitLayout _splitterLayout;
        
        public UISplitPanel()
        {
            _splitterLayout = new SplitLayout(Orientation.Horizontal);
        }

        public UISplitPanel(UIWidget parent)
            :base(parent)
        {
            _splitterLayout = new SplitLayout(Orientation.Horizontal);

            // create widgets
            Panel1 = new UIWidget(this);
            Splitter = new UISplitter(this, Orientation.Horizontal);
            Panel2 = new UIWidget(this);
        }

        #region Properties

        public override Layout? ChildrenLayout
        {
            get => _splitterLayout;
            set
            {
                // no - op: this is fixed
            }
        }

        public Orientation Orientation
        {
            get => _splitterLayout.Orientation;
            set
            {
                _splitterLayout.Orientation = value;
                if(Splitter != null)
                {
                    Splitter.Orientation = value;
                }
            }
        }
        public Vector2 Spacing
        {
            get => _splitterLayout.Spacing;
            set => _splitterLayout.Spacing = value;
        }

        // these acts as containers meaning you should set all your widgets to these panels
        public UIWidget? Panel1 { get; private set; }
        public UIWidget? Panel2 { get; private set; }
        public UISplitter? Splitter { get; private set; }

        #endregion

        #region Layout

        /// <inheritdoc />
        public override Vector2 PreferredSize(NvgContext ctx)
        {
            return Vector2.Max(MinSize, _splitterLayout.PreferredSize(ctx, this));
        }

        /// <inheritdoc />
        public override void PerformLayout(NvgContext ctx)
        {
            _splitterLayout.PerformLayout(ctx, this);
        }

        #endregion
    }
}
