#nullable enable

using System.Runtime.CompilerServices;

namespace Emotion.Primitives;

public record struct IntRectangle
{
    public IntVector2 Position;

    public IntVector2 Size;

    [DontSerialize]
    public int X { readonly get => Position.X; set => Position.X = value; }

    [DontSerialize]
    public int Y { readonly get => Position.Y; set => Position.Y = value; }

    [DontSerialize]
    public int Width { readonly get => Size.X; set => Size.X = value; }

    [DontSerialize]
    public int Height { readonly get => Size.Y; set => Size.Y = value; }

    [DontSerialize]
    public int Left { readonly get => X; set => X = value; }

    [DontSerialize]
    public int Right { readonly get => X + Width; set => X = value - Width; }

    [DontSerialize]
    public int Top { readonly get => Y; set => Y = value; }

    [DontSerialize]
    public int Bottom { readonly get => Y + Height; set => Y = value - Height; }

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

    public Rectangle GetRect()
    {
        return new Rectangle(X, Y, Width, Height);
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

    public override readonly string ToString()
    {
        return $"{X} {Y}:{Width} {Height}";
    }
}