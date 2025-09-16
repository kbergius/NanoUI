using System;
using System.Numerics;

namespace NanoUI.Serializers
{
    /// <summary>
    /// NumericsArrayConverter is used when serializing JSON.
    /// </summary>
    public static class NumericsArrayConverter
    {
        // Vector2
        public static Vector2 ToVector2(this float[] arr)
        {
            if (arr == null || arr.Length != 2)
                throw new ArgumentException("Array length must be 2");

            return new Vector2(arr[0], arr[1]);
        }

        public static float[] ToArray(this Vector2 vec2)
        {
            return [ vec2.X, vec2.Y ];
        }

        // Vector3
        public static float[] ToArray(this Vector3 vec3)
        {
            return [ vec3.X, vec3.Y, vec3.Z ];
        }
        public static Vector3 ToVector3(this float[] arr)
        {
            if (arr == null || arr.Length != 3)
                throw new ArgumentException("Array length must be 3");

            return new Vector3(arr[0], arr[1], arr[2]);
        }

        // Vector4
        public static float[] ToArray(this Vector4 vec4)
        {
            return [ vec4.X, vec4.Y, vec4.Z, vec4.W ];
        }

        public static Vector4 ToVector4(this float[] arr)
        {
            if (arr == null || arr.Length != 4)
                throw new ArgumentException("Array length must be 4");

            return new Vector4(arr[0], arr[1], arr[2], arr[3]);
        }

        // Quaternion
        public static float[] ToArray(this Quaternion quaternion)
        {
            return [ quaternion.X, quaternion.Y, quaternion.Z, quaternion.W ];
        }

        public static Quaternion ToQuaternion(this float[] arr)
        {
            if (arr == null || arr.Length != 4)
                throw new ArgumentException("Array length must be 4");

            return new Quaternion(arr[0], arr[1], arr[2], arr[3]);
        }

        // Matrix3x2
        public static float[] ToArray(this Matrix3x2 mat)
        {
            return
            [
                mat.M11, mat.M12,
                mat.M21, mat.M22,
                mat.M31, mat.M32
            ];
        }

        // Matrix4x4
        public static float[] ToArray(this Matrix4x4 mat)
        {
            return
            [
                mat.M11, mat.M12, mat.M13, mat.M14,
                mat.M21, mat.M22, mat.M23, mat.M24,
                mat.M31, mat.M32, mat.M33, mat.M34,
                mat.M41, mat.M42, mat.M43, mat.M44,
            ];
        }

        // Plane
        public static float[] ToArray(this Plane value)
        {
            return [ value.Normal.X, value.Normal.Y, value.Normal.Z, value.D ];
        }
    }
}
