using NanoUI.Common;
using NanoUI.Nvg;
using System.Numerics;

namespace NanoUI.Components
{
    // popup class that can be used as triggered from PopupButton ot manually
    // note : there is special handling for RequestFocus
    // note2: popup extends UIWindow that is IScrollable, but popup currently doesn't support scrolling directly
    // (all scrolling - when needed - should be handled in UIScrollPanel that is set as first & only
    // child of the popup)
    // todo: use UIWindow scrolling (& bars) and remove UIScrollPanel as a child where scrolling is needed
    public class UIPopup : UIWindow
    {
        // these are to check if parent has moved and/or size changed
        // if yes - we must recalculate popup position
        Vector2 _parentPosition = new Vector2(-1);
        Vector2 _parentSize = new Vector2(-1);

        UIPopupButton _parentButton;

        // this is ctor for theme/layout generation (if you use this otherwise, set parent before using widget)
        public UIPopup()
        {
            // set defaults to theme impl - prevents circular reference
            // defaults - window related
            DragResizable = false;
            Draggable = false;
            CornerRadius = default;

            // Hide by default
            Visible = false;
        }

        public UIPopup(UIPopupButton parentButton)
            :this(parentButton.Screen)
        {
            _parentButton = parentButton;
        }

        // this is for individual popups (no popupbutton - like context menu)
        // do not create titlebar
        public UIPopup(UIScreen screen)
            : base(screen, null, ScrollbarType.NONE)
        {            
            // defaults
            AnchorPos = Vector2.Zero;
            RelativePosition = PopupPosition.RightMiddle;

            // Hide by default
            Visible = false;

            // Window related
            DragResizable = false;
            Draggable = false;
            CornerRadius = new CornerRadius(0);

            ThemeType = typeof(UIPopup);
        }

        #region Properties

        public virtual Color AnchorBackgroundColor { get; set; }
        public virtual int AnchorSize { get; set; }
        public Vector2 AnchorPos { get; set; }
        public PopupPosition RelativePosition { get; set; }

        #endregion

        #region Methods

        public UIPopupButton? GetParentButton()
        {
            return _parentButton;
        }

        public virtual void Show(bool visible)
        {
            if (Visible == visible)
            {
                // no need to do anything
                return;
            }

            Visible = visible;

            if (visible)
            {
                // bring to topmost
                this.MoveToLast();
            }
            else
            {
                // this loop all children popup buttons & popups
                RecursiveCloseChildPopups(this);
            }
        }

        protected void RecursiveCloseChildPopups(UIPopup popup)
        {
            if (popup == null)
                return;

            // set
            popup.Visible = false;

            // check if there is popupbutton in childs
            foreach (var child in popup.Children.AsReadOnlySpan())
            {
                if (child is UIPopupButton popupButton)
                {
                    popupButton.Pushed = false;

                    RecursiveCloseChildPopups(popupButton.Popup);
                }
            }
        }

        #endregion

        #region Events

        // we override window implementation in order to keep popup open while no widget is responding to
        // OnPointerUpDown + RequestFocus
        // note: widgets in popup has special handling of RequestFocus (Screen.UpdateFocus is not called)
        public override void RequestFocus()
        {
            // we got new focus - update in screen
            if (Focused)
            {
                base.RequestFocus();
            }
            else
            {
                // no widget is responding to OnPointerUpDown, but pointer click is still inside popup
                // -> do nothing (no close etc)
            }
        }

        // this is a special case when popup button forwards event here (support for shortcuts in menus etc)
        // note: normally widget must be in focus path to receive OnKeyUpDown event
        // note2: OnKeyChar event still needs menu item to be in focuspath, so editing menu item (button) values in popup
        // doesn't work since clicking menu item closes popup that removes menu item from focuspath
        // todo: this works now with menus. are there any downsides?
        // todo2: we could still restrict checking that _parentButton is MenuButton
        public override bool OnKeyUpDown(Key key, bool down, KeyModifiers modifiers)
        {
            if(_parentButton != null)
            {
                foreach (var child in Children.AsReadOnlySpan())
                {
                    // check if child is "valid" & handles the event
                    if (!child.Visible || child.Disabled)
                        continue;

                    if(child.OnKeyUpDown(key, down, modifiers))
                    {
                        return true;
                    }
                }
            }

            // this returns false
            return base.OnKeyUpDown(key, down, modifiers);
        }

        #endregion

        #region Layout

