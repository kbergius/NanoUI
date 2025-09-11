using NanoUI.Common;
using NanoUI.Components.Views;
using NanoUI.Components.Simple;
using NanoUI.Components.Views.Items;
using System.IO;

namespace NanoUI.Components.Files
{
    // note: supports dynamic theming
    // note2: if dynamic theming is not needed, use IconPart instead of FileIconPart with fixed Icon & IconColor
    // (better performance)

    // todo : should we have 2-way selection changed process
    // 1. set selected
    // 2. fire selection changed

    // by now both actions are combined into one

    /// <summary>
    /// UIFileFolderList.
    /// </summary>
    public class UIFileFolderList : UIListView<FileFolderInfo>
    {
        // this is ctor for theme/layout generation (if you use this otherwise, set parent before using widget)
        public UIFileFolderList()
        {

        }

        public UIFileFolderList(UIWidget parent)
            : base(parent)
        {
            // init default columns
            ViewPanel.Columns = GetTheme().Files.ListColumns;

            // note: we get basic properties from ViewWidget
            //ThemeType = typeof(FileFolderList);
        }

        #region Properties

        public bool ShowFiles = true;

        // should we automatically change parent folder if selected changes
        bool? _autoFolderSelectedChange;
        public bool AutoFolderSelectedChange
        {
            get => _autoFolderSelectedChange?? GetTheme().FileFolderDetails.AutoFolderSelectedChange;
            set => _autoFolderSelectedChange = value;
        }

        bool? _showDirectoryUp;
        public bool ShowDirectoryUp
        {
            get => _showDirectoryUp?? GetTheme().FileFolderDetails.ShowDirectoryUp;
            set => _showDirectoryUp = value;
        }

        #endregion

        #region Methods

        public void CreateList(string parentFolder)
        {
            // Check is valid directory
            // todo : absolute path?
            if (string.IsNullOrEmpty(parentFolder) || !Directory.Exists(parentFolder))
            {
                // todo : message box
                return;
            }

            // Clear list - if changed
            ClearChildren();

            DoCreateList(new DirectoryInfo(parentFolder));

            RequestLayoutUpdate(this);
        }

        void DoCreateList(DirectoryInfo di)
        {
            // we check also that we are not in root folder/drive
            if (ShowDirectoryUp && Path.GetDirectoryName(di.FullName) != null)
            {
                // create link to parent parent (up in hierarcy)
                string directoryUpPath;

                if (di.Parent != null)
                {
                    directoryUpPath = di.Parent.FullName;
                }
                else
                {
                    // root folder
                    directoryUpPath = di.Name;
                }

                // set nagigation to parent parent
                Add(CreateFileItem(
                    Globals.FILEFOLDER_DIRECTORY_UP,
                    new FileFolderInfo(directoryUpPath, FileFolderType.Folder)));
            }

            // Loop child folders
            foreach (var folder in di.EnumerateDirectories())
            {
                // hide hidden folders
                if ((folder.Attributes & FileAttributes.Hidden) != 0)
                    continue;

                Add(CreateFileItem(
                    Path.GetFileName(folder.Name),
                    new FileFolderInfo(folder.FullName, FileFolderType.Folder)));

            }

            // Show files?
            if (ShowFiles)
            {
                // Loop child files
                foreach (var file in di.EnumerateFiles())
                {
                    // hide hidden files
                    if ((file.Attributes & FileAttributes.Hidden) != 0)
                        continue;

                    Add(CreateFileItem(
                        Path.GetFileName(file.Name),
                        new FileFolderInfo(file.FullName, FileFolderType.File)));
                }
            }
        }

        // note: this is protected to support user customize columns & their contents
        protected virtual RowItem<FileFolderInfo> CreateFileItem(string displayName, in FileFolderInfo eventData, bool isDrive = false)
        {
            return new RowItem<FileFolderInfo>(eventData)
            {
                Widgets = [new UIFileIcon(eventData), new UIText(displayName)]
            };
        }

        #endregion

        #region Events

        /// <inheritdoc />
        public override void OnSelectedChanged(FileFolderInfo data)
        {
            base.OnSelectedChanged(data);

            if (AutoFolderSelectedChange && data.FileFolderType == FileFolderType.Folder)
            {
                // we must clear selection since we create totally new
                ResetIndexes();

                // change path
                CreateList(data.Path);
            }
        }

        #endregion
    }
}
