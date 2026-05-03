#nullable enable

using static Emotion.Game.Systems.UI.UIBaseWindow;

namespace Emotion.Game.Systems.UI.Layouts;

public class OutsideParentLayout : FreeLayout
{
    protected override IntRectangle GetLayoutRect(UIBaseWindow self)
    {
        return self.CalculatedMetrics.Bounds;
    }
}
