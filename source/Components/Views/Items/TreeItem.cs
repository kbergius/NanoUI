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
        /// <summary>
        /// Obligatory in flat but mabdatory in hierarcial structures.
        /// </summary>
        public string? Id { get; internal set; }

        /// <summary>
        /// Needed to place correctly into hierarcial structure.
        /// </summary>
        public string? ParentId { get; internal set; }

        public UIWidget[]? Widgets { get; set; }
        public T? EventData { get; set; }

        /// <summary>
        /// If row height is not defined, view item widget uses default row height from theme.
        /// </summary>
        public int? RowHeight { get; set; }

        /// <summary>
        /// For hierarcial structures (treeview) - id & parentId are mandatory.
        /// </summary>
        public TreeItem(T eventData, string? id, string? parentId)
        {
            EventData = eventData;
            Id = id;
            ParentId = parentId;
        }

        /// <summary>
        /// For hierarcial structures (treeview) - id & parentId are mandatory.
        /// </summary>
        public TreeItem(UIWidget[] widgets, T eventData, string? id, string? parentId)
        {
            Widgets = widgets;
            EventData = eventData;
            Id = id;
            ParentId = parentId;
        }
    }
}
