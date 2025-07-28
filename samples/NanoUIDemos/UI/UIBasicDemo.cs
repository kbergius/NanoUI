using NanoUI;
using NanoUI.Common;
using NanoUI.Components;
using NanoUI.Components.Buttons;
using NanoUI.Components.Dialogs;
using NanoUI.Components.Files;
using NanoUI.Layouts;
using NanoUI.Components.Views;
using NanoUI.Components.Views.Items;
using NanoUI.Fonts;
using Color = NanoUI.Common.Color;
using System.Numerics;
using NanoUI.Components.Colors;
using NanoUI.Components.Simple;

namespace NanoUIDemos.UI
{
    public class UIBasicDemo : DemoBase
    {
        const string _longText = "Lorem ipsum dolor sit amet, consectetur adipiscing elit, " +
                "sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. \n\n" +
                "Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris nisi ut " +
                "aliquip ex ea commodo consequat.\n\n Duis aute irure dolor in reprehenderit in " +
                "voluptate velit esse cillum dolore eu fugiat nulla pariatur. Excepteur sint " +
                "occaecat cupidatat non proident, sunt in culpa qui officia deserunt mollit anim " +
                "id est laborum." +
                "Lorem ipsum dolor sit amet, consectetur adipiscing elit, " +
                "sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. \n\n" +
                "Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris nisi ut " +
                "aliquip ex ea commodo consequat.\n\n Duis aute irure dolor in reprehenderit in " +
                "voluptate velit esse cillum dolore eu fugiat nulla pariatur. Excepteur sint " +
                "occaecat cupidatat non proident, sunt in culpa qui officia deserunt mollit anim " +
                "id est laborum.";

        // we update progress here, so we must have reference
        UIProgressbar? _progress;

        public UIBasicDemo(UIScreen screen)
            :base(screen)
        {
            // Buttons & Tooltips (set mouse over first 2 buttons)
            TestButtons(screen);

            // Message/File/Folder/ColorDialogs
            TestDialogs(screen);

            // ComboBox, DropDownView, ListBox & ListView
            TestDropDownsLists(screen);

            // CheckBox, Progressbar, Slider
            TestCheckboxProgressSlider(screen);

            // TreeView
            TestTreeView(screen);

            // TableView
            TestTableView(screen);

            // Grid
            TestGrid(screen);
        }

        #region Buttons

