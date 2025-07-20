using System.Numerics;

namespace NanoUI.Nvg.Data
{
    internal struct NvgBounds
    {
        public Vector2 Min;
        public Vector2 Max;

        public float X
        {
            get => Min.X;
            set => Min.X = value;
        }

        public float Y
        {
            get => Min.Y;
            set => Min.Y = value;
        }

        public float X2
        {
            get => Max.X;
            set => Max.X = value;
        }

        public float Y2
        {
            get => Max.Y;
            set => Max.Y = value;
        }

        public NvgBounds(Vector2 min, Vector2 max)
        {
            Min = min;
            Max = max;
        }

        public void Reset()
        {
            Min = new Vector2(1e6f);
            Max = new Vector2(-1e6f);
        }
    }
}