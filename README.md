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

If you are new to NanoUI, please read [INTRODUCTION](docs/INTRODUCTION.md).


## Screenshot(s)

![UI_Basic_](docs/screenshots/ui_basic.png)
![Drawing](docs/screenshots/drawing.png)

[See MORE screenshots](docs/screenshots/SCREENSHOTS.md)

## How to use NanoUI

NanoUI has both **immediate** and **retained** mode drawing. You can use either of these - or both.

### Immediate mode

You can issue draw commands directly to the **NvgContext** class (drawing layer API).

For example this will draw rectangle at position (100, 100) with size (120, 30) and fill it with blue color:

```C
var ctx = NvgContext.Instance;

ctx.BeginPath();
ctx.Rect(100, 100, 120, 30);
ctx.FillColor(Color.Blue);
ctx.Fill();
```

**NvgContext** can draw shapes, images, texts, text shapes and SVGs with different brushes and it can handle transforms (translate, rotate, scale, skew) & scissoring.

The **NvgContext** API is modeled loosely on the HTML5 canvas API. So if you have web dev experience, you're up to speed with NanoUI in no time. The limitation of the immediate mode drawing is, that is purely drawing; so no actions/callbacks, dynamic layouting & styling etc.


### Retained mode

The retained mode (UI layer) has the bells & whistles of the modern UI. It is basically tree of the ui widgets, where the root widget (owner) of the tree is **UIScreen**: So you first task is to create **UIScreen** object and then add any widget - you like - to it:

```C

var ctx = NvgContext.Instance;

// load fonts
int normal_font_id = ctx.CreateFont("Normal", <your normal font path>);
int icons_font_id = ctx.CreateFont("Icons", <your icons font path>);

// create FontsStyle
FontsStyle fonts = new FontsStyle()
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
since you are going to issue user inputs and update & draw method to it
and it will handle rest.

```

After this you can add, remove, modify any widget (based on the **UIWidget** class) in the **UIScreen's** widget tree both in the initializing and running mode.

You can also modify dynamically at runtime layouts and theme properties (for example you can change the theme class in the **UIScreen** and all the widgets then use this new theme). You can also have many predefined **UIScreen's** (with their own widget sets) and change them whenever you want.

Every widget has it's own ctor method, but basically they are like **new UIWidget('widget's parent widget', 'widget params if any')**. The parent widget must be defined, if the widget is going to be added to the **UIScreen's** widget tree.

There is plenty of examples, how to create/use widgets in the **samples/NanoUIDemos/UI** folder.

**Note:** All the drawing commands must be executed in your rendering cycle between **ctx.BeginFrame()** and **ctx.EndFrame()** calls.


## Integrating NanoUI

NanoUI knows nothing about your OS, graphics system and windowing environment. So you must create 2 files/classes, that act as a bridge between your application and NanoUI.

### InputMappings

InputMappings should map user inputs from your windowing environment to format, that NanoUI understands.
This is basically quite simple task to achieve: you just convert keyboard keys, mouse buttons and pointer types to the NanoUI format.

### INvgRenderer

**INvgRenderer** is an interface to your NanoUI renderer implementation. When you init NanoUI, you must pass this implementation to NanoUI.

**INvgRenderer** has 2 basic purposes:
1. Handle texture actions (create, update, delete, resize etc)
2. Do the real rendering (it is called when you issue **EndFrame** command)

**Note:** NanoUI treates all textures as ints. All negative values and 0 are treated as there is no texture.

Rendering is bit more complicated since NanoUI uses 3 different kind of pipelines:
- **Standard:** This is normal/basic alpha blend draw pipeline
- **FillStencil:** This just fills stencil buffer with values (no drawing here)
- **Fill:** This uses stencil buffer as a mask and really draws the fills

This way NanoUI can handle about any shape; not just predefined primitives.

### Example

There is an example with [Veldrid](https://github.com/veldrid/veldrid) backend, that can give you hints, when you create your implementation.

There are also sample shaders in GLSL and HLSL format in **samples/NanoUIDemos/Assets/shaders**.


## Credits

**Base library**:
- [NanoVG](https://github.com/memononen/nanovg)
- [NanoGUI](https://github.com/wjakob/nanogui)
- [StbTruetype](https://github.com/nothings/stb)

**Samples**:
- [Silk.NET](https://github.com/dotnet/Silk.NET)
- [Veldrid](https://github.com/veldrid/veldrid)
- [StbImageSharp](https://github.com/StbSharp/StbImageSharp)
