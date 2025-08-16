#nullable enable

using Standart.Hash.xxHash;

namespace Emotion.Primitives;

public struct DoubleVector3
{
    public double X;
    public double Y;
    public double Z;

    public DoubleVector3(double x, double y, double z)
    {
        X = x;
        Y = y;
        Z = z;
    }

    public unsafe override int GetHashCode()
    {
        DoubleVector3 localThis = this; // Copy to stack
        DoubleVector3* thisPtr = &localThis;
        ReadOnlySpan<byte> asSpan = new ReadOnlySpan<byte>(thisPtr, sizeof(double) * 3);
        return (int) xxHash32.ComputeHash(asSpan, asSpan.Length);
    }

    public Vector2 ToVec2()
    {
        return new Vector2((float) X, (float) Y);
    }

    public Vector3 ToVec3()
    {
        return new Vector3((float) X, (float) Y, (float) Z);
    }

    public static DoubleVector3 operator *(DoubleVector3 a, DoubleVector3 b)
    {
        return new DoubleVector3(a.X * b.X, a.Y * b.Y, a.Z * b.Z);
    }
}
