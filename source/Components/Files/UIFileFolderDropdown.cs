using NanoUI.Common;
using NanoUI.Components.Simple;
using NanoUI.Components.Views;
using NanoUI.Components.Views.Items;
using NanoUI.Nvg;
using System.IO;

namespace NanoUI.Components.Files
{
    // note: supports dynamic theming
    // note2: if dynamic theming is not needed, use IconPart instead of FileIconPart with fixed Icon & IconColor
    // (better performance)
    // todo2: this works with invisible _textPart. better solution could we add into FileFonderInfo
    // display text & switch with ".." & display text (SelectedText function) OR collect selected text
    // and ser it into part when necessary
    public class UIFileFolderDropdown : UIDropDownView<FileFolderInfo>
    {
        const int TEXT_PART_INDEX = 1;

        bool _drives = false;

        // this is to handle display text, when part has "..."
        UIText? _textPart;

        // this is ctor for theme/layout generation (if you use this otherwise, set parent before using widget)
        public UIFileFolderDropdown()
        {

        }

        public UIFileFolderDropdown(UIWidget parent)
            : base(parent)
        {
            // init columns
            Columns = GetTheme().Files.DropdownColumns;
            
            // note : we get basic properties from DropDownView
            //ThemeType = typeof(FileFolderDropdown);

            // create invisible text part to deal with "..."
            _textPart = new UIText(this);
            _textPart.Visible = false;
        }

        #region Properties

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

        // Drives
        public void CreateDriveDropDown()
        {
            _drives = true;

            // create previous if necessary
            ClearChilren();

            foreach (DriveInfo d in DriveInfo.GetDrives())
            {
                Add(CreateFileItem(d.Name, new FileFolderInfo(d.Name, FileFolderType.HardDrive), true));
            }
        }

        // Folders
        public void CreateFolderDropDown(string parentFolder)
        {
            // Check is valid directory
            // todo : absolute path?
            if (string.IsNullOrEmpty(parentFolder) || !Directory.Exists(parentFolder))
            {
                // todo : message box
                return;
            }

            _drives = false;

            // create previous if necessary
            ClearChilren();

            DoCreateList(new DirectoryInfo(parentFolder));

            // set initial selected folder/drive
            SetSelectedText(parentFolder);

            RequestListUpdate();
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

                // set navigation to parent parent
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
        }

        // note: this is protected to leave room user customize columns & their content
        protected virtual RowItem<FileFolderInfo> CreateFileItem(string displayName, in FileFolderInfo eventData, bool isDrive = false)
        {
            return new RowItem<FileFolderInfo>(eventData)
            {
                Widgets = [new UIFileIcon(eventData), new UIText(displayName)]
            };
        }

        #endregion

        #region Events

        // var change folder dropdown (with new parent folder)
        public override void OnSelectedChanged(UIViewItemWidget<FileFolderInfo> widget)
        {
            base.OnSelectedChanged(widget);

            if (AutoFolderSelectedChange && !_drives)
            {
                // we must clear selection since we create totally new dropdown list
                ClearSelection();

                CreateFolderDropDown(widget.EventData.Path);

                // we must check if selected was directory up
                if (widget.Children[TEXT_PART_INDEX] is UIText textPart)
                {
                    if (textPart.Text == Globals.FILEFOLDER_DIRECTORY_UP)
                    {
                        // set selected
                        SetSelectedText(widget.EventData.Path);
                    }
                }

                // we created totally new list of popup items, so we have to update popup layout
                RequestLayoutUpdate(Popup);
            }
        }

        // todo:
        void SetSelectedText(string path)
        {
            // we must get right display value
            string? text = Path.GetFileName(path);

            if (string.IsNullOrEmpty(Path.GetDirectoryName(path)))
            {
                text = Path.GetPathRoot(path);
            }

            if (_textPart != null)
            {
                _textPart.Text = text;
            }
        }

        #endregion

        #region Drawing

        // we must override Draw parts in case there is "..."
        protected override void DrawSelected(NvgContext ctx)
        {
            if (_drives)
            {
                base.DrawSelected(ctx);
                return;
            }

            // we get selected index (if no item is selected, we use first, if there are childs)
            var seLectecIndex = ViewPanel.GetSelectedIndexOrDefault();

            if (seLectecIndex >= 0)
            {
                if (ViewPanel.Children[seLectecIndex] is UIViewItemWidget<FileFolderInfo> viewItem)
                {
                    // get the position conversion
                    var pos = Position - viewItem.Position;

                    // make "correction" from selected item coordinate space to this
                    ctx.Translate(pos);

                    // loop parts
                    for (int i = 0; i < viewItem.Children.Count; i++)
                    {
                        // check we have column
                        if (Columns.Length < i)
                            break;

                        UIWidget? part;

                        // special handling for text part
                        if(i == TEXT_PART_INDEX)
                        {
                            // text part
                            part = _textPart;
                            if (part != null)
                            {
                                part.Position = viewItem.Children[i].Position;
                                part.Size = viewItem.Children[i].Size;
                            }
                        }
                        else
                        {
                            part = viewItem.Children[i];
                        }

                        if(part != null)
                        {
                            // set scissor to part - so it doesn't overflow
                            ctx.SaveState();

                            ctx.IntersectScissor(part.Position, part.Size);

                            // do draw
                            part.Draw(ctx);

                            ctx.RestoreState();
                        }
                    }


                    ctx.Translate(-pos);
                }
            }
        }

        #endregion
    }
}
