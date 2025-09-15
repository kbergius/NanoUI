using NanoUI.Common;
using NanoUI.Components;
using NanoUI.Components.Bars;
using NanoUI.Components.Dialogs;
using NanoUI.Nvg;
using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;

namespace NanoUI
{
    // todo?: function Set all properties fixed in all widgets (no calling GetTheme() ...) -
    // - loop whole widget tree & use GetProperty, SetProperty to set properties (so no calling GetTheme() ...)
    // - may have some performance gain!
    // con for above: allocates more!

    // todo: tab key press -. focus next widget that can get focus
    
    /// <summary>
    /// UIScreen.
    /// Note: you can put any widget directly under screen,
    /// but only UIWindows (& their extensions) support automatic "layering".
    /// </summary>
    public partial class UIScreen : UIWidget
    {
        ArrayBuffer<UIWidget> _focusPath = new();

        // post draw list - for drawing "overlay" over normal drawing
        ArrayBuffer<UIWidget> _postDrawList = new();

        // Drag
        UIWidget? _dragWidget;

        // flag to indicate, if we should call TryAttach after dragging is finished (pointer up)
        // note: only docking uses this by now
        bool _tryAttach;

        // this is to be sure that only 1 widget can have pointer focus everytime
        UIWidget? _pointerFocusWidget;

        // this is called in Draw - before actual drawing
        ArrayBuffer<UIWidget> _needsLayoutUpdate = new();

        // Dialogs (we use only 1 instance of all dialogs - so no new())
        // note: this also disallows to use many dialogs of same type at the same time
        Dictionary<Type, UIDialog> _dialogs = new();

        // note: screen is only widget that has no parent
        // we inherit from widget so that we can use widget's event handling mechanics &
        // having screen as parent
        // however most widget properties are not used in screen

        /// <summary>
        /// UIScreen.
        /// </summary>
        public UIScreen(UITheme theme, Vector2 size)
            : base(null, size)
        {
            Theme = theme;

            _tooltip = new UITooltip(this);

            // no hover tint
            DisablePointerFocus = true;

            // basic dialogs
            RegisterDialog(new UIMessageBox(this));
            RegisterDialog(new UIMultilineMessageBox(this));
            RegisterDialog(new UIColorDialog(this));
            RegisterDialog(new UIFileDialog(this));
            RegisterDialog(new UIFolderDialog(this));

            // when created set this to update queue
            // so that all layouts are set in first draw call
            RequestLayoutUpdate(this);
        }

        #region Properties

        // this is only point where theme is stored (should not be null)

        /// <summary>
        /// Theme.
        /// </summary>
        public UITheme Theme { get; set; }

        // ctor creates default, simple tooltip
        // if you want to use customized tooltip, set it here
        // note : use can use same tooltip widget in many screens, since widget's parent property is not used /
        // should not be used
        // note2: be sure that tooltip's visibility is set to false, so it doesn't effect any layout code &
        // is not drawn in normal draw
        UITooltip _tooltip;

        /// <summary>
        /// Tooltip.
        /// </summary>
        public new UITooltip Tooltip
        {
            get => _tooltip;
            set
            {
                _tooltip = value;

                // find previous & remove it (if it is not same as new tooltip)
                foreach (var child in Children.AsReadOnlySpan())
                {
                    if(child is UITooltip tooltip && tooltip != _tooltip)
                    {
                        Children.Remove(child);
                        break;
                    }
                }
            }
        }

        // this is a list that holds all widgets that are "post-drawn" after real draw process
        // you can add widget to this list if you want to draw overlay over whole screen
        // note: your widget must override PostDraw(NvgContext ctx, Vector2 pointerPosition) function

        /// <summary>
        /// PostDrawList.
        /// </summary>
        public ArrayBuffer<UIWidget> PostDrawList => _postDrawList;

        // these are stored in case some widget needs to access these outside the events

        /// <summary>
        /// KeyModifiers.
        /// </summary>
        public KeyModifiers KeyModifiers { get; private set; }

        /// <summary>
        /// PointerPosition.
        /// </summary>
        public Vector2 PointerPosition { get; private set; }

        // we need to control in screen level dragging in process

        /// <summary>
        /// IsDragActive.
        /// </summary>
        public bool IsDragActive => _dragWidget != null;

        // note: this does not take into account screen

        /// <summary>
        /// IsPointerInsideUI.
        /// </summary>
        public bool IsPointerInsideUI => _pointerFocusWidget != null;

