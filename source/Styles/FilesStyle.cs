using NanoUI.Common;
using System;

namespace NanoUI.Styles
{
    // todo: struct
    public class FilesStyle
    {
        public Color HardDriveColor { get; set; }
        public Color FolderColor { get; set; }
        public Color FileColor { get; set; }

        public ColumnDefinition[] DetailsColumns { get; set; } = Array.Empty<ColumnDefinition>();
        public ColumnDefinition[] DropdownColumns { get; set; } = Array.Empty<ColumnDefinition>();
        public ColumnDefinition[] ListColumns { get; set; } = Array.Empty<ColumnDefinition>();
        public ColumnDefinition[] TreeColumns { get; set; } = Array.Empty<ColumnDefinition>();
    }
}
