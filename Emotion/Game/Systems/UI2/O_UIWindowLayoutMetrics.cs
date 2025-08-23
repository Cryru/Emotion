#nullable enable

using Emotion.Game.Systems.UI;

namespace Emotion.Game.Systems.UI2;

public class O_UIWindowLayoutMetrics
{
    public Vector2 MinSize = new Vector2();
    public Vector2 MaxSize = new Vector2(99999, 99999);
    public UIRectangleSpacingMetric Padding;

    public LayoutMode LayoutMode = LayoutMode.Free;
    public Vector2 ListSpacing;

    // new
    public bool FitX = true;
    public bool FitY = true;

    public bool GrowX = false;
    public bool GrowY = false;

    public override string ToString()
    {
        return "Metrics";
    }
}
