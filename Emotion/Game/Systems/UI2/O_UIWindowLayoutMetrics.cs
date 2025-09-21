#nullable enable

using Emotion.Game.Systems.UI;

namespace Emotion.Game.Systems.UI2;

public class O_UIWindowLayoutMetrics
{
    public const float DefaultMaxSize = 99999;

    public Vector2 Offset = Vector2.Zero;

    /// <summary>
    /// The minimum size the window can be.
    /// </summary>
    public Vector2 MinSize;

    public float MinSizeX
    {
        get => MinSize.X;
        set => MinSize.X = value;
    }

    public float MinSizeY
    {
        get => MinSize.Y;
        set => MinSize.Y = value;
    }

    public Vector2 MaxSize = new Vector2(DefaultMaxSize);

    public float MaxSizeX
    {
        get => MaxSize.X;
        set => MaxSize.X = value;
    }
    public float MaxSizeY
    {
        get => MaxSize.Y;
        set => MaxSize.Y = value;
    }

    public UIRectangleSpacingMetric Padding;
    public UIRectangleSpacingMetric Margins;

    public UILayoutMethod LayoutMethod = UILayoutMethod.Free(UIAnchor.TopLeft, UIAnchor.TopLeft);
    public UISizing SizingX = UISizing.Grow();
    public UISizing SizingY = UISizing.Grow();

    public override string ToString()
    {
        return "Metrics";
    }
}
