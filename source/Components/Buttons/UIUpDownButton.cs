using NanoUI.Common;
using NanoUI.Layouts;
using NanoUI.Nvg;
using System;
using System.Numerics;

namespace NanoUI.Components.Buttons
{
    #region Base

    // note: there is really only dimension property used. Other properties comes from button
    public class UIUpDownButton : UIWidget
    {
        public UIUpDownButton()
        {
            // set defaults to theme impl - prevents circular reference
            Dimension = default;
        }

        protected UIUpDownButton(UIWidget parent)
            : base(parent)
        {
            ThemeType = typeof(UIUpDownButton);
        }

        #region Properties

        uint? _dimension;
        public uint Dimension
        {
            get => _dimension?? GetTheme().UpDownButton.Dimension;
            set => _dimension = value;
        }

        #endregion
    }

    #endregion

    public class UpDownButton<T> : UIUpDownButton where T : INumber<T>
    {
        int _lastState = 0;
        float _lastDelta = 0;

        UIButton _up;
        UIButton _down;

        // informs pushed numeric step value (+Step / -Step)
        public Action<T> ButtonPushed;

        // this is ctor for theme/layout generation (if you use this otherwise, set parent before using widget)
        public UpDownButton()
        {
            // set defaults to theme impl - prevents circular reference

            // todo : this ip not correct with numeric text box
            // should we calculate dynamically from icon scale & font size?
            Dimension = default;

            DisablePointerFocus = true;
            Border = false;
        }

        public UpDownButton(UIWidget parent, Orientation orientation = Orientation.Horizontal)
            : base(parent)
        {
            // generates widgets & layout
            Orientation = orientation;

            DisablePointerFocus = true;
            Border = false;
        }

        #region Properties

        // supports dynamically changing orientation
        Orientation _orientation;
        public Orientation Orientation
        {
            get => _orientation;
            set
            {
                _orientation = value;

                // clear previous
                Children.Clear();

                if (_orientation == Orientation.Vertical)
                {
                    _up = new UIToolButton(this, GetTheme().Fonts.IconCaretUp);
                    _down = new UIToolButton(this, GetTheme().Fonts.IconCaretDown);
                }
                else
                {
                    _down = new UIToolButton(this, GetTheme().Fonts.IconCaretLeft);
                    _up = new UIToolButton(this, GetTheme().Fonts.IconCaretRight);
                }

                // set flags
                _up.Flags = ButtonFlags.NormalButton;
                _down.Flags = ButtonFlags.NormalButton;

                // pass icons extra scale
                // todo: should we calculate dynamically from dimension?
                _up.IconExtraScale = IconExtraScale;
                _down.IconExtraScale = IconExtraScale;

                ChildrenLayout = new StackLayout(_orientation, LayoutAlignment.Fill);

                RequestLayoutUpdate(this);
            }
        }

        // value to increase every push/repeat event
        public T Step { get; set; } = T.One;        

        #endregion

        #region Layout

        public override void PerformLayout(NvgContext ctx)
        {
            FixedSize = new Vector2(0, Dimension);

            // set buttons fixed size
            if (Orientation == Orientation.Horizontal)
            {
                _up.FixedSize = new Vector2(FixedSize.Y);
                _up.IconExtraScale = IconExtraScale;

                _down.FixedSize = new Vector2(FixedSize.Y);
                _down.IconExtraScale = IconExtraScale;
            }
            else
            {
                _up.FixedSize = new Vector2(FixedSize.Y / 2);
                _up.IconExtraScale = IconExtraScale / 2;

                _down.FixedSize = new Vector2(FixedSize.Y / 2);
                _down.IconExtraScale = IconExtraScale / 2;
            }

            base.PerformLayout(ctx);
        }

        #endregion

        #region Drawing

        // todo: we can clear _lastDelta when pointer up?
        public override void Draw(NvgContext ctx)
        {
            base.Draw(ctx);

            // set action
            int curState = _down.Pushed ? -1 : _up.Pushed ? 1 : 0;

            if (curState != 0 && _lastState == curState)
            {
                _lastDelta += Screen.DeltaSeconds;

                if(Globals.UPDATE_DELAY < _lastDelta)
                {
                    T val = curState == 1? Step : -Step;

                    // inform pushed
                    ButtonPushed?.Invoke(val);

                    // reset last delta
                    _lastDelta = 0;
                }
            }
            
            _lastState = curState;
        }

        #endregion
    }
}