#nullable enable

using System.Runtime.CompilerServices;

namespace Emotion.Primitives;

public record struct IntRectangle
{
    public IntVector2 Position;

    public IntVector2 Size;

    [DontSerialize]
    public int X { get => Position.X; set => Position.X = value; }

    [DontSerialize]
    public int Y { get => Position.Y; set => Position.Y = value; }

    [DontSerialize]
    public int Width { get => Size.X; set => Size.X = value; }

    [DontSerialize]
    public int Height { get => Size.Y; set => Size.Y = value; }

    [DontSerialize]
    public int Left { get => X; set => X = value; }

    [DontSerialize]
    public int Right { get => X + Width; set => X = value - Width; }

    [DontSerialize]
    public int Top { get => Y; set => Y = value; }

    [DontSerialize]
    public int Bottom { get => Y + Height; set => Y = value - Height; }

    public IntRectangle(IntVector2 pos, IntVector2 size)
    {
        Position = pos;
        Size = size;
    }

    public IntRectangle(int x, int y, int w, int h)
    {
        Position = new IntVector2(x, y);
        Size = new IntVector2(w, h);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly bool Contains(Vector2 value)
    {
        return Position.X < value.X && value.X < Position.X + Size.X && Position.Y < value.Y && value.Y < Position.Y + Size.Y;
    }

    public static IntRectangle FromMinMaxPoints(IntVector2 min, IntVector2 max)
    {
        return new IntRectangle(min.X, min.Y, max.X - min.X, max.Y - min.Y);
    }
}