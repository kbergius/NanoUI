namespace NanoUI.Components.Views.Items
{
    // note: items are just sligthly different, but they are in separate classes in order to force use
    // right ctor params & with correct view
    // todo: struct?
    // todo: use guid's as id

    /// <summary>
    /// RowItem<T>.
    /// </summary>
    public class RowItem<T> : IViewItem<T>
    {
        /// <summary>
        /// Obligatory in flat but mabdatory in hierarcial structures.
        /// </summary>
        public string? Id { get; }

        /// <summary>
        /// Needed to place correctly into hierarcial structure.
        /// </summary>
        public string? ParentId { get; }

        public UIWidget[]? Widgets { get; set; }
        public T? EventData { get; set; }

        /// <summary>
        /// If row height is not defined, view item widget uses default row height from theme.
        /// </summary>
        public int? RowHeight { get; set; }

        // todo : check, cells are set in extension

        public RowItem(T eventData)
        {
            EventData = eventData;
        }

        public RowItem(UIWidget[] widgets, T eventData)
        {
            Widgets = widgets;
            EventData = eventData;
        }
    }
}