        void TestButtons(UIScreen screen)
        {
            UIWindow window = new UIWindow(screen, "Buttons");
            window.Position = new Vector2(15);
            window.ChildrenLayout = new GroupLayout();

            var lblEvent = new UILabel(window, "Event Value");
            lblEvent.TextColor = Color.Red;

            new UILabel(window, "Push buttons");

            // normal
            UIButton b = new UIButton(window, "Normal button");
            b.Clicked += () => { lblEvent.Caption = "Normal clicked"; };
            b.Tooltip = "short tooltip";

            // styled
            b = new UIButton(window, "Styled", (int)FontAwesomeIcon.Rocket);
            b.IconAlign = IconAlign.LeftCentered;
            //b.BackgroundColor = new Color(0, 0, 255, 120);
            b.Clicked += () => { lblEvent.Caption = "Styled clicked"; };
            b.Tooltip = "This button has a fairly long tooltip. It is so long, in " +
                "fact, that the shown text will span several lines.";

            b.BackgroundFocused = b.BackgroundUnfocused =
                new LinearGradient(new Color(0, 0, 255, 180), new Color(0, 0, 255, 60), new CornerRadius(3));

            // disabled
            b = new UIButton(window, "Disabled button");
            b.Disabled = true;

            // Toggle button
            new UILabel(window, "Toggle button");

            b = new UIButton(window, "Toggle me");
            b.BackgroundFocused = b.BackgroundUnfocused = new SolidBrush(new Color(0, 0, 255, 255));
            b.Flags = ButtonFlags.ToggleButton;
            b.StateChanged += (button, pushed) =>
            {
                lblEvent.Caption = "Toggle pushed: " + pushed;
            };

            // Radio buttons
            new UILabel(window, "Radio buttons");

            b = new UIButton(window, "Radio button 1");
            b.Flags = ButtonFlags.RadioButton;
            b.Pushed = true;
            b.StateChanged += (button, pushed) =>
            {
                if (pushed)
                {
                    lblEvent.Caption = "Radio1 pushed: " + pushed;
                }
            };

            b = new UIButton(window, "Radio button 2");
            b.Flags = ButtonFlags.RadioButton;
            b.StateChanged += (button, pushed) =>
            {
                if (pushed)
                {
                    lblEvent.Caption = "Radio2 pushed: " + pushed;
                }
            };

            // Updown
            float upDownValue = 0;

            new UILabel(window, "UpDown button");

            var upDown = new UpDownButton<float>(window);
            upDown.ButtonPushed += (val) =>
            {
                // we get step
                upDownValue += val;
                lblEvent.Caption = "Updown: " + upDownValue;
            };

            // Tool buttons
            new UILabel(window, "Tool buttons");
            UIWidget tools = new UIWidget(window);
            tools.ChildrenLayout = new StackLayout(Orientation.Horizontal)
            {
                Alignment = LayoutAlignment.Middle,
                Spacing = new Vector2(6)
            };

            b = new UIToolButton(tools, screen.Theme.Fonts.IconFile);
            b = new UIToolButton(tools, (int)FontAwesomeIcon.Scissors);
            b = new UIToolButton(tools, (int)FontAwesomeIcon.Copy);
            b = new UIToolButton(tools, (int)FontAwesomeIcon.Paste);

            // Popup button
            new UILabel(window, "Popup button");

            UIPopupButton mainPopupButton = new UIPopupButton(window, "Popup")
            {
                // icon - todo : should we have default icon in theme?
                Icon = (int)FontAwesomeIcon.ChevronDown,
                IconAlign = IconAlign.Right,
                TextHorizontalAlignment = TextHorizontalAlign.Left,
                TextVerticalAlignment = TextVerticalAlign.Middle,
                Padding = new Thickness(10, 4)
            };

            Thickness popupMargin = new Thickness(10);

            UIPopup mainPopup = mainPopupButton.Popup;
            mainPopup.ChildrenLayout = new GroupLayout();
            mainPopup.RelativePosition = PopupPosition.Bottom;
            mainPopup.AnchorSize = 10;
            mainPopup.Margin = popupMargin;

            new UILabel(mainPopup, "Arbitrary widgets can be placed here");
            new UICheckBox(mainPopup, "A check box");

            // popup right
            var popupButtonRight = new UIPopupButton(mainPopup, "Recursive popup")
            {
                Icon = (int)FontAwesomeIcon.ChevronRight,
                IconAlign = IconAlign.Right,
            };

            UIPopup popupRight = popupButtonRight.Popup;
            popupRight.ChildrenLayout = new GroupLayout();
            popupRight.Margin = popupMargin;

            new UICheckBox(popupRight, "Another check box");

            // popup left
            var popupButtonLeft = new UIPopupButton(mainPopup, "Recursive popup")
            {
                Icon = (int)FontAwesomeIcon.ChevronLeft,
                IconAlign = IconAlign.Left,
            };

            popupButtonLeft.Popup.RelativePosition = PopupPosition.LeftMiddle;

            UIPopup popupLeft = popupButtonLeft.Popup;
            popupLeft.ChildrenLayout = new GroupLayout();
            popupLeft.Margin = popupMargin;

            new UICheckBox(popupLeft, "Another check box");
        }

        #endregion

        #region Dialogs

