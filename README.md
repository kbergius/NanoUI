[![Actions Status](https://github.com/kbergius/NanoUI/workflows/Build/badge.svg)](https://github.com/kbergius/NanoUI/Build)
[![NuGet](https://img.shields.io/nuget/v/NanoUI.svg)](https://www.nuget.org/packages/NanoUI)

NanoUI is a small, extendable and quite feature-rich UI library. It doesn't know anything **where** / **why** / **how** you use it (your OS, graphics, windowing etc). This means that you must create some wrapper classes, that act as a bridge between your application and NanoUI.

## Main features
- Independent drawing layer
- UI layer that handles user inputs, layouting, theming/styling etc
- 50+ extendable UI widgets
- Support for custom widgets
- Dynamic, extendable layouting
- Dynamic, extendable theming/styling
- SVG support
- Docking (barebones)

**... and NanoUI is pretty fast**.

If you are new to NanoUI, please read [Introduction](docs/INTRODUCTION.md).

## Screenshot(s)

![Drawing](docs/screenshots/drawing.png)

[See MORE screenshots](docs/screenshots/SCREENSHOTS.md)

## Credits

**Base library**:
- [nanovg](https://github.com/memononen/nanovg)
- [nanogui](https://github.com/wjakob/nanogui)
- [stbtruetype](https://github.com/nothings/stb)

**Samples**:
- [Silk.NET](https://github.com/dotnet/Silk.NET)
- [Veldrid](https://github.com/veldrid/veldrid)
- [StbImageSharp](https://github.com/StbSharp/StbImageSharp)
