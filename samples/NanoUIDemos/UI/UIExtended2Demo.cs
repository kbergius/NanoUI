using NanoUI;
using NanoUI.Common;
using NanoUI.Components;
using NanoUI.Components.Files;
using NanoUI.Layouts;
using NanoUI.Components.Scrolling;
using NanoUI.Components.Views;
using NanoUI.Components.Simple;
using NanoUI.Components.Views.Items;
using Color = NanoUI.Common.Color;
using System.Numerics;
using static NanoUI.Components.UIGridView;

namespace NanoUIDemos.UI
{
    public class UIExtended2Demo : DemoBase
    {
        const string startFolder = "C:\\";

        public UIExtended2Demo(UIScreen screen)
            :base(screen)
        {
            // FileFolderTree
            TestFileFolderTree(screen);

            // File/folder/directory dropdows & details
            TestFileWidgets(screen);

            // File/folder list
            FileFolderList(screen);

            // FileFolderFlow
            TestFileFolderFlow(screen);

            // GridView
            TestGridView(screen);

            // ScrollPanel with both vertical & horizontal scrollbar
            TestScrollPanel(screen);
        }

        #region FileFolderTree

        void TestFileFolderTree(UIScreen screen)
        {
            var window = new UIWindow(screen, "FileFolderTree")
            {
                Position = new Vector2(15, 15),
                ChildrenLayout = new GroupLayout()
            };
            
            var lblEvent = new UILabel(window, "Event Value")
            {
                TextColor = Color.Red
            };
            
            var fileFolderTree = new UIFileFolderTree(window, "Root");
            fileFolderTree.Size = new Vector2(400, 290);

            // note: can "hang" if too many folders & files
            // should always use specific subfolder
            // todo : lazy loading
            string baseFolder = AppDomain.CurrentDomain.BaseDirectory;

            fileFolderTree.SelectedChanged += (data) =>
            {
                var selType = data.FileFolderType == FileFolderType.Folder ? "FOLDER" : "FILE";
                // we get only valid part
                lblEvent.Caption = selType + " : " + data.Path.Substring(baseFolder.Length);
            };

            // Create tree - with maximum levels
            fileFolderTree.CreateTree(baseFolder, 3);

            lblEvent.Caption = "FOLDER: " + Path.GetFileName(Path.GetDirectoryName(baseFolder));
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
                    string groupCaption = "Folder" + i + "-Level" + level;

                    if (string.IsNullOrEmpty(parentId))
                    {
                        group = "Folder" + i;
                    }
                    else
                    {
                        group = parentId + "/Folder" + i;
                    }

                    UIWidget[] cells =
                    [
                        new UIIcon { Icon = screen.Theme.Fonts.IconFolder },
                        new UIText { Text = groupCaption },
                    ];

                    tree.AddGroup(new TreeItem<string>(cells, group, group, parentId));

                    // create sub folders/files
                    CreateGroups(screen, 2, tree, group, level + 1, maximumLevel);
                }
            }

            // create some files
            int fileCount = Math.Min(3, level);

