[![Actions Status](https://github.com/kbergius/NanoUI/workflows/Build/badge.svg)](https://github.com/kbergius/NanoUI/Build)
[![NuGet](https://img.shields.io/nuget/v/NanoUI.svg)](https://www.nuget.org/packages/NanoUI)

NanoUI is a small, extendable and quite feature-rich UI library with no external dependencies and no extra assets.

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

![Drawing](docs/screenshots/ui_basic.png)

[See MORE screenshots](docs/screenshots/SCREENSHOTS.md)


## Integrating NanoUI

NanoUI knows nothing about your OS, graphics system and windowing environment. So you must create 2 files/classes, that act as a bridge between your application and NanoUI.

### InputMappings

InputMappings should map user inputs from your windowing environment to format, that NanoUI understands.
This is basically quite simple task to achieve: you just convert keyboard keys, mouse buttons and pointer types to the NanoUI format.

### INvgRenderer

**INvgRenderer** is an interface to your NanoUI renderer implementation. When you init NanoUI, you must pass this implementation to NanoUI.

**INvgRenderer** has 2 basic purposes:
1. Handle texture actions (create, update, delete, etc)
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
- [nanovg](https://github.com/memononen/nanovg)
- [nanogui](https://github.com/wjakob/nanogui)
- [stbtruetype](https://github.com/nothings/stb)

**Samples**:
- [Silk.NET](https://github.com/dotnet/Silk.NET)
- [Veldrid](https://github.com/veldrid/veldrid)
- [StbImageSharp](https://github.com/StbSharp/StbImageSharp)
