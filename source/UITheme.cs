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

    /// <summary>
    /// UITheme is a class, that holds all theming/styling information for widgets.
    /// It acts like CSS file in web. When widgets want to get their styling info,
    /// they get it from the UITheme.
    /// Note: UITheme is only stored into the UIScreen, so you must pass it to screen when
    /// you create it. There is also possibility to dynamically change some/all styling
    /// at runtime and all the widgets use immediately the new styling info.
    /// </summary>
    public class UITheme
    {
        // note: we can't (de)serialize widget dictionary, since Type is not serializable as key &
        // Type is more convenient key than whatever string
        readonly Dictionary<Type, UIWidget> _widgets = [];

        /// <summary>
        /// Ctor.
        /// </summary>
        public UITheme()
        {

        }

        #region Global styles

        /// <summary>
        /// Global border style.
        /// </summary>
        public BorderStyle Borders { get; set; }

        /// <summary>
        /// Global common style.
        /// </summary>
        public CommonStyle Common { get; set; }

        /// <summary>
        /// Global docking style.
        /// </summary>
        public DockingStyle Docks { get; set; }

        /// <summary>
        /// Global file style.
        /// </summary>
        public FilesStyle Files { get; set; }

        /// <summary>
        /// Global font style.
        /// </summary>
        public FontsStyle Fonts { get; set; } = new();

        /// <summary>
        /// Global pointer style.
        /// </summary>
        public PointerStyle Pointer { get; set; }

        /// <summary>
        /// Global scrollbar style.
        /// </summary>
        public ScrollbarStyle Scrollbars { get; set; }

        /// <summary>
        /// Global window style.
        /// </summary>
        public WindowStyle Windows { get; set; }

        #endregion

        #region Widget styles

        // note: this is basically called from base widget when getting basic properties
        // internally type is stored in ThemeType in Widget.
        
        /// <summary>
        /// Get the theme prototype widget of type.
        /// Note: Prototype widgets are lazy-initialized and
        /// created with empty constructor like new UIWidget().
        /// </summary>
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

        /// <summary>
        /// Get the theme prototype widget of type T.
        /// Note: Prototype widgets are lazy-initialized and
        /// created with empty constructor like new UIWidget().
        /// </summary>
        public T Get<T>() where T : UIWidget, new()
        {
            if (!_widgets.TryGetValue(typeof(T), out UIWidget? widget))
            {
                widget = new T();
                _widgets[typeof(T)] = widget;
            }

            return (T)widget;
        }

        /// <summary>
        /// Store the theme prototype widget of type T.
        /// </summary>
        public void Set<T>(T widget) where T : UIWidget
        {
            _widgets[typeof(T)] = widget;
        }

        // todo: could also dynamically find widgets in Assemby & create them with Activator?
        // should also use Reflection PropertyInfo to set them in correct property?
        
        /// <summary>
        /// Default theme prototype widget, that is used if spesified widget is not found or defined.
        /// </summary>
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

        /// <summary>
        /// Default theme prototype widget for dialogs.
        /// </summary>
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

        /// <summary>
        /// Default theme prototype widget for UIPopupButton and all widgets that are derived from it
        /// (like UIDropDownView<T>, UIComboBox<T>, UIEnumDropDown<T>).
        /// </summary>
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

        /// <summary>
        /// Default theme prototype widget for UIViewPanel and all widgets that are derived from it or use it.
        /// </summary>
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

        /// <summary>
        /// Gets file/folder/hard drive icon. Supports dynamic theming.
        /// </summary>
        public virtual (int, Color) GetFileIcon(in FileFolderInfo fileFolderInfo) => fileFolderInfo.FileFolderType switch
        {
            FileFolderType.HardDrive => (Fonts.IconHardDrive, Files.HardDriveColor),
            FileFolderType.Folder => (Fonts.IconFolder, Files.FolderColor),
            // file & default
            _ => (Fonts.IconFile, Files.FileColor)
        };

        /// <summary>
        /// Creates default theme (see Utils/DefaultTheme class).
        /// This function should only be used to get your app quickly running.
        /// It is recommended that you use Load function & provide your own theme file.
        /// Note: T must have paramless ctor new T().
        /// </summary>
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

        /// <summary>
        /// Loads theme of type T from the file path "themefile".
        /// </summary>
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
            theme?.Fonts.Init(ctx);

            return theme;
        }

        /// <summary>
        /// Saves theme to the theme file.
        /// </summary>
        public void Save(string filename)
        {
            ThemeSerializer serializer = new();
            serializer.Save(filename, this);
        }

        /// <summary>
        /// Dispose.
        /// </summary>
        public virtual void Dispose()
        {

        }

        #endregion
    }
}
