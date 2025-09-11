using NanoUI.Common;
using NanoUI.Components.Files;
using NanoUI.Layouts;
using NanoUI.Nvg;
using System;
using System.IO;
using System.Numerics;

namespace NanoUI.Components.Dialogs
{
    // todo : Create folder
    // todo: should inhert from abstract Dialog widget
    // todo: calculate dynmically layout & widgets
    // todo: should there be differnt lables for path & selected folder name?

    /// <summary>
    /// UIFolderDialog.
    /// </summary>
    public class UIFolderDialog : UIDialog
    {
        const int COMPONENTS_WIDTH = 400;

        bool _inited;

        UIFileFolderList? _fileFolderList;
        UITextField? _selectionPathText;

        Action<UIWidget, string>? _folderSelected;

        // this is ctor for theme/layout generation (if you use this otherwise, set parent before using widget)
        public UIFolderDialog()
        {

        }

        public UIFolderDialog(UIScreen screen)
            : base(screen)
        {
            Title = "Choose folder";

            ChildrenLayout = new StackLayout(Orientation.Vertical, LayoutAlignment.Middle);
            FixedSize = new Vector2(COMPONENTS_WIDTH + 50, 378);

            // Folder list
            _fileFolderList = new UIFileFolderList(this);
            _fileFolderList.Size = new Vector2(COMPONENTS_WIDTH, 250);
            _fileFolderList.ShowFiles = false;

            // Selected
            var selectedPanel = new UIWidget(this);
            selectedPanel.FixedSize = new Vector2(COMPONENTS_WIDTH, 0);
            selectedPanel.ChildrenLayout = new StackLayout(Orientation.Horizontal)
            {
                Alignment = LayoutAlignment.Fill,
                Spacing = new Vector2(15)
            };

            new UILabel(selectedPanel, "Path: ");

            _selectionPathText = new UITextField(selectedPanel);
            _selectionPathText.Editable = false;
            _selectionPathText.FixedSize = new Vector2(COMPONENTS_WIDTH, 0);

            _fileFolderList.SelectedChanged += (data) =>
            {
                _selectionPathText.Text = data.Path;
            };

            // Buttons
            UIWidget buttonPanel = new UIWidget(this);
            buttonPanel.ChildrenLayout = new StackLayout(Orientation.Horizontal, LayoutAlignment.Middle) { Spacing = new Vector2(10) };
            buttonPanel.Margin = ButtonPanelMargin;

            _okButton = new UIButton(buttonPanel, "OK");
            _okButton.FixedSize = new Vector2(90, 0);
            _okButton.Clicked += () =>
            {
                if (_selectionPathText != null && _selectionPathText.Text.Trim().Length == 0)
                {
                    // show message box
                    UIMessageBox? box = screen.GetDialog<UIMessageBox>();
                    if (box != null)
                    {
                        box.DialogType = MessageDialogType.Error;
                        box.Text = "Path is invalid: <empty>";
                    }
                    
                    return;
                }

                if(_caller != null && _selectionPathText != null)
                {
                    _folderSelected?.Invoke(_caller, _selectionPathText.Text);
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

        UIButton? _okButton;
        public UIButton? OKButton => _okButton;
        
        UIButton? _cancelButton;
        public UIButton? CancelButton => _cancelButton;
        
        #endregion

        #region Methods

        // we use caller as identifier
        public void SetCallback(UIWidget caller, Action<UIWidget, string> action)
        {
            _caller = caller;
            _folderSelected = action;
            Visible = true;

            _inited = false;
        }

        #endregion

        #region Drawing

        /// <inheritdoc />
        public override void Draw(NvgContext ctx)
        {
            if (!_inited)
            {
                // delayed set
                _inited = true;

                _fileFolderList?.CreateList(_startPath);

                if(_selectionPathText != null)
                {
                    _selectionPathText.Text = _startPath;
                }
                
                base.ReInit(ctx);
            }

            base.Draw(ctx);
        }

        #endregion
    }
}
