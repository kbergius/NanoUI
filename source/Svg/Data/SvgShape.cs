using System;
using System.Numerics;

namespace NanoUI.Svg.Data
{
    // collect here all params from <svg> & <defs> elements
    internal struct SvgShape
    {
        // this comes from svg xml file
        public Vector2 Size;

        public SvgPath[] Paths;

        // these are paths bounds
        public Vector2 PathsMin;
        public Vector2 PathsMax;

        public void Reset()
        {
            Size = Vector2.Zero;

            Paths = Array.Empty<SvgPath>();
        }
    }
}