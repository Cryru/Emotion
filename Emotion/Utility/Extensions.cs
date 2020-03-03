#region Using

using System;
using System.Numerics;

#endregion

namespace Emotion.Utility
{
    /// <summary>
    /// Extension functionality of other classes not within Emotion.
    /// </summary>
    public static class Extensions
    {
        /// <summary>
        /// Returns an inverted copy of this instance.
        /// </summary>
        public static Matrix4x4 Inverted(this Matrix4x4 m)
        {
            Matrix4x4.Invert(m, out Matrix4x4 inv);
            return inv;
        }

        /// <summary>
        /// Converts a vector3 to a vector2.
        /// </summary>
        /// <param name="v">The vector to convert.</param>
        /// <returns>The Vector3 as a Vector2, with the Z unit stripped.</returns>
        public static Vector2 ToVec2(this Vector3 v)
        {
            return new Vector2(v.X, v.Y);
        }

        /// <summary>
        /// Converts a vector2 to a vector3.
        /// </summary>
        /// <param name="v">The vector to convert.</param>
        /// <returns>The Vector2 as a Vector3, with the Z unit added.</returns>
        public static Vector3 ToVec3(this Vector2 v, float z = 0)
        {
            return new Vector3(v.X, v.Y, z);
        }

        /// <summary>
        /// Converts a vector4 to a vector3.
        /// </summary>
        /// <param name="v">The vector to convert.</param>
        /// <returns>The Vector4 as a Vector3, with the W unit stripped.</returns>
        public static Vector3 ToVec3(this Vector4 v)
        {
            return new Vector3(v.X, v.Y, v.Z);
        }

        public static Vector3 Floor(this Vector3 v)
        {
            v.X = MathF.Floor(v.X);
            v.Y = MathF.Floor(v.Y);
            v.Z = MathF.Floor(v.Z);
            return v;
        }

        public static Vector2 Floor(this Vector2 v)
        {
            v.X = MathF.Floor(v.X);
            v.Y = MathF.Floor(v.Y);
            return v;
        }

        public static Vector3 Ceiling(this Vector3 v)
        {
            v.X = MathF.Ceiling(v.X);
            v.Y = MathF.Ceiling(v.Y);
            v.Z = MathF.Ceiling(v.Z);
            return v;
        }

        public static Vector2 Ceiling(this Vector2 v)
        {
            v.X = MathF.Ceiling(v.X);
            v.Y = MathF.Ceiling(v.Y);
            return v;
        }

        public static Vector2 RoundClosest(this Vector2 v)
        {
            v.X = MathF.Floor(v.X + 0.5f);
            v.Y = MathF.Floor(v.Y + 0.5f);
            return v;
        }

        public static bool LargerOrEqual(this Vector2 v, Vector2 comp)
        {
            return v.X >= comp.X && v.Y >= comp.Y;
        }

        public static bool SmallerOrEqual(this Vector2 v, Vector2 comp)
        {
            return v.X <= comp.X && v.Y <= comp.Y;
        }

        public static T[] AddToArray<T>(this T[] array, T element)
        {
            Array.Resize(ref array, array.Length + 1);
            array[^1] = element;
            return array;
        }

        public static int IndexOf<T>(this T[] array, T element)
        {
            for (var i = 0; i < array.Length; i++)
            {
                if (array[i].Equals(element)) return i;
            }

            return -1;
        }
    }
}