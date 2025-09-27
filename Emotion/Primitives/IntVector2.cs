#nullable enable

using System.Runtime.CompilerServices;

namespace Emotion.Primitives;

public record struct IntVector2
{
    public static readonly IntVector2 Zero = new IntVector2(0, 0);

    public int X;
    public int Y;

    public IntVector2(int x, int y)
    {
        X = x;
        Y = y;
    }

    public IntVector2(int v)
    {
        X = v;
        Y = v;
    }

    public readonly Vector2 ToVec2()
    {
        return new Vector2(X, Y);
    }

    public readonly Vector3 ToVec3(float z = 0)
    {
        return new Vector3(X, Y, z);
    }

    #region Operands

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override readonly int GetHashCode() => HashCode.Combine(X, Y);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly bool Equals(IntVector2 other) => other.X == X && other.Y == Y;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static IntVector2 operator *(IntVector2 a, IntVector2 b) => new IntVector2(a.X * b.X, a.Y * b.Y);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static IntVector2 operator +(IntVector2 a, IntVector2 b) => new IntVector2(a.X + b.X, a.Y + b.Y);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static IntVector2 operator -(IntVector2 a, IntVector2 b) => new IntVector2(a.X - b.X, a.Y - b.Y);

    #endregion

    public int this[int index]
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        readonly get
        {
            Assert(index == 0 || index == 1);
            if (index == 0) return X;
            return Y;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        set
        {
            Assert(index == 0 || index == 1);
            if (index == 0)
                X = value;
            else
                Y = value;
        }
    }

    #region Statics

    public static IntVector2 FromVec2Floor(Vector2 vec)
    {
        vec = vec.Floor();
        return new IntVector2((int)vec.X, (int)vec.Y);
    }

    public static IntVector2 FromVec2Ceiling(Vector2 vec)
    {
        vec = vec.Ceiling();
        return new IntVector2((int)vec.X, (int)vec.Y);
    }

    public readonly IntVector2 CeilMultiply(float scale)
    {
        return new IntVector2(
            (int)Math.Ceiling(X * scale),
            (int)Math.Ceiling(Y * scale)
        );
    }

    public readonly IntVector2 CeilMultiply(Vector2 scale)
    {
        return new IntVector2(
            (int)Math.Ceiling(X * scale.X),
            (int)Math.Ceiling(Y * scale.Y)
        );
    }

    public readonly IntVector2 FloorMultiply(float scale)
    {
        return new IntVector2(
            (int)Math.Floor(X * scale),
            (int)Math.Floor(Y * scale)
        );
    }

    public readonly IntVector2 FloorMultiply(Vector2 scale)
    {
        return new IntVector2(
            (int)Math.Floor(X * scale.X),
            (int)Math.Floor(Y * scale.Y)
        );
    }

    public static IntVector2 Max(IntVector2 a, IntVector2 b)
    {
        return new IntVector2(
            Math.Max(a.X, b.X),
            Math.Max(a.Y, b.Y)
        );
    }

    public static IntVector2 Clamp(IntVector2 val, IntVector2 min, IntVector2 max)
    {
        return new IntVector2(
            Math.Clamp(val.X, min.X, max.X),
            Math.Clamp(val.Y, min.Y, max.Y)
        );
    }

    #endregion
}
