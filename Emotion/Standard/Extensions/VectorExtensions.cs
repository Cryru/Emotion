#nullable enable

using System.Runtime.CompilerServices;

namespace Emotion.Standard.Extensions;

/// <summary>
/// Extension functionality of other classes not within Emotion.
/// </summary>
public static class VectorExtensions
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

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector4 ToVec4(this Vector3 v)
    {
        return new Vector4(v.X, v.Y, v.Z, 1.0f);
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

    public static IntVector2 ToIVec2Floor(this Vector2 v)
    {
        return IntVector2.FromVec2Floor(v);
    }

    public static IntVector2 ToIVec2Ceil(this Vector2 v)
    {
        return IntVector2.FromVec2Ceiling(v);
    }

    /// <summary>
    /// Round the vector by casting it's components to ints.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector2 IntCastRound(this Vector2 v)
    {
        v.X = (int)v.X;
        v.Y = (int)v.Y;
        return v;
    }

    /// <summary>
    /// Get the vector's components as an int tuple.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static (int x, int y) AsInt(this Vector2 v)
    {
        return ((int)v.X, (int)v.Y);
    }

    /// <summary>
    /// Round the vector's components down.
    /// </summary>
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
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector2 Ceiling(this Vector2 v)
    {
        v.X = MathF.Ceiling(v.X);
        v.Y = MathF.Ceiling(v.Y);
        return v;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector2 Round(this Vector2 v, MidpointRounding midPointRounding = MidpointRounding.AwayFromZero)
    {
        return Vector2.Round(v, midPointRounding);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector3 Round(this Vector3 v, MidpointRounding midPointRounding = MidpointRounding.AwayFromZero)
    {
        return Vector3.Round(v, midPointRounding);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector4 Round(this Vector4 v, MidpointRounding midPointRounding = MidpointRounding.AwayFromZero)
    {
        return Vector4.Round(v, midPointRounding);
    }

    /// <summary>
    /// Round the vector's components away from 0 even when not at the midpoint.
    /// </summary>
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
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector3 RoundAwayFromZero(this Vector3 v)
    {
        v.X = MathF.Round(v.X + 0.49f, MidpointRounding.AwayFromZero);
        v.Y = MathF.Round(v.Y + 0.49f, MidpointRounding.AwayFromZero);
        v.Z = MathF.Round(v.Z + 0.49f, MidpointRounding.AwayFromZero);
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
    /// Round the vector by casting it's components to ints.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector3 IntCastRound(this Vector3 v)
    {
        v.X = (int)v.X;
        v.Y = (int)v.Y;
        v.Z = (int)v.Z;
        return v;
    }

    /// <summary>
    /// Round the vector by casting it's components to ints.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector3 IntCastRoundXY(this Vector3 v)
    {
        v.X = (int)v.X;
        v.Y = (int)v.Y;
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
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool LargerOrEqual(this Vector2 v, Vector2 comp)
    {
        return v.X >= comp.X && v.Y >= comp.Y;
    }

    /// <summary>
    /// Returns whether the components of the second vector are all smaller or equal to this vector's.
    /// Basically &lt;=
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool SmallerOrEqual(this Vector2 v, Vector2 comp)
    {
        return v.X <= comp.X && v.Y <= comp.Y;
    }


    /// <summary>
    /// Returns whether the components of the second vector are all larger or equal to this vector's.
    /// >=
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool LargerOrEqual(this Vector3 v, Vector3 comp)
    {
        return v.X >= comp.X && v.Y >= comp.Y && v.Z >= comp.Z;
    }

    /// <summary>
    /// Returns whether the components of the second vector are all smaller or equal to this vector's.
    /// Basically &lt;=
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool SmallerOrEqual(this Vector3 v, Vector3 comp)
    {
        return v.X <= comp.X && v.Y <= comp.Y && v.Z <= comp.Z;
    }

    /// <summary>
    /// Get the vector perpendicular (90deg) to this one.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector2 Perpendicular(this Vector2 me)
    {
        return new Vector2(me.Y, -me.X);
    }

    /// <summary>
    /// Flip the X and Y of a vector.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector2 FlipXY(this Vector2 me)
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

    /// <summary>
    /// Returns a vector in the same direction, but with a length of 1.
    /// If the vector is zero, zero is returned.
    /// Modifies the reference.
    /// </summary>
    public static Vector3 SafeNormalize(this ref Vector3 v)
    {
        v = v == Vector3.Zero ? v : Vector3.Normalize(v);
        return v;
    }

    /// <summary>
    /// Returns a vector in the same direction, but with a length of 1.
    /// If the vector is zero, zero is returned.
    /// </summary>
    public static Vector3 SafeNormalize(Vector3 v)
    {
        return v.SafeNormalize();
    }

    /// <summary>
    /// Returns a vector in the same direction, but with a length of 1.
    /// If the vector is zero, zero is returned.
    /// </summary>
    public static Vector2 SafeNormalize(this ref Vector2 v)
    {
        v = v == Vector2.Zero ? v : Vector2.Normalize(v);
        return v;
    }

    /// <summary>
    /// Returns a vector in the same direction, but with a length of 1.
    /// If the vector is zero, zero is returned.
    /// </summary>
    public static Vector2 SafeNormalize(Vector2 v)
    {
        return v.SafeNormalize();
    }

    public static void SortComponents(Vector3 v, out Vector3 sorted, out Vector3 remap)
    {
        // Values
        float a = v.X, b = v.Y, c = v.Z;
        // Indices
        float ia = 0f, ib = 1f, ic = 2f;

        // First sort a and b
        float minAB = MathF.Min(a, b);
        float maxAB = MathF.Max(a, b);
        float idxMinAB = (a < b) ? ia : ib;
        float idxMaxAB = (a < b) ? ib : ia;

        a = maxAB; ia = idxMaxAB;
        b = minAB; ib = idxMinAB;

        // Sort a and c
        float minAC = MathF.Min(a, c);
        float maxAC = MathF.Max(a, c);
        float idxMinAC = (a < c) ? ia : ic;
        float idxMaxAC = (a < c) ? ic : ia;

        a = maxAC; ia = idxMaxAC;
        c = minAC; ic = idxMinAC;

        // Sort b and c
        float minBC = MathF.Min(b, c);
        float maxBC = MathF.Max(b, c);
        float idxMinBC = (b < c) ? ib : ic;
        float idxMaxBC = (b < c) ? ic : ib;

        b = maxBC; ib = idxMaxBC;
        c = minBC; ic = idxMinBC;

        sorted = new Vector3(a, b, c);
        remap = new Vector3(ia, ib, ic);
    }
}