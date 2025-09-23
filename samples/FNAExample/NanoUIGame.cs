using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using NanoUI.Nvg;
using NanoUIDemos;
using System;
using System.Collections.Generic;
using System.Linq;
using static SDL3.SDL;
// Map to NanoUI
using UIKey = NanoUI.Common.Key;
using UIPointerButton = NanoUI.Common.PointerButton;
using UIPointerType = NanoUI.Common.PointerType;
using UIVector2 = System.Numerics.Vector2;

namespace FNAExample
{
    // note: I had to add NPE hack in the beginning of the SDL3_FNAPlatform.PollEvents method
    // if(textInputControlDown == null)
	// {
	//    textInputControlDown = new bool[7];
	// }
    // There could better solution.
    public class NanoUIGame : Game
    {
        // DemoTypes:
        // Docking, Drawing, SDFText, SvgShapes, TextShapes, UIBasic, UIExtended, UIExtended2,
        // UIExperimental, UILayouts
        static DemoType _demoType = DemoType.Drawing;

        // FNA doesn't "like" unsafe version
        static bool _useSafeFontManager = true;

        GraphicsDeviceManager _graphics;

        // these are set nullables to prevent warnings
        NvgContext? _ctx;
        FNARenderer? _renderer;
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
            // todo: way to setup MSAA?
            _renderer = new FNARenderer(GraphicsDevice);

            // create nvg context with default (safe/unsafe) font manager (StbTrueType)
            _ctx = new NvgContext(_renderer, _useSafeFontManager, GetDevicePixelRatio());

            // create demo
            _demo = DemoFactory.CreateDemo(_ctx, _demoType, new UIVector2(GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height));

            // wrap clipboard & pointer type events
            if (_demo.Screen != null)
            {
                // set clipboard & cursor callbacks
                _demo.Screen.ClipboardSet = OnClipboardSet;
                _demo.Screen.ClipboardGet = OnClipboardGet;
                // note: this is used when you don't override DrawPointer function in Screen
                _demo.Screen.PointerTypeChanged = OnPointerTypeChanged;
                // wrap text input
                _demo.Screen.OnStartTextInput += StartTextInput;
                _demo.Screen.OnStopTextInput += StopTextInput;
            }

            // create performance graph
            _perfGraph = new PerfGraph(PerfGraph.GraphRenderStyle.Fps, "Frame Time");

            Window.Title = $"NanoUI - {_demoType.ToString()}";

            // add window, key & char events
            Window.ClientSizeChanged += OnWindowResized;
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

        void OnWindowResized(object? sender, EventArgs e)
        {
            // FNA handles?

            // pass event to demo screen & all its widgets (if there is screen)
            _demo?.ScreenResize(new UIVector2(GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height), _ctx!);
        }

        #endregion

        #region Keys

        HashSet<Keys> _handledKeys = new HashSet<Keys>();

        // old & new pressed keys
        Keys[] _pressedKeys = Array.Empty<Keys>();
        Keys[] _newpressedKeys = Array.Empty<Keys>();

        void CheckKeyboard(in KeyboardState keyboardState)
        {
            // clear handled
            _handledKeys.Clear();

            // get pressed keys
            _newpressedKeys = keyboardState.GetPressedKeys();

            // loop old presssed keys
            foreach(Keys oldKey in _pressedKeys)
            {
                // check if same status
                if (_newpressedKeys.Contains(oldKey))
                {
                    _handledKeys.Add(oldKey);
                }
                else
                {
                    // key up
                    OnKeyUp(oldKey);
                }
            }

            // loop new presssed keys
            foreach (Keys newKey in _newpressedKeys)
            {
                // check if handled
                if (_handledKeys.Contains(newKey))
                {
                    // do nothing
                }
                else
                {
                    // key down
                    OnKeyDown(newKey);
                }
            }

            // store _newpressedKeys
            _pressedKeys = _newpressedKeys;
        }

        void OnKeyDown(Keys key)
        {
            if(InputMappings.TryGetKey(key, true, out UIKey uiKey))
            {
                _demo?.OnKeyUpDown(uiKey, true, InputMappings.GetKeyModifiers());
            }
        }

        void OnKeyUp(Keys key)
        {
            if (InputMappings.TryGetKey(key, false, out UIKey uiKey))
            {
                _demo?.OnKeyUpDown(uiKey, false, InputMappings.GetKeyModifiers());
            }
        }

        #endregion

        #region TextInput

        // flag to indicate if we use TextInput or Keys
        bool _textInputActive = false;

        // map chars since we must send these with OnKeyUpDown
        // this is from FNAPlatform, since it is internal
        readonly Dictionary<char, Keys> _textInputCharacters = new Dictionary<char, Keys>
        {
            {(char) 2, Keys.Home },	// Home
			{(char) 3, Keys.End },	// End
			{(char) 8, Keys.Back },	// Backspace
			{(char) 9, Keys.Tab },	// Tab
			{(char) 13, Keys.Enter },	// Enter
			{(char) 127, Keys.Delete },	// Delete
			{(char) 22, Keys.None }	// Ctrl+V (Paste)
        };

