using System.Numerics;

namespace NanoUI.Nvg.Data
{
    internal struct NvgScissor
    {
        public Matrix3x2 Transform;
        public Vector2 Extent;

        public void Reset()
        {
            // todo: identity?
            Transform = default;
            Extent = new Vector2(-1.0f);
        }
    }
}