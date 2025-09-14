using NanoUI.Common;
using NanoUI.Components.Menus;
using NanoUI.Layouts;
using NanoUI.Nvg;
using System;
using System.ComponentModel;
using System.Numerics;
using System.Text.Json.Serialization;

namespace NanoUI.Components
{
    // note: The Size property has no logic to contruct "real" size from Size, FixedSize & MinSize
    // (only layouting does these calculations).
    // Reason for this is, that if user deliberately sets Size in code (after layouting has been performed or
    // there are no layouting), the widget should respect that and not do its own size calculations with
    // Size, MinSize & FixedSize.
    // todo: tab navigation? (in screen?), MaxSize?

    /// <summary>
    /// UIWidget is the base class in UI layer.
    /// It provides most common properties and methods.
    /// Note: all widgets in UILayer should be derived from this, if they are going to be
    /// added into the widget tree.
    /// </summary>
    public partial class UIWidget : IDisposable
    {
        WidgetList _children;
        // objects
        UIScreen? _screen;
        UIWidget? _parent;
        Layout? _childrenLayout;
        UIContextMenu? _contextMenu;

        bool _isParentPopup = false;

        /// <summary>
        /// Constructor for theme prototype widgets.
        /// Normally use ctor new UIWidget(UIWidget? parent, <params>), when attaching widgets into the widget tree.
        /// Note: if you create your own theme widget prototype, be careful to set all themable
        /// properties in order to avoid circular reference.
        /// </summary>
        public UIWidget()
        {
            // todo: should we set all these to theme?
            // we must set here "sensible" default values for this "style" widget
            // - so there is no circular reference when this is used in style as a prototype
            _border = true;
            _cornerRadius = new CornerRadius(0);
            _fontSize = 0;
            _textColor = Color.Transparent;
            _textDisabledColor = Color.Transparent;
            _textHorizontalAlignment = TextHorizontalAlign.Left;
            // note: we set this to top - so it is not written in json theme
            _textVerticalAlignment = TextVerticalAlign.Top;
            _backgroundFocused = new BrushBase();
            _backgroundUnfocused = new BrushBase();
            _backgroundPushed = new BrushBase();
            _fontFaceId = 0;
            _fontIconsId = 0;
            _margin = new();

            // default with string is null
            FontType = string.Empty;
            IconExtraScale = default;

            _children = new(this);
        }

        // note: this is only needed for scene, since passes null as parent
        internal UIWidget(UIWidget? parent, Vector2 size)
            :this(parent)
        {
            Size = size;
        }

        /// <summary>
        /// Basic constructor where widget is created and added to parent widget's
        /// WidgetList. Derived widgets can have additional params.
        /// </summary>
        public UIWidget(UIWidget? parent)
        {
            _parent = parent;
            _isParentPopup = _parent != null && _parent is UIPopup;

            _children = new(this);

            // add to parent (screen has no parent!)
            parent?.Children.Add(this);

            // generate unique id (if loaded from file etc could be overwritten)
            Id = Guid.NewGuid();
        }

        #region Objects

        /// <summary>
        /// Children list.
        /// Note: All widgets have this; so they can act as a container oy layer.
        /// </summary>
        [JsonIgnore]
        public WidgetList Children => _children;

        /// <summary>
        /// UIScreen is the root widget of the widget tree.
        /// </summary>
        [JsonIgnore]
        public virtual UIScreen? Screen
        {
            get
            {
                if (_screen == null)
                {
                    if (this is UIScreen sc)
                    {
                        _screen = sc;
                    }
                    else
                    {
                        _screen = this.FindParent<UIScreen>();
                    }
                }

                return _screen;
            }
        }

        /// <summary>
        /// Parent widget.
        /// </summary>
        [JsonIgnore]
        public virtual UIWidget? Parent
        {
            get => _parent;
            set
            {
                if (_parent != value)
                {
                    // remove from old parent
                    if (_parent != null)
                    {
                        _parent.Children.Remove(this);

                        // set old parent to layout update list
                        RequestLayoutUpdate(_parent);
                    }

                    // set new parent
                    _parent = value;

                    _isParentPopup = _parent != null && _parent is UIPopup;

                    // add to new parent
                    if (_parent != null)
                    {
                        _parent?.Children.Add(this);

                        // set new parent to layout update list
                        // we know _parent is not null, so "!"
                        RequestLayoutUpdate(_parent!);
                    }

                    // we just nullify screen - so it is searched again
                    // (handles case when parent is in different screen/window)
                    _screen = null;
                }
            }
        }