        #endregion

        #region Methods

        // note: set null, if you want to remove drag widget
        // note2: tryAttach is a boolean flag, that indicates if we should call TryAttach after dragging is
        // finished (pointer up). only docking / dock window uses this by now

        /// <summary>
        /// SetDragWidget.
        /// </summary>
        public void SetDragWidget(UIWidget? widget, bool tryAttach = false)
        {
            _dragWidget = widget;
            _tryAttach = tryAttach;
        }

        // note: this changes dialog's parent to this, so it can be shown in this screen

        /// <summary>
        /// RegisterDialog<T>.
        /// </summary>
        public void RegisterDialog<T>(T dialog) where T : UIDialog
        {
            if (dialog == null)
                return;

            dialog.Parent = this;

            _dialogs[typeof(T)] = dialog;
        }

        // todo: test nested dialogs FileDialog --> MessageBox
        // todo : TryGet

        /// <summary>
        /// GetDialog<T>.
        /// </summary>
        public T? GetDialog<T>() where T : UIDialog
        {
            if (_dialogs.TryGetValue(typeof(T), out var dialog))
            {
                // we must reset values, since we use same instance
                dialog.Reset();

                return (T)dialog;
            }

            return null;
        }

        // Update focus path
        // todo: handle looping by not sending OnFocusChanged
        // if new & old focus path contains same widgets
        // todo: can't handle recursive popups (HandlePopupFocused)
        // todo: should we set here pointer focus widget???
        bool looping = false;

        /// <summary>
        /// UpdateFocus.
        /// </summary>
        public void UpdateFocus(UIWidget? widget)
        {
            if (widget == null || looping)
                return;
            
            looping = true;

            foreach (var w in _focusPath.AsReadOnlySpan())
            {
                if (!w.Focused)
                    continue;

                w.OnFocusChanged(false);
            }
            
            _focusPath.Clear();

            // clear pointer focus
            // todo: should we set pointer focus to widget, so IsPointerInsideUI function returns
            // right value
            RequestPointerFocus(null);

            UIWindow? window = null;

            while (widget != null)
            {
                _focusPath.Add(widget);

                if (widget is UIWindow w)
                    window = w;

                widget = widget.Parent;
            }

            // release unused
            _focusPath.ReleaseUnused();

            foreach (var w in _focusPath.AsReadOnlySpan())
                w.OnFocusChanged(true);

            if (window != null)
                window.MoveToLast();

            looping = false;
        }

        // widget wants its layout updated (PerformLayout)
        // note: we "queue" widgets since there can be many same widgets or one of the 
        // widgets parent has already requested layout update
        // real update is executed before draw operations

        /// <summary>
        /// RequestLayoutUpdate.
        /// </summary>
        public override void RequestLayoutUpdate(UIWidget? widget)
        {
            // check for null, duplicates or parent(s)
            if (widget != null && !_needsLayoutUpdate.Contains(widget))
            {
                bool add = true;

                var parent = widget;

                // screen has no parent - so finally ends there
                while (parent != null)
                {
                    parent = parent.Parent;

                    if (parent != null && _needsLayoutUpdate.Contains(parent))
                    {
                        // found
                        add = false;
                        break;
                    }
                }

                if (add)
                {
                    _needsLayoutUpdate.Add(widget);
                }
            }
        }

        // this is called from Widget.Dispose()

        // note: if Widget.Dispose(Widget caller) called - focus is automatically set to caller
        // note: if this is called directly and widget is focused,
        // then user must manually handle focus change
        // todo: we could check if widget's parent is this (screen) aka is window & it is focused -> set remaining
        // topmost window focused

        /// <summary>
        /// RemoveFromScreen.
        /// </summary>
        public void RemoveFromScreen(UIWidget widget)
        {
            // parent
            if(widget.Parent != null && widget.Parent.Children.Contains(widget))
            {
                widget.Parent.Children.Remove(widget);
            }

            // focuspath
            if (_focusPath.Contains(widget))
            {
                _focusPath.Remove(widget);
            }

            // post draw list (removes if exists)
            _postDrawList.Remove(widget);

            // drag widget
            if (_dragWidget == widget)
                _dragWidget = null;

            // pointer focus
            if(_pointerFocusWidget == widget)
            {
                RequestPointerFocus(null);
            }

            // dispose children - calls dispose, that calls this function again
            // supports extendable dispose mechanics
            foreach (var child in widget.Children.AsReadOnlySpan())
            {
                child?.Dispose();
            }
        }

