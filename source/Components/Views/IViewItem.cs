
namespace NanoUI.Components.Views
{
    // todo: use guid's as id
    public interface IViewItem<T>
    {
        // this is obligatory in "flat" but mandatory in hierarcial structures
        string Id { get; }

        // needed to place correctly into hierarcial structure
        string ParentId { get; }

        UIWidget[] Widgets { get; set; }
        T? EventData { get; set; }
        
        // if this is not defined view item widget uses default row height from theme
        int? RowHeight { get; set; }
    }
}