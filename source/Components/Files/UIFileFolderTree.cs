using NanoUI.Common;
using NanoUI.Components.Views;
using NanoUI.Components.Simple;
using NanoUI.Components.Views.Items;
using System;
using System.IO;

namespace NanoUI.Components.Files
{
    // todo : CHECK ABOSULUTE PATH
    // todo : FileIcon by file type
    // todo : hide hidden/system files/folders
    // todo : load dynamically tree (or change parent?) - must INSERT rows to correct position (after parent)
    // todo : CREATE ROOT WITH CORRECT COLUMNS & make its properties editable (caption, visibility)
    // todo : CHANGE ICON (Folder collapsed/expanded)
    // todo : SHOULD WE HAVE BASE PATH?

    /// <summary>
    /// UIFileFolderTree shows folders (and possibly files).
    /// Note: supports dynamic theming. If dynamic theming is not needed,
    /// use IconPart instead of FileIconPart with fixed Icon & IconColor (better performance).
    /// </summary>
    public class UIFileFolderTree : UITreeView<FileFolderInfo>
    {
        /// <inheritdoc />
        public UIFileFolderTree()
        {

        }

        /// <inheritdoc />
        public UIFileFolderTree(UIWidget parent)
            : this(parent, AppDomain.CurrentDomain.BaseDirectory)
        {
        }

        /// <inheritdoc />
        public UIFileFolderTree(UIWidget parent, string rootFolder)
            : base(parent, new FileFolderInfo(rootFolder, FileFolderType.Folder))
        {
            // init default columns
            ViewPanel.Columns = GetTheme().Files.TreeColumns;

            // note: we get properties from ViewWidget
            //ThemeType = typeof(FileFolderTree);
        }

        #region Properties

        /// <summary>
        /// Show files. Default: true.
        /// </summary>
        public bool ShowFiles = true;

        #endregion

        #region Methods

        /// <summary>
        /// Clears previous tree if any.
        /// </summary>
        public void CreateTree(string parentFolder, int maximumLevels)
        {
            // clear previous
            ClearChildren();

            // Check is valid directory
            // todo : absolute path?
            if (string.IsNullOrEmpty(parentFolder) || !Directory.Exists(parentFolder))
            {
                // todo : message box
                return;
            }

            CreateTree(parentFolder, new DirectoryInfo(parentFolder), 0, maximumLevels);
        }

        // recursive function
        void CreateTree(string parentFolder, DirectoryInfo di, int level, int maximumLevels)
        {
            // Loop child folders
            foreach (var folder in di.EnumerateDirectories())
            {
                // hide hidden folders
                if ((folder.Attributes & FileAttributes.Hidden) != 0)
                    continue;

                // add folder row
                AddGroup(CreateTreeItem(
                    folder.Name,
                    new FileFolderInfo(folder.FullName, FileFolderType.Folder),
                    folder.FullName,
                    parentFolder));

                // stop endless creation
                if (level < maximumLevels)
                {
                    // continue loop
                    CreateTree(folder.FullName, folder, level + 1, maximumLevels);
                }
            }

            // show files?
            if (ShowFiles)
            {
                // Loop child files
                foreach (var file in di.EnumerateFiles())
                {
                    // hide hidden files
                    if ((file.Attributes & FileAttributes.Hidden) != 0)
                        continue;

                    // add file row
                    AddItem(CreateTreeItem(
                        file.Name, 
                        new FileFolderInfo(file.FullName, FileFolderType.File),
                        file.FullName,
                        parentFolder));
                }
            }
        }

        /// <summary>
        /// For hierarcial structures (treeview) - id & parentId are mandatory.
        /// </summary>
        protected virtual TreeItem<FileFolderInfo> CreateTreeItem(string displayName, in FileFolderInfo eventData, string id, string parentId)
        {
            return new TreeItem<FileFolderInfo>(eventData, id, parentId)
            {
                Widgets = [new UIFileIcon(eventData), new UIText(displayName)]
            };
        }

        #endregion
    }
}
