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

    // todo: should handle also cases, when screen's position is not (0, 0).

    // todo: tab key press -. focus next widget that can get focus

    /// <summary>
    /// UIScreen is the root widget in the widget tree.
    /// Its main purpose is to orchestrate all the widgets in the widget tree.
    /// UIScreen is also derived from the UIWidget, but it has no parent widget and
    /// its methods differ quite a lot from the other widgets.
    /// When you want to send user input events and execute Update & Draw commands,
    /// you use UIScreen.
    /// Note: you can put any widget directly under screen,
    /// but only UIWindows (& its extensions) support automatic "layering".
    /// </summary>
    public partial class UIScreen : UIWidget
    {
        // currently focused widgets
        ArrayBuffer<UIWidget> _focusPath = new();

        // post draw list - for drawing an overlay over normal drawing
        ArrayBuffer<UIWidget> _postDrawList = new();

        // dragging
        UIWidget? _dragWidget;

        // flag to indicate, if we should call TryAttach after dragging is finished (pointer up)
        // note: only docking uses this by now
        bool _tryAttach;

        // this is to be sure that only 1 widget can have pointer focus anytime
        UIWidget? _pointerFocusWidget;

        // this is called in Draw - before actual drawing
        ArrayBuffer<UIWidget> _needsLayoutUpdate = new();

        // Dialogs (we use only 1 instance of all dialogs - so no new())
        // note: this also disallows to use many dialogs of same type at the same time
        Dictionary<Type, UIDialog> _dialogs = new();

        /// <summary>
        /// Creates screen with given theme and size (normally your window size).
        /// </summary>
        public UIScreen(UITheme theme, Vector2 size)
            : base(null, size)
        {
            Theme = theme;

            // create default, simple tooltip
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

        /// <summary>
        /// This is only place where theme is stored (should not be null).
        /// </summary>
        public UITheme Theme { get; set; }

        UITooltip _tooltip;

        /// <summary>
        /// Gets/sets tooltip.
        /// If you don't spesify this, screen uses its own simple tooltip widget.
        /// </summary>
        public new UITooltip Tooltip
        {
            get => _tooltip;
            set
            {
                if (value == null)
                    return;

                // find previous & remove it
                foreach (var child in Children.AsReadOnlySpan())
                {
                    if(child is UITooltip tooltip)
                    {
                        Children.Remove(child);
                        break;
                    }
                }

                // set new
                _tooltip = value;

                // set visibility to false,
                // so it doesn't affect any layout code &
                // it is not drawn in normal draw.
                _tooltip.Visible = false;

                // set parent
                _tooltip.Parent = this;
            }
        }

        /// <summary>
        /// PostDrawList is a list of widgets, that are post-drawn after normal draw process.
        /// You can add any widget to this list and it will be drawn as an overlay over whole screen.
        /// </summary>
        /// <remarks>Your widget must override and implement widget's PostDraw method.</remarks>
        public ArrayBuffer<UIWidget> PostDrawList => _postDrawList;

        /// <summary>
        /// KeyModifiers are stored in case some widgets need to access to these outside the events.
        /// </summary>
        public KeyModifiers KeyModifiers { get; private set; }

        /// <summary>
        /// PointerPosition is the pointer's display position.
        /// </summary>
        public Vector2 PointerPosition { get; private set; }

        /// <summary>
        /// IsDragActive flag tells if dragging is happening.
        /// </summary>
        public bool IsDragActive => _dragWidget != null;

        #endregion

        #region Methods

        /// <summary>
        /// IsPointerInsideUI checks if pointer is in some visible widget's area.
        /// It doesn't take into account screen itself.
        /// </summary>
        public bool IsPointerInsideUI(Vector2 pointerPosition)
        {
            var pos = pointerPosition - Position;

            // todo: there could be some corner cases, when widgets draw outside
            // their scissors area.

            // loop immediate childs
            foreach (var child in Children.AsReadOnlySpan())
            {
                if (child.Visible && child.Contains(pos))
                    return true;
            }

            return false;
        }

        /// <summary>
        /// Sets current drag widget. Set null, if you want to remove drag widget.
        /// </summary>
        /// <remarks>
        /// TryAttach is a boolean flag, that indicates if screen should call
        /// TryAttach after dragging is finished (pointer up).
        /// Only docking / dock window uses tryAttach flag by now.
        /// </remarks>
        public void SetDragWidget(UIWidget? widget, bool tryAttach = false)
        {
            _dragWidget = widget;
            _tryAttach = tryAttach;
        }

        /// <summary>
        /// You can register your own dialogs (derived from UIDialog) to the screen,
        /// so you don't need to create/dispose them every time you want to use them.
        /// </summary>
        /// <remarks>
        /// Registering changes dialog's parent to this screen, so it can be shown.
        /// </remarks>
        public void RegisterDialog<T>(T dialog) where T : UIDialog
        {
            if (dialog == null)
                return;

            dialog.Parent = this;

            _dialogs[typeof(T)] = dialog;
        }

        /// <summary>
        /// Tries to get the dialog of type T and call its Reset method.
        /// </summary>
        public bool TryGetDialog<T>(out T dialog) where T : UIDialog, new()
        {
            if (_dialogs.TryGetValue(typeof(T), out var d))
            {
                dialog = (T)d;

                // we must reset values, since we use same instance
                dialog.Reset();

                return true;
            }

            dialog = new T();

            return false;
        }

        // todo: handle looping by not sending OnFocusChanged
        // if new & old focus path contains same widgets
        // todo: can't handle recursive popups (HandlePopupFocused)
        // todo: should we set here pointer focus widget???
        bool looping = false;

        /// <summary>
        /// Widgets can call UpdateFocus, when they want to be focused/active widget.
        /// This method sends OnFocusChanged(false) event to all widgets that are in
        /// current focus path and recreates focus path and sends OnFocusChanged(true) event
        /// to widgets in the new focus path. It also rearranges UIWindows.
        /// Focus path contains widget itself and all its direct parents.
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

        /// <summary>
        /// Widgets can call RequestLayoutUpdate, when they want to recalculate children
        /// positions and sizes (for example when adding/removing widgets).
        /// </summary>
        /// <remarks>
        /// This method queues the layout requests and they are executed before
        /// actual drawing is processsed. This method also tries to execute layout commands
        /// for the same widgets only once. If you want to perform immediate layout change,
        /// call PerformLayout method in widget.
        /// </remarks>
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

        // todo: we could check if widget's parent is this (screen) aka is window & it is focused -> set remaining
        // topmost window focused

        /// <summary>
        /// Removes widget from the screen. This is automatically called from
        /// Widget.Dispose() method.
        /// </summary>
        /// <remarks>
        /// If you call this directly and widget is focused,
        /// you must manually handle focus change (set focus to some other widget).
        /// </remarks>
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

        /// <summary>
        /// RequestPointerFocus is used basically to handle hovering.
        /// This sends OnPointerEnter(false & true) events and
        /// store new pointer focus widget.
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

        /// <summary>
        /// Resets pointer type to the one specified in theme.
        /// </summary>
        /// <remarks>
        /// Screen resets pointer type automatically,
        /// when pointer focus widget changes.
        /// </remarks>
        public virtual void ResetPointerType()
        {
            SetPointerType(Theme.Pointer.PointerType);
        }

        /// <summary>
        /// Delta seconds is stored here from Update method.
        /// </summary>
        public float DeltaSeconds { get; private set; }

        // todo: this could be the place to set any animation/timer support?

        /// <summary>
        /// Doesn't actually do anything more than store deltaSeconds value.
        /// So screen doesn't loop all widgets in the widget tree and send them
        /// "Update" method. DeltaSeconds is used only in the need-to-know basis
        /// (for example widgets want to do some animations).
        /// You can do your animation/update processes in Draw method before
        /// actually drawing.
        /// </summary>
        /// <remarks>
        /// If you want to process some additional logic here, you can create your
        /// own screen (based on UIScreen) and override this method.
        /// </remarks>
        public override void Update(float deltaSeconds)
        {
            DeltaSeconds = deltaSeconds;
        }

        /// <summary>
        /// Finds topmost widget.
        /// </summary>
        public UIWidget? FindTopmost(Vector2 p)
        {
            // todo: should we restict only to last (topmost window)?
            // todo : should we return screen if none found?

            return Children.FindTopmost(p - Position);
        }

        /// <summary>
        /// Process pointer event check if we should actually process pointer event.
        /// </summary>
        /// <remarks>
        /// This checks by now, if we have modal window & pointer is outside modal window.
        /// </remarks>
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

        /// <inheritdoc />
        public override bool OnPointerUpDown(Vector2 pointerPos, PointerButton button, bool down)
        {
            // order to determine which widgets gets the event
            // 1. focus path + modal + pointer outside
            // 2. context menu
            // 3. drag'n drop
            // 4. base

            // 1. focus path + modal + pointer outside
            // note: PointerPosition is set in OnPointerMove
            if (!ProcessPointerEvent(pointerPos))
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

        /// <inheritdoc />
        public override bool OnPointerDoubleClick(Vector2 p, PointerButton button)
        {
            // todo: we could restrict this solely to focuspath?

            if (!ProcessPointerEvent(p))
                return false;

            return base.OnPointerDoubleClick(p, button);
        }

        #endregion

        #region OnPointerMove

        /// <inheritdoc />
        public override bool OnPointerMove(Vector2 pointerPos, Vector2 rel)
        {
            // note : we could restrict pointer enter/move events only in topmost
            // (focused) window & allways have 1 focused window (if there is any windows)

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

        /// <inheritdoc />
        public override bool OnPointerScroll(Vector2 pointerPos, Vector2 scroll)
        {
            if (!ProcessPointerEvent(pointerPos))
                return false;

            return base.OnPointerScroll(pointerPos, scroll);
        }

        #endregion

        #region OnKeyUpDown

        /// <inheritdoc />
        public override bool OnKeyUpDown(Key key, bool down, KeyModifiers modifiers)
        {
            // we restrict OnKeyUpDown event only to first widget in focuspath and menubars.
            // the widget that gets event can decide if it propagates event further.

            // note: screen can´t get event (stack overflow)
            // todo: navigation with TAB

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

        /// <inheritdoc />
        public override bool OnKeyChar(char c)
        {
            // we restrict OnKeyChar event only to first widget in focuspath
            
            // note: screen can´t get event (stack overflow)

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

        /// <inheritdoc />
        public override bool OnFileDrop(string filename)
        {
            // File drop is restricted to widgets in focus path (screen can't get event).

            // note: we don't operate with byte array (byte[]) since windget that accepts
            // filedrop, may not need it.

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

        /// <summary>
        /// Gets clipboard string from the user application.
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
        /// Sets clipboard string to the user application.
        /// </summary>
        public virtual void SetClipboardString(ReadOnlySpan<char> text)
        {
            ClipboardSet?.Invoke(text.ToString());
        }

        int? _oldPointerType;
        int? _newPointerType;

        /// <summary>
        /// Gets current pointer type.
        /// </summary>
        public int GetCurrentPointerType()
        {
            return _newPointerType.HasValue ? _newPointerType.Value : Theme.Pointer.PointerType;
        }

        /// <summary>
        /// Sets pointer type.
        /// </summary>
        /// <remarks>
        /// This could be called many times in frame (OnPointerMove etc).
        /// So callback call is delayed until all draw actions have been done.
        /// </remarks>
        public override void SetPointerType(int pointerType)
        {
            if (_newPointerType == pointerType)
                return;

            _newPointerType = pointerType;
        }

        /// <summary>
        /// Invokes OnStartTextInput action if defined.
        /// </summary>
        /// <remarks>
        /// Some windowing systems may not handle by themself OnKeyChar/OnTextInput actions.
        /// So this is here to provide fallback method. Only widgets that provide
        /// editable texts should call this when they get focused.
        /// </remarks>
        public void StartTextInput()
        {
            OnStartTextInput?.Invoke();
        }

        /// <summary>
        /// Invokes OnStopTextInput action if defined.
        /// </summary>
        public void StopTextInput()
        {
            OnStopTextInput?.Invoke();
        }

        #endregion

        #region Drawing

        /// <inheritdoc />
        public override void Draw(NvgContext ctx)
        {
            // note: this is specialized Draw method, that differs a lot from
            // the draw methods in widgets.

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
            foreach (var postDrawWidget in PostDrawList.AsReadOnlySpan())
            {
                // we send pointer display position
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

        /// <summary>
        /// Invokes PointerTypeChanged action if any, if pointer type has really changed.
        /// </summary>
        /// <remarks>
        /// You can override this method in your own extended UIScreen,
        /// if you want to draw your custom pointer type.
        /// </remarks>
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
