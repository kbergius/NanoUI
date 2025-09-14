using NanoUI.Common;
using NanoUI.Components.Views;
using NanoUI.Components.Simple;
using NanoUI.Components.Views.Items;
using System.IO;

namespace NanoUI.Components.Files
{
    /// <summary>
    /// UIFileFolderDetails.
    /// Note: supports dynamic theming. If dynamic theming is not needed,
    /// use IconPart instead of FileIconPart with fixed Icon & IconColor (better performance).
    /// </summary>
    public class UIFileFolderDetails : UITableView<FileFolderInfo>
    {
        /// <inheritdoc />
        public UIFileFolderDetails()
        {

        }

        /// <inheritdoc />
        public UIFileFolderDetails(UIWidget parent)
            : base(parent)
        {
            // init default columns
            ViewPanel.Columns = GetTheme().Files.DetailsColumns;

            // override default cell selection mode
            ViewPanel.ViewSelectionMode = ViewSelectionMode.Item;

            // note: we get properties from ViewWidget
            //ThemeType = typeof(FileFolderDetails);
        }

        #region Properties

        /// <summary>
        /// Open dialog = true, Save>s = false.
        /// </summary>
        public bool ShowFiles = true;

        // todo: should we automatically change parent folder if selected changes?

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

        public void CreateView(string parentFolder)
        {
            // Check is valid directory
            // todo : absolute path?
            if (string.IsNullOrEmpty(parentFolder) || !Directory.Exists(parentFolder))
            {
                // todo : log/message box?
                
                // set to default
                parentFolder = Globals.DEFAULT_FOLDER_PATH;
            }

            // clear previous
            ClearChildren();

            // create view
            DoCreateView(new DirectoryInfo(parentFolder));

            // we must change layout
            RequestLayoutUpdate(this);
        }

        void DoCreateView(DirectoryInfo di)
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

                // set navigation to parent parent
                Add(CreateDetailsItem(
                    Globals.FILEFOLDER_DIRECTORY_UP,
                    di.LastWriteTime,
                    new FileFolderInfo(directoryUpPath, FileFolderType.Folder)));
            }

            // Loop folders
            foreach (var folder in di.EnumerateDirectories())
            {
                // hide hidden folders
                if ((folder.Attributes & FileAttributes.Hidden) != 0)
                    continue;

                Add(CreateDetailsItem(
                    folder.Name,
                    folder.LastWriteTime,
                    new FileFolderInfo(folder.FullName, FileFolderType.Folder)));
            }

            if (ShowFiles)
            {
                // Loop files
                foreach (var file in di.EnumerateFiles())
                {
                    // hide hidden files
                    if ((file.Attributes & FileAttributes.Hidden) != 0)
                        continue;

                    Add(CreateDetailsItem(
                        file.Name,
                        file.LastWriteTime,
                        new FileFolderInfo(file.FullName, FileFolderType.File)));
                }
            }
        }

        /// <summary>
        /// Virtual method since user may want to customize columns & their contents.
        /// </summary>
        protected virtual RowItem<FileFolderInfo> CreateDetailsItem(string displayName, System.DateTime lastWrite, in FileFolderInfo eventData)
        {
            return new RowItem<FileFolderInfo>(eventData)
            {
                Widgets = [
                    new UIFileIcon(eventData),
                    new UIText(displayName),
                    new UIText(lastWrite.ToString())]
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
                CreateView(data.Path);
            }
        }

        #endregion
    }
}
