### Note: this repository is under construction. See you later after couple of days!

NanoUI is a library (not an out-of-box solution) and it doesn't know anything where/why/how you use it (OS, graphics, windowing etc).

This means that you must create some wrapper classes, that act as a "bridge" between your application and NanoUI. You can check what is needed from the samples.

## Main features:
- independent drawing layer
- UI layer that handles user inputs, layouting, theming/styling etc
- 50+ extendable ui widgets
- support for custom widgets
- dynamic, extendable layouting
- dynamic, extendable theming/styling
- 4 ways to render texts
- SVG support
- Docking (barebones)

See "Screenshots" for more info what NanoUI can do.

If you are new to NanoUI, please read "Basics" to better understand, how NanoUI is designed & working.

## Contributing

All bug reports, feature requests & PRs are of course very welcome.

However NanoUI promises to be "simplistic" and it doesn't accept any external dependencies or extra assets. So your submission may be rejected for these reasons. In these cases you can create your own repository and it can be linked here, if it is deemed to be useful for other users.

Please, read "Contributing" document before you send any PRs.

## Links

## Credits

**Base library**:
- [nanovg](https://github.com/memononen/nanovg)
- [nanogui](https://github.com/wjakob/nanogui)
- [stbtruetype](https://github.com/nothings/stb)

**Samples**:
- [MoonWorks](https://github.com/MoonsideGames/MoonWorks)
- [Silk.NET](https://github.com/dotnet/Silk.NET)
- [Veldrid](https://github.com/veldrid/veldrid)
- [StbImageSharp](https://github.com/StbSharp/StbImageSharp)