            for (int i = 0; i < fileCount; i++)
            {
                string file;
                string fileCaption = "File" + i;
                if (string.IsNullOrEmpty(parentId))
                {
                    file = "File" + i;
                }
                else
                {
                    file = parentId + "/File" + i;
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

        #region FileWidgets

        void TestFileWidgets(UIScreen screen)
        {
            var window = new UIWindow(screen, "File dropdows & details")
            {
                Position = new Vector2(480, 15),
                ChildrenLayout = new GroupLayout()
            };

            var lblEvent = new UILabel(window, "Event Value")
            {
                TextColor = Color.Red,
                Caption = startFolder
            };
            
            new UILabel(window, "Dropdowns");

            // Drives
            var filesystemDropDown = new UIFileFolderDropdown(window);

            filesystemDropDown.CreateDriveDropDown();

            filesystemDropDown.SelectedChanged += (data) =>
            {
                lblEvent.Caption = "DriveDropDown: " + data.Path;
            };

            // Folders
            var filesystemDropDown2 = new UIFileFolderDropdown(window);
            filesystemDropDown2.MaxPopupHeight = 250;

            filesystemDropDown2.CreateFolderDropDown(startFolder);

            filesystemDropDown2.SelectedChanged += (data) =>
            {
                if (data.FileFolderType == FileFolderType.Folder)
                {
                    lblEvent.Caption = "FolderDropDown: " + data.Path;
                }
            };

            // Details
            new UILabel(window, "DetailsView");

            var fileDetailsView = new UIFileFolderDetails(window);
            fileDetailsView.Size = new Vector2(320, 170);

            fileDetailsView.CreateView(startFolder);

            fileDetailsView.SelectedChanged += (data) =>
            {
                lblEvent.Caption = "DetailsView: " + data.Path;
            };
        }
        #endregion

        #region FileFolderList

        void FileFolderList(UIScreen screen)
        {
            var window = new UIWindow(screen, "FileFolderList")
            {
                Position = new Vector2(870, 15),
                ChildrenLayout = new GroupLayout()
            };
            
            var lblEvent = new UILabel(window, "Event Value")
            {
                TextColor = Color.Red,
                Caption = startFolder
            };
            
            var fileFolderList = new UIFileFolderList(window);
            fileFolderList.Size = new Vector2(250, 190);

            fileFolderList.CreateList(startFolder);

            fileFolderList.SelectedChanged += (data) =>
            {
                lblEvent.Caption = data.Path;
            };
        }

        #endregion

        #region FileFolderFlow

        void TestFileFolderFlow(UIScreen screen)
        {
            var window = new UIWindow(screen, "FileFolderFlow")
            {
                Position = new Vector2(15, 400),
                ChildrenLayout = new GroupLayout(),
                Size = new Vector2(400, 200)
            };
            
            var lblEvent = new UILabel(window, "Event Value")
            {
                TextColor = Color.Red,
                Caption = startFolder
            };
            
            var fileFolderFlow = new UIFileFolderFlow(window);
            fileFolderFlow.Size = window.Size;

            fileFolderFlow.CreateView(startFolder);

            fileFolderFlow.CellSelectedChanged += (widget, columnIndex) =>
            {
                lblEvent.Caption = widget.EventData.Path;
            };
        }

        #endregion

        #region TestGridView

        void TestGridView(UIScreen screen)
        {
            var window = new UIWindow(screen, "GridView")
            {
                Position = new Vector2(480, 400),
                ChildrenLayout = new GroupLayout()
            };
            
            var cols = new List<int> { 100, 120, 100 };
            var rows = new List<int> { 30, 30, 30, 50 };
            int gridPadding = 0;

            var lblEvent = new UILabel(window, $"This is {cols.Count}x{rows.Count} grid. Stretch in column 0");
            lblEvent.TextColor = Color.Red;

            var grid = new UIGridView(window, cols, rows, gridPadding);

            grid.SetColumnStretch(0, 10f);
            //grid.SetColumnStretch(1, 0.1f);
            //grid.SetColumnStretch(2, 0.8f);

            // TODO: must be OTHER THAN GroupLayout
            grid.SetRowStretch(1, 1);

            // row0
            var label00 = new UILabel(grid, "Column span 2");
            label00.BackgroundFocused = label00.BackgroundUnfocused = new SolidBrush(Color.Blue);
            grid.SetCell(label00, new Cell(0, 0, 2, 1, LayoutAlignment.Fill, LayoutAlignment.Fill));

            var button02 = new UIButton(grid, "Button0");
            grid.SetCell(button02, new Cell(2, 0, 1, 1, LayoutAlignment.Fill, LayoutAlignment.Fill));

            // Row1
            var check10 = new UICheckBox(grid, "Check1");
            grid.SetCell(check10, new Cell(0, 1, 1, 1, LayoutAlignment.Fill, LayoutAlignment.Middle));

            var label12 = new UILabel(grid, "Column & Row span 2");
            label12.BackgroundFocused = label12.BackgroundUnfocused = new SolidBrush(Color.Blue);
            grid.SetCell(label12, new Cell(1, 1, 2, 2, LayoutAlignment.Fill, LayoutAlignment.Fill));

            // Row2
            var textBox20 = new UITextField(grid, "TextBox");
            textBox20.Editable = true;
            grid.SetCell(textBox20, new Cell(0, 2, 1, 1, LayoutAlignment.Fill, LayoutAlignment.Fill));

            // Row3
            var textBox30 = new UITextField(grid, "Extra font size & row height");
            textBox30.Editable = true;
            textBox30.FontSize = 26;
            grid.SetCell(textBox30, new Cell(0, 3, 3, 1, LayoutAlignment.Fill, LayoutAlignment.Fill));
        }

        #endregion

        #region ScrollPanel

        void TestScrollPanel(UIScreen screen)
        {
            var window = new UIWindow(screen, "ScrollPanel")
            {
                Position = new Vector2(870, 300),
                ChildrenLayout = new GroupLayout()
            };
            
            // attach scroll
            var scroll = new UIScrollPanel(window);
            scroll.Size = new Vector2(250, 320);

            // create panel (container) & wrap with scroll
            var scrollContent = new UIWidget(scroll);
            scrollContent.ChildrenLayout = new StackLayout(Orientation.Vertical, LayoutAlignment.Fill);

            for (int i = 0; i < 30; i++)
            {
                // Add any widgets you want
                // note: no need to set parent - since scroll panel sets it automatically to container
                // note2: if you want to use complicated layout, it is better to create
                // custom widget and handle there layout & drawing etc
                new UILabel(scrollContent, "Label-" + i + " with long caption that overflows, so we need horizontal scroll");
                new UICheckBox(scrollContent, " CheckBox-" + i);
            }
        }

        #endregion
    }
}
