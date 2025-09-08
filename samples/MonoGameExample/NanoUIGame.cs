using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using NanoUI.Nvg;
using NanoUIDemos;
using System;
// Map to NanoUI
using UIKey = NanoUI.Common.Key;
using UIPointerButton = NanoUI.Common.PointerButton;
using UIPointerType = NanoUI.Common.PointerType;
using UIVector2 = System.Numerics.Vector2;

namespace MonoGameExample
{
    public class NanoUIGame : Game
    {
        // DemoTypes:
        // Docking, Drawing, SDFText, SvgShapes, TextShapes, UIBasic, UIExtended, UIExtended2,
        // UIExperimental, UILayouts
        static DemoType _demoType = DemoType.Drawing;

        // MonoGame doesn't "like" unsafe version (maybe the fault is in NanoUI?)
        static bool _useSafeFontManager = true;

        GraphicsDeviceManager _graphics;

        // note: these are set nullables to prevent warnings
        NvgContext? _ctx;
        MGRenderer? _renderer;
        DemoBase? _demo;
        PerfGraph? _perfGraph;

        Color _clearColor = new Color(0.3f, 0.3f, 0.3f);

        public NanoUIGame()
        {
            _graphics = new GraphicsDeviceManager(this)
            {
                PreferredBackBufferWidth = 1200,
                PreferredBackBufferHeight = 700,
                PreferredDepthStencilFormat = DepthFormat.Depth24Stencil8
            };

            Content.RootDirectory = "Content";
            IsMouseVisible = true;
            Window.AllowUserResizing = true;
            IsFixedTimeStep = false;
            _graphics.SynchronizeWithVerticalRetrace = false;            
        }

        protected override void LoadContent()
        {
            Window.Title = $"NanoUI - {_demoType.ToString()}";

            // add window, key & char events
            Window.ClientSizeChanged += OnWindowResized;
            Window.KeyDown += OnKeyDown;
            Window.KeyUp += OnKeyUp;
            Window.TextInput += OnChar;

            // NanoUI stuff

            // todo: way to setup MSAA
            _renderer = new MGRenderer(GraphicsDevice);

            // create nvg context with default (safe/unsafe) font manager (StbTrueType)
            _ctx = new NvgContext(_renderer, _useSafeFontManager, GetDevicePixelRatio());

            // create demo
            _demo = DemoFactory.CreateDemo(_ctx, _demoType, new Vector2(GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height).ToNumerics());

            // wrap clipboard & pointer type events
            if (_demo.Screen != null)
            {
                // set clipboard & cursor callbacks
                _demo.Screen.ClipboardSet = OnClipboardSet;
                _demo.Screen.ClipboardGet = OnClipboardGet;
                // note: this is used when you don't override DrawPointer function in Screen
                _demo.Screen.PointerTypeChanged = OnPointerTypeChanged;
            }

            // create performance graph
            _perfGraph = new PerfGraph(PerfGraph.GraphRenderStyle.Fps, "Frame Time");
        }

        // to set DPI
        float GetDevicePixelRatio()
        {
            float fbSize = GraphicsDevice.DisplayMode.Width;
            float wSize = Window.ClientBounds.Width;

            return fbSize / wSize;
        }

        #region Events

        #region Window

        void OnWindowResized(object? _, EventArgs e)
        {
            // MonoGame handles?

            // pass event to demo screen & all its widgets (if there is screen)
            _demo?.ScreenResize(new UIVector2(GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height), _ctx!);
        }

        #endregion

        #region Keys, Chars

        void OnKeyDown(object? _, InputKeyEventArgs e)
        {
            if(InputMappings.TryGetKey(e.Key, true, out UIKey key))
            {
                _demo?.OnKeyUpDown(key, true, InputMappings.GetKeyModifiers());
            }
        }

        void OnKeyUp(object? _, InputKeyEventArgs e)
        {
            if (InputMappings.TryGetKey(e.Key, false, out UIKey key))
            {
                _demo?.OnKeyUpDown(key, false, InputMappings.GetKeyModifiers());
            }
        }

        void OnChar(object? _, TextInputEventArgs e)
        {
            // todo: we could check here if current widgets accepts chars (OnKeyChar implemented)
            // Possible wrap StartTextInput() & StopTextInput().
            _demo?.OnKeyChar(e.Character);
        }

        #endregion

        #region Mouse

        // todo: MouseDoubleClick

        // we must check if mouse status changed
        Point _previousMouse = Point.Zero;
        // - MouseButtonDownUp (Left, Middle, Right)
        bool[] _mouseDown = new bool[3];
        // - MouseWheel
        const float WHEEL_DELTA = 120;
        UIVector2 _previousMouseScroll = UIVector2.Zero;

