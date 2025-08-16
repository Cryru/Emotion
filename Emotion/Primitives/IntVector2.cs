#nullable enable

namespace Emotion.Primitives;

public struct IntVector2
{
    public int X;
    public int Y;

    public IntVector2(int x, int y)
    {
        X = x;
        Y = y;
    }

    public override int GetHashCode()
    {
        return Maths.GetCantorPair(X, Y);
    }

    public Vector2 ToVec2()
    {
        return new Vector2(X, Y);
    }

    public Vector3 ToVec3(float z = 0)
    {
        return new Vector3(X, Y, z);
    }

    public static IntVector2 FromVec2Floor(Vector2 vec)
    {
        vec = vec.Floor();
        return new IntVector2((int) vec.X, (int) vec.Y);
    }

    public static IntVector2 operator /(IntVector2 iv2, Vector2 v2)
    {
        Vector2 iv2F = iv2.ToVec2();
        return FromVec2Floor(iv2F);
    }

    public static IntVector2 operator *(IntVector2 a, IntVector2 b)
    {
        return new IntVector2(a.X * b.X, a.Y * b.Y);
    }
}
