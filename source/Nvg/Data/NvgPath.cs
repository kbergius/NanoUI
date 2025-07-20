using NanoUI.Common;

namespace NanoUI.Nvg.Data
{
    internal struct NvgPath
    {
        public bool Closed;
        public uint BevelCount;
        public bool Convex;
        public Winding Winding;
        public NvgBounds Bounds;

        // structs params
        public int PointOffset;
        public int PointCount;
        public int FillCount;
        public int FillOffset;
        public int StrokeCount;
        public int StrokeOffset;

        // when adding path we must reset values
        public void Reset()
        {
            Closed = default;
            BevelCount = default;
            Convex = default;
            // default winding marks solid fill
            Winding = Winding.CounterClockwise;

            Bounds.Reset();

            // structs params
            PointOffset = default;
            PointCount = default;
            FillCount = default;
            FillOffset = default;
            StrokeCount = default;
            StrokeOffset = default;
        }
    }
}