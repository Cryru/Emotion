#nullable enable

namespace Emotion.Game.Systems.UI2;

public struct O_UIWindowCalculatedMetrics
{
    public Vector2 Position;
    public Vector2 Size;

    public Rectangle Bounds
    {
        get => new Primitives.Rectangle(Position, Size);
    }

    public Vector2 Scale;
    public float ScaleF { get => MathF.Max(Scale.X, Scale.Y); }

    public override string ToString()
    {
        return $"{Position}:{Size}";
    }
}