using NanoUI.Common;
using System.Numerics;

namespace NanoUI.Nvg.Data
{
    internal struct NvgPathCommand
    {
        public NvgPathCommandType CommandType;
        public Vector2 P0;
        public Vector2 P1;
        public Vector2 P2;

        public float TessTol;
        public Winding Winding;
    }
}