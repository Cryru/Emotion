#region Using

using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Emotion.Utility;

#endregion

namespace System.Numerics
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
        /// Round the vector's components using Math.Round.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector2 Round(this Vector2 v, int digits = 5, MidpointRounding midPointRounding = MidpointRounding.AwayFromZero)
        {
            v.X = MathF.Round(v.X, digits, midPointRounding);
            v.Y = MathF.Round(v.Y, digits, midPointRounding);
            return v;
        }

        /// <summary>
        /// Round the vector's components away from 0 even when not at the midpoint.
        /// </summary>
        /// <param name="v"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector2 RoundAwayFromZero(this Vector2 v)
        {
            v.X = MathF.Round(v.X + 0.49f, MidpointRounding.AwayFromZero);
            v.Y = MathF.Round(v.Y + 0.49f, MidpointRounding.AwayFromZero);
            return v;
        }

        /// <summary>
        /// Round the vector's components away from 0 even when not at the midpoint.
        /// </summary>
        /// <param name="v"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3 RoundAwayFromZero(this Vector3 v)
        {
            v.X = MathF.Round(v.X + 0.49f, MidpointRounding.AwayFromZero);
            v.Y = MathF.Round(v.Y + 0.49f, MidpointRounding.AwayFromZero);
            v.Z = MathF.Round(v.Z + 0.49f, MidpointRounding.AwayFromZero);
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
        /// Round off any amounts smaller than epsilon.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector2 EpsilonRound(this Vector2 v)
        {
            Vector2 floor = v.Floor();
            Vector2 subTract = v - floor;
            if (subTract.X < Maths.EPSILON) v.X = floor.X;
            if (subTract.Y < Maths.EPSILON) v.Y = floor.Y;
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
        /// Round the vector by casting it's components to ints.
        /// </summary>
        /// <param name="v"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3 IntCastRound(this Vector3 v)
        {
            v.X = (int) v.X;
            v.Y = (int) v.Y;
            v.Z = (int) v.Z;
            return v;
        }

        /// <summary>
        /// Transform a two dimensional vector2 to a one dimensional in whichever direction the larger component is.
        /// </summary>
        public static Vector2 OneDirectionOnly(this Vector2 v)
        {
            float x = MathF.Abs(v.X);
            float y = MathF.Abs(v.Y);
            if (x > y)
                v.Y = 0;
            else if (y > x) v.X = 0;
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
        /// Returns whether the components of the second vector are all larger or equal to this vector's.
        /// >=
        /// </summary>
        /// <param name="v"></param>
        /// <param name="comp"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool LargerOrEqual(this Vector3 v, Vector3 comp)
        {
            return v.X >= comp.X && v.Y >= comp.Y && v.Z >= comp.Z;
        }

        /// <summary>
        /// Returns whether the components of the second vector are all smaller or equal to this vector's.
        /// Basically &lt;=
        /// </summary>
        /// <param name="v"></param>
        /// <param name="comp"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool SmallerOrEqual(this Vector3 v, Vector3 comp)
        {
            return v.X <= comp.X && v.Y <= comp.Y && v.Z <= comp.Z;
        }

        /// <summary>
        /// "Cross product" approximation for 2D vectors.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float Cross(this Vector2 me, Vector2 other)
        {
            return Vector3.Cross(me.ToVec3(), other.ToVec3()).Z;
        }

        /// <summary>
        /// Get the vector perpendicular (90deg) to this one.
        /// </summary>
        /// <param name="me"></param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector2 Perpendicular(this Vector2 me)
        {
            return new Vector2(me.Y, me.X);
        }

        /// <summary>
        /// Pow both components of the vector2.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector2 Pow(this Vector2 me, float pow)
        {
            return new Vector2(MathF.Pow(me.X, pow), MathF.Pow(me.Y, pow));
        }
    }
}

namespace Emotion.Utility
{
    public static class Extensions
    {
        /// <summary>
        /// Adds an element to an array. This is worse than using a list and will resize the array.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T[] AddToArray<T>(this T[] array, T element)
        {
            Array.Resize(ref array, array.Length + 1);
            array[^1] = element;
            return array;
        }

        /// <summary>
        /// Join two arrays to create a new array contains the items of both.
        /// </summary>
        public static T[] JoinArrays<T>(T[] arrayOne, T[] arrayTwo)
        {
            var newArray = new T[arrayOne.Length + arrayTwo.Length];
            Array.Copy(arrayOne, 0, newArray, 0, arrayOne.Length);
            Array.Copy(arrayTwo, 0, newArray, arrayOne.Length, arrayTwo.Length);
            return newArray;
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

        /// <summary>
        /// Get all enum flags.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="flags"></param>
        /// <returns></returns>
        public static IEnumerable<T> GetEnumFlags<T>(this Enum flags) where T : Enum
        {
            foreach (Enum value in Enum.GetValues(flags.GetType()))
            {
                if (flags.HasFlag(value)) yield return (T) value;
            }
        }
    }
}