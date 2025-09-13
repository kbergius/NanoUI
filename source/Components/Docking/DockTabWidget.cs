namespace NanoUI.Components.Docking
{
    // this is extension of tabwidget so user can have different styling for dock tabs & normal tabwidget
    // todo: we could have here also some spesific methods (remove from DockNode)

    /// <summary>
    /// DockTabWidget.
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
