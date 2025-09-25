using System.Numerics;

namespace NanoUI.Common
{
    /// <summary>
    /// Rect.
    /// </summary>
    public struct Rect
    {
        /// <summary>
        /// Position
        /// </summary>
        public Vector2 Position { get; set; }

        /// <summary>
        /// Size
        /// </summary>
        public Vector2 Size { get; set; }

        /// <summary>
        /// Creates rectangle.
        /// </summary>
        public Rect() { }

        /// <summary>
        /// Creates rectangle.
        /// </summary>
        /// <param name="x">X</param>
        /// <param name="y">Y</param>
        /// <param name="width">Width</param>
        /// <param name="height">Height</param>
        public Rect(float x, float y, float width, float height)
        {
            Position = new Vector2(x, y);
            Size = new Vector2(width, height);
        }

        /// <summary>
        /// Creates rectangle.
        /// </summary>
        /// <param name="x">X</param>
        /// <param name="y">Y</param>
        /// <param name="size">Size</param>
        public Rect(float x, float y, Vector2 size)
        {
            Position = new Vector2(x, y);
            Size = size;
        }

        /// <summary>
        /// Creates rectangle.
        /// </summary>
        /// <param name="position">Position</param>
        /// <param name="size">Size</param>
        public Rect(Vector2 position, Vector2 size)
        {
            Position = position;
            Size = size;
        }

        /// <summary>
        /// X
        /// </summary>
        public float X => Position.X;

        /// <summary>
        /// Y
        /// </summary>
        public float Y => Position.Y;

        /// <summary>
        /// Width
        /// </summary>
        public float Width => Size.X;

        /// <summary>
        /// Height
        /// </summary>
        public float Height => Size.Y;

        /// <summary>
        /// Center
        /// </summary>
        public Vector2 Center => Position + Size * 0.5f;

        /// <summary>
        /// Max
        /// </summary>
        public Vector2 Max => Position + Size;

        /// <summary>
        /// Contains
        /// </summary>
        /// <param name="pt">Point</param>
        /// <returns>Contains?</returns>
        public readonly bool Contains(Vector2 pt) => Contains(pt.X, pt.Y);

        /// <summary>
        /// Contains
        /// </summary>
        /// <param name="x">X</param>
        /// <param name="y">Y</param>
        /// <returns>Contains?</returns>
        public readonly bool Contains(float x, float y)
        {
            return Position.X <= x && x < Position.X + Size.X && Position.Y <= y && y < Position.Y + Size.Y;
        }

        /// <summary>
        /// Intersects with
        /// </summary>
        /// <param name="rect">Rectangle</param>
        /// <returns></returns>
        public readonly bool IntersectsWith(in Rect rect)
        {
            return (rect.Position.X < Position.X + Size.X) && (Position.X < rect.Position.X + rect.Size.X) &&
                (rect.Position.Y < Position.Y + Size.Y) && (Position.Y < rect.Position.Y + rect.Size.Y);
        }
    }
}
