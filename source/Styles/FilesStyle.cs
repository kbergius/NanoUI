using NanoUI.Common;
using System;

namespace NanoUI.Styles
{
    /// <summary>
    /// Global file style.
    /// </summary>
    public struct FilesStyle
    {
        /// <summary>
        /// Hard drive color.
        /// </summary>
        public Color HardDriveColor { get; set; }

        /// <summary>
        /// Folder color
        /// </summary>
        public Color FolderColor { get; set; }

        /// <summary>
        /// File color.
        /// </summary>
        public Color FileColor { get; set; }

        /// <summary>
        /// Column definitions for details (UIFileFolderDetails).
        /// </summary>
        public ColumnDefinition[] DetailsColumns { get; set; } = Array.Empty<ColumnDefinition>();

        /// <summary>
        /// Column definitions for dropdown (UIFileFolderDropdown).
        /// </summary>
        public ColumnDefinition[] DropdownColumns { get; set; } = Array.Empty<ColumnDefinition>();

        /// <summary>
        /// Column definitions for list (UIFileFolderList).
        /// </summary>
        public ColumnDefinition[] ListColumns { get; set; } = Array.Empty<ColumnDefinition>();

        /// <summary>
        /// Column definitions for tree (UIFileFolderTree).
        /// </summary>
        public ColumnDefinition[] TreeColumns { get; set; } = Array.Empty<ColumnDefinition>();

        public FilesStyle() { }
    }
}