        /// <summary>
        /// Context menu if any.
        /// </summary>
        [JsonIgnore]
        public UIContextMenu? ContextMenu
        {
            get => _contextMenu;
            set
            {
                // dispose previous?
                _contextMenu?.Dispose();

                _contextMenu = value;
            }
        }

        /// <summary>
        /// Determines which layout implementation calculates child widhets' positions and sizes.
        /// </summary>
        [JsonIgnore]
        public virtual Layout? ChildrenLayout
        {
            get => _childrenLayout;
            set => _childrenLayout = value;
        }

        Type? _themeType;

        /// <summary>
        /// Theme type is a type that determines which widget in theme we use when getting theneable property.
        /// If extended widget doesn't set this, uses base Widget theme properties.
        /// Note: setting this overrides all base widget themeable properties.
        /// So you must configure all of them at your specified theme widget.
        /// </summary>
        [JsonIgnore]
        public Type ThemeType
        {
            get => _themeType ?? typeof(UIWidget);
            set
            {
                // todo: should we produce error?
                if(!typeof(UIWidget).IsAssignableFrom(value))
                    return;

                _themeType = value;
            }
        }

        #endregion

        #region Properties

        #region Basic

        /// <summary>
        /// Id. Mostly used internally to support widget identification & loading/saving from/to file.
        /// </summary>
        [Browsable(false)]
        public Guid Id { get; set; }

        /// <summary>
        /// Name can be used for searching, identifiction.
        /// </summary>
        [Category(Globals.CATEGORY_BASIC)]
        public string? Name { get; set; }

        /// <summary>
        /// Sort key is sorting helper. Default sorter use this directly and solely.
        /// </summary>
        [Category(Globals.CATEGORY_BASIC)]
        public int SortKey { get; set; }

        /// <summary>
        /// Tooltip text.
        /// </summary>
        [Category(Globals.CATEGORY_BASIC)]
        public virtual string? Tooltip { get; set; }

        #endregion

        #region Layout

        /// <summary>
        /// Widget's top left position in parent space.
        /// </summary>
        [Category(Globals.CATEGORY_LAYOUT)]
        public virtual Vector2 Position { get; set; }

        /// <summary>
        /// Widget's size.
        /// Note: Layouting code calculates this based on Size, MinSize & FixedSize values,
        /// if widget is inside layout and visible. If not this value is used as-is.
        /// Note: You can manually reset this value after layouting has calculated this in order
        /// tweak/correct size.
        /// </summary>
        [Category(Globals.CATEGORY_LAYOUT)]
        public virtual Vector2 Size { get; set; }

        /// <summary>
        /// Widget's fixed size.
        /// Note: this is mainly used in layouting. The X & Y values are only used, if their values are > 0.
        /// Core widgtets use mainly Size property in drawing phase.
        /// </summary>
        [Category(Globals.CATEGORY_LAYOUT)]
        public virtual Vector2 FixedSize { get; set; }

        /// <summary>
        /// Widget's minimum size.
        /// Note: this is mainly used in layouting; calculates the "real" size with Vector2.Max(Size, MinSize).
        /// Core widgtets use mainly Size property in drawing phase.
        /// </summary>
        [Category(Globals.CATEGORY_LAYOUT)]
        public virtual Vector2 MinSize { get; set; }

        /// <summary>
        /// Width is just a helper property, that is same as Size.X.
        /// </summary>
        [JsonIgnore]
        [Browsable(false)]
        public virtual float Width
        {
            get => Size.X;
            set => Size = new Vector2(value, Size.Y);
        }

        /// <summary>
        /// Height is just a helper property, that is same as Size.Y.
        /// </summary>
        [JsonIgnore]
        [Browsable(false)]
        public virtual float Height
        {
            get => Size.Y;
            set => Size = new Vector2(Size.X, value);
        }

