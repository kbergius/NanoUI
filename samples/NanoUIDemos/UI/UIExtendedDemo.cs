using NanoUI;
using NanoUI.Common;
using NanoUI.Components;
using NanoUI.Components.Bars;
using NanoUI.Components.Buttons;
using NanoUI.Components.Colors;
using NanoUI.Layouts;
using NanoUI.Components.Menus;
using NanoUI.Components.Views;
using NanoUI.Components.Simple;
using NanoUI.Components.Views.Items;
using NanoUI.Fonts;
using NanoUI.Nvg;
using Color = NanoUI.Common.Color;
using System.Numerics;

namespace NanoUIDemos.UI
{
    public class UIExtendedDemo : DemoBase
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

        const float _windowTop = 95;
        int _svgShape;
        int _svgBrush;

        public UIExtendedDemo(UIScreen screen, NvgContext ctx)
            :base(screen)
        {
            // Create svgs
            _svgShape = ctx.CreateSvg("./Assets/svg/tiger2.svg");
            _svgBrush = ctx.CreateSvg("./Assets/svg/decoration.svg");

            if (_screen != null)
            {
                var info = new UILabel(_screen, "If window is selected, shortcuts goes to window, otherwise to screen.");
                info.Position = new Vector2(35, 60);
                info.TextColor = Color.White;
                info.FontSize = 22;
            }

            // ImageViewer & Flows
            TestImageViewerFlows(screen);

            // Numeric textBoxes & updowns
            TextNumericTextUpDowns(screen);

            // Screen & Window menus and toolbars & ContextMenu & SVG
            TestMenus(ctx);

            // TabWidget
            TestTabWidget(screen);
        }

        #region ImageViewerFlows

        void TestImageViewerFlows(UIScreen screen)
        {
            UIWindow window = new UIWindow(screen, "Image viewer & flows");
            window.Position = new Vector2(15, _windowTop);
            window.ChildrenLayout = new GroupLayout();

            var lblEvent = new UILabel(window, "Event Value");
            lblEvent.TextColor = Color.Red;

            // viewer
            new UILabel(window, "Image Viewer");

            UIImageViewer imageViewer = new UIImageViewer(window);
            imageViewer.Size = new Vector2(245, 150);

            if (DemoAssets.Textures != null && DemoAssets.Textures.Length > 0)
            {
                imageViewer.Texture = DemoAssets.Textures[0];
            }

            // ImageFlow
            new UILabel(window, "ImageFlow");

            var flowView = new UIFlowView<int>(window);
            flowView.Size = new Vector2(245, 150);
            flowView.PartSize = new Vector2(64);

            if(DemoAssets.Textures != null)
            {
                for (int i = 0; i < DemoAssets.Textures.Length; i++)
                {
                    // we set data cell since it is in tableView
                    flowView.Add(new FlowItem<int>(new UIImage(DemoAssets.Textures[i]), DemoAssets.Textures[i]));
                }
            }

            flowView.CellSelectedChanged += (widget, columnIndex) =>
            {
                int texture = widget.EventData;

                imageViewer.Texture = texture;
                lblEvent.Caption = "ImageFlow: " + texture;
            };

            new UILabel(window, "TextFlow");

            var flowView2 = new UIFlowView<string>(window);
            flowView2.Size = new Vector2(245, 80);
            flowView2.PartSize = new Vector2(50, 20);

            for (int i = 0; i < 30; i++)
            {
                var text = "Item" + i;
                flowView2.Add(new FlowItem<string>(new UIText(text), text));
            }

            flowView2.CellSelectedChanged += (widget, columnIndex) =>
            {
                string? text = widget.EventData;

                if(text != null)
                {
                    lblEvent.Caption = "TextFlow: " + text;
                }
            };
        }

        #endregion

        #region NumericTextUpDowns

