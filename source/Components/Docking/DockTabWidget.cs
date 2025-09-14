namespace NanoUI.Components.Docking
{
    // todo: we could have here also some spesific methods (remove from DockNode)

    /// <summary>
    /// DockTabWidget is an extension of normal tabwidget.
    /// So you can have different styling for dock tabs & normal tabwidget.
    /// </summary>
    public class DockTabWidget : UITabWidget
    {
        /// <inheritdoc />
        public DockTabWidget()
            : base()
        {

        }

        /// <inheritdoc />
        public DockTabWidget(DockNode parent)
            : base(parent)
        {
        }
    }
}
