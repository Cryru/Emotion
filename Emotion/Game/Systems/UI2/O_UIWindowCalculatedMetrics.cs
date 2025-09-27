#nullable enable

namespace Emotion.Game.Systems.UI2;

public struct O_UIWindowCalculatedMetrics
{
    public IntVector2 Position;
    public IntVector2 Size;
    public IntVector2 PaddingsAndMarginsSize;

    public IntRectangle Bounds
    {
        get => new IntRectangle(Position, Size);
    }

    public Vector2 Scale;

    public float ScaleF { get => MathF.Max(Scale.X, Scale.Y); }

    public bool InsideParent;

    public override string ToString()
    {
        return $"{Position}:{Size}";
    }
}