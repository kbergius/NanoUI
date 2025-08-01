﻿using NanoUI.Common;
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
    public partial class UIWidget : IDisposable
    {
        WidgetList _children;
        // objects
        UIScreen? _screen;
        UIWidget? _parent;
        Layout? _childrenLayout;
        UIContextMenu? _contextMenu;

        bool _isParentPopup = false;

        // this is ctor for theme/layout generation (if you use this otherwise, set parent before using widget)
        // note: all widgets that are used as theme widget in theme should have paramless ctor
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

        [JsonIgnore]
        public WidgetList Children => _children;

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

        // todo:
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

        [JsonIgnore]
        public virtual Layout? ChildrenLayout
        {
            get => _childrenLayout;
            set => _childrenLayout = value;
        }

        // this is a type that determines whivh theme widget we use when getting theneable property.
        // if extended widget doesn't set this, we use base Widget theme properties.
        // type must be for widget that is extended from base widget

        // note: setting this overrides all base widget themeable properties, so you must configure
        // them at your theme widget, which type is this ThemeType
        // note2: it is not recommended to use interfaces, since then you have to provide all themeable
        // property getters & setters in your interface
        Type? _themeType;
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

        // mostly internal to support widget identification & loading/saving in file
        [Browsable(false)]
        public Guid Id { get; set; }

        // for searching, user identifiction
        // todo: nullable?
        [Category(Globals.CATEGORY_BASIC)]
        public string? Name { get; set; }
        
        // sorting helper (default sort operator)
        [Category(Globals.CATEGORY_BASIC)]
        public int SortKey { get; set; }
        
        // todo: nullable?
        [Category(Globals.CATEGORY_BASIC)]
        public virtual string? Tooltip { get; set; }
        
        #endregion

        #region Layout

        // this is widget's top left position in parent space
        [Category(Globals.CATEGORY_LAYOUT)]
        public virtual Vector2 Position { get; set; }

        // note: The layouting code calculates this based on Size, MinSize & FixedSize values,
        // if widget is inside layout & layouting is performed. If not this value is used as-is.
        // So you can manually reset this value after layouting has been done.
        [Category(Globals.CATEGORY_LAYOUT)]
        public virtual Vector2 Size { get; set; }
        
        // note: this is mainly used in layouting. The X & Y values are only used if their values are > 0.
        [Category(Globals.CATEGORY_LAYOUT)]
        public virtual Vector2 FixedSize { get; set; }

        // note: this is mainly used in layouting. We calculate the "real" size with Vector2.Max(Size, MinSize)
        [Category(Globals.CATEGORY_LAYOUT)]
        public virtual Vector2 MinSize { get; set; }

        // note: this is just a helper property, that is same as Soze.X
        [JsonIgnore]
        [Browsable(false)]
        public virtual float Width
        {
            get => Size.X;
            set => Size = new Vector2(value, Size.Y);
        }

        // note: this is just a helper property, that is same as Soze.Y
        [JsonIgnore]
        [Browsable(false)]
        public virtual float Height
        {
            get => Size.Y;
            set => Size = new Vector2(Size.X, value);
        }

        // note: this is just a helper property, that is same as FixedSize.X
        [JsonIgnore]
        [Browsable(false)]
        public virtual float FixedWidth
        {
            get => FixedSize.X;
            set => FixedSize = new Vector2(value, FixedSize.Y);
        }

        // note: this is just a helper property, that is same as FixedSize.Y
        [JsonIgnore]
        [Browsable(false)]
        public virtual float FixedHeight
        {
            get => FixedSize.Y;
            set => FixedSize = new Vector2(FixedSize.X, value);
        }

        // "Hard" margin for all contents (children etc) in widget
        // this is mostly used in children layouts, but you can use it also when manually
        // positioning any other content in widget. Layouts use this & GetLayoutArea function.

        // note: widgets can also have their own padding property for the content (text, icon, image etc),
        // so please don't use this property in these situations
        Thickness? _margin;
        [Category(Globals.CATEGORY_LAYOUT)]
        public Thickness Margin
        {
            get => _margin ?? GetTheme().Get(ThemeType).Margin;
            set => _margin = value;
        }

        #endregion

        #region State

        // these goes to Widget type category (first)

        // note : Changing visible property does NOT trigger layout update in parent
        // If layout update is needed, use also function RequestLayoutUpdate(Widget widget)
        public virtual bool Visible { get; set; } = true;
        public virtual bool Disabled { get; set; }
        
        // Is in screen's focus path?
        // todo : should be limited to widget extensions
        [Browsable(false)]
        public virtual bool Focused { get; set; }
        
        // some widgets may not want to have pointer focus
        // means also that HoverTint is not drawn
        public bool DisablePointerFocus { get; set; }

        // is pointer "inside" widget?
        // note: you should not normally set pointer focus manually, instead you should let NanoUI
        // handle it automatically (the exception is if your widget handles its children hovering - like views)
        bool _pointerFocus;
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

        // this is bsically used by Button by now
        [Browsable(false)]
        [JsonIgnore]
        public virtual bool Pushed { get; set; }
        
        #endregion

        #region Appearance

        // note: we should handle invalid draw action in spesified widget that checks invalid format
        // => must override DrawBackgroundBrush methods

        BrushBase? _backgroundFocused;
        public virtual BrushBase? BackgroundFocused
        {
            get => _backgroundFocused ?? GetTheme().Get(ThemeType).BackgroundFocused;
            set => _backgroundFocused = value;
        }

        BrushBase? _backgroundUnfocused;
        public virtual BrushBase? BackgroundUnfocused
        {
            get => _backgroundUnfocused ?? GetTheme().Get(ThemeType).BackgroundUnfocused;
            set => _backgroundUnfocused = value;
        }

        BrushBase? _backgroundPushed;
        public virtual BrushBase? BackgroundPushed
        {
            get => _backgroundPushed ?? GetTheme().Get(ThemeType).BackgroundPushed;
            set => _backgroundPushed = value;
        }

        bool? _border;
        [Category(Globals.CATEGORY_APPEARANCE)]
        public virtual bool Border
        {
            get => _border ?? GetTheme().Get(ThemeType).Border;
            set => _border = value;
        }

        CornerRadius? _cornerRadius;
        [Category(Globals.CATEGORY_APPEARANCE)]
        public virtual CornerRadius CornerRadius
        {
            get => _cornerRadius ?? GetTheme().Get(ThemeType).CornerRadius;
            set => _cornerRadius = value;
        }

        #endregion

        #region Text

        // note: this is used as save, load from json & get right font face id
        // note2: if font type is invalid we get default font from FontDecorator (font with id = 0)
        string? _fontType;
        [Category(Globals.CATEGORY_TEXT)]
        public string FontType
        {
            get => _fontType ?? GetTheme().Get(ThemeType).FontType;
            set => _fontType = value;
        }

        // note: if font type is invalid we get default font from FontDecorator (font with id = 0)
        int? _fontFaceId;
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

        // note: this is normally same in all widgets, so we don't get theme widget FontSize
        // so user must set value to every widget instance
        float? _fontSize;
        [Category(Globals.CATEGORY_TEXT)]
        public virtual float FontSize
        {
            get => _fontSize ?? GetTheme().Get(ThemeType).FontSize;
            set => _fontSize = value;
        }

        // note: this is normally same in all widgets, so we don't get theme widget FontIconsId
        // so user must set value to every widget instance
        int? _fontIconsId;
        [Category(Globals.CATEGORY_TEXT)]
        [JsonIgnore]
        public virtual int FontIconsId
        {
            get => _fontIconsId?? GetTheme().Fonts.GetFontId(GetTheme().Fonts.GetDefaultIconType());
            set => _fontIconsId = value;
        }

        [JsonIgnore]
        public virtual float IconScale => IconExtraScale > 0? GetTheme().Fonts.IconBaseScale * IconExtraScale : GetTheme().Fonts.IconBaseScale;

        float? _iconExtraScale;
        [Category(Globals.CATEGORY_TEXT)]
        public virtual float IconExtraScale
        {
            get => _iconExtraScale ?? GetTheme().Get(ThemeType).IconExtraScale;
            set => _iconExtraScale = value;
        }

        // note: this is normally same in all widgets, so we don't get theme widget TextHorizontalAlignment
        // so user must set value to every widget instance
        TextHorizontalAlign? _textHorizontalAlignment;
        [Category(Globals.CATEGORY_TEXT)]
        public virtual TextHorizontalAlign TextHorizontalAlignment
        {
            get => _textHorizontalAlignment?? GetTheme().Get(ThemeType).TextHorizontalAlignment;
            set => _textHorizontalAlignment = value;
        }

        // note: this is normally same in all widgets, so we don't get theme widget TextVerticalAlignment
        // so user must set value to every widget instance
        TextVerticalAlign? _textVerticalAlignment;
        [Category(Globals.CATEGORY_TEXT)]
        public virtual TextVerticalAlign TextVerticalAlignment
        {
            get => _textVerticalAlignment?? GetTheme().Get(ThemeType).TextVerticalAlignment;
            set => _textVerticalAlignment = value;
        }

        // note: this is normally same in all widgets, so we don't get theme widget TextColor
        // so user must set value to every widget instance
        Color? _textColor;
        [Category(Globals.CATEGORY_TEXT)]
        public virtual Color TextColor
        {
            get => _textColor ?? GetTheme().Get(ThemeType).TextColor;
            set => _textColor = value;
        }

        Color? _textDisabledColor;
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

        // Relative children layout area (not taking into account widget's position & margins);
        // children positions are relative
        // This is mainly used in layouts when performing layouting (positioning & sizing child widgets)

        // note: the position element of the rect is children's offset/topLeft (for example in window this consists
        // titlebar y offset); size property tells children area size that can be smaller than widgets own size
        // (you can have content "below" children - like statusbar)

        // note2: if offset is not (0, 0) then you have to take it into account when calculating size in
        // PreferredSize function
        // note3: Layouts calvulate positions in top down order, so offset indicates where layout
        // begins calculations (any content after layout should we set omitted in size)
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

        // note: this function is normally called in widget's OnPointerMove or OnPointerEnter events
        public virtual void SetPointerType(int pointerType)
        {
            Screen?.SetPointerType(pointerType);
        }

        // get the theme from screen
        // todo: me should use fallback theme in case these is no Screen (where to store it?)
        public UITheme GetTheme() => Screen?.Theme ?? new UITheme();

        // Request the focus to be moved to this widget
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

        // This triggers layout update before next Draw call
        public virtual void RequestLayoutUpdate(UIWidget? widget)
        {
            if(widget == null)
                return;

            // null is handled in screen
            Screen?.RequestLayoutUpdate(widget);
        }

        // Check if the widget contains a certain position
        public virtual bool Contains(float x, float y)
        {
            return Contains(new Vector2(x, y));
        }
        public virtual bool Contains(Vector2 position)
        {
            var d = position - Position;

            return d.X >= 0 && d.Y >= 0 &&
                   d.X < Size.X && d.Y < Size.Y;
        }

        // this is basic operation, override to add some additional logic
        public virtual void Close()
        {
            Dispose();
        }

        #endregion

        #region BackgroundBrush

        // METHODS
        public virtual void DrawBackgroundBrush(NvgContext ctx)
        {
            DrawBackgroundBrush(ctx, Position, Size);
        }

        public virtual void DrawBackgroundBrush(NvgContext ctx, Vector2 topLeft, Vector2 size)
        {
            // we draw tint when enabled &pointer mocus (hovered "action")
            GetBackgroundBrush()?.Draw(ctx, topLeft, size, !Disabled && PointerFocus ? GetTheme().Common.BackgroundHoverTint : null);
        }

        public virtual void DrawBackgroundBrush(NvgContext ctx, Vector2 topLeft, Vector2 size,
            BrushBase? brush, Color? tint)
        {
            brush?.Draw(ctx, topLeft, size, tint);
        }

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

        // Handle a pointer button event (default implementation: propagate to children)

        // note: context menu (PointerButton.Right) is handled in screen
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

        // Handle a pointer motion event
        // note: we reset pointer type here. so if your widget wants to set pointer type, you must call
        // first base.OnPointerMove & then set the pointer type (if needed) in your widget
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

        // Handle a pointer enter/leave event (default implementation: record this fact, but do nothing)
        // this is mainly called from screen after screen pointer focus widget has changed
        // note: you don't need to extent this unless your widget manages its children pointer focuses
        // (like views) OR your widget sets some other status flags (like cursor type)
        public virtual void OnPointerEnter(bool enter)
        {
            PointerFocus = enter;
        }

        // Handle a pointer drag event (default implementation: do nothing)
        public virtual bool OnPointerDrag(Vector2 p, Vector2 rel)
        {
            return false;
        }

        // Scroll is Vector2 in order to support trackballs
        // note: hardly any core widget uses scroll.X value
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

        // Handle a focus change event (default implementation: record the focus status, but do nothing)
        public virtual bool OnFocusChanged(bool focused)
        {
            Focused = focused;

            return false;
        }

        // Handle a keyboard event (default implementation: do nothing)

        // note: OnKeyUpDown event is restricted only to widgets in focuspath
        // so in order to widget get this event, widget must be in focuspath
        // (RequestFocus() called)
        // note: there is an exception in Popup (handles shortcut keys in menus)
        public virtual bool OnKeyUpDown(Key key, bool down, KeyModifiers modifiers)
        {
            return false;
        }

        // Handle text input (default implementation: do nothing)

        // note: OnKeyChar event is restricted only to widgets in focuspath
        // so in order to widget get this event, widget must be in focuspath
        // (RequestFocus() called)
        public virtual bool OnKeyChar(char c)
        {
            return false;
        }

        // this is an event that UIScreen passes only to its direct children. they can then decisde
        // if they do something and if they want to pass event to their children.
        // the default action is do nothing.

        // note: if widget resizes itself, it should then call PerformLayout or RequestLayoutUpdate
        public virtual void OnScreenResize(Vector2 size, NvgContext ctx)
        {
        
        }

        // Handle a file drop event (default : do nothing)
        // Active only when in focus path?
        public virtual bool OnFileDrop(string filename)
        {
            return false; // To be overridden
        }

        // this is an event, that child fires when it wants to break loose from parental control.
        // it is by now used in docking, when dragging/reordering dock components
        // part of drag & drop process
        // the default answer to this request is firm "No!".
        public virtual bool OnDetach(UIWidget child)
        {
            return false;
        }

        // screen calls this when dragging ends (dragwidget != null && pointer button up)
        // this is used by now in docking. position is relative pointer position
        // part of drag & drop process
        // if you want to use this, extend this function, otherwise this does nothing
        public virtual bool OnAttach(UIWidget widget, Vector2 position)
        {
            return false;
        }

        #endregion

        #region Layout

        // Compute the preferred size of the widget
        // if layout specified, use layout to calculate. Else calculate from size values
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

        // Invoke the associated layout generator to properly place child widgets, if any
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

        // note: we are inside scissor
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

        // this is called from screen after all widgets have been drawn.
        // this gives a possiblity to draw "overlay" over all widgets (widget must be put in screen's
        // post draw list to invoke this)
        // todo: screen passes pointer absolute position, so you must convert it to local position
        public virtual void PostDraw(NvgContext ctx, Vector2 pointerPosition)
        {
        
        }

        #endregion

        #region Dispose

        // Handles disposing & set focus to caller
        public void Dispose(UIWidget caller)
        {
            Dispose();

            caller?.RequestFocus();
        }

        // this could be overriden if additional dispose mechancis needed
        // this handles only screen update
        bool _disposing;
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