        void TestDialogs(UIScreen screen)
        {
            UIWindow window = new UIWindow(screen, "Dialogs");
            window.Position = new Vector2(220, 15);
            window.ChildrenLayout = new GroupLayout();
            //window.FixedSize = new Vector2(350);

            var lblEvent = new UILabel(window, "Event Value");
            lblEvent.TextColor = Color.Red;

            // Message boxes
            new UILabel(window, "Message dialogs");

            UIWidget boxes = new UIWidget(window);
            boxes.ChildrenLayout = new StackLayout(Orientation.Horizontal)
            {
                Alignment = LayoutAlignment.Middle,
                Spacing = new Vector2(6)
            };

            UIButton b1 = new UIButton(boxes, "Info");
            b1.Name = "Info";
            b1.Clicked += () =>
            {
                UIMessageBox? dlg = screen.GetDialog<UIMessageBox>();
                if (dlg != null)
                {
                    dlg.DialogType = MessageDialogType.Information;
                    dlg.Text = "This is an information message.";
                    dlg.ScrollText = false;

                    dlg.SetCallback(b1, MessageBoxRes);
                }
            };

            // this is to test very long message text
            UIButton b2 = new UIButton(boxes, "Info2");
            b2.Name = "Info2";
            b2.Clicked += () =>
            {
                UIMessageBox? dlg = screen.GetDialog<UIMessageBox>();
                if (dlg != null)
                {
                    dlg.DialogType = MessageDialogType.Information;
                    dlg.Text = _longText.Substring(0, 200);
                    dlg.ScrollText = true;
                    dlg.SetCallback(b2, MessageBoxRes);
                }
            };

            // this is message box with user specified wrapping (new line char '\n')
            UIButton b3 = new UIButton(boxes, "Warn");
            b3.Name = "Warn";
            b3.Clicked += () =>
            {
                UIMessageBox? dlg = screen.GetDialog<UIMessageBox>();
                if (dlg != null)
                {
                    dlg.DialogType = MessageDialogType.Warning;
                    dlg.Text = "This is a warning message\nwrapped by new line char.";
                    dlg.ScrollText = false;
                    dlg.SetCallback(b3, MessageBoxRes);
                }
            };

            UIButton b4 = new UIButton(boxes, "Ask");
            b4.Name = "Ask";
            b4.Clicked += () =>
            {
                UIMessageBox? dlg = screen.GetDialog<UIMessageBox>();
                if (dlg != null)
                {
                    dlg.DialogType = MessageDialogType.Question;
                    dlg.Text = "This is a question message?";
                    dlg.ScrollText = false;
                    // alt button is by default invisible
                    if (dlg.AltButton != null)
                    {
                        dlg.AltButton.Visible = true;
                    }
                    dlg.SetCallback(b4, MessageBoxRes);
                }
            };

            var multi = new UIButton(boxes, "Multi");
            multi.Name = "Multi";
            multi.Clicked += () =>
            {
                UIMultilineMessageBox? dlg = screen.GetDialog<UIMultilineMessageBox>();
                if (dlg != null)
                {
                    dlg.Text = _longText;
                    dlg.Title = "License";

                    // todo: SetButtonTexts(string[] texts) or localization
                    if (dlg.OKButton != null)
                    {
                        dlg.OKButton.Caption = "Agree";
                    }

                    if (dlg.AltButton != null)
                    {
                        dlg.AltButton.Caption = "Disagree";
                        dlg.AltButton.Visible = true;
                    }

                    dlg.SetCallback(multi, MessageBoxRes);
                }
            };

            void MessageBoxRes(UIWidget caller, int res)
            {
                lblEvent.Caption = caller.Name + ": Button-" + res + " clicked";
            }

            // Color dialog
            new UILabel(window, "ColorDialog");

            var colorButton = new UIButton(window, "Choose color");

            // set initial color
            var initialColor = Color.Blue;
            colorButton.BackgroundFocused = colorButton.BackgroundUnfocused = new SolidBrush(initialColor);

            //colorButton.TextColor = initialColor.ContrastingColor();

            colorButton.Clicked += () =>
            {
                UIColorDialog? dlg = screen.GetDialog<UIColorDialog>();
                if (dlg != null)
                {
                    // todo: SetButtonTexts(string[] texts) or localization
                    if (dlg.PickButton != null)
                    {
                        dlg.PickButton.Caption = "Pick";
                    }

                    if (dlg.CancelButton != null)
                    {
                        dlg.CancelButton.Caption = "Cancel";
                    }

                    // set start color
                    dlg.SetColor(initialColor);

                    dlg.SetCallback(colorButton, ColorChanged);
                }                

                void ColorChanged(UIWidget widget, Color color)
                {
                    //colorButton.BackgroundColor = color;
                    colorButton.BackgroundFocused = colorButton.BackgroundUnfocused = new SolidBrush(color);

                    colorButton.TextColor = color.ContrastingColor();
                    initialColor = color;
                }
            };

            // File dialogs
            var startPath = "C:\\";

            new UILabel(window, "File dialogs");

            UIWidget tools = new UIWidget(window);
            tools.ChildrenLayout = new StackLayout(Orientation.Horizontal)
            {
                Alignment = LayoutAlignment.Middle,
                Spacing = new Vector2(6)
            };

            var newFile = new UIButton(tools, "New...");
            newFile.Name = "NewFile";
            newFile.Clicked += () =>
            {
                UIFileDialog? dlg = screen.GetDialog<UIFileDialog>();
                if (dlg != null)
                {
                    dlg.Title = "New file";
                    dlg.StartPath = startPath;
                    dlg.DialogType = FileDialogType.New;
                    // todo: SetButtonTexts(string[] texts) or localization
                    if (dlg.OKButton != null)
                    {
                        dlg.OKButton.Caption = "Create";
                    }
                    if (dlg.CancelButton != null)
                    {
                        dlg.CancelButton.Caption = "Cancel";
                    }

                    dlg.SetCallback(newFile, FileChanged);
                }
            };

            var openFile = new UIButton(tools, "Open...");
            openFile.Name = "OpenFile";
            openFile.Clicked += () =>
            {
                UIFileDialog? dlg = screen.GetDialog<UIFileDialog>();
                if (dlg != null)
                {
                    dlg.Title = "Open file";
                    dlg.StartPath = startPath;
                    dlg.DialogType = FileDialogType.Open;
                    // todo: SetButtonTexts(string[] texts) or localization
                    if (dlg.OKButton != null)
                    {
                        dlg.OKButton.Caption = "Open";
                    }
                    if (dlg.CancelButton != null)
                    {
                        dlg.CancelButton.Caption = "Cancel";
                    }

                    dlg.SetCallback(openFile, FileChanged);
                }
            };

            var saveAs = new UIButton(tools, "SaveAs...");
            saveAs.Name = "SaveAsFile";
            saveAs.Clicked += () =>
            {
                UIFileDialog? dlg = screen.GetDialog<UIFileDialog>();
                if (dlg != null)
                {
                    dlg.Title = "Save file as";
                    dlg.StartPath = startPath;
                    dlg.DialogType = FileDialogType.SaveAs;
                    // todo: SetButtonTexts(string[] texts) or localization
                    if (dlg.OKButton != null)
                    {
                        dlg.OKButton.Caption = "Save";
                    }
                    if (dlg.CancelButton != null)
                    {
                        dlg.CancelButton.Caption = "Cancel";
                    }

                    dlg.SetCallback(saveAs, FileChanged);
                } 
            };

            void FileChanged(UIWidget caller, FileFolderInfo fileFolderInfo)
            {
                lblEvent.Caption = caller.Name + ": " + fileFolderInfo.Path;
            }

            // Folder dialog
            new UILabel(window, "Folder dialog");

            var chooseFolder = new UIButton(window, "Choose folder");
            chooseFolder.Clicked += () =>
            {
                UIFolderDialog? dlg = screen.GetDialog<UIFolderDialog>();
                if (dlg != null)
                {
                    dlg.StartPath = startPath;
                    // todo: SetButtonTexts(string[] texts) or localization
                    if (dlg.OKButton != null)
                    {
                        dlg.OKButton.Caption = "OK";
                    }
                    if (dlg.CancelButton != null)
                    {
                        dlg.CancelButton.Caption = "Cancel";
                    }
                    
                    dlg.SetCallback(chooseFolder, ChangeFoldet);
                }

                void ChangeFoldet(UIWidget widget, string path)
                {
                    lblEvent.Caption = "Folder: " + path;
                }
            };
        }

