using System;
using System.Collections.Generic;
using System.Numerics;
using System.Linq;

namespace NanoUI.Utils
{
    /// <summary>
    /// MathUtils provides some helper math functions.
    /// </summary>
    public static class MathUtils
    {
        /// <summary>
        /// Is point inside triangle?
        /// </summary>
        /// <param name="point">Point</param>
        /// <param name="a">Triangle corner1</param>
        /// <param name="b">Triangle corner2</param>
        /// <param name="c">Triangle corner3</param>
        /// <returns>Success</returns>
        public static bool IsPointInsideTriangle(Vector2 point, Vector2 a, Vector2 b, Vector2 c)
        {
            bool b1 = ((point.X - b.X) * (a.Y - b.Y) - (point.Y - b.Y) * (a.X - b.X)) < 0.0f;
            bool b2 = ((point.X - c.X) * (b.Y - c.Y) - (point.Y - c.Y) * (b.X - c.X)) < 0.0f;
            bool b3 = ((point.X - a.X) * (c.Y - a.Y) - (point.Y - a.Y) * (c.X - a.X)) < 0.0f;

            return (b1 == b2) && (b2 == b3);
        }

        /// <summary>
        /// Swap values.
        /// </summary>
        /// <typeparam name="T">Value types</typeparam>
        /// <param name="a">Value1</param>
        /// <param name="b">Value2</param>
        public static void Swap<T>(ref T a, ref T b)
        {
            (a, b) = (b, a);
        }

        /// <summary>
        /// Cross vectors.
        /// </summary>
        /// <param name="a">Vector2-1</param>
        /// <param name="b">Vector2-2</param>
        /// <returns>cross result</returns>
        public static float Cross(this Vector2 a, Vector2 b)
        {
            // note: .NET 10 has this implemented

            return b.X * a.Y - a.X * b.Y;
        }

        /// <summary>
        /// Get average scale.
        /// </summary>
        /// <param name="m">Matrix3x2</param>
        /// <returns>Average scale</returns>
        public static float GetAverageScale(Matrix3x2 m)
        {
            float sx = MathF.Sqrt(m.M11 * m.M11 + m.M21 * m.M21);
            float sy = MathF.Sqrt(m.M12 * m.M12 + m.M22 * m.M22);

            return (sx + sy) * 0.5f;
        }

        /// <summary>
        /// Rotates around point
        /// </summary>
        /// <param name="v">Vector2</param>
        /// <param name="radians">Radians</param>
        /// <param name="point">Point</param>
        /// <returns>Rotation result</returns>
        public static Vector2 RotateAroundPoint(this Vector2 v, float radians, Vector2 point)
        {
            return Vector2.Transform(v - point, Matrix4x4.CreateRotationZ(radians)) + point;
        }

        /// <summary>
        /// Quotient.
        /// </summary>
        /// <param name="a">Vector2-1</param>
        /// <param name="b">Vector2-2</param>
        /// <returns>Result</returns>
        public static Vector2 Quotient(this Vector2 a, Vector2 b)
        {
            return new Vector2(a.X / b.X, a.Y / b.Y);
        }

        /// <summary>
        /// Replace zero.
        /// </summary>
        /// <param name="a">Vector2-1</param>
        /// <param name="b">Vector2-2</param>
        /// <returns>Result</returns>
        public static Vector2 ReplaceZero(this Vector2 a, Vector2 b)
        {
            return new Vector2(a.X > 0 ? a.X : b.X, a.Y > 0 ? a.Y : b.Y);
        }

        /// <summary>
        /// SquaredNorm.
        /// </summary>
        /// <param name="v">Vector2</param>
        /// <returns>Result</returns>
        public static float SquaredNorm(this Vector2 v)
        {
            return v.X * v.X + v.Y * v.Y;
        }

        /// <summary>
        /// MinCoefficient
        /// </summary>
        /// <param name="v">Vector2</param>
        /// <returns>Result</returns>
        public static float MinCoefficient(this Vector2 v)
        { 
            return v.X > v.Y ? v.Y : v.X;
        }

        /// <summary>
        /// Calculates sum of the values in array.
        /// </summary>
        /// <typeparam name="T">Values type</typeparam>
        /// <param name="array">Array</param>
        /// <returns>Sum</returns>
        public static T Sum<T>(T[] array) where T : INumber<T>
        {
            if (array == null || array.Length == 0)
                return T.Zero;

            return array.Aggregate((a, b) => a + b);
        }

        /// <summary>
        /// Calculates sum of the values in list.
        /// </summary>
        /// <typeparam name="T">Values type</typeparam>
        /// <param name="list">List</param>
        /// <returns>Sum</returns>
        public static T Sum<T>(List<T> list) where T : INumber<T>
        {
            if (list == null || list.Count == 0)
                return T.Zero;

            return list.Aggregate((a, b) => a + b);
        }
    }
}
