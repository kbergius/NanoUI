using NanoUI.Common;

namespace NanoUI.Styles
{
    public class FilesStyle
    {
        public Color HardDriveColor { get; set; }
        public Color FolderColor { get; set; }
        public Color FileColor { get; set; }

        public ColumnDefinition[] DetailsColumns { get; set; }
        public ColumnDefinition[] DropdownColumns { get; set; }
        public ColumnDefinition[] ListColumns { get; set; }
        public ColumnDefinition[] TreeColumns { get; set; }
    }
}