        #endregion

        #region ComboBox, DropDown, ListBox & ListView

        void TestDropDownsLists(UIScreen screen)
        {
            UIWindow window = new UIWindow(screen, "DropDowns & Lists");
            window.Position = new Vector2(600, 15);
            window.ChildrenLayout = new GroupLayout();

            var lblEvent = new UILabel(window, "Event Value");
            lblEvent.TextColor = Color.Red;

            new UILabel(window, "Combo box");

            var combo = new UIComboBox<int>(window);

            for (int i = 0; i < 20; i++)
            {
                combo.AddItem("Item " + i, i);
            }

            combo.SelectedChanged += (val) =>
            {
                lblEvent.Caption = "ComboBox: " + val;
            };

            // DropDown
            new UILabel(window, "DropDownView");

            var dropDownView = new UIDropDownView<string>(window);
            dropDownView.MaxPopupHeight = 200;

            // Columns
            dropDownView.Columns =
            [
                new ColumnDefinition(20),
                new ColumnDefinition(80),
                new ColumnDefinition(100)
            ];

            for (int i = 0; i < 50; i++)
            {
                UIWidget[] cells =
                [
                    new UIIcon(screen.Theme.Fonts.IconFile),
                    new UIText("Item-" + i),
                    new UIText("DESC-" + i),
                ];
                dropDownView.Add(new RowItem<string>(cells, "ROW-" + i));
            }

            dropDownView.SelectedChanged += (data) =>
            {
                lblEvent.Caption = "DropDown: " + data;
            };

            // ListBox
            new UILabel(window, "ListBox");

            var listboxT = new UIListBox<int>(window);
            listboxT.Size = new Vector2(listboxT.Size.X, 80);

            for (int i = 0; i < 10; i++)
                listboxT.AddItem("Item " + i, i);

            listboxT.SelectedChanged += (data) =>
            {
                lblEvent.Caption = "ListBox: " + data;
            };
            listboxT.SelectedIndex = 5;

            // ListView
            new UILabel(window, "ListView");

            var listView = new UIListView<string>(window);
            listView.Size = new Vector2(220, 80);

            listView.TextColor = Color.Red;

            // Columns
            listView.ViewPanel.Columns =
            [
                new ColumnDefinition(20),
                new ColumnDefinition(80),
                new ColumnDefinition(200)
            ];

            for (int i = 0; i < 10; i++)
            {
                UIWidget[] cells =
                [
                    new UIIcon(screen.Theme.Fonts.IconFile),
                    new UIText("Item-" + i),
                    new UIText("DESC-" + i),
                ];
                listView.Add(new RowItem<string>(cells, "ROW-" + i));
            }

            listView.SelectedChanged += (data) =>
            {
                lblEvent.Caption = "ListView: " + data;
            };
        }