        /// <summary>
        /// Fixed width is just a helper property, that is same as FixedSize.X.
        /// </summary>
        [JsonIgnore]
        [Browsable(false)]
        public virtual float FixedWidth
        {
            get => FixedSize.X;
            set => FixedSize = new Vector2(value, FixedSize.Y);
        }

        /// <summary>
        /// Fixed height is just a helper property, that is same as FixedSize.Y.
        /// </summary>
        [JsonIgnore]
        [Browsable(false)]
        public virtual float FixedHeight
        {
            get => FixedSize.Y;
            set => FixedSize = new Vector2(FixedSize.X, value);
        }

        Thickness? _margin;

        /// <summary>
        /// Sets/gets "hard" margin for all contents (children etc) in widget.
        /// This is mostly used in layouts, but you can use it also when manually
        /// positioning any other content in widget.
        /// Note: widgets can also have their own padding property for the content,
        /// so don't use this property in these situations.
        /// </summary>
        [Category(Globals.CATEGORY_LAYOUT)]
        public Thickness Margin
        {
            get => _margin ?? GetTheme().Get(ThemeType).Margin;
            set => _margin = value;
        }

        #endregion

        #region State

        /// <summary>
        /// Sets widget's visibility. If widget is not visible, it can't get any
        /// events and it is also neglected in Draw phase.
        /// Note: Changing visible property does NOT automatically trigger layout update in parent widget.
        /// If layout update is needed, call PerformLayout or RequestLayoutUpdate with parent widget.
        /// There are still few widgets that does this automatically in order to preserve
        /// UIScreen's consitency.
        /// </summary>
        public virtual bool Visible { get; set; } = true;

        /// <summary>
        /// Disabled.
        /// </summary>
        public virtual bool Disabled { get; set; }

        // todo: Is in screen's focus path?
        
        /// <summary>
        /// Tells if widget is focused.
        /// </summary>
        [Browsable(false)]
        public virtual bool Focused { get; set; }

        /// <summary>
        /// If set to true, widget is not set as pointer focus widget.
        /// Normally this means also, that HoverTint is not drawn.
        /// </summary>
        public bool DisablePointerFocus { get; set; }

        bool _pointerFocus;

        /// <summary>
        /// Tells if pointer is "inside" widget.
        /// Note: you should not normally set pointer focus manually, instead you should let NanoUI
        /// handle it automatically (the exception is, if your widget handles its children hovering - like views).
        /// </summary>
        [Browsable(false)]
        public virtual bool PointerFocus
        {
            get => _pointerFocus;
            set
            {
                // only change if can receive pointer focus
                if (!DisablePointerFocus)
                    _pointerFocus = value;
                else
                    _pointerFocus = false;
            }
        }

        /// <summary>
        /// Pushed.
        /// </summary>
        [Browsable(false)]
        [JsonIgnore]
        public virtual bool Pushed { get; set; }
        
        #endregion

        #region Appearance

        // note: we should handle invalid draw action in spesified widget that checks invalid format
        // => must override DrawBackgroundBrush methods

        BrushBase? _backgroundFocused;

        /// <summary>
        /// BackgroundFocused.
        /// </summary>
        public virtual BrushBase? BackgroundFocused
        {
            get => _backgroundFocused ?? GetTheme().Get(ThemeType).BackgroundFocused;
            set => _backgroundFocused = value;
        }

        BrushBase? _backgroundUnfocused;

        /// <summary>
        /// BackgroundUnfocused.
        /// </summary>
        public virtual BrushBase? BackgroundUnfocused
        {
            get => _backgroundUnfocused ?? GetTheme().Get(ThemeType).BackgroundUnfocused;
            set => _backgroundUnfocused = value;
        }

        BrushBase? _backgroundPushed;

        /// <summary>
        /// BackgroundPushed.
        /// </summary>
        public virtual BrushBase? BackgroundPushed
        {
            get => _backgroundPushed ?? GetTheme().Get(ThemeType).BackgroundPushed;
            set => _backgroundPushed = value;
        }

