using System.Numerics;

namespace NanoUI.Rendering.Data
{
    /// <summary>
    /// Uniform used in fragment/pixel shader.
    /// Note: You shouldn't normally worry about this, since NanoUI sets values and
    /// your fragment/pixel shader uses them.
    /// </summary>
    public struct FragmentUniform
    {
        /// <summary>
        /// Scissor matrix.
        /// </summary>
        public Matrix4x4 ScissorMat;

        /// <summary>
        /// Paint matrix.
        /// </summary>
        public Matrix4x4 PaintMat;

        /// <summary>
        /// Inner color.
        /// </summary>
        public Vector4 InnerCol;

        /// <summary>
        /// Outer color.
        /// </summary>
        public Vector4 OuterCol;

        /// <summary>
        /// Scissor extent.
        /// </summary>
        public Vector2 ScissorExt;

        /// <summary>
        /// Scissor scale.
        /// </summary>
        public Vector2 ScissorScale;

        /// <summary>
        /// Extent.
        /// </summary>
        public Vector2 Extent;

        /// <summary>
        /// Radius.
        /// </summary>
        public float Radius;

        /// <summary>
        /// Feather.
        /// </summary>
        public float Feather;

        /// <summary>
        /// Action type.
        /// Note: the value here is actually int, but some HLSL shaders don't like ints.
        /// </summary>
        public float ActionType;

        /// <summary>
        /// Font size.
        /// </summary>
        public float FontSize;

        /// <summary>
        /// Reserved for future usage. Needed to set correct alignment.
        /// </summary>
        private float Unused1;

        /// <summary>
        /// Reserved for future usage. Needed to set correct alignment.
        /// </summary>
        private float Unused2;
    }
}