        #endregion

        #region CheckboxProgressSlider

        void TestCheckboxProgressSlider(UIScreen screen)
        {
            var window = new UIWindow(screen, "CheckboxProgressSlider");
            window.Position = new Vector2(225, 390);
            window.ChildrenLayout = new GroupLayout();

            var lblEvent = new UILabel(window, "Event Value");
            lblEvent.TextColor = Color.Red;

            new UILabel(window, "Checkbox");
            UICheckBox cb = new UICheckBox(window, "Normal");
            cb.CheckedChanged += (state) =>
            {
                lblEvent.Caption = "CheckBox-checked: " + state;
            };
            cb.Checked = true;

            cb = new UICheckBox(window, "Disabled");
            cb.Disabled = true;
            cb.Checked = true;

            new UILabel(window, "Progress bar");
            _progress = new UIProgressbar(window);
            _progress.Size = new Vector2(70, 12);

            new UILabel(window, "Slider and text box");
            UIWidget panel = new UIWidget(window);
            panel.ChildrenLayout = new StackLayout(Orientation.Horizontal)
            {
                Alignment = LayoutAlignment.Middle,
                Spacing = new Vector2(10)
            };

            UISlider slider = new UISlider(panel);
            slider.Value = 0.5f;
            slider.Size = new Vector2(170, 26);

            UITextField textBox = new UITextField(panel);
            textBox.FixedSize = new Vector2(60, 25);
            textBox.Text = "50";
            textBox.Units = "%";
            textBox.FixedSize = new Vector2(60, 25);
            textBox.FontSize = 16;
            textBox.TextHorizontalAlignment = TextHorizontalAlign.Right;

            slider.ValueChanged += (value) =>
            {
                textBox.Text = ((int)(value * 100)).ToString();
            };

            slider.FinalValue += (value) =>
            {
                lblEvent.Caption = "Slider-final: " + value.ToString();
            };
        }