        bool? _border;

        /// <summary>
        /// Border.
        /// </summary>
        [Category(Globals.CATEGORY_APPEARANCE)]
        public virtual bool Border
        {
            get => _border ?? GetTheme().Get(ThemeType).Border;
            set => _border = value;
        }

        CornerRadius? _cornerRadius;

        /// <summary>
        /// CornerRadius.
        /// </summary>
        [Category(Globals.CATEGORY_APPEARANCE)]
        public virtual CornerRadius CornerRadius
        {
            get => _cornerRadius ?? GetTheme().Get(ThemeType).CornerRadius;
            set => _cornerRadius = value;
        }

        #endregion

        #region Text

        string? _fontType;

        /// <summary>
        /// FontType. This is used in load/save from/to json & get right font face id.
        /// If font type is invalid, gets default font (font with id = 0).
        /// </summary>
        [Category(Globals.CATEGORY_TEXT)]
        public string FontType
        {
            get => _fontType ?? GetTheme().Get(ThemeType).FontType;
            set => _fontType = value;
        }

        int? _fontFaceId;

        /// <summary>
        /// FontFaceId. If font type is invalid. gets default font (font with id = 0).
        /// </summary>
        [Category(Globals.CATEGORY_TEXT)]
        [JsonIgnore]
        [Browsable(false)]
        public virtual int FontFaceId
        {
            get => _fontFaceId?? GetTheme().Fonts.GetFontId(FontType);
            set
            {
                // sync with FontType
                var type = GetTheme().Fonts.GetFontType(value);
                if (type == null)
                {
                    _fontFaceId = null;
                    _fontType = null;
                    return;
                }

                _fontFaceId = value;
                FontType = type;
            }
        }

        float? _fontSize;

        /// <summary>
        /// FontSize.
        /// </summary>
        [Category(Globals.CATEGORY_TEXT)]
        public virtual float FontSize
        {
            get => _fontSize ?? GetTheme().Get(ThemeType).FontSize;
            set => _fontSize = value;
        }

        int? _fontIconsId;

        /// <summary>
        /// FontIconsId is normally same in all widgets.
        /// </summary>
        [Category(Globals.CATEGORY_TEXT)]
        [JsonIgnore]
        public virtual int FontIconsId
        {
            get => _fontIconsId?? GetTheme().Fonts.GetFontId(GetTheme().Fonts.GetDefaultIconType());
            set => _fontIconsId = value;
        }

        /// <summary>
        /// IconScale.
        /// </summary>
        [JsonIgnore]
        public virtual float IconScale => IconExtraScale > 0? GetTheme().Fonts.IconBaseScale * IconExtraScale : GetTheme().Fonts.IconBaseScale;

        float? _iconExtraScale;

        /// <summary>
        /// IconExtraScale.
        /// </summary>
        [Category(Globals.CATEGORY_TEXT)]
        public virtual float IconExtraScale
        {
            get => _iconExtraScale ?? GetTheme().Get(ThemeType).IconExtraScale;
            set => _iconExtraScale = value;
        }

        TextHorizontalAlign? _textHorizontalAlignment;

        /// <summary>
        /// TextHorizontalAlignment.
        /// </summary>
        [Category(Globals.CATEGORY_TEXT)]
        public virtual TextHorizontalAlign TextHorizontalAlignment
        {
            get => _textHorizontalAlignment?? GetTheme().Get(ThemeType).TextHorizontalAlignment;
            set => _textHorizontalAlignment = value;
        }

        TextVerticalAlign? _textVerticalAlignment;

        /// <summary>
        /// TextVerticalAlignment.
        /// </summary>
        [Category(Globals.CATEGORY_TEXT)]
        public virtual TextVerticalAlign TextVerticalAlignment
        {
            get => _textVerticalAlignment?? GetTheme().Get(ThemeType).TextVerticalAlignment;
            set => _textVerticalAlignment = value;
        }

        Color? _textColor;

        /// <summary>
        /// TextColor.
        /// </summary>
        [Category(Globals.CATEGORY_TEXT)]
        public virtual Color TextColor
        {
            get => _textColor ?? GetTheme().Get(ThemeType).TextColor;
            set => _textColor = value;
        }

