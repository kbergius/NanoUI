using NanoUI.Common;
using NanoUI.Components.Simple;
using NanoUI.Components.Views;
using NanoUI.Components.Views.Items;
using System.IO;

namespace NanoUI.Components.Files
{
    // note: supports dynamic theming
    // note2: if dynamic theming is not needed, use IconTextPart instead of FileIconTextPart with fixed Icon & IconColor
    // (better performance)

    /// <summary>
    /// UIFileFolderFlow.
    /// </summary>
    public class UIFileFolderFlow : UIFlowView<FileFolderInfo>
    {
        // this is ctor for theme/layout generation (if you use this otherwise, set parent before using widget)
        public UIFileFolderFlow()
        {

        }

        public UIFileFolderFlow(UIWidget parent)
            : base(parent)
        {
            // todo : handle different sizes (Large, Medium, ...)

            // init defualt cell size
            PartSize = GetTheme().FileFolderFlow.PartSize;

            // note: we get basic properties from ViewWidget
            //ThemeType = typeof(FileFolderFlow);
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

        public void CreateView(string parentFolder)
        {
            // Check is valid directory
            // todo : absolute path?
            if (string.IsNullOrEmpty(parentFolder) || !Directory.Exists(parentFolder))
            {
                // todo : message box
                return;
            }

            ClearChildren();

            DoCreateView(new DirectoryInfo(parentFolder));

            RequestLayoutUpdate(this);
        }

        // note: we must set data to each cells, since we are using cell select method
        void DoCreateView(DirectoryInfo di)
        {
            // we check also that we are not in root folder/drive
            if (ShowDirectoryUp && Path.GetDirectoryName(di.FullName) != null)
            {
                // create link to parent parent(up in hierarcy)
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
                Add(CreateFlowItem(
                    Globals.FILEFOLDER_DIRECTORY_UP,
                    new FileFolderInfo(directoryUpPath, FileFolderType.Folder)));
            }

            // Loop child folders
            foreach (var folder in di.EnumerateDirectories())
            {
                // hide hidden folders
                if ((folder.Attributes & FileAttributes.Hidden) != 0)
                    continue;

                Add(CreateFlowItem(
                    Path.GetFileName(folder.Name),
                    new FileFolderInfo(folder.FullName, FileFolderType.Folder)));

            }

            if (ShowFiles)
            {
                // Loop files
                foreach (var file in di.EnumerateFiles())
                {
                    // hide hidden folders
                    if ((file.Attributes & FileAttributes.Hidden) != 0)
                        continue;

                    Add(CreateFlowItem(
                        Path.GetFileName(file.Name),
                        new FileFolderInfo(file.FullName, FileFolderType.File)));
                }
            }
        }

        // this is prorected to leave room for user to customize columns & their content
        protected virtual FlowItem<FileFolderInfo> CreateFlowItem(string displayName, in FileFolderInfo eventData)
        {
            return new FlowItem<FileFolderInfo>(eventData)
            {
                Widgets = [new UIFileIconText(eventData, displayName)]
            };
        }

        #endregion

        #region Events

        /// <inheritdoc />
        public override void OnCellSelectedChanged(UIViewItemWidget<FileFolderInfo> widget, int columnIndex)
        {
            base.OnCellSelectedChanged(widget, columnIndex);

            FileFolderInfo data = widget.EventData;

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