        #endregion

        #region TreeView

        void TestTreeView(UIScreen screen)
        {
            UIWindow window = new UIWindow(screen, "TreeView");
            window.Position = new Vector2(890, 15);

            window.ChildrenLayout = new GroupLayout();

            var lblEvent = new UILabel(window, "Event Value");
            lblEvent.TextColor = Color.Red;

            new UILabel(window, "Tree view");

            var tree = new UITreeView<string>(window, "Root");
            tree.Size = new Vector2(230, 280);

            // Columns
            tree.ViewPanel.Columns =
            [
                new ColumnDefinition(20),
                new ColumnDefinition(210),
            ];

            tree.SelectedChanged += (data) =>
            {
                lblEvent.Caption = "Tree selected: " + data;

            };

            // add some test data
            CreateGroups(screen, 2, tree, "", 0, 3);
        }

        // TreeView test data
        void CreateGroups(UIScreen screen, int count, UITreeView<string> tree, string parentId, int level, int maximumLevel)
        {
            // stop endless creation
            if (level < maximumLevel)
            {
                for (int i = 0; i < count; i++)
                {
                    string group;
                    string groupCaption = "Group" + i + "-Level" + level;
                    if (string.IsNullOrEmpty(parentId))
                    {
                        group = "Group" + i;
                    }
                    else
                    {
                        group = parentId + "/Group" + i;
                    }

                    UIWidget[] cells =
                    [
                        new UIIcon { Icon = screen.Theme.Fonts.IconFolder },
                        new UIText { Text = groupCaption },
                    ];

                    tree.AddGroup(new TreeItem<string>(cells, group, group, parentId));

                    // create nested groups
                    CreateGroups(screen, 2, tree, group, level + 1, maximumLevel);
                }
            }

            // create some items
            int fileCount = Math.Min(3, level);

            for (int i = 0; i < fileCount; i++)
            {
                string file;
                string fileCaption = "Item" + i;

                if (string.IsNullOrEmpty(parentId))
                {
                    file = "Item" + i;
                }
                else
                {
                    file = parentId + "/Item" + i;
                }

                UIWidget[] cells =
                [
                        new UIIcon { Icon = screen.Theme.Fonts.IconFile },
                        new UIText { Text = fileCaption },
                ];

                tree.AddItem(new TreeItem<string>(cells, file, file, parentId));
            }
        }

        #endregion

        #region TableView

