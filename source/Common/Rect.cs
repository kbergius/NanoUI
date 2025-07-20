using System.Numerics;

namespace NanoUI.Common
{
    public struct Rect
    {
        public Vector2 Position { get; set; }
        public Vector2 Size { get; set; }

        public Rect() { }
        
        public Rect(float x, float y, float width, float height)
        {
            Position = new Vector2(x, y);
            Size = new Vector2(width, height);
        }

        public Rect(float x, float y, Vector2 size)
        {
            Position = new Vector2(x, y);
            Size = size;
        }

        public Rect(Vector2 position, Vector2 size)
        {
            Position = position;
            Size = size;
        }

        public float X => Position.X;
        public float Y => Position.Y;
        public float Width => Size.X;
        public float Height => Size.Y;
        public Vector2 Center => Position + Size * 0.5f;
        public Vector2 Max => Position + Size;

        public readonly bool Contains(Vector2 pt) => Contains(pt.X, pt.Y);
        
        public readonly bool Contains(float x, float y)
        {
            return Position.X <= x && x < Position.X + Size.X && Position.Y <= y && y < Position.Y + Size.Y;
        }

        public readonly bool IntersectsWith(in Rect rect)
        {
            return (rect.Position.X < Position.X + Size.X) && (Position.X < rect.Position.X + rect.Size.X) &&
                (rect.Position.Y < Position.Y + Size.Y) && (Position.Y < rect.Position.Y + rect.Size.Y);
        }
    }
}