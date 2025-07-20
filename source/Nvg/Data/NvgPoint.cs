using System.Numerics;

namespace NanoUI.Nvg.Data
{
    internal struct NvgPoint
    {
        public Vector2 Position;
        public Vector2 Delta;
        public float Length;
        public Vector2 DeltaM;
        public NvgPointFlags Flags;

        public void Reset()
        {
            Position = Delta = DeltaM = Vector2.Zero;
            Length = 0;
            Flags = 0;// default;
        }
    }
}