        // sends OnPointerEnter events & store new pointerFocus widget

        /// <summary>
        /// RequestPointerFocus.
        /// </summary>
        public void RequestPointerFocus(UIWidget? widget)
        {
            if (_pointerFocusWidget == widget)
                return;

            // old
            _pointerFocusWidget?.OnPointerEnter(false);
            // we make sure to clear pointer type
            ResetPointerType();

            // store
            _pointerFocusWidget = widget;

            // new
            _pointerFocusWidget?.OnPointerEnter(true);
        }

        // this function resets pointer type to the one specified in theme
        // note: screen resets pointer type automatically when pointer focus widget changes

        /// <summary>
        /// ResetPointerType.
        /// </summary>
        public virtual void ResetPointerType()
        {
            SetPointerType(Theme.Pointer.PointerType);
        }

        /// <summary>
        /// DeltaSeconds.
        /// </summary>
        public float DeltaSeconds { get; private set; }

        // note: this could be the place to set any animation/timer support

        /// <summary>
        /// Update.
        /// </summary>
        public virtual void Update(float deltaSeconds)
        {
            DeltaSeconds = deltaSeconds;
        }

        // todo: should we restict only to last (topmost window)?
        // todo : should we return screen if none found?

        /// <summary>
        /// FindTopmost.
        /// </summary>
        public UIWidget? FindTopmost(Vector2 p)
        {
            return Children.FindTopmost(p - Position);
        }

        // check if we should process pointer event
        // note: this checks by now if we have modal window & pointer is outside modal window

        /// <summary>
        /// ProcessPointerEvent.
        /// </summary>
        bool ProcessPointerEvent(Vector2 pointerPos)
        {
            if (_focusPath.Count > 0)
            {
                // Neglect event if we have modal window & pointerPos is outside of the modal window
                foreach (var widget in _focusPath.AsReadOnlySpan())
                {
                    if (widget is UIWindow window)
                    {
                        if (window.Modal && !window.Contains(pointerPos))
                            return false;
                    }
                }
            }

            return true;
        }

        #endregion

        #region Events

        #region OnPointerUpDown

        // order to determine which widgets gets the event
        // 1. focus path + modal + pointer outside
        // 2. context menu
        // 3. drag'n drop
        // 4. base

        /// <inheritdoc />
        public override bool OnPointerUpDown(Vector2 pointerPos, PointerButton button, bool down)
        {
            // 1. focus path + modal + pointer outside
            // note: PointerPosition is set in OnPointerMove
            if(!ProcessPointerEvent(pointerPos))
                return false;

            // 2. context menu
            // we restrict context menu only to topmost widget 
            if (button == PointerButton.Right)
            {
                UIWidget? topMost = FindTopmost(pointerPos);

                // check widget & context menu

                // todo : should we RequestFocus for topmost
                // so if it doesn't have context menu, it closes (possibly) open context menus
                // note: when focus lost. context menu closed
                if(topMost != null && topMost.ContextMenu != null)
                {
                    return topMost.ContextMenu.OnFocusChanged(true);
                }

                // screen
                if(ContextMenu != null)
                {
                    return ContextMenu.OnFocusChanged(true);
                }
                
                return false;
            }

            // 3. drag'n drop
            if (_dragWidget != null && _dragWidget.Parent != null && !down)
            {
                _dragWidget.OnPointerUpDown(
                        pointerPos - _dragWidget.Parent.GetDisplayPosition(), button,
                        false);

                if (_tryAttach)
                {
                    // we try to attach drag widget
                    bool res = Children.TryAttachWidget(_dragWidget, pointerPos);

                    _tryAttach = false;
                }

                // clear drag widget on pointer up
                _dragWidget = null;
            }

            // 4. base
            // todo: is this needed when we used drag widget? we may still have new drag widget
            return base.OnPointerUpDown(pointerPos, button, down);
        }

        #endregion

        #region OnPointerDoubleClick

        // todo: we could restrict this solely to focuspath?

        /// <inheritdoc />
        public override bool OnPointerDoubleClick(Vector2 p, PointerButton button)
        {
            if (!ProcessPointerEvent(p))
                return false;

            return base.OnPointerDoubleClick(p, button);
        }

        #endregion

