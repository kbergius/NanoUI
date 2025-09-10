using System.Numerics;

namespace NanoUI.Rendering.Data
{
    /// <summary>
    /// Vertex to store into VertexBuffer.
    /// </summary>
    public struct Vertex
    {
        /// <summary>
        /// Size in bytes.
        /// </summary>
        public const byte SizeInBytes = 16;

        /// <summary>
        /// Position.
        /// </summary>
        public Vector2 Position;

        /// <summary>
        /// Texture coordinate.
        /// </summary>
        public Vector2 UV;
    }
}
