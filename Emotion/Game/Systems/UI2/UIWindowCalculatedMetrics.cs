#nullable enable

namespace Emotion.Game.Systems.UI2;

public struct UIWindowCalculatedMetrics
{
    public IntVector2 Position;
    public IntVector2 Size;

    public IntVector2 PaddingLeftTop;
    public IntVector2 PaddingRightBottom;
    public readonly IntVector2 PaddingTotalSize => PaddingLeftTop + PaddingRightBottom;

    public IntVector2 MarginLeftTop;
    public IntVector2 MarginRightBottom;
    public readonly IntVector2 MarginTotalSize => MarginLeftTop + MarginRightBottom;

    public IntVector2 Offsets;

    public IntVector2 GetContentSize() => Size - PaddingTotalSize;
    public IntRectangle GetContentRect() => new IntRectangle(
        Position + PaddingLeftTop,
        GetContentSize()
    );

    public IntRectangle Bounds
    {
        get => new IntRectangle(Position, Size);
    }

    public Vector2 Scale;

    public float ScaleF { get => MathF.Max(Scale.X, Scale.Y); }

    public bool InsideParent;

    public IntVector2 ChildrenSize;

    public UIWindowCalculatedMetrics()
    { 
    }

    public override string ToString()
    {
        return Bounds.ToString();
    }
}