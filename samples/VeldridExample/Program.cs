using NanoUI.Nvg;
using NanoUIDemos;
using Silk.NET.Input;
using Silk.NET.Maths;
using Silk.NET.Windowing;
using System.Numerics;
using Key = Silk.NET.Input.Key;
using MouseButton = Silk.NET.Input.MouseButton;
using VeldridExample;

public class Program
{
    static IWindow _window;
    static IInputContext _input;
    static NvgContext _ctx;
    static VeldridRenderer _renderer;

    static DemoBase _demo;
    
    // DemoTypes:
    // Docking, Drawing, SDFText, SvgShapes, TextShapes, UIBasic, UIExtended, UIExtended2,
    // UIExperimental, UILayouts
    static DemoType _demoType = DemoType.UIBasic;

    static PerfGraph _frameGraph;

    // Multisample anti-aliasing
    static bool _msaa = true;

    public static void Main()
    {
        WindowOptions windowOptions = WindowOptions.Default;
        windowOptions.FramesPerSecond = -1;
        windowOptions.Size = new Vector2D<int>(1200, 700);
        windowOptions.Title = $"NanoUI - {_demoType.ToString()}";
        windowOptions.IsContextControlDisabled = true;
        windowOptions.VSync = false;
        windowOptions.PreferredBitDepth = new Vector4D<int>(32);
        windowOptions.PreferredDepthBufferBits = 24;
        windowOptions.PreferredStencilBufferBits = 8;
        windowOptions.API = GraphicsAPI.Default;

        GraphicsAPI tempVer = GraphicsAPI.Default;
        tempVer.Version = new APIVersion(4, 0);
        windowOptions.API = tempVer;

        _window = Window.Create(windowOptions);

        _window.Load += Load;
        _window.FramebufferResize += OnFramebufferResize;
        _window.Update += OnUpdate;
        _window.Render += Render;
        _window.Closing += Dispose;

        _window.Run();

        _window.Dispose();
    }

    #region Window Events

    static void Load()
    {
        _input = _window.CreateInput();

        foreach (IKeyboard keyboard in _input.Keyboards)
        {
            keyboard.KeyDown += OnKeyDown;
            keyboard.KeyUp += OnKeyUp;
            keyboard.KeyChar += OnKeyChar;
            break;
        }
        
        foreach (IMouse mouse in _input.Mice)
        {
            //mouse.Click
            mouse.DoubleClick += OnMouseDoubleClick;
            mouse.MouseDown += OnMouseDown;
            mouse.MouseMove += OnMouseMove;
            mouse.MouseUp += OnMouseUp;
            mouse.Scroll += OnMouseScroll;
            break;
        }

        LoadExample();
    }

    static void LoadExample()
    {
        _renderer = new(_window, _msaa);

        // create nvg context width default font manager (StbTrueType)
        _ctx = new NvgContext(_renderer, GetDevicePixelRatio());

        _demo = DemoFactory.CreateDemo(_ctx, _demoType, new Vector2(_window.Size.X, _window.Size.Y));
        
        if(_demo.Screen != null)
        {
            // set clipboard & cursor callbacks
            _demo.Screen.ClipboardSet = OnClipboardSet;
            _demo.Screen.ClipboardGet = OnClipboardGet;
            // note: this is used when you don't override DrawPointer function in Screen
            _demo.Screen.PointerTypeChanged = OnPointerTypeChanged;
        }

        // Perforrmance graph
        _frameGraph = new PerfGraph(PerfGraph.GraphRenderStyle.Fps, "Frame Time");
    }

    // The device pixel ratio is the ratio between physical pixels and logical pixels.
    static float GetDevicePixelRatio()
    {
        Vector2D<float> wSize = _window.Size.As<float>();
        Vector2D<float> fbSize = _window.FramebufferSize.As<float>();

        return fbSize.X / wSize.X;
    }

    static void OnFramebufferResize(Vector2D<int> size)
    {
        var windowSize = new Vector2(size.X, size.Y);

        // handles all resource recreation
        _renderer.WindowResize(windowSize);

        // pass event to demo screen & all its widgets (if there is screen)
        _demo.ScreenResize(windowSize, _ctx);
    }

