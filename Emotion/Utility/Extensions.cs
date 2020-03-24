﻿#region Using

using System;
using System.Numerics;
using System.Runtime.CompilerServices;

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
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
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
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector2 ToVec2(this Vector3 v)
        {
            return new Vector2(v.X, v.Y);
        }

        /// <summary>
        /// Converts a vector2 to a vector3.
        /// </summary>
        /// <param name="v">The vector to convert.</param>
        /// <param name="z">The z value to set to the new component.</param>
        /// <returns>The Vector2 as a Vector3, with the Z unit added.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3 ToVec3(this Vector2 v, float z = 0)
        {
            return new Vector3(v.X, v.Y, z);
        }

        /// <summary>
        /// Converts a vector4 to a vector3.
        /// </summary>
        /// <param name="v">The vector to convert.</param>
        /// <returns>The Vector4 as a Vector3, with the W unit stripped.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3 ToVec3(this Vector4 v)
        {
            return new Vector3(v.X, v.Y, v.Z);
        }

        /// <summary>
        /// Round the vector by casting it's components to ints.
        /// </summary>
        /// <param name="v"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector2 IntCastRound(this Vector2 v)
        {
            v.X = (int) v.X;
            v.Y = (int) v.Y;
            return v;
        }

        /// <summary>
        /// Get the vector's components as an int tuple.
        /// </summary>
        /// <param name="v"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static (int x, int y) AsInt(this Vector2 v)
        {
            return ((int) v.X, (int) v.Y);
        }

        /// <summary>
        /// Round the vector's components down.
        /// </summary>
        /// <param name="v"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3 Floor(this Vector3 v)
        {
            v.X = MathF.Floor(v.X);
            v.Y = MathF.Floor(v.Y);
            v.Z = MathF.Floor(v.Z);
            return v;
        }

        /// <summary>
        /// Round the vector's components down.
        /// </summary>
        /// <param name="v"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector2 Floor(this Vector2 v)
        {
            v.X = MathF.Floor(v.X);
            v.Y = MathF.Floor(v.Y);
            return v;
        }

        /// <summary>
        /// Round the vector's components up.
        /// </summary>
        /// <param name="v"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3 Ceiling(this Vector3 v)
        {
            v.X = MathF.Ceiling(v.X);
            v.Y = MathF.Ceiling(v.Y);
            v.Z = MathF.Ceiling(v.Z);
            return v;
        }

        /// <summary>
        /// Round the vector's components up.
        /// </summary>
        /// <param name="v"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector2 Ceiling(this Vector2 v)
        {
            v.X = MathF.Ceiling(v.X);
            v.Y = MathF.Ceiling(v.Y);
            return v;
        }

        /// <summary>
        /// Round the vector's components using Maths.RoundClosest.
        /// </summary>
        /// <param name="v"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector2 RoundClosest(this Vector2 v)
        {
            v.X = Maths.RoundClosest(v.X);
            v.Y = Maths.RoundClosest(v.Y);
            return v;
        }

        /// <summary>
        /// Round the vector's components using Maths.RoundClosest.
        /// </summary>
        /// <param name="v"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3 RoundClosest(this Vector3 v)
        {
            v.X = Maths.RoundClosest(v.X);
            v.Y = Maths.RoundClosest(v.Y);
            v.Z = Maths.RoundClosest(v.Z);
            return v;
        }

        /// <summary>
        /// Returns whether the components of the second vector are all larger or equal to this vector's.
        /// >=
        /// </summary>
        /// <param name="v"></param>
        /// <param name="comp"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool LargerOrEqual(this Vector2 v, Vector2 comp)
        {
            return v.X >= comp.X && v.Y >= comp.Y;
        }

        /// <summary>
        /// Returns whether the components of the second vector are all smaller or equal to this vector's.
        /// Basically &lt;=
        /// </summary>
        /// <param name="v"></param>
        /// <param name="comp"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool SmallerOrEqual(this Vector2 v, Vector2 comp)
        {
            return v.X <= comp.X && v.Y <= comp.Y;
        }

        /// <summary>
        /// Adds an element to an array. This is worse than using a list and will resize the array.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="array"></param>
        /// <param name="element"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T[] AddToArray<T>(this T[] array, T element)
        {
            Array.Resize(ref array, array.Length + 1);
            array[^1] = element;
            return array;
        }

        /// <summary>
        /// Returns the index of an element within an array, or -1 if not found.
        /// This will loop the whole array.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="array"></param>
        /// <param name="element"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int IndexOf<T>(this T[] array, T element)
        {
            for (var i = 0; i < array.Length; i++)
            {
                if (array[i].Equals(element)) return i;
            }

            return -1;
        }

        /// <summary>
        /// Swap two items in an array by index.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="array"></param>
        /// <param name="idx"></param>
        /// <param name="withIdx"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ArraySwap<T>(this T[] array, int idx, int withIdx)
        {
            T temp = array[idx];
            array[idx] = array[withIdx];
            array[withIdx] = temp;
        }
    }
}