        Color? _textDisabledColor;

        /// <summary>
        /// TextDisabledColor.
        /// </summary>
        [Category(Globals.CATEGORY_TEXT)]
        public virtual Color TextDisabledColor
        {
            get => _textDisabledColor ?? GetTheme().Get(ThemeType).TextDisabledColor;
            set => _textDisabledColor = value;
        }

        #endregion

        #endregion

        #region Methods

        // todo: should this be public?

        /// <summary>
        /// CreateParented.
        /// </summary>
        internal bool CreateParented(UIWidget parent)
        {
            if (parent == null)
                return false;

            // note: we must remove from old parent!
            Parent = parent;
            _isParentPopup = _parent is UIPopup;

            // notre: there could be some childs already
            if (_children == null)
            {
                _children = new(this);
            }

            // add to parent (screen has no parent!)
            //parent?.Children.Add(this);

            // generate unique id (if loaded from file etc could be overwritten)
            Id = Guid.NewGuid();

            return true;
        }

        /// <summary>
        /// Relative children layout area (not taking into account widget's relative position & margins).
        /// This is mainly used in layouts when performing layouting (positioning & sizing child widgets).
        /// Note: the position element of the rect is children's offset/topLeft
        /// (for example in window this consists titlebar y offset).
        /// Size property tells children area size, that can be smaller than widgets own size
        /// (you can have content "below" children - like statusbar).
        /// Layouts calculate positions in top down order, so offset indicates where layout
        /// begins calculations (any content after layout should we set omitted in size).
        /// </summary>
        public virtual Rect GetLayoutArea()
        {
            return new Rect(
                Vector2.Zero,
                new Vector2
                {
                    X = FixedSize.X > 0 ? FixedSize.X : MathF.Max(Size.X, MinSize.X),
                    Y = FixedSize.Y > 0 ? FixedSize.Y : MathF.Max(Size.Y, MinSize.Y)
                });
        }

        /// <summary>
        /// Sets the pointer type, that is propgated to UIScreen and user application.
        /// Note: this function is normally called in widget's OnPointerMove or OnPointerEnter events.
        /// </summary>
        public virtual void SetPointerType(int pointerType)
        {
            Screen?.SetPointerType(pointerType);
        }

        // todo: me should use fallback theme in case these is no Screen (where to store it?)

        /// <summary>
        /// Gets the theme from UIScreen.
        /// </summary>
        public UITheme GetTheme() => Screen?.Theme ?? new UITheme();

        /// <summary>
        /// Requests the focus to be moved to this widget and removes focus
        /// from the previously focused widget if any.
        /// </summary>
        public virtual void RequestFocus()
        {
            if (_isParentPopup)
            {
                foreach (var child in Children.AsReadOnlySpan())
                {
                    if(child.Focused && child != this)
                    {
                        child.OnFocusChanged(false);
                        break;
                    }
                }
            }
            else
            {
                Screen?.UpdateFocus(this);
            }
        }

        /// <summary>
        /// Queues layout update (PerfomLayout), that is
        /// executed before next Draw call.
        /// </summary>
        public virtual void RequestLayoutUpdate(UIWidget? widget)
        {
            if(widget == null)
                return;

            // null is handled in screen
            Screen?.RequestLayoutUpdate(widget);
        }

        /// <summary>
        /// Checks if widget contains position (tests in local space).
        /// </summary>
        public virtual bool Contains(float x, float y)
        {
            return Contains(new Vector2(x, y));
        }

        /// <summary>
        /// Checks if widget contains position (tests in local space).
        /// </summary>
        public virtual bool Contains(Vector2 position)
        {
            var d = position - Position;

            return d.X >= 0 && d.Y >= 0 &&
                   d.X < Size.X && d.Y < Size.Y;
        }

        /// <summary>
        /// Calls Dispose() method. If you don't this override and
        /// add some other logic.
        /// </summary>
        public virtual void Close()
        {
            Dispose();
        }

        #endregion

        #region BackgroundBrush

