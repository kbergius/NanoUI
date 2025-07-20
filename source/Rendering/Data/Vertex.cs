using System.Numerics;

namespace NanoUI.Rendering.Data
{
    public struct Vertex
    {
        public const byte SizeInBytes = 16;

        public Vector2 Position;
        public Vector2 UV;
    }
}