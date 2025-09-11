namespace NanoUI.Components.Views.Items
{
    // note: items are just sligthly different, but they are in separate classes in order to force use
    // right ctor params & with correct view
    // todo: struct?
    // todo: use guid's as id

    /// <summary>
    /// TreeItem<T>.
    /// </summary>
    public class TreeItem<T> : IViewItem<T>
    {
        // this is obligatory in "flat" but mandatory in hierarcial structures
        public string? Id { get; internal set; }

        // needed to place correctly into hierarcial structure
        public string? ParentId { get; internal set; }

        public UIWidget[]? Widgets { get; set; }
        public T? EventData { get; set; }

        // if this is not defined view item widget uses default row height from theme
        public int? RowHeight { get; set; }

        // for hierarcial structures (treeview) - id & parentId are mandatory
        public TreeItem(T eventData, string? id, string? parentId)
        {
            EventData = eventData;
            Id = id;
            ParentId = parentId;
        }

        public TreeItem(UIWidget[] widgets, T eventData, string? id, string? parentId)
        {
            Widgets = widgets;
            EventData = eventData;
            Id = id;
            ParentId = parentId;
        }
    }
}
