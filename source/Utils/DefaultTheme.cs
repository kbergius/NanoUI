using NanoUI.Common;
using NanoUI.Styles;
using System.Numerics;

namespace NanoUI.Utils
{
    // this cn be used to create default theme properties

    // todo: when we don't set any widget properties - then the properties are not saved in JSON theme &
    // we can have default values there!!!
    // note: it is recommended that you only have few theme widgets for basic properties
    // (like UiWidget, UILabel, UiButton etc) and set all extended widgets use as ThemeType their theme type.
    // you can of course extend any property in extended widget when it is different than in base teheme type.
    // so any special themetype in these extended widgets can be accessed directly calling extensted theme widget
    public static class DefaultTheme
    {
        public static void Populate(UITheme theme)
        {
            #region Global styles

            theme.Borders = new BorderStyle
            {
                Dark = new Color(29, 29, 29, 255),
                Light = new Color(90, 90, 90, 255),
                Medium = new Color(65, 65, 65, 255),
                PointerFocus = new Color(122, 122, 122, 255)
            };

            theme.Common = new CommonStyle
            {
                AccentColor = Color.DarkRed,
                BackgroundSunken = new BoxGradient(new Color(0, 0, 0, 224), new Color(0, 0, 0, 128), 3),
                BackgroundDisabled = new SolidBrush(new Color(255, 255, 255, 40)),
                BackgroundInvalid = new SolidBrush(new Color(255, 0, 0, 200)),
                BackgroundHoverTint = new Color(255, 255, 255, 40),
                FrontendDisabledColor = new Color(255, 255, 255, 60),
            };

            theme.Docks = new DockingStyle
            {
                HitAreaCornerRadius = 3f,
                HitAreaBackgroundColor = Color.White,
                // this visualizes hit area (left, top, right, bottom, center)
                HitAreaFillBrush = new BoxGradient(new Color(0, 0, 255, 128), new Color(0, 0, 255, 164), 3, 6),
                OverlayColor = new Color(0, 0, 255, 128),
                // titlebas
                TitleBackgroundFocused = new SolidBrush(Color.Blue),
                TitleBackgroundUnfocused = new SolidBrush(Color.Gray),
                TitleFontType = "Bold",
                TitleFontSize = 16,
                TitleButtonSize = new Vector2(20)
            };

            theme.Files = new FilesStyle
            {
                HardDriveColor = Color.White,
                FolderColor = Color.Gold,
                FileColor = Color.White,
                DetailsColumns = [
                    new ColumnDefinition(20),
                    new ColumnDefinition(180, true), // strech
                    new ColumnDefinition(150),
                ],
                DropdownColumns = [
                    new ColumnDefinition(20),
                    new ColumnDefinition(200, true), // strech
                ],
                ListColumns = [
                    new ColumnDefinition(20),
                    new ColumnDefinition(200, true), // strech
                ],
                TreeColumns = [
                    new ColumnDefinition(20),
                    new ColumnDefinition(100, true),  // strech
                ]
            };

            theme.Pointer = new PointerStyle
            {
                PointerType = (int)PointerType.Arrow,
                MarkerColor = Color.Red,
                MarkerRadius = 14f
            };

            theme.Scrollbars = new ScrollbarStyle
            {
                ScrollbarDimension = 11,
                BackgroundEnabled = new BoxGradient(new Color(0, 0, 0, 32), new Color(0, 0, 0, 92), 3),
                SliderBrush = new BoxGradient(new Color(220, 220, 220, 180), new Color(120, 120, 120, 180), 3, 4),
                SliderHoverTint = new Color(255, 255, 255, 100),
            };

            // Windows
            CornerRadius windowHeaderRounding = new CornerRadius(5, 5, 0, 0);

            theme.Windows = new WindowStyle
            {
                BorderFocusedColor = Color.DarkRed,
                BorderUnfocusedColor = new Color(90, 90, 90, 255),
                BorderResizingColor = Color.Red,
                DragAreaWidth = 10,
                ResizingTriangleSize = 23,
            };

            #endregion

            #region Widget styles

            // Widget
            theme.Widget.TextHorizontalAlignment = TextHorizontalAlign.Left;
            theme.Widget.TextVerticalAlignment = TextVerticalAlign.Middle;
            theme.Widget.TextColor = Color.White;
            theme.Widget.TextDisabledColor = new Color(255, 255, 255, 160);
            theme.Widget.FontSize = 16;

            // Button
            // note: these affects buttons & all widgets that are derived from button (popup button etc)
            theme.Button.BackgroundFocused = theme.Button.BackgroundUnfocused =
                new LinearGradient(new Color(80, 80, 80, 255), new Color(48, 48, 48, 255), new CornerRadius(3));
            theme.Button.IconAlign = IconAlign.LeftCentered;
            theme.Button.TextHorizontalAlignment = TextHorizontalAlign.Center;
            theme.Button.FontType = "Bold";
            theme.Button.Padding = new Thickness(8, 0);
            theme.Button.TextColor = theme.Widget.TextColor;
            theme.Button.TextDisabledColor = theme.Widget.TextDisabledColor;
            theme.Button.FontSize = theme.Widget.FontSize;

            // CheckBox (basic propertioes are read from UIWidget)
            theme.CheckBox.Padding = new Thickness(2);
            theme.CheckBox.BoxBackgroundBrush = theme.Common.BackgroundSunken;

            // CollapsablePanel

            // UICollapsablePanelHeader (basic properties are read from UIButton)
            // todo this affects to text not icon (should be icon & text is calculated from padding + icon width)
            theme.UICollapsablePanelHeader.Padding = new Thickness(28,4);
            theme.UICollapsablePanelHeader.BackgroundFocused = theme.UICollapsablePanelHeader.BackgroundUnfocused =
                new LinearGradient(Color.DarkRed, new Color(89, 0, 0, 255), new CornerRadius(3));

            // ColorWheel

            // Dialog
            theme.Dialog.BackgroundFocused = theme.Dialog.BackgroundUnfocused =
                new SolidBrush(new Color(55, 55, 55, 230));
            theme.Dialog.Margin = new Thickness(25, 20);
            theme.Dialog.ButtonPanelMargin = new Thickness(0, 10);

            // FileFolderDetails
            theme.FileFolderDetails.AutoFolderSelectedChange = true;
            theme.FileFolderDetails.ShowDirectoryUp = true;

            // FileFolderDropdown
            theme.FileFolderDropdown.AutoFolderSelectedChange = true;
            theme.FileFolderDropdown.ShowDirectoryUp = true;
            
            // FileFolderFlow
            theme.FileFolderFlow.AutoFolderSelectedChange = true;
            theme.FileFolderFlow.ShowDirectoryUp = true;
            // todo : handle different sizes (Large, Medium, ...)
            theme.FileFolderFlow.PartSize = new Vector2(100, 90);

            // FileFolderList
            theme.FileFolderList.AutoFolderSelectedChange = true;
            theme.FileFolderList.ShowDirectoryUp = true;
            
            // FileFolderTree

            // ImageViewer
            theme.ImageViewer.ZoomSensitivity = 1.1f;

            // Label
            theme.Label.FontType = "Bold";
            theme.Label.Padding = new Thickness(8, 4);
            theme.Label.FontSize = theme.Widget.FontSize;
            theme.Label.LineHeight = 1.2f;
            theme.Label.CharSpacing = 0;
            theme.Label.TextColor = theme.Widget.TextColor;
            theme.Label.TextDisabledColor = theme.Widget.TextDisabledColor;
            theme.Label.TextHorizontalAlignment = TextHorizontalAlign.Left;

            // Menubar
            theme.Menubar.BackgroundFocused = theme.Menubar.BackgroundUnfocused =
                new SolidBrush(new Color(50, 50, 50, 255));
            theme.Menubar.Margin = new Thickness(10, 0);

            // Menu
            theme.Menu.BackgroundFocused = theme.MenuItem.BackgroundUnfocused =
                new SolidBrush(new Color(50, 50, 50, 255));
            theme.Menu.PopupWidth = 220;
            theme.Menu.FontType = "Normal";
            theme.Menu.TextColor = theme.Widget.TextColor;
            theme.Menu.TextDisabledColor = theme.Widget.TextDisabledColor;
            theme.Menu.FontSize = theme.Widget.FontSize;

            // MenuItem (also UIMenuSubmenu) - get most from UIMenu
            theme.MenuItem.BackgroundFocused = theme.MenuItem.BackgroundUnfocused =
                theme.Menu.BackgroundFocused;
            theme.MenuItem.Padding = new Thickness(35, 0);
            theme.MenuItem.FontType = theme.Menu.FontType;
            theme.MenuItem.TextColor = theme.Menu.TextColor;
            theme.MenuItem.TextDisabledColor = theme.Menu.TextDisabledColor;
            theme.MenuItem.FontSize = theme.Menu.FontSize;

            // ContextMenu (inherit from UIMenu)
            theme.ContextMenu.BackgroundFocused = theme.ContextMenu.BackgroundUnfocused =
                theme.Menu.BackgroundFocused;
            theme.ContextMenu.FontType = theme.Menu.FontType;
            theme.ContextMenu.TextColor = theme.Menu.TextColor;
            theme.ContextMenu.TextDisabledColor = theme.Menu.TextDisabledColor;
            theme.ContextMenu.FontSize = theme.Menu.FontSize;

            // MessageBox

            // Popup
            theme.Popup.AnchorBackgroundColor = new Color(50, 50, 50, 255);
            theme.Popup.BackgroundFocused = theme.Popup.BackgroundUnfocused =
                new SolidBrush(new Color(50, 50, 50, 255));

            // PopupButton
            theme.PopupButton.MaxPopupHeight = 150;

            // Progressbar
            theme.Progressbar.BackgroundFocused = theme.Progressbar.BackgroundUnfocused =
                theme.Common.BackgroundSunken;
            theme.Progressbar.ValueColor = theme.Common.AccentColor;

            // Slider - same settings as UIProgressbar
            theme.Slider.ValueColor = theme.Common.AccentColor;
            theme.Slider.BackgroundFocused = theme.Slider.BackgroundUnfocused =
                theme.Common.BackgroundSunken;

            // Spinner (uses UILabel as base type)
            theme.Spinner.Speed = 1.0f;
            theme.Spinner.Radius = 0.75f;
            theme.Spinner.InnerRadius = 0.6f;
            // linear gradient?
            theme.Spinner.StartColor = new Color(0, 255, 0, 80);
            theme.Spinner.EndColor = new Color(0, 255, 0, 255);

            // Splitter
            theme.Splitter.Dimension = 10;
            theme.Splitter.DotsRadius = 2.2f;
            theme.Splitter.DotsSpacing = 8;
            theme.Splitter.DotsColor = theme.Widget.TextColor;
            theme.Splitter.BackgroundFocused = theme.Splitter.BackgroundUnfocused =
                new BoxGradient(new Color(64, 64, 64, 255), Color.Gray, 0);

            // TabWidget
            theme.TabWidget.TabPaddingHorizontal = 10;
            theme.TabWidget.TabPaddingVertical = 5;
            theme.TabWidget.ContentPaddingHorizontal = 5;
            theme.TabWidget.ContentPaddingVertical = 5;
            theme.TabWidget.TabCornerRadius = 3;
            theme.TabWidget.TabDragColor = new Color(255, 255, 255, 60);
            theme.TabWidget.TabInactiveTop = new Color(41, 41, 41, 255);
            theme.TabWidget.TabInactiveBottom = new Color(29, 29, 29, 255);

            // TextField
            theme.TextField.CornerRadius = new CornerRadius(1);
            theme.TextField.Padding = new Thickness(8, 4);
            theme.TextField.UnitsColor = theme.Widget.TextColor;
            theme.TextField.CaretColor = Color.DarkGray;
            theme.TextField.SelectionColor = new Color(255, 255, 255, 80);
            theme.TextField.PlaceHolderColor = new Color(220, 220, 220, 180);
            theme.TextField.BackgroundFocused = theme.TextField.BackgroundUnfocused =
                theme.Common.BackgroundSunken;
            theme.TextField.TextColor = theme.Widget.TextColor;
            theme.TextField.TextDisabledColor = theme.Widget.TextDisabledColor;
            theme.TextField.FontSize = theme.Widget.FontSize;
            theme.TextField.LineHeight = 1.6f;

            // Toolbar
            theme.Toolbar.BackgroundFocused = theme.Toolbar.BackgroundUnfocused =
                theme.Menubar.BackgroundFocused;
            theme.Toolbar.Margin = theme.Menubar.Margin;

            // ToolButton
            // todo: same as button
            // same as button - or should it be Boxgradient?
            theme.ToolButton.BackgroundFocused = theme.ToolButton.BackgroundUnfocused =
                new LinearGradient(new Color(80, 80, 80, 255), new Color(48, 48, 48, 255), new CornerRadius(3));
            theme.ToolButton.Dimension = 32;
            theme.ToolButton.Flags = ButtonFlags.ToggleButton;
            theme.ToolButton.TextColor = theme.Button.TextColor;
            theme.ToolButton.TextDisabledColor = theme.Button.TextDisabledColor;

            // Tooltip
            theme.Tooltip.MaxWidth = 150;
            theme.Tooltip.MaxTextRows = 50;
            theme.Tooltip.FontSize = theme.Widget.FontSize;
            // todo: anchor
            theme.Tooltip.BackgroundColor = Color.Yellow;
            theme.Tooltip.TextColor = Color.Black;

            // UpDownButton
            // todo: should dynamically calculate both values (if in NumericUpDown) based on TextBox height
            theme.UpDownButton.Dimension = 28;
            theme.UpDownButton.IconExtraScale = 1.2f;

            // ViewPanel
            theme.ViewPanel.BackgroundFocused = theme.ViewPanel.BackgroundUnfocused =
                new SolidBrush(new Color(50, 50, 50, 255));
            theme.ViewPanel.Padding = new Thickness(10, 4); // todo: should be same as in popupbutton so they align nicely!
            // note: these colors are needed, when item/cell content hides totally background;
            // if you set them Color.Transparent they are not shown
            theme.ViewPanel.HoverBorderColor = Color.White;
            theme.ViewPanel.SelectedBorderColor = Color.White;
            theme.ViewPanel.ItemBorderWidth = 1;
            theme.ViewPanel.ItemSelectedBackgroundBrush =
                new SolidBrush(theme.Common.AccentColor);
            theme.ViewPanel.ItemHoverBackgroundBrush =
                new SolidBrush(new Color(100, 100, 100, 255));
            theme.ViewPanel.RowHeight = 25;

            // Window
            theme.Window.CornerRadius = new CornerRadius(2);
            theme.Window.BackgroundFocused = theme.Window.BackgroundUnfocused =
                new SolidBrush(new Color(55, 55, 55, 230));
            theme.Window.BorderSize = 1;
            theme.Window.ScrollbarDimension = theme.Scrollbars.ScrollbarDimension + 3;
            theme.Window.Draggable = true;
            theme.Window.DragResizable = true;
            theme.Window.Margin = new Thickness(20, 10);

            // Titlebar
            theme.Titlebar.BackgroundFocused = new LinearGradient(Color.Red, Color.DarkRed, windowHeaderRounding);
            theme.Titlebar.BackgroundUnfocused = new LinearGradient(
                    new Color(120, 120, 120, 255), new Color(48, 48, 48, 255), windowHeaderRounding);
            theme.Titlebar.ButtonSize = new Vector2(25);
            theme.Titlebar.ButtonFontIconsId = theme.Fonts.GetFontId(theme.Fonts.DefaultIconsType);
            theme.Titlebar.ButtonIcons = [theme.Fonts.IconClose];
            theme.Titlebar.Margin = new Thickness(10, 5);
            // to label
            theme.Titlebar.FontType = "Bold";
            theme.Titlebar.FontSize = theme.Widget.FontSize + 3;
           
            // DOCKING

            // DockPanel
            // DockWidget

            #endregion
        }
    }
}