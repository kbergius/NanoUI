using Nano;
using Nano.Graphics;
using NanoUI.Common;
using NanoUI.Nvg;
using NanoUIDemos;
using System;
using System.Numerics;
using static SDL3.SDL;

namespace SDL3Example
{
    public class NanoUIApp : Game
    {
        NvgContext _ctx;
        NanoUIRenderer _renderer;
        DemoBase _demo;
        PerfGraph _frameGraph;

        public NanoUIApp(
            AppInfo appInfo,
            WindowCreateInfo windowCreateInfo,
            FramePacingSettings framePacingSettings,
            DemoType demoType,
            bool debugMode = false)
            : base(
                    appInfo,
                    windowCreateInfo,
                    framePacingSettings,
                    ShaderFormat.SPIRV | ShaderFormat.DXIL | ShaderFormat.MSL | ShaderFormat.DXBC,
                    debugMode)
        {

            ShaderCross.Initialize();

            // window changed callback
            MainWindow.RegisterSizeChangeCallback(OnWindowResized);

            // create renderer
            _renderer = new();
            _renderer.Init(this);

            // create nvg context with default font manager (StbTrueType)
            _ctx = new NvgContext(_renderer, GetDevicePixelRatio());

            // create demo
            _demo = DemoFactory.CreateDemo(_ctx, demoType,
                new Vector2(MainWindow.Width, MainWindow.Height));

            if (_demo.Screen != null)
            {
                // set clipboard & cursor callbacks
                _demo.Screen.ClipboardSet = OnClipboardSet;
                _demo.Screen.ClipboardGet = OnClipboardGet;
                // note: this is used when you don't override DrawPointer function in Screen
                _demo.Screen.PointerTypeChanged = OnPointerTypeChanged;
                _demo.Screen.OnStartTextInput += StartTextInput;
                _demo.Screen.OnStopTextInput += StopTextInput;
            }

            // create perforrmance graph
            _frameGraph = new PerfGraph(PerfGraph.GraphRenderStyle.Fps, "Frame Time", "FIXED");
        }

        #region Input events

        protected override void OnPointerMove(SDL_MouseMotionEvent ev)
        {
            _demo.OnPointerMove(new Vector2(ev.x, ev.y), new Vector2(ev.xrel, ev.yrel));
        }

        protected override void OnPointerUpDown(SDL_MouseButtonEvent ev)
        {
            _demo.OnPointerUpDown(new Vector2(ev.x, ev.y), InputMappings.GetPointerButton(ev.button), ev.down);
        }

        protected override void OnPointerWheel(SDL_MouseWheelEvent ev)
        {
            _demo.OnPointerScroll(new Vector2(ev.mouse_x, ev.mouse_y), new Vector2(ev.x, ev.y));
        }

        protected override void OnKeyUpDown(SDL_KeyboardEvent ev)
        {
            Key key = InputMappings.GetKey(ev.scancode);

            // check that NanoUI supports the key
            if (key != Key.Unknown)
            {
                _demo.OnKeyUpDown(key, ev.down || ev.repeat, InputMappings.GetKeyModifiers(ev.mod));
            }
        }

        protected override void OnKeyChar(char c)
        {
            _demo.OnKeyChar(c);
        }

        #endregion

        protected override void Update(TimeSpan delta)
        {
            // pass update method to demo & perf graph
            float deltaSeconds = (float)delta.TotalSeconds;

            // update demo
            _demo?.Update(deltaSeconds);

            // update performance graph
            _frameGraph?.Update(deltaSeconds);
        }

        protected override void Draw(double alpha)
        {
            _ctx.BeginFrame();

            _demo.Draw(_ctx);

            // Performance graph
            _frameGraph.Draw(15.0f, MainWindow.Height - 65, _ctx);

            // triggers rendering in renderer specified in NvgContext
            _ctx.EndFrame();
        }

        protected override void Destroy()
        {
            _demo.Dispose();

            _renderer?.Dispose();

            _ctx.Dispose();
        }

        void OnWindowResized(uint width, uint height)
        {
            var windowSize = new Vector2(width, height);

            // handles all resource recreation
            _renderer.WindowResize(windowSize);

            _demo?.ScreenResize(windowSize, _ctx);
        }

        float GetDevicePixelRatio()
        {
            return MainWindow.DisplayScale;
        }

        // clipboard set
        static void OnClipboardSet(string text)
        {
            SDL_SetClipboardText(text);
        }

        // clipboard get
        static string OnClipboardGet()
        {
            return SDL_GetClipboardText();
        }

        // pointer changed
        static void OnPointerTypeChanged(int pointerTpe)
        {
            if(pointerTpe >= 0 && pointerTpe < Enum.GetValues<PointerType>().Length)
            {
                // get system cursor type
                var systemCursor = InputMappings.GetSDLSystemCursor((PointerType)pointerTpe);

                // set system cursor type
                SDL_SetCursor(SDL_CreateSystemCursor(systemCursor));
            }
            else
            {
                // custom pointer value - user handles this
            }
        }

        void StartTextInput()
        {
            MainWindow?.StartTextInput();
        }

        void StopTextInput()
        {
            MainWindow?.StopTextInput();
        }
    }
}