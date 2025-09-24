
namespace NanoUI.Components.Views
{
    // todo: use guid's as id

    /// <summary>
    /// IViewItem<T>.
    /// </summary>
    public interface IViewItem<T>
    {
        /// <summary>
        /// Obligatory id in flat but mabdatory in hierarcial structures.
        /// </summary>
        string? Id { get; }

        /// <summary>
        /// ParentId is needed to place correctly into hierarcial structure.
        /// </summary>
        string? ParentId { get; }

        /// <summary>
        /// Widgets
        /// </summary>
        UIWidget[]? Widgets { get; set; }

        /// <summary>
        /// Event data
        /// </summary>
        T? EventData { get; set; }

        /// <summary>
        /// If row height is not defined, view item widget uses default row height from theme.
        /// </summary>
        int? RowHeight { get; set; }
    }
}