        #region OnPointerMove

        // note : we could restrict pointer enter/move events only in topmost
        // (focused) window & allways have 1 focused window (if there is any windows)

        /// <inheritdoc />
        public override bool OnPointerMove(Vector2 pointerPos, Vector2 rel)
        {
            PointerPosition = pointerPos;

            if (!ProcessPointerEvent(pointerPos))
                return false;

            bool ret = false;

            if (!IsDragActive)
            {
                // note: we could reset pointer type here also (maybe more safe)
            }
            else
            {
                // shouldn't be null - but check anyway
                if (_dragWidget != null && _dragWidget.Parent != null)
                {
                    ret = _dragWidget.OnPointerDrag(
                        pointerPos - _dragWidget.Parent.GetDisplayPosition(), rel);
                }
            }

            if (!ret)
            {
                ret = base.OnPointerMove(pointerPos, rel);
            }

            if (!ret)
            {
                // no widget found/responded
                RequestPointerFocus(null);
            }

            return ret;
        }

        #endregion

        #region OnPointerScroll

        // OnPointerScroll event is sent to to all children (windows) in backwards order,
        // but first window that is not collapsed & visible & contains pointer position
        // stops looping (after it tests its children)

        /// <inheritdoc />
        public override bool OnPointerScroll(Vector2 pointerPos, Vector2 scroll)
        {
            if (!ProcessPointerEvent(pointerPos))
                return false;

            return base.OnPointerScroll(pointerPos, scroll);
        }

        #endregion

        #region OnKeyUpDown

        // we restrict OnKeyUpDown event only to first widget in focuspath (should be focused)
        // (in focus path can (?) be widgets that can't have focus -> Focused = false)

        // note: screen can´t get event (stack overflow)
        // todo: navigation with TAB