        /// <summary>
        /// DrawBackgroundBrush.
        /// </summary>
        public virtual void DrawBackgroundBrush(NvgContext ctx)
        {
            DrawBackgroundBrush(ctx, Position, Size);
        }

        /// <summary>
        /// DrawBackgroundBrush.
        /// </summary>
        public virtual void DrawBackgroundBrush(NvgContext ctx, Vector2 topLeft, Vector2 size)
        {
            // we draw tint when enabled &pointer mocus (hovered "action")
            GetBackgroundBrush()?.Draw(ctx, topLeft, size, !Disabled && PointerFocus ? GetTheme().Common.BackgroundHoverTint : null);
        }

        /// <summary>
        /// DrawBackgroundBrush.
        /// </summary>
        public virtual void DrawBackgroundBrush(NvgContext ctx, Vector2 topLeft, Vector2 size,
            BrushBase? brush, Color? tint)
        {
            brush?.Draw(ctx, topLeft, size, tint);
        }

        /// <summary>
        /// GetBackgroundBrush.
        /// </summary>
        protected BrushBase? GetBackgroundBrush()
        {
            if (Disabled)
            {
                return GetTheme().Common.BackgroundDisabled;
            }
            else if (Pushed)
            {
                return BackgroundPushed;
            }
            else if (Focused)
            {
                return BackgroundFocused;
            }

            return BackgroundUnfocused;
        }

        /// <summary>
        /// GetDrawMode.
        /// </summary>
        protected WidgetState GetDrawMode()
        {
            if (Disabled)
            {
                return WidgetState.Disabled;
            }
            else if (Pushed)
            {
                return WidgetState.Pushed;
            }
            else if (Focused)
            {
                return WidgetState.Focused;
            }

            return WidgetState.Unfocused;
        }

        #endregion

        #region Events

        /// <summary>
        /// Handles pointer button events (up & down).
        /// Default action: propagate to children.
        /// Note: UIContextMenu (PointerButton.Right) is handled in UIScreen.
        /// </summary>
        public virtual bool OnPointerUpDown(Vector2 p, PointerButton button, bool down)
        {
            if (Children.Count > 0)
            {
                // loop backwards since we probably want topmost widget
                for (int i = _children.Count - 1; i >= 0; i--)
                {
                    UIWidget child = _children[i];

                    // check if child is "valid" & handles the event
                    if (!child.Visible || child.Disabled)
                        continue;

                    if (child.Contains(p - Position) && child.OnPointerUpDown(p - Position, button, down))
                    {
                        return true;
                    }
                }
            }

            if (button == PointerButton.Left && down && !Focused)
            {
                RequestFocus();

                // todo : check
                // do not go further
                return true;
            }

            return false;
        }

