[![Actions Status](https://github.com/kbergius/NanoUI/workflows/Build/badge.svg)](https://github.com/kbergius/NanoUI/Build)
[![NuGet](https://img.shields.io/nuget/v/NanoUI.svg)](https://www.nuget.org/packages/NanoUI)
[![Nuget](https://img.shields.io/nuget/dt/NanoUI)](https://www.nuget.org/packages/NanoUI/)

NanoUI is a small, extendable and quite feature-rich UI & drawing library with no external dependencies, native libraries and no extra assets.

## Main features
- Independent drawing layer
- UI layer that handles user inputs, layouting, theming/styling etc
- 50+ extendable UI widgets
- Support for custom widgets
- Dynamic, extendable layouting
- Dynamic, extendable theming/styling
- Text shapes with custom fills
- SVG support
- Docking (barebones)

**... and NanoUI is pretty fast**.


## Screenshot(s)

![Drawing](docs/screenshots/drawing.png)
![UI_Basic](docs/screenshots/ui_basic.png)

[See MORE screenshots](docs/screenshots/SCREENSHOTS.md)


## How to setup NanoUI

### 1. Create INvgRenderer

NanoUI knows nothing about your OS, graphics system and windowing environment.

In the **samples** folder there are ready-made renderers for **MonoGame** and **Veldrid** - you can use them.

If your platform is different, you must create your own **INvgRenderer** implementation.

**Note:** If your platform already has a [Dear ImGui](https://github.com/ocornut/imgui) renderer, **NanoUI** renderer will be quite similar.


### 2. Init NvgContext

**NvgContext** is the real engine of the NanoUI.

You must init and store it at your application's startup. You will be using it a lot.

```cs
var ctx = new NvgContext(<your renderer>, <use safe/unsafe font manager>, <your display's dpi scale>);
```

**Note:** If you are using user inputs (keys, mouse buttons, pointer types), you should map them to the format that NanoUI understands.


### 3. Modify your main Draw/Render method

At the end of this method add code:

```cs
var ctx = NvgContext.Instance;

// clear NanoUI buffers
ctx.BeginFrame();

// issue draw commands
<your NanoUI drawing code>

// trigger NanoUI rendering
ctx.EndFrame();
```

This will render UI as an overlay.

**Hint:** You can also put **BeginFrame()** call at the beginning of your main draw/render method. So you can for example display your orcs' healthbars using **immediate mode** drawing and setting scale based on orc's distance to camera.

## How to use NanoUI

NanoUI has both **immediate** and **retained** mode drawing. You can use either of these - or both.

### Immediate mode

You can issue draw commands directly to the **NvgContext** (drawing layer API).

For example this will draw rectangle at the position (100, 100) with size (120, 30) and fill it with blue color:

```cs
var ctx = NvgContext.Instance;

ctx.BeginPath();
ctx.Rect(100, 100, 120, 30);
ctx.FillColor(Color.Blue);
ctx.Fill();
```

**NvgContext** can draw shapes, images, texts, text shapes and SVGs with different colors & brushes and it can handle transforms (translate, rotate, scale, skew) & scissoring.

The **NvgContext** API is modeled loosely after the HTML5 canvas API. So if you have web dev experience, you're up to speed with NanoUI in no time.

The limitation of the immediate mode drawing is, that it is purely drawing; so no built-in actions/callbacks, dynamic layouting & styling etc.


### Retained mode

The retained mode (UI layer) has the bells & whistles of the modern UI. It is basically tree of the ui widgets, where the root widget (owner) of the tree is **UIScreen**. So your first task is to create **UIScreen** object:

```cs

var ctx = NvgContext.Instance;

// load fonts
int normal_font_id = ctx.CreateFont("Normal", <your normal font path>);
int icons_font_id = ctx.CreateFont("Icons", <your icons font path>);

// create FontsStyle
var fonts = new FontsStyle()
{
    DefaultFontType = "Normal",
    DefaultIconsType = "Icons",
};

fonts.FontTypes.Add("Normal", normal_font_id);
fonts.FontTypes.Add("Icons", icons_font_id);

// create your theme (it is like CSS file in the web; you can create also your own theme)
var theme = UITheme.CreateDefault<UITheme>(ctx, fonts);

// finally create screen
var screen = new UIScreen(theme, <your windowSize>);

// store this screen to the place, where you can easily access it,
// since you are going to issue user inputs and update & draw methods to it
// and it will handle rest.
```

After this you can add, remove, modify any widget (based on the **UIWidget** class) in the **UIScreen's** widget tree both in the initializing and running mode.

You can also modify dynamically at runtime layouts and theme properties (for example you can change the theme in the **UIScreen** and all the widgets then use this new theme). You can also have many predefined **UIScreens** (with their own widget sets) and change them whenever you want.

Every widget has it's own ctor method, but basically they are like **new UIWidget('widget's parent widget', 'widget params if any')**. The parent widget must be defined, if the widget is going to be added to the **UIScreen's** widget tree.

There are plenty of examples, how to create/use widgets in the **samples/NanoUIDemos/UI** folder.

There is also more information in the [BASIC CONCEPTS](docs/BASICCONCEPTS.md) document.


## Examples

There are examples, that use [MonoGame](https://monogame.net/) or [Veldrid](https://github.com/veldrid/veldrid) backend.

There are also sample shaders in GLSL and HLSL format in the **samples/NanoUIDemos/Assets/shaders** folder.


## Credits

**Base library**:
- [NanoVG](https://github.com/memononen/nanovg)
- [NanoGUI](https://github.com/wjakob/nanogui)
- [StbTruetype](https://github.com/nothings/stb)

**Samples**:
- [Silk.NET](https://github.com/dotnet/Silk.NET)
- [Veldrid](https://github.com/veldrid/veldrid)
- [MonoGame](https://monogame.net/)
- [StbImageSharp](https://github.com/StbSharp/StbImageSharp)