        void StartTextInput()
        {
            // start SDL's text input state
            TextInputEXT.TextInput += OnChar;
            TextInputEXT.StartTextInput();

            _textInputActive = true;
        }

        void StopTextInput()
        {
            // stop SDL's text input state
            TextInputEXT.StopTextInput();
            TextInputEXT.TextInput -= OnChar;

            _textInputActive = false;
        }

        void OnChar(char c)
        {
            // must check special chars - special handling
            if (_textInputCharacters.TryGetValue(c, out Keys key))
            {
                // special case for Ctrl+V (Paste)
                if(key == Keys.None)
                {
                    // no-op handled in Update
                }
                else
                {
                    OnKeyDown(key);
                }
            }
            else
            {
                // send char
                _demo?.OnKeyChar(c);
            }
        }

        #endregion

        #region Mouse

        // todo: MouseDoubleClick

        // we must check if mouse status changed
        int _previousMouseX = 0;
        int _previousMouseY = 0;
        // - MouseButtonDownUp (Left, Middle, Right)
        bool[] _mouseDown = new bool[3];
        // - MouseWheel
        int _previousMouseScroll = 0;

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
            int mouseX = mouse.X;
            int mouseY = mouse.Y;

            if (mouse.LeftButton == ButtonState.Pressed && !_mouseDown[0])
            {
                // Left MouseDown
                _mouseDown[0] = true;
                _demo?.OnPointerUpDown(new UIVector2(mouseX, mouseY), UIPointerButton.Left, true);
            }
            else if (mouse.LeftButton == ButtonState.Released && _mouseDown[0])
            {
                // Left MouseUp
                _mouseDown[0] = false;
                _demo?.OnPointerUpDown(new UIVector2(mouseX, mouseY), UIPointerButton.Left, false);
            }
            else if (mouse.MiddleButton == ButtonState.Pressed && !_mouseDown[1])
            {
                // Middle MouseDown
                _mouseDown[1] = true;
                _demo?.OnPointerUpDown(new UIVector2(mouseX, mouseY), UIPointerButton.Middle, true);
            }
            else if (mouse.MiddleButton == ButtonState.Released && _mouseDown[1])
            {
                // Middle MouseUp
                _mouseDown[1] = false;
                _demo?.OnPointerUpDown(new UIVector2(mouseX, mouseY), UIPointerButton.Middle, false);
            }
            else if (mouse.RightButton == ButtonState.Pressed && !_mouseDown[2])
            {
                // Right MouseDown
                _mouseDown[2] = true;
                _demo?.OnPointerUpDown(new UIVector2(mouseX, mouseY), UIPointerButton.Right, true);
            }
            else if (mouse.RightButton == ButtonState.Released && _mouseDown[2])
            {
                // Right MouseUp
                _mouseDown[2] = false;
                _demo?.OnPointerUpDown(new UIVector2(mouseX, mouseY), UIPointerButton.Right, false);
            }
            else
            {
                // get scroll value
                int scrollValue = mouse.ScrollWheelValue;
                
                if (scrollValue != _previousMouseScroll)
                {
                    // MouseScroll, only Y
                    _demo?.OnPointerScroll(new UIVector2(mouseX, mouseY), new UIVector2(0, scrollValue));

                    // store
                    _previousMouseScroll = scrollValue;
                }
                else if (_previousMouseX != mouseX || _previousMouseY != mouseY)
                {
                    // MouseMove
                    var newPos = new UIVector2(mouseX, mouseY);

                    _demo?.OnPointerMove(newPos, newPos - new UIVector2(_previousMouseX, _previousMouseY));
                }
            }

            // store mouse pos
            _previousMouseX = mouseX;
            _previousMouseY = mouseY;
        }

        #endregion

        #region Clipboard & PointerType

        void OnClipboardSet(string text)
        {
            SDL_SetClipboardText(text);
        }

        string OnClipboardGet()
        {
            return SDL_GetClipboardText();
        }

        void OnPointerTypeChanged(int pointerType)
        {
            if (pointerType >= 0 && pointerType < Enum.GetValues<UIPointerType>().Length)
            {
                // get system cursor type
                var systemCursor = InputMappings.GetMouseCursor((UIPointerType)pointerType);

                // set system cursor type
                SDL_SetCursor(SDL_CreateSystemCursor(systemCursor));
            }
            else
            {
                // custom pointer value - user handles this
            }
        }

        #endregion

        #endregion

        protected override void UnloadContent()
        {
            // dispose resources
            _renderer?.Dispose();
            _ctx?.Dispose();

            // FNA disposes the rest?
            // base.Dispose();
        }

        float _deltaSeconds = 0;
        protected override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            // Handle mouse events
            CheckMouseEvents();

            // get keyboard state
            var keyboardState = Keyboard.GetState();

            // check text input state and support for Ctrl+X, Ctrl+C and Ctrl+V in editable texts
            // todo: support for navigational keys etc
            if (!_textInputActive ||
                keyboardState.IsKeyDown(Keys.LeftControl) ||
                keyboardState.IsKeyDown(Keys.RightControl))
            {
                CheckKeyboard(keyboardState);
            }

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
