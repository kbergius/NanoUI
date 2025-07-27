using NanoUI.Common;
using Color = NanoUI.Common.Color;
using NanoUI.Components;
using NanoUI.Components.Bars;
using NanoUI.Components.Buttons;
using NanoUI.Components.Colors;
using NanoUI.Components.Dialogs;
using NanoUI.Components.Files;
using NanoUI.Components.Menus;
using NanoUI.Components.Views;
using NanoUI.Nvg;
using NanoUI.Serializers;
using NanoUI.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using NanoUI.Styles;

namespace NanoUI
{
    // todo : inherit styles from default styles
    // todo : Add Dictionary to user defined styles
    // GetStyle<T>() --> style.Property
    // todo: there should be event OnThemeChanged - we must change some text colors etc
    // todo: remove all generics & replace them with non-generic variant
    // todo: when we don't set any widget properties - then the properties are not saved in JSON theme &
    // we can have default values there!!!
    public class UITheme
    {
        // this is real place for all theme widgets
        // needed to support theming of all properties (no need to overloads if property getter/setter
        // does nothing extra
        // note: we can't (de)serialize widget dictionary, since Type is not serializable as key &
        // Type is more convenient key than whatever string
        readonly Dictionary<Type, UIWidget> _widgets = [];

        // note if you extend this class, extended theme class should have empty constructor
        // if you use static CreateDefault or Load methods
        public UITheme()
        {
            // todo: styles should be structs so we don't need these
            Borders = new();
            Common = new();
            Docks = new();
            Files = new();
            Pointer = new();
            Scrollbars = new();
            Windows = new();
        }

        #region Global styles

        // Decorators
        public BorderStyle Borders { get; set; }
        public CommonStyle Common { get; set; }
        public DockingStyle Docks { get; set; }
        public FilesStyle Files { get; set; }
        public FontsStyle Fonts { get; set; } = new();
        public PointerStyle Pointer { get; set; }
        public ScrollbarStyle Scrollbars { get; set; }
        public WindowStyle Windows { get; set; }

        #endregion

        #region Widget styles

        // note: this is basically called from base widget when getting basic properties
        // internally type is stored in ThemeType in Widget.
        // note: type should inherit from Widget & have empty constructor (no params)
        public UIWidget Get(Type type)
        {
            if (!_widgets.TryGetValue(type, out UIWidget? widget))
            {
                object? obj = Activator.CreateInstance(type);
                if(obj != null && obj is UIWidget w)
                {
                    _widgets[type] = w;
                    return w;
                }
                return new UIWidget();
            }

            return widget!;
        }

        // note: we lazy initialize (create only when called first time)
        // note: T should have empty constructor (no params)
        public T Get<T>() where T : UIWidget, new()
        {
            if (!_widgets.TryGetValue(typeof(T), out UIWidget? widget))
            {
                widget = new T();
                _widgets[typeof(T)] = widget;
            }
            return (T)widget;
        }

        public void Set<T>(T widget) where T : UIWidget
        {
            _widgets[typeof(T)] = widget;
        }

        // note: we must have theme widgets as properties in order to support easy serialization/deserialization
        // also type is not (de)serializable as key

        // create theme widgets
        // note: theme widgets count is quite big (so some kind of dynamic creating?)!

        // todo: should we dynamically find widgets in Assemby & create them with Activator?
        // should also use Reflection PropertyInfo to set them in correct property?
        // note: not all widgets have widget spesific theme (uses then underlying widgets theme)

        // this provides default properties
        public UIWidget Widget
        {
            get => Get<UIWidget>();
            set=> Set(value);
        }

        public UIButton Button
        {
            get => Get<UIButton>();
            set => Set(value);
        }

        public UICheckBox CheckBox
        {
            get => Get<UICheckBox>();
            set => Set(value);
        }

        // todo: should this be combined with collapsable panel?
        public UICollapsablePanelHeader UICollapsablePanelHeader
        {
            get => Get<UICollapsablePanelHeader>();
            set => Set(value);
        }

        public UIColorWheel ColorWheel
        {
            get => Get<UIColorWheel>();
            set => Set(value);
        }
        
        public UIContextMenu ContextMenu
        {
            get => Get<UIContextMenu>();
            set => Set(value);
        }

        // all dialogs
        public UIDialog Dialog
        {
            get => Get<UIDialog>();
            set => Set(value);
        }

        public UIFileFolderDetails FileFolderDetails
        {
            get => Get<UIFileFolderDetails>();
            set => Set(value);
        }

        public UIFileFolderDropdown FileFolderDropdown
        {
            get => Get<UIFileFolderDropdown>();
            set => Set(value);
        }