        // Order to check:
        // 1. Left button
        // 2. Middle button
        // 3. Right button
        // 4. Mouse wheel
        // 5. Mouse move
        void CheckMouseEvents()
        {
            // get mouse state
            var mouse = Mouse.GetState();

            // get current position
            Point pos = mouse.Position;            

            if (mouse.LeftButton == ButtonState.Pressed && !_mouseDown[0])
            {
                // Left MouseDown
                _mouseDown[0] = true;
                _demo?.OnPointerUpDown(new UIVector2(pos.X, pos.Y), UIPointerButton.Left, true);
            }
            else if (mouse.LeftButton == ButtonState.Released && _mouseDown[0])
            {
                // Left MouseUp
                _mouseDown[0] = false;
                _demo?.OnPointerUpDown(new UIVector2(pos.X, pos.Y), UIPointerButton.Left, false);
            }
            else if (mouse.MiddleButton == ButtonState.Pressed && !_mouseDown[1])
            {
                // Middle MouseDown
                _mouseDown[1] = true;
                _demo?.OnPointerUpDown(new UIVector2(pos.X, pos.Y), UIPointerButton.Middle, true);
            }
            else if (mouse.MiddleButton == ButtonState.Released && _mouseDown[1])
            {
                // Middle MouseUp
                _mouseDown[1] = false;
                _demo?.OnPointerUpDown(new UIVector2(pos.X, pos.Y), UIPointerButton.Middle, false);
            }
            else if (mouse.RightButton == ButtonState.Pressed && !_mouseDown[2])
            {
                // Right MouseDown
                _mouseDown[2] = true;
                _demo?.OnPointerUpDown(new UIVector2(pos.X, pos.Y), UIPointerButton.Right, true);
            }
            else if (mouse.RightButton == ButtonState.Released && _mouseDown[2])
            {
                // Right MouseUp
                _mouseDown[2] = false;
                _demo?.OnPointerUpDown(new UIVector2(pos.X, pos.Y), UIPointerButton.Right, false);
            }
            else
            {
                // get scroll value
                // todo: should we use ints & Point?
                UIVector2 scrollValue = new UIVector2(
                    (mouse.HorizontalScrollWheelValue - _previousMouseScroll.X) / WHEEL_DELTA,
                    (mouse.ScrollWheelValue - _previousMouseScroll.Y) / WHEEL_DELTA);

                if(scrollValue != _previousMouseScroll)
                {
                    // MouseScroll
                    _demo?.OnPointerScroll(new UIVector2(pos.X, pos.Y), scrollValue);

                    // reset
                    _previousMouseScroll = scrollValue;
                }
                else if (_previousMouse.X != pos.X || _previousMouse.Y != pos.Y)
                {
                    // MouseMove
                    var newPos = new UIVector2(pos.X, pos.Y);

                    _demo?.OnPointerMove(newPos, newPos - new UIVector2(_previousMouse.X, _previousMouse.Y));
                }
            }

            // store mouse pos
            _previousMouse = pos;
        }

        #endregion

        #region Clipboard & PointerType

        // I didn't find way to use MonoGame's clipboard functions (if there is any?).
        // So here is a simple way to handle clipboard functions
        string _clipboardText = string.Empty;

        void OnClipboardSet(string text)
        {
            _clipboardText = text;
        }

        string OnClipboardGet()
        {
            return _clipboardText;
        }

        void OnPointerTypeChanged(int pointerType)
        {
            if (pointerType >= 0 && pointerType < Enum.GetValues<UIPointerType>().Length)
            {
                Mouse.SetCursor(InputMappings.GetMouseCursor((UIPointerType)pointerType));
            }
            else
            {
                // user should handle this (this is just a default)
                Mouse.SetCursor(MouseCursor.Arrow);
            }
        }

        #endregion

        #endregion

        protected override void UnloadContent()
        {
            // dispose resources
            _renderer?.Dispose();
            _ctx?.Dispose();

            // MonoGame disposes the rest?
            // base.Dispose();
        }

        float _deltaSeconds = 0;
        protected override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            // Handle mouse events (key/char events are wrapped actions)
            CheckMouseEvents();

            // delta seconds from last update
            _deltaSeconds = (float)gameTime.ElapsedGameTime.TotalSeconds;

            // update demo & perf graph
            _demo?.Update(_deltaSeconds);
            _perfGraph?.Update(_deltaSeconds);
        }

        protected override void Draw(GameTime gameTime)
        {
            // clear
            GraphicsDevice.Clear(_clearColor);

            // draw game?

            base.Draw(gameTime);

            // start NanoUI drawing (overlay)
            _ctx?.BeginFrame();

            // draw ui
            _demo?.Draw(_ctx!);

            // draw performance graph
            _perfGraph?.Draw(15.0f, Window.ClientBounds.Height - 65, _ctx!);

            // trigger NanoUI rendering in MGRenderer Render() method
            _ctx?.EndFrame();
        }
    }
}