        // Invoke the associated layout generator to properly place child widgets, if any
        public override void PerformLayout(NvgContext ctx)
        {
            if (ChildrenLayout != null || Children.Count != 1)
            {
                base.PerformLayout(ctx);
            }
            else
            {
                // children occupies whole popup window
                Children[0].Position = Vector2.Zero;
                Children[0].Size = Size;
                Children[0].PerformLayout(ctx);
            }
        }

        #endregion

        #region Drawing

        public override void Draw(NvgContext ctx)
        {
            if (!Visible)
                return;

            // check if parent popup button is moved, resized or "scrolled"
            RefreshPlacement();

            // Draw anchor
            if(AnchorSize > 0)
            {
                ctx.SaveState();

                ctx.ResetScissor();

                ctx.BeginPath();
                ctx.RoundedRectVarying(Position, Size, CornerRadius);
                                
                if (RelativePosition == PopupPosition.RightMiddle || RelativePosition == PopupPosition.RightTop)
                {
                    ctx.MoveTo(AnchorPos.X, AnchorPos.Y);
                    ctx.LineTo(AnchorPos.X + AnchorSize, AnchorPos.Y - AnchorSize);
                    ctx.LineTo(AnchorPos.X + AnchorSize, AnchorPos.Y + AnchorSize);
                }
                else if (RelativePosition == PopupPosition.LeftMiddle || RelativePosition == PopupPosition.LeftTop)
                {
                    ctx.MoveTo(AnchorPos.X + AnchorSize, AnchorPos.Y);
                    ctx.LineTo(AnchorPos.X, AnchorPos.Y - AnchorSize);
                    ctx.LineTo(AnchorPos.X, AnchorPos.Y + AnchorSize);
                }
                else
                {
                    // bottom
                    ctx.MoveTo(AnchorPos.X, AnchorPos.Y);
                    ctx.LineTo(AnchorPos.X - AnchorSize, AnchorPos.Y + AnchorSize);
                    ctx.LineTo(AnchorPos.X + AnchorSize, AnchorPos.Y + AnchorSize);
                }

                ctx.FillColor(AnchorBackgroundColor);
                ctx.Fill();

                ctx.RestoreState();
            }

            // Draw window & content
            // todo : check window drawing functions
            base.Draw(ctx);
        }

        #endregion

        #region Private

        // we must call this in draw method, since there is no way to determine if parent popup button
        // has moved, resized or "scrolled".
        // note: there could be action in UIPopupButton that fires when changes has happened, but then
        // button must somehow get info for these changes. So this is simpler and more robust way to handle changes.
        void RefreshPlacement()
        {
            // we are directly on screen or context menu - no need to recalculate position?
            if (_parentButton == null)
                return;

            // check if parent button position & size changed or it is in scrollable & scroll offset changed
            var parentScreenPos = _parentButton.GetDisplayPosition();

            // no changes?
            if (_parentPosition == parentScreenPos && _parentSize == _parentButton.Size)
                return;

            // set new values
            _parentPosition = parentScreenPos;
            _parentSize = _parentButton.Size;

            // set position & anchor position
            if (RelativePosition == PopupPosition.LeftMiddle)
            {
                Position = parentScreenPos +
                    new Vector2(-Size.X - AnchorSize, (_parentButton.Size.Y - Size.Y) / 2);
                AnchorPos = parentScreenPos +
                    new Vector2(-AnchorSize, _parentButton.Size.Y / 2);
            }
            else if (RelativePosition == PopupPosition.LeftTop)
            {
                Position = parentScreenPos + new Vector2(-Size.X - AnchorSize, 0);
                AnchorPos = parentScreenPos +
                    new Vector2(-AnchorSize, _parentButton.Size.Y / 2);
            }
            else if (RelativePosition == PopupPosition.RightMiddle)
            {
                Position = parentScreenPos +
                    new Vector2(_parentButton.Size.X + AnchorSize, (_parentButton.Size.Y - Size.Y) / 2);
                AnchorPos = parentScreenPos +
                    new Vector2(_parentButton.Size.X, _parentButton.Size.Y / 2);
            }
            else if (RelativePosition == PopupPosition.RightTop)
            {
                // menus etc
                Position = parentScreenPos +
                    new Vector2(_parentButton.Size.X + AnchorSize, 0);
                AnchorPos = parentScreenPos +
                    new Vector2(_parentButton.Size.X, _parentButton.Size.Y / 2);
            }
            else
            {
                // this is popup side bottom
                Position = parentScreenPos + new Vector2(0, _parentButton.Size.Y);

                if (AnchorSize > 0)
                {
                    AnchorPos = parentScreenPos +
                        new Vector2(_parentButton.Size.X / 2, _parentButton.Size.Y);
                }
            }
        }

        #endregion
    }
}