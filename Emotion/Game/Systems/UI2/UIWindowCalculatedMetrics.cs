#nullable enable

namespace Emotion.Game.Systems.UI2;

public struct UIWindowCalculatedMetrics
{
    public IntVector2 Position;
    public IntVector2 Size;

    public IntVector2 PaddingsSize;
    public IntVector2 MarginsSize;
    public IntVector2 Offsets;

    public IntRectangle Bounds
    {
        get => new IntRectangle(Position, Size);
    }

    public Vector2 Scale;

    public float ScaleF { get => MathF.Max(Scale.X, Scale.Y); }

    public bool InsideParent;

    public IntVector2 ChildrenSize;

    public override string ToString()
    {
        return Bounds.ToString();
    }
}