    // this supports repeated down key events (KeyDown is only fired once)
    // used primarily with non-char keys: Backspace, Delete
    static double _cumulative = 0;
    static void OnUpdate(double deltaSeconds)
    {
        _demo?.Update((float)deltaSeconds);

        // Performance graph
        _frameGraph.Update((float)deltaSeconds);

        // increase cumulative time value
        _cumulative += deltaSeconds;

        if(_cumulative > 0.1)
        {
            _cumulative = 0;

            // check repeat key (in OnKeyDown, OnKeyUp events _repeatKey is modified)
            if (_repeatKey.HasValue)
            {
                OnKeyDown(_input.Keyboards[0], _repeatKey.Value, 0);
            }
        }
    }

    static void Render(double _)
    {
        _ctx.BeginFrame();

        _demo.Draw(_ctx);

        // Performance graph
        _frameGraph.Draw(15.0f, _window.Size.Y - 65, _ctx);

        // triggers rendering in renderer specified in NvgContext
        _ctx.EndFrame();
    }

    static void Dispose()
    {
        _demo.Dispose();
        
        _renderer?.Dispose();

        _ctx.Dispose();
    }

    #endregion

    #region Input Events

    // support for repeated key down - used primarily with non-char keys: Backspace, Delete
    // todo: we could also have navigation keys (right, left, up, down) as repeat keys
    static Key? _repeatKey;
    static Vector2 _previousMousePosition = Vector2.Zero;
    static Vector2 _mousePosition = Vector2.Zero;
    static void OnKeyDown(IKeyboard _, Key key, int _2)
    {
        if (key == Key.Escape)
        {
            _window.Close();
            return;
        }

        if (InputMappings.TryMapKey(key, true, out var uiKey, out bool isRepeat))
        {
            _demo.OnKeyUpDown(uiKey, true, InputMappings.KeyModifiers);

            _repeatKey = isRepeat ? key : null;
        }
    }

    static void OnKeyUp(IKeyboard _, Key key, int _2)
    {
        if (InputMappings.TryMapKey(key, false, out var uiKey, out bool isRepeat))
        {
            _demo.OnKeyUpDown(uiKey, false, InputMappings.KeyModifiers);

            if (isRepeat)
            {
                _repeatKey = null;
            }
        }        
    }

    static void OnKeyChar(IKeyboard _, char c)
    {
        _demo.OnKeyChar(c);
    }

    static void OnMouseDown(IMouse _, MouseButton mouseButton)
    {
        _demo.OnPointerUpDown(_mousePosition, InputMappings.MapMouseButtons(mouseButton), true);
    }

    static void OnMouseMove(IMouse _, Vector2 mousePosition)
    {
        _mousePosition = mousePosition;

        _demo.OnPointerMove(_mousePosition, _mousePosition - _previousMousePosition);

        _previousMousePosition = _mousePosition;
    }

    static void OnMouseUp(IMouse _, MouseButton mouseButton)
    {
        _demo.OnPointerUpDown(_mousePosition, InputMappings.MapMouseButtons(mouseButton), false);
    }

    static void OnMouseDoubleClick(IMouse _, MouseButton mouseButton, Vector2 pos)
    {
        _demo.OnPointerDoubleClick(pos, InputMappings.MapMouseButtons(mouseButton));
    }

    // Scroll is Vector2 in order to support trackballs
    // note: hardly any core widget uses scroll.X value
    static void OnMouseScroll(IMouse _, ScrollWheel scrollWheel)
    {
        _demo.OnPointerScroll(_mousePosition, new Vector2(scrollWheel.X, scrollWheel.Y));
    }

    #endregion

    #region Clipboard & Pointer type Events

    static void OnClipboardSet(string text)
    {
        _input.Keyboards[0].ClipboardText = text;
    }

    static string OnClipboardGet()
    {
        return _input.Keyboards[0].ClipboardText;
    }

    static void OnPointerTypeChanged(int pointerTpe)
    {
        var standardCursor = InputMappings.GetCursorType(pointerTpe, out CursorType cursorType);

        if(cursorType == CursorType.Standard)
        {
            _input.Mice[0].Cursor.Type = cursorType;
            _input.Mice[0].Cursor.StandardCursor = standardCursor;
        }
        else
        {
            // cursorType is custom

            // note: you should impelement your own logic when cursor type is custom

            // this uses the default
            _input.Mice[0].Cursor.Type = CursorType.Standard;
            _input.Mice[0].Cursor.StandardCursor = StandardCursor.Default;
        }
    }

    #endregion
}