        /// <summary>
        /// Handles pointer double click.
        /// Default action: propagate to children.
        /// </summary>
        public virtual bool OnPointerDoubleClick(Vector2 p, PointerButton button)
        {
            if (Children.Count > 0)
            {
                // loop backwards since we probably want topmost widget
                for (int i = _children.Count - 1; i >= 0; i--)
                {
                    UIWidget child = _children[i];

                    // check if child is "valid" & handles the event
                    if (!child.Visible || child.Disabled)
                        continue;

                    if (child.Contains(p - Position) && child.OnPointerDoubleClick(p - Position, button))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// Handles pointer motion event.
        /// Note: we reset pointer type here. So if your widget wants to set pointer type,
        /// you must call first base.OnPointerMove & then set the pointer type (if needed) in your widget.
        /// </summary>
        public virtual bool OnPointerMove(Vector2 p, Vector2 rel)
        {
            if (Children.Count > 0)
            {
                Vector2 childPos = p - Position;

                // loop backwards since we probably want "topmost" widget
                for (int i = _children.Count - 1; i >= 0; i--)
                {
                    UIWidget child = Children[i];

                    if (!child.Visible || child.Disabled)
                        continue;

                    // check if current pointer pos was inside
                    if (child.Contains(childPos))
                    {
                        // check first subchildren
                        bool handled = child.OnPointerMove(childPos, rel);

                        if (!handled)
                        {
                            // use child as pointer focus
                            Screen?.RequestPointerFocus(child);
                        }

                        // no need to go further - we have "match"
                        return true;
                    }
                }
            }

            // we check this widget
            if (Contains(p))
            {
                Screen?.RequestPointerFocus(this);

                // no need to go further - we have "match"
                return true;
            }

            // note: if we don't find any widget "match" after looping all widgets
            // then we are at the screen & it handles event
            return false;
        }

        /// <summary>
        /// Handles pointer enter/leave event.
        /// Default action: record this fact, but do nothing.
        /// This is mainly called from UIScreen after screen pointer focus widget has changed.
        /// Note: you don't need to extent this unless your widget manages its children pointer focuses (like views) OR
        /// your widget sets some other status flags (like cursor type).
        /// </summary>
        public virtual void OnPointerEnter(bool enter)
        {
            PointerFocus = enter;
        }

        /// <summary>
        /// Handles pointer drag event. Default action: do nothing.
        /// </summary>
        public virtual bool OnPointerDrag(Vector2 p, Vector2 rel)
        {
            return false;
        }

        /// <summary>
        /// Handles pointer scroll/wheel event.
        /// Scroll is Vector2 in order to support trackballs.
        /// Note: hardly any core widget uses scroll.X value.
        /// </summary>
        public virtual bool OnPointerScroll(Vector2 p, Vector2 scroll)
        {
            if(Children.Count > 0)
            {
                // loop backwards since we probably want topmost widget
                for (int i = Children.Count - 1; i >= 0; i--)
                {
                    UIWidget child = Children[i];

                    if (!child.Visible || child.Disabled)
                        continue;

                    if (child.Contains(p - Position) && child.OnPointerScroll(p - Position, scroll))
                        return true;
                }
            }
            
            return false;
        }

        /// <summary>
        /// Handles focus change event.
        /// Default action: record the focus status, but do nothing.
        /// </summary>
        public virtual bool OnFocusChanged(bool focused)
        {
            Focused = focused;

            return false;
        }

        /// <summary>
        /// Handles keyboard event. Default action: do nothing.
        /// Note: OnKeyUpDown event is restricted only to widgets in UIScreen's focuspath.
        /// So in order to widget get this event, widget must be in focuspath (RequestFocus() called).
        /// Note3: there is an exception in UIPopup (handles shortcut keys in menus).
        /// </summary>
        public virtual bool OnKeyUpDown(Key key, bool down, KeyModifiers modifiers)
        {
            return false;
        }

        /// <summary>
        /// Handles text input. Default action: do nothing.
        /// Note: OnKeyChar event is restricted only to widgets in UIScreen's focuspath.
        /// So in order to widget get this event, widget must be in focuspath (RequestFocus() called).
        /// </summary>
        public virtual bool OnKeyChar(char c)
        {
            return false;
        }

        /// <summary>
        /// OnScreenResize is an event that UIScreen passes only to its direct children.
        /// They can then decide, if they do something and if they want to pass event to their children.
        /// Default action: do nothing.
        /// Note: if widget resizes itself, it should then call PerformLayout or RequestLayoutUpdate.
        /// </summary>
        public virtual void OnScreenResize(Vector2 size, NvgContext ctx)
        {
        
        }

        /// <summary>
        /// Handles file drop event. Default action: do nothing.
        /// </summary>
        public virtual bool OnFileDrop(string filename)
        {
            // todo: active only when in focus path?

            return false;
        }

        /// <summary>
        /// OnDetach is an event, that child fires, when it wants to break loose from parental control.
        /// It is by now used only in docking, when dragging/reordering dock components as a part of drag & drop process.
        /// The default answer to this request, is firm "No!".
        /// </summary>
        public virtual bool OnDetach(UIWidget child)
        {
            return false;
        }

        /// <summary>
        /// UIScreen calls OnAttach when dragging ends (dragwidget != null && pointer button up).
        /// This is only used by now in docking as a part of drag & drop process.
        /// Note: position is relative pointer position.
        /// If you want to use this, extend this function, otherwise this does nothing.
        /// </summary>
        public virtual bool OnAttach(UIWidget widget, Vector2 position)
        {
            return false;
        }

        #endregion

        #region Layout

        /// <summary>
        /// Computes the preferred size of the widget.
        /// If layout specified, use layout to calculate.
        /// Else calculates from size values.
        /// </summary>
        public virtual Vector2 PreferredSize(NvgContext ctx)
        {
            if (_childrenLayout != null)
            {
                // check also this min size
                return Vector2.Max(MinSize, _childrenLayout.PreferredSize(ctx, this));
            }
            else
            {
                // calculate "base" value
                Vector2 ret = Vector2.Max(Size, MinSize);

                // check fixed
                if (FixedSize.X > 0)
                    ret.X = FixedSize.X;

                if (FixedSize.Y > 0)
                    ret.Y = FixedSize.Y;

                return ret;
            }
        }

        /// <summary>
        /// PerformLayout invokes the associated layout to properly place child widgets, if any.
        /// Note: layouts neglect invisible widgets (Visible property = false).
        /// </summary>
        public virtual void PerformLayout(NvgContext ctx)
        {
            if (_childrenLayout != null)
            {
                _childrenLayout.PerformLayout(ctx, this);
            }
            else
            {
                foreach (var c in Children.AsReadOnlySpan())
                {
                    Vector2 pref = c.PreferredSize(ctx);
                    Vector2 fix = c.FixedSize;

                    c.Size = new Vector2(
                        fix.X > 0 ? fix.X : pref.X,
                        fix.Y > 0 ? fix.Y : pref.Y
                    );

                    c.PerformLayout(ctx);
                }
            }
        }

        #endregion

        #region Drawing

        /// <summary>
        /// Draw is a method that is executed every frame. So avoid placing here
        /// any expensive operations.
        /// Note: widget is "inside" scissor, that it's parent has set.
        /// </summary>
        public virtual void Draw(NvgContext ctx)
        {
            // Draw children?
            if (_children.Count > 0)
            {
                ctx.Translate(Position);

                foreach (var child in _children.AsReadOnlySpan())
                {
                    if (!child.Visible)
                        continue;

                    ctx.SaveState();

                    // Sets sciccor to child (can't draw outside its area)
                    ctx.IntersectScissor(child.Position.X, child.Position.Y,
                                        child.Size.X, child.Size.Y);

                    child.Draw(ctx);

                    ctx.RestoreState();
                }

                ctx.Translate(-Position);
            }

            // Draw debug borders if specified
            if (Globals.DEBUG_WIDGET_BOUNDS)
            {
                this.DrawDebug(ctx);
            }
        }

        /// <summary>
        /// PostDraw is called from UIScreen after all widgets have been drawn.
        /// This gives a possiblity to draw overlay over all widgets.
        /// Note: widget must be in screen's post draw list to get this call.
        /// UIScreen passes pointer's absolute position.
        /// </summary>
        public virtual void PostDraw(NvgContext ctx, Vector2 pointerPosition)
        {
        
        }

        #endregion

        #region Dispose

        /// <summary>
        /// Handles disposing and sets focus to caller.
        /// </summary>
        public void Dispose(UIWidget caller)
        {
            Dispose();

            caller?.RequestFocus();
        }

        // this could be overriden if additional dispose mechancis needed
        // this handles only screen update
        bool _disposing;

        /// <summary>
        /// Disposes and removes widget from UIScreen.
        /// </summary>
        public virtual void Dispose()
        {
            if (!_disposing)
            {
                _disposing = true;

                // we must also dispose possible context menu
                ContextMenu?.Dispose();

                // Inform screen (dragwidget, focusparh)
                Screen?.RemoveFromScreen(this);

                // todo?:
                // This object will be cleaned up by the Dispose method.
                // Therefore, you should call GC.SuppressFinalize to
                // take this object off the finalization queue
                // and prevent finalization code for this object
                // from executing a second time.
                GC.SuppressFinalize(this);           
            }
        }
        
        // we come here, if all references wiped out
        ~UIWidget()
        {
            // if already disposing - do not execute
            if(!_disposing)
                Dispose();
        }

        #endregion
    }
}
