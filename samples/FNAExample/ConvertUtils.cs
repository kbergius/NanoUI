using Microsoft.Xna.Framework;
using System.Runtime.CompilerServices;

namespace FNAExample
{
    internal static class ConvertUtils
    {
        public static Vector2 ToFna(this System.Numerics.Vector2 v)
        {
            return Unsafe.As<System.Numerics.Vector2, Vector2>(ref v);
        }

        public static Vector4 ToFna(this System.Numerics.Vector4 v)
        {
            return Unsafe.As<System.Numerics.Vector4, Vector4>(ref v);
        }

        public static Matrix ToFna(this System.Numerics.Matrix4x4 m)
        {
            return Unsafe.As<System.Numerics.Matrix4x4, Matrix>(ref m);
        }
    }
}