        void TextNumericTextUpDowns(UIScreen screen)
        {
            UIWindow window = new UIWindow(screen, "NumericText & UpDowns");
            window.Position = new Vector2(350, _windowTop);
            window.ChildrenLayout = new GridLayout(Orientation.Horizontal,2, LayoutAlignment.Fill);

            var lblEvent = new UILabel(window, "Event Value");
            lblEvent.TextColor = Color.Red;
            lblEvent.FixedSize = new Vector2(200, 35);
            var w = new UIWidget(window);
            w.FixedSize = new Vector2(200, 35);

            new UILabel(window, "Int");
            var numeric1 = new UINumericTextBox<int>(window, 50);
            numeric1.ValueChanged += (val) => lblEvent.Caption = $"INT: {val}";

            new UILabel(window, "Float");
            var numeric1f = new UINumericTextBox<float>(window, 50, NumericFormat.Decimal2);// Globals.NUMBER_FORMAT_FLOAT);
            numeric1f.ValueChanged += (val) => lblEvent.Caption = "FLOAT: " + numeric1f.GetFormatted();
            numeric1f.InvalidFormat += () => { };

            new UILabel(window, "Range int (0 - 100)");
            var numeric2 = new UINumericTextBox<int>(window, 50)
            {
                Min = 0,
                Max = 100
            };
            numeric2.ValueChanged += (val) => lblEvent.Caption = $"RANGE INT: {val}";

            new UILabel(window, "BYTE");
            var numeric3 = new UINumericUpDown<byte>(window, 50) { Size = new Vector2(200,0) };
            numeric3.ValueChanged += (val) => lblEvent.Caption = $"BYTE: {val}";

            new UILabel(window, "FLOAT");
            var numeric3f = new UINumericUpDown<float>(window, 50, NumericFormat.Decimal2) { Size = new Vector2(200, 0) };
            numeric3f.ValueChanged += (val) => lblEvent.Caption = "FLOAT: " + numeric3f.GetFormatted();

            new UILabel(window, "CURRENCY");
            var numeric3c = new UINumericUpDown<float>(window, 50, NumericFormat.Decimal2) { Size = new Vector2(200, 0) };
            numeric3c.Units = "€";
            numeric3c.ValueChanged += (val) => lblEvent.Caption = "CURRENCY: " + numeric3c.GetFormatted() + numeric3c.Units;

            new UILabel(window, "PERCENT (0-100)");
            var numeric3p = new UINumericUpDown<float>(window, 50, NumericFormat.Decimal0) { Size = new Vector2(200, 0) };
            numeric3p.Units = "%";
            numeric3p.Min = 0;
            numeric3p.Max = 100;
            numeric3p.ValueChanged += (val) => lblEvent.Caption = "PERCENT: " + numeric3p.GetFormatted() + numeric3p.Units;

            new UILabel(window, "CUSTOM UNIT");
            var numeric3cu = new UINumericUpDown<float>(window, 50, NumericFormat.Decimal0) { Size = new Vector2(200, 0) };
            numeric3cu.Step = 1;
            numeric3cu.Units = "MB";
            numeric3cu.ValueChanged += (val) => lblEvent.Caption = "CUSTOM UNIT: " + numeric3cu.GetFormatted();
        }

        #endregion

        #region TestMenus & Toolbar & SVG

        // these are just test icons for menus
        int[] _menuIcons = [
            (int)FontAwesomeIcon.File,
            (int)FontAwesomeIcon.Scissors,
            (int)FontAwesomeIcon.Copy,
            (int)FontAwesomeIcon.Paste,
            (int)FontAwesomeIcon.Exclamation,
            (int)FontAwesomeIcon.Question,
            (int)FontAwesomeIcon.TriangleExclamation,
            (int)FontAwesomeIcon.Check,
            ];

        Shortcut _menuShortcut = new Shortcut { Modifiers = KeyModifiers.Control };
        // menu item id "generator"
        int _menuItemId = -1;

        Key GetMenuShortCutKey()
        {
            return (Key)((int)Key.A + _menuItemId);
        }

