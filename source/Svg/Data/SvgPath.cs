using NanoUI.Nvg.Data;

namespace NanoUI.Svg.Data
{
    // this is the result - it is drawn in own BeginPath() function
    internal struct SvgPath
    {
        // collected commands
        public NvgPathCommand[] Commands;

        // style info (fill, stroke etc)
        public SvgStyle Style;

        public NvgBounds Bounds;

        // transform already applied when commands created
    }
}