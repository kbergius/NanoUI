using NanoUI.Common;
using NanoUI.Components.Files;
using NanoUI.Layouts;
using NanoUI.Nvg;
using System;
using System.IO;
using System.Numerics;

namespace NanoUI.Components.Dialogs
{
    // todo : Filter by filetype
    // todo : tree in left side?
    // todo: calculate dynamically layout & widgets
    public class UIFileDialog : UIDialog
    {
        Action<UIWidget, FileFolderInfo>? _selected;

        bool _inited;

        UIFileFolderList? _fileFolderList;
        UIFileFolderDetails? _fileFolderDetails;

        UILabel? _selectionPathText;
        // Open - not editable, Save as editable
        UITextField? _selectionFilename;

        // this is ctor for theme/layout generation (if you use this otherwise, set parent before using widget)
        public UIFileDialog()
        {

        }

        // note: it is not recommended to call in your code. Instead call Screen.GetDialog<FileDialog>.
        // if you still want to call this, you are responsible to handle dispose new instance manually
        public UIFileDialog(UIScreen screen)
            : base(screen)
        {
            ChildrenLayout = new StackLayout(Orientation.Vertical, LayoutAlignment.Middle) { Spacing = new Vector2(10) };
            FixedSize = new Vector2(650, 435);

            // Top
            var topPanel = new UIWidget(this);
            topPanel.FixedSize = new Vector2(600, 0);
            topPanel.ChildrenLayout = new StackLayout(Orientation.Horizontal)
            {
                Alignment = LayoutAlignment.Fill,
                Spacing = new Vector2(10)
            };
            new UILabel(topPanel, "Path: ");

            _selectionPathText = new UILabel(topPanel);
            // todo: dynamically calculate
            _selectionPathText.FixedSize = new Vector2(550, 0);

            var fileViewPanel = new UIWidget(this);
            // todo: dynamically calculate
            fileViewPanel.Size = new Vector2(600, 250);

            // splitter layout!
            fileViewPanel.ChildrenLayout = new SplitLayout(Orientation.Horizontal);

            // folder list
            _fileFolderList = new UIFileFolderList(fileViewPanel);
            _fileFolderList.Size = new Vector2(200, 250);
            _fileFolderList.ShowFiles = false;

            // splitter
            new UISplitter(fileViewPanel, Orientation.Horizontal);

            // file details
            _fileFolderDetails = new UIFileFolderDetails(fileViewPanel);
            _fileFolderDetails.Size = new Vector2(400, 250);

            // we can navigate with list & details (they can go out of sync)
            
            // Path text box at top tells where we are in file dierctory
            
            // wrap folder list change
            _fileFolderList.SelectedChanged += (data) =>
            {
                if (data.FileFolderType == FileFolderType.Folder)
                {
                    _fileFolderDetails.CreateView(data.Path);

                    SetSelection(data);
                }
            };

            // wrap fileFolderDetails
            _fileFolderDetails.SelectedChanged += (data) =>
            {
                SetSelection(data);
            };

            // Selection
            var selectionFilePanel = new UIWidget(this);
            selectionFilePanel.FixedSize = new Vector2(600, 0);
            selectionFilePanel.ChildrenLayout = new StackLayout(Orientation.Horizontal)
            {
                Alignment = LayoutAlignment.Fill,
                Spacing = new Vector2(15)
            };
            new UILabel(selectionFilePanel, "Filename: ");

            _selectionFilename = new UITextField(selectionFilePanel);
            _selectionFilename.FixedSize = new Vector2(500, 0);

            // Buttons
            UIWidget buttonPanel = new UIWidget(this);
            buttonPanel.ChildrenLayout = new StackLayout(Orientation.Horizontal, LayoutAlignment.Middle) { Spacing = new Vector2(10) };
            buttonPanel.Margin = ButtonPanelMargin;

            _okButton = new UIButton(buttonPanel, "OK");
            _okButton.FixedSize = new Vector2(90, 0);
            _okButton.Clicked += () =>
            {
                if (_selectionFilename != null && _selectionFilename.Text.Trim().Length == 0)
                {
                    // show message box
                    var box = Screen?.GetDialog<UIMessageBox>();
                    if(box != null)
                    {
                        box.DialogType = MessageDialogType.Error;
                        box.Text = "Path is invalid: <empty>";
                        // todo : test!
                        box.SetCallback(this, null);
                    }
                    
                    return;
                }

                // we combine - todo: should not need trimming?
                if(_caller != null && _selectionPathText != null && _selectionFilename != null)
                {
                    _selected?.Invoke(_caller,
                    new FileFolderInfo(Path.Combine(
                        _selectionPathText.Caption, _selectionFilename.Text.Trim()), FileFolderType.File));
                }

                Close();
            };

            _cancelButton = new UIButton(buttonPanel, "Cancel");
            _cancelButton.FixedSize = new Vector2(90, 0);
            _cancelButton.Clicked += () =>
            {
                Close();
            };
        }

        #region Properties

        // default path
        string _startPath = Globals.DEFAULT_FOLDER_PATH;
        public string StartPath
        {
            get => _startPath;
            set
            {
                // we check path is valid
                if (Directory.Exists(value))
                {
                    _startPath = value;
                }
            }
        }

        public FileDialogType DialogType { get; set; } = FileDialogType.Open;

        UIButton? _okButton;
        public UIButton? OKButton => _okButton;
        
        UIButton? _cancelButton;
        public UIButton? CancelButton => _cancelButton;
        
        #endregion

        #region Methods

        // we use caller as identifier
        public void SetCallback(UIWidget caller, Action<UIWidget, FileFolderInfo> action)
        {
            _caller = caller;
            _selected = action;
            Visible = true;

            _inited = false;
        }

        #endregion

        #region Drawing

        public override void Draw(NvgContext ctx)
        {
            if (!_inited)
            {
                // delayed set
                _inited = true;

                // Set initial values
                SetSelection(new FileFolderInfo(_startPath, FileFolderType.Folder));

                // set params (open has different, save as & new requires filename)
                if(_fileFolderDetails != null)
                {
                    _fileFolderDetails.ShowFiles = DialogType == FileDialogType.Open ? true : false;
                }
                if (_selectionFilename != null)
                {
                    _selectionFilename.Editable = DialogType == FileDialogType.Open ? false : true;
                }

                // init file views
                _fileFolderList?.CreateList(_startPath);
                _fileFolderDetails?.CreateView(_startPath);

                base.ReInit(ctx);
            }

            base.Draw(ctx);
        }

        #endregion

        #region Private

        void SetSelection(FileFolderInfo selectedInfo)
        {
            // SET FILENAME
            if (selectedInfo.FileFolderType == FileFolderType.Folder)
            {
                // SET PATH
                if(_selectionPathText != null)
                {
                    _selectionPathText.Caption = selectedInfo.Path;
                }

                // can't be string.Empty
                if (_selectionFilename != null)
                {
                    _selectionFilename.Text = " ";
                }
            }
            else
            {
                var dir = Path.GetDirectoryName(selectedInfo.Path);
                if (_selectionPathText != null)
                {
                    _selectionPathText.Caption = dir ?? selectedInfo.Path;
                }

                if (_selectionFilename != null)
                {
                    _selectionFilename.Text = Path.GetFileName(selectedInfo.Path);
                }
            }
        }

        #endregion
    }
}