        void TestMenus(NvgContext ctx)
        {
            if (_screen == null)
                return;

            // screen menu
            var screenMenubar = new UIMenubar(_screen);
            
            // menu buttons
            for(int i = 0; i < 2; i++)
            {
                int val = i;

                UIMenu screenMenuButton = new UIMenu(screenMenubar, $"ScreenMenu {val}");

                screenMenuButton.MenuItemSelected += (menuItemId) =>
                {
                    screenMenuButton.Caption = $"ScreenMenu {val}: {menuItemId}";
                };

                CreateMainMenu(screenMenuButton.Popup, i == 0 ? KeyModifiers.Control : KeyModifiers.Alt);

                // reset counter
                _menuItemId = -1;
            }

            // screen toolbar
            CreateToolbar(_screen, screenMenubar.PreferredSize(ctx).Y);

            // window
            UIWindow window = new UIWindow(_screen, "Menu, Toolbar & SVGs");
            window.Position = new Vector2(630, _windowTop);
            window.ChildrenLayout = new GroupLayout();
            window.FixedSize = new Vector2(550, 550);

            // window menubar
            var menubar = new UIMenubar(window);

            // menu button
            for (int i = 0; i < 2; i++)
            {
                int val = i;

                UIMenu mainMenuButton = new UIMenu(menubar, $"MainMenu {val}");

                mainMenuButton.MenuItemSelected += (menuItemId) =>
                {
                    mainMenuButton.Caption = $"MainMenu {val}: {menuItemId}";
                };

                CreateMainMenu(mainMenuButton.Popup, i == 0? KeyModifiers.Control : KeyModifiers.Alt);

                // reset counter
                _menuItemId = -1;
            }

            // window toolbar
            CreateToolbar(window);

            var resizeInfo = new UILabel(window, "Resize window to see scrollbars in action");

            // Context menu
            // reset counter
            _menuItemId = -1;

            var contextMenuInfo = new UILabel(window, "Right-click in window area to launch context menu");

            var contextMenuLabel = new UILabel(window, "ContextMenuResult:");
            contextMenuLabel.TextColor = Color.Red;

            UIContextMenu contextMenu = new UIContextMenu(window);

            contextMenu.MenuItemSelected += (menuItemId) =>
            {
                contextMenuLabel.Caption = $"ContextMenuResult: {menuItemId}";
            };

            // context menu has no shortcuts
            CreateMainMenu(contextMenu, KeyModifiers.Control, false);

            // SVGs
            new UILabel(window, "SVG background in UILabel");

            var lbl = new UILabel(window, "tiger2.svg");
            lbl.FontSize = 30;
            lbl.FixedSize = new Vector2(250, 80);
            lbl.TextHorizontalAlignment = TextHorizontalAlign.Center;
            lbl.TextVerticalAlignment = TextVerticalAlign.Middle;
            lbl.BackgroundFocused = lbl.BackgroundUnfocused = new SvgBrush(_svgBrush);

            // we get widget size so we can have correct width/height proportion
            // (widget sets scale when drawing svg)
            if (ctx.TryGetSvgSize(_svgShape, out Vector2 svgSize))
            {
                new UILabel(window, "SVG widget");

                UISvgWidget svgPanel = new UISvgWidget(window);
                svgPanel.FixedSize = svgSize / 1.25f;
                svgPanel.SvgId = _svgShape;
            }
        }

        void CreateMainMenu(UIPopup mainPopup, KeyModifiers modifiers, bool shortcuts = true)
        {
            for (int i = 0; i < 8; i++)
            {
                _menuItemId++;

                var item = new UIMenuItem(mainPopup, $"Menu {i}", _menuIcons[i], _menuItemId);

                if (shortcuts)
                {
                    _menuShortcut.Modifiers = modifiers;
                    _menuShortcut.Key = GetMenuShortCutKey();
                    item.Shortcut = _menuShortcut;
                }

                if (i == 3 || i == 6)
                {
                    new UIMenuSeparator(mainPopup);
                }

                if (i == 5)
                {
                    new UIMenuSeparator(mainPopup);

                    UIMenuSubmenu popupButtonRight = new UIMenuSubmenu(mainPopup, "SubPopup 1");

                    CreateSubmenu(popupButtonRight.Popup, modifiers, shortcuts);
                }
            }
        }

