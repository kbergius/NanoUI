using System.Numerics;

namespace NanoUI.Rendering.Data
{
    public struct FragmentUniform
    {
        public Matrix4x4 ScissorMat;
        public Matrix4x4 PaintMat;

        public Vector4 InnerCol;
        public Vector4 OuterCol;

        public Vector2 ScissorExt;
        public Vector2 ScissorScale;

        public Vector2 Extent;
        public float Radius;
        public float Feather;

        public float ActionType; // this is normally int, but hlsl doesn't like it
        public float FontSize;
        private float Unused1;
        private float Unused2;
    }
}