        /// <inheritdoc />
        public override bool OnKeyUpDown(Key key, bool down, KeyModifiers modifiers)
        {
            KeyModifiers = modifiers;

            if (_focusPath.Count > 0)
            {
                foreach (var widget in _focusPath.AsReadOnlySpan())
                {
                    // prevent stack overflow
                    if (widget is UIScreen)
                    {
                        //continue;
                        // screen is last so no need to go further
                        break;
                    }

                    if (widget.Focused && widget.OnKeyUpDown(key, down, modifiers))
                        return true;
                }
            }

            // check if screen has main menubar - we must check shortcuts
            /*if (down)
            {
                foreach (var widget in Children.AsReadOnlySpan())
                {
                    if (widget is MenubarOLD menubar)
                    {
                        return menubar.OnKeyUpDown(key, down, modifiers);
                    }
                }
            }*/
            foreach (var child in Children.AsReadOnlySpan())
            {
                if (!child.Visible || child.Disabled)
                    continue;

                if (child is UIMenubar)
                {
                    if (child.OnKeyUpDown(key, down, modifiers))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        #endregion

        #region OnKeyChar

        // we restrict OnKeyChar event only to first widget in focuspath (should be focused)
        // (in focus path can (?) be widgets that can't have focus -> Focused = false)

        // note: screen can´t get event (stack overflow)

        /// <inheritdoc />
        public override bool OnKeyChar(char c)
        {
            if (_focusPath.Count > 0)
            {
                foreach (var widget in _focusPath.AsReadOnlySpan())
                {
                    // prevent stack overflow - call self
                    if (widget is UIScreen)
                        continue;

                    if (widget.Focused && widget.OnKeyChar(c))
                        return true;
                }
            }

            return false;
        }

        #endregion

        #region OnScreenResize

        // note: this event is only passed to screen's direct children. they can then decide
        // if they want to pass event to their children. the default action in UIWidget is
        // do nothing

        /// <inheritdoc />
        public override void OnScreenResize(Vector2 size, NvgContext ctx)
        {
            Size = size;

            foreach (var child in Children.AsReadOnlySpan())
            {
                if (!child.Visible)
                    continue;

                child.OnScreenResize(size, ctx);
            }
        }

        #endregion

        #region OnFileDrop

        // File drop is restricted to widgets in focus path (screen can't get event)

        // note: we don't operate with byte array (byte[]) since windget that accepts
        // filedrop, may not need it

        /// <inheritdoc />
        public override bool OnFileDrop(string filename)
        {
            if (!File.Exists(filename))
            {
                return false;
            }

            // search in this focus path
            foreach (var widget in _focusPath.AsReadOnlySpan())
            {
                // prevent stack overflow
                if (widget is UIScreen)
                    continue;

                if (widget.OnFileDrop(filename))
                {
                    return true;
                }
            }

            return false;
        }

        #endregion

        #endregion

        #region Clipboard & Pointer type & Input text start/stop

        public Action<string>? ClipboardSet;
        public Func<string>? ClipboardGet;
        public Action<int>? PointerTypeChanged;
        public Action? OnStartTextInput;
        public Action? OnStopTextInput;

        // User should extend these (they need access to windowing platform)

        /// <summary>
        /// GetClipboardString.
        /// </summary>
        public virtual string GetClipboardString()
        {
            if(ClipboardGet != null)
            {
                return ClipboardGet.Invoke();
            }
            return string.Empty;
        }

        /// <summary>
        /// SetClipboardString.
        /// </summary>
        public virtual void SetClipboardString(ReadOnlySpan<char> text)
        {
            ClipboardSet?.Invoke(text.ToString());
        }

        int? _oldPointerType;
        int? _newPointerType;

        // note: widgets (ie Windows) may set pointer type in event handling &
        // then check what is the current pointer type in draw event

        /// <summary>
        /// GetCurrentPointerType.
        /// </summary>
        public int GetCurrentPointerType()
        {
            return _newPointerType.HasValue ? _newPointerType.Value : Theme.Pointer.PointerType;
        }

        // This could be called many times in frame (OnPointerMove etc). so we delay callback call until
        // all draw actions have been done

        /// <summary>
        /// SetPointerType.
        /// </summary>
        public override void SetPointerType(int pointerType)
        {
            if (_newPointerType == pointerType)
                return;

            _newPointerType = pointerType;
        }

        // widget should call this in OnFocusChanged(true) event, if it wants to get OnKeyChar events

        /// <summary>
        /// StartTextInput.
        /// </summary>
        public void StartTextInput()
        {
            OnStartTextInput?.Invoke();
        }

        // widget should call this in OnFocusChanged(false) event

        /// <summary>
        /// StopTextInput.
        /// </summary>
        public void StopTextInput()
        {
            OnStopTextInput?.Invoke();
        }

        #endregion

        #region Drawing

        /// <summary>
        /// Draw.
        /// </summary>
        public override void Draw(NvgContext ctx)
        {
            // counter to restrict too many layout updates in frame
            int counter = 0;

            // update needed layouts
            while (_needsLayoutUpdate.Count > 0)
            {
                _needsLayoutUpdate[0]?.PerformLayout(ctx);
                _needsLayoutUpdate.RemoveAt(0);

                // we restrict layout updates per frame
                if (counter++ > Globals.MAX_LAYOUT_UPDATE_PER_FRAME)
                    break;
            }

            // draw background
            DrawBackgroundBrush(ctx);

            // Draw childs recursively
            base.Draw(ctx);

            // this method draws overlay over normal drawing
            // todo: should we send relative position?
            foreach (var postDrawWidget in PostDrawList.AsReadOnlySpan())
            {
                postDrawWidget?.PostDraw(ctx, PointerPosition);
            }

            // Tooltip
            if (Globals.SHOW_TOOLTIPS)
            {
                // note: we allways call draw method in tooltip
                // tooltip implementation determines whether it will draw anyting or not & what to draw
                var topMost = FindTopmost(PointerPosition);

                if (topMost != null)
                {
                    Tooltip?.Draw(ctx, topMost);
                }
            }

            // we invoke pointer type callback if pointer type really changed
            DrawPointer(GetCurrentPointerType(), PointerPosition);
        }

        // this is a function that you can override, if you want to draw your own pointer type
        // the default implementation just invokes PointerTypeChanged action if pointer type has really changed
        // this is called after all widgets has been drawn

        /// <summary>
        /// DrawPointer.
        /// </summary>
        protected virtual void DrawPointer(int pointerType, Vector2 position)
        {
            if (_oldPointerType != pointerType)
            {
                _oldPointerType = pointerType;

                PointerTypeChanged?.Invoke(pointerType);
            }
        }

        #endregion

        #region Dispose

        /// <summary>
        /// Dispose.
        /// </summary>
        public override void Dispose()
        {
            // call dispose on base
            base.Dispose();

            Theme?.Dispose();
            Tooltip?.Dispose();
        }

        #endregion
    }
}
