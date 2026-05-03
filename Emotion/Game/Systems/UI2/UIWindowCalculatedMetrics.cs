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
    public IntVector2 MinSize;
    public IntVector2 PreferredSize;
    public IntVector2 MaxSize;

    public int GridColumnCount;
    public int GridRowCount;
    public List<int>? GridRowHeights;
    public List<int>? GridColumnWidths;

    public int MainAxis;
    public int CrossAxis;

    public Vector2 MaxScroll;

    public readonly IntVector2 GetViewportSize() => Size - PaddingTotalSize;
    public readonly IntRectangle GetViewportRect() => new(Position + PaddingLeftTop, GetViewportSize());

    public readonly IntRectangle Bounds
    {
        get => new(Position, Size);
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