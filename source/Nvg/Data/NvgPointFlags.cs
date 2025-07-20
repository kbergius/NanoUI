using System;

namespace NanoUI.Nvg.Data
{
    [Flags]
    internal enum NvgPointFlags
    {
        Corner = 1 << 0,
        Left = 1 << 1,
        Bevel = 1 << 2,
        InnerBevel = 1 << 3
    }
}