        void CreateSubmenu(UIPopup submenu, KeyModifiers modifiers, bool shortcuts = true)
        {
            for (int i = 0; i < 6; i++)
            {
                _menuItemId++;

                var item = new UIMenuItem(submenu, $"Submenu {i}", _menuIcons[i], _menuItemId);

                if (shortcuts)
                {
                    _menuShortcut.Modifiers = modifiers;
                    _menuShortcut.Key = GetMenuShortCutKey();
                    item.Shortcut = _menuShortcut;
                }

                if (i == 3)
                {
                    new UIMenuSeparator(submenu);
                }

                if (i == 4)
                {
                    UIMenuSubmenu popupButtonRight = new UIMenuSubmenu(submenu, "SubSubPopup 1");

                    CreateSubSubmenu(popupButtonRight.Popup, modifiers, shortcuts);
                }
            }
        }

        void CreateSubSubmenu(UIPopup submenu, KeyModifiers modifiers, bool shortcuts = true)
        {
            for (int i = 0; i < 6; i++)
            {
                _menuItemId++;

                var item = new UIMenuItem(submenu, $"SubSubmenu {i}", _menuIcons[i], _menuItemId);

                if (shortcuts)
                {
                    _menuShortcut.Modifiers = modifiers;
                    _menuShortcut.Key = GetMenuShortCutKey();
                    item.Shortcut = _menuShortcut;
                }

                if (i == 3)
                {
                    new UIMenuSeparator(submenu);
                }
            }
        }

        // note: window toolbar positioning is handled in UIWindow
        void CreateToolbar(UIWidget parent, float offsetY = 0)
        {
            if (_screen == null)
                return;

            var toolbar = new UIToolbar(parent);
            toolbar.Position = new Vector2(0, offsetY);

            new UILabel(toolbar, "ToolButtons:");

            // create toolbuttons
            for (int i = 0; i < 4; i++)
            {
                new UIToolButton(toolbar, _menuIcons[i]);
            }
            
            var lbl = new UILabel(toolbar, "DropDown:");
            lbl.TextColor = Color.Yellow;

            // todo: min size for dropdown
            var enumDropDown = new UIEnumDropDown<PointerType>(toolbar);
            enumDropDown.FixedSize = new Vector2(100, _screen.Theme.ToolButton.Size.Y);
            enumDropDown.SetSelected(PointerType.SizeAll);

            var btn = new UIButton(toolbar, "button");
        }

        #endregion

        #region TabWidget

        void TestTabWidget(UIScreen screen)
        {
            UIWindow window = new UIWindow(screen, "TabWidget");
            window.Position = new Vector2(350, 350);
            window.ChildrenLayout = new GroupLayout();

            UITabWidget tabWidget = new UITabWidget(window);
            tabWidget.ContentPaddingHorizontal = 10;
            tabWidget.ContentPaddingVertical = 5;

            // MultilineText
            var tabTextArea = new UITabItem(tabWidget) { Caption = "MultilineText" };
            tabTextArea.ChildrenLayout = new GroupLayout();

            var textArea = new UIScrollableLabel(tabTextArea);
            textArea.Size = new Vector2(320, 200);
            textArea.SetText(_longText);

            // ColorWheel
            var tabColorWheel = new UITabItem(tabWidget) { Caption = "Color Wheel" };
            tabColorWheel.ChildrenLayout = new GroupLayout();

            var wheel = new UIColorWheel(tabColorWheel, Color.White);
            wheel.Size = new Vector2(150);

            // Spinners
            var tabSpinners = new UITabItem(tabWidget) { Caption = "Spinners" };
            tabSpinners.ChildrenLayout = new GroupLayout();

            var spinnerWidget = new UIWidget(tabSpinners)
            {
                ChildrenLayout = new StackLayout(Orientation.Horizontal, LayoutAlignment.Middle),
            };

            new UISpinner(spinnerWidget) { Size = new Vector2(80) };
            new UISpinner(spinnerWidget) { Speed = -0.5f, Size = new Vector2(120), InnerRadius = 0.65f };
            new UISpinner(spinnerWidget) { Speed = 0.2f, Size = new Vector2(80), HalfCircle = true };
        }

        #endregion
    }
}
