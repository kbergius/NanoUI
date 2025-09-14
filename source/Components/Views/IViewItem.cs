
namespace NanoUI.Components.Views
{
    // todo: use guid's as id

    /// <summary>
    /// IViewItem<T>.
    /// </summary>
    public interface IViewItem<T>
    {
        /// <summary>
        /// Obligatory in flat but mabdatory in hierarcial structures.
        /// </summary>
        string? Id { get; }

        /// <summary>
        /// Needed to place correctly into hierarcial structure.
        /// </summary>
        string? ParentId { get; }

        UIWidget[]? Widgets { get; set; }
        T? EventData { get; set; }

        /// <summary>
        /// If row height is not defined, view item widget uses default row height from theme.
        /// </summary>
        int? RowHeight { get; set; }
    }
}
