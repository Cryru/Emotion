#nullable enable

using Emotion.Game.Systems.UI;

namespace Emotion.Game.Systems.UI2;

public class O_UIWindowLayoutMetrics
{
    public Vector2 Offset = Vector2.Zero;

    public Vector2 MinSize = new Vector2();
    public Vector2 MaxSize = new Vector2(99999, 99999);
    public UIRectangleSpacingMetric Padding;

    public LayoutMode LayoutMode = LayoutMode.Free;
    public Vector2 ListSpacing;

    public UISizing SizingX = UISizing.Fit();
    public UISizing SizingY = UISizing.Fit();

    public override string ToString()
    {
        return "Metrics";
    }
}