        public UIFileFolderFlow FileFolderFlow
        {
            get => Get<UIFileFolderFlow>();
            set => Set(value);
        }

        public UIFileFolderList FileFolderList
        {
            get => Get<UIFileFolderList>();
            set => Set(value);
        }

        public UIFileFolderTree FileFolderTree
        {
            get => Get<UIFileFolderTree>();
            set => Set(value);
        }

        public UIImageViewer ImageViewer
        {
            get => Get<UIImageViewer>();
            set => Set(value);
        }

        public UILabel Label
        {
            get => Get<UILabel>();
            set => Set(value);
        }

        public UIMenubar Menubar
        {
            get => Get<UIMenubar>();
            set => Set(value);
        }

        public UIMenu Menu
        {
            get => Get<UIMenu>();
            set => Set(value);
        }

        public UIMenuItem MenuItem
        {
            get => Get<UIMenuItem>();
            set => Set(value);
        }

        public UIPopup Popup
        {
            get => Get<UIPopup>();
            set => Set(value);
        }

        // note: this is also theming widget for DropDownView<T>, ComboBox<T>, EnumDropDown<T>, MenuView<T>
        public UIPopupButton PopupButton
        {
            get => Get<UIPopupButton>();
            set => Set(value);
        }

        public UIProgressbar Progressbar
        {
            get => Get<UIProgressbar>();
            set => Set(value);
        }

        public UISlider Slider
        {
            get => Get<UISlider>();
            set => Set(value);
        }

        public UISpinner Spinner
        {
            get => Get<UISpinner>();
            set => Set(value);
        }

        public UISplitter Splitter
        {
            get => Get<UISplitter>();
            set => Set(value);
        }

        public UITabWidget TabWidget
        {
            get => Get<UITabWidget>();
            set => Set(value);
        }

        public UITextField TextField
        {
            get => Get<UITextField>();
            set => Set(value);
        }

        public UITitlebar Titlebar
        {
            get => Get<UITitlebar>();
            set => Set(value);
        }

        public UIToolbar Toolbar
        {
            get => Get<UIToolbar>();
            set => Set(value);
        }

        public UIToolButton ToolButton
        {
            get => Get<UIToolButton>();
            set => Set(value);
        }

        public UITooltip Tooltip
        {
            get => Get<UITooltip>();
            set => Set(value);
        }
        
        public UIUpDownButton UpDownButton
        {
            get => Get<UIUpDownButton>();
            set => Set(value);
        }

        // this is ViewPanel basic properties for all ViewWidget<T> view panel implementations.
        // note: properties could be overridden in concrete implemention
        public UIViewPanel ViewPanel
        {
            get => Get<UIViewPanel>();
            set => Set(value);
        }

        public UIWindow Window
        {
            get => Get<UIWindow>();
            set => Set(value);
        }

        #endregion

        #region Methods

        // this supports dynamic theming with file parts
        // note: this can be extended with using Path in FileFolderInfo (different filetypes ...)
        public virtual (int, Color) GetFileIcon(in FileFolderInfo fileFolderInfo) => fileFolderInfo.FileFolderType switch
        {
            FileFolderType.HardDrive => (Fonts.IconHardDrive, Files.HardDriveColor),
            FileFolderType.Folder => (Fonts.IconFolder, Files.FolderColor),
            // file & default
            _ => (Fonts.IconFile, Files.FileColor)
        };

        // Init with default theme
        // T must be Theme or extension of it
        // note: T must have paramless ctor new T()

        // This function should only be used to get your app quickly running
        // It is recommended that you use Load function & provide your own theme file
        public static T CreateDefault<T>(NvgContext ctx, FontsStyle fonts) where T : UITheme, new()
        {
            T theme = new();
            // populate values with default values
            DefaultTheme.Populate(theme);

            // set fonts
            theme.Fonts = fonts;
            theme.Fonts.Init(ctx);

            return theme;
        }

        // T must be Theme or extension of it
        public static T? Load<T>(NvgContext ctx, string themefile) where T : UITheme
        {
            if (!File.Exists(themefile))
                throw new FileNotFoundException(themefile);

            ThemeSerializer serializer = new();
            
            // todo/note : we configure font ids after theme widgets created, so we can't have
            // font ids set there!!!!
            // note : font ids are set to 0 in theme widgets (whilch is normal font)
            T? theme = serializer.Load<T>(themefile);

            // init fonts etc (read from theme xml)
            theme?.Fonts?.Init(ctx);

            return theme;
        }

        public void Save(string filename)
        {
            ThemeSerializer serializer = new();
            serializer.Save(filename, this);
        }

        public virtual void Dispose()
        {

        }

        #endregion
    }
}