        void TestTableView(UIScreen screen)
        {
            UIWindow window = new UIWindow(screen, "TableView");
            window.Position = new Vector2(530, 450);
            window.ChildrenLayout = new GroupLayout();

            var lblEvent = new UILabel(window, "Event Value");
            lblEvent.TextColor = Color.Red;

            var tableView = new UITableView<string>(window);
            tableView.Size = new Vector2(290, 150);

            // Columns
            tableView.ViewPanel.Columns =
            [
                new ColumnDefinition(20),
                new ColumnDefinition(80),
                new ColumnDefinition(210, true), // we stretch/diminish this size (only first strechable is used)
                new ColumnDefinition(50)
            ];

            for (int i = 0; i < 15; i++)
            {
                // textures (there is < 20 textures - so we must reuse some)
                var calculus = Math.DivRem(i, DemoAssets.Textures.Length);
                int textureIndex = calculus.Remainder;

                UIWidget[] cells =
                [
                    new UIIcon(screen.Theme.Fonts.IconFolder),
                    new UIText("String-" + i),
                    new UIText("Row" + i + "-Cell2"),
                    new UIImage(DemoAssets.Textures[textureIndex])
                ];

                tableView.Add(new RowItem<string>(cells, "ROW-Item-" + i) { RowHeight = 50 });
            }

            tableView.CellSelectedChanged += (widget, columnIndex) =>
            {
                var cell = widget.Children[columnIndex];

                string res = string.Empty;

                switch (columnIndex)
                {
                    case 0: // IconCell
                        res = "ICON-" + ((UIIcon)cell).Icon.ToString();
                        break;
                    case 1: // TextCell
                        res = "TEXT-" + ((UIText)cell).Text;
                        break;
                    case 2: // TextCell
                        res = "TEXT-" + ((UIText)cell).Text;
                        break;
                    case 3: // ImageCell
                        res = "TEXTURE-" + ((UIImage)cell).Texture;
                        break;
                }
                lblEvent.Caption = "Grid-Cell: " + res;
            };
        }

        #endregion

        #region TestGrid

        void TestGrid(UIScreen screen)
        {
            UIWindow window = new UIWindow(screen, "Grid");
            window.Position = new Vector2(890, 410);
            window.ChildrenLayout = new GroupLayout();

            var grid = new UIGrid(window)
            {
                Orientation = Orientation.Horizontal,
                ColumnOrRowCount = 2,
                Spacing = new Vector2(5, 5),
                Margin = new Thickness(10)
            };

            // we have 2 columns
            grid.SetColumnAlignments([LayoutAlignment.Minimum, LayoutAlignment.Fill]);

            // TextBox
            {
                new UILabel(grid, "TEXT BOX :");
                UITextField textBox = new UITextField(grid, "äöåÄÖÅ");
                textBox.Editable = true;
                textBox.TextHorizontalAlignment = TextHorizontalAlign.Left;
            }

            // float
            {
                new UILabel(grid, "Floating point :");
                var floatBox = new UINumericTextBox<float>(grid, 50);
                floatBox.Editable = true;
                floatBox.Units = "GiB";
                floatBox.TextHorizontalAlignment = TextHorizontalAlign.Right;
            }

            // Positive integer
            {
                new UILabel(grid, "Positive integer :");
                var intBox = new UINumericTextBox<int>(grid, 50);
                intBox.Editable = true;
                intBox.Units = "Mhz";
                intBox.DefaultText = "0";
                intBox.Min = 0;
                intBox.Max = int.MaxValue;
                intBox.TextHorizontalAlignment = TextHorizontalAlign.Right;
            }

            // Checkbox
            {
                new UILabel(grid, "Checkbox :");
                UICheckBox cb = new UICheckBox(grid, "Check me");
                cb.Checked = true;
            }

            new UILabel(grid, "Color picker :");

            var cp = new UIColorPicker(grid, new Color(255, 120, 0, 255));
            cp.FinalColor += (color) => { };

            // Simple combobox
            new UILabel(grid, "ComboBox :");

            var comboBox = new UIComboBox<int>(grid);

            for (int i = 0; i < 50; i++)
            {
                comboBox.AddItem("Item-" + i, i);
            }
            comboBox.SelectedIndex = 20;
            comboBox.SelectedChanged += (data) => { };

            // Enum dropdown
            new UILabel(grid, "EnumDropDown :") { };

            var enumDropDown = new UIEnumDropDown<PointerType>(grid);
            enumDropDown.SetSelected(PointerType.SizeAll);
            enumDropDown.SelectedChanged += (cursor) => { };
        }

        #endregion

        public override void Update(float deltaSeconds)
        {
            // progress bar
            if (_progress != null)
            {
                _progress.Value += deltaSeconds / 5;

                if (_progress.Value >= 1)
                    _progress.Value = 0;
            }

            base.Update(deltaSeconds);
        }
    }
}
