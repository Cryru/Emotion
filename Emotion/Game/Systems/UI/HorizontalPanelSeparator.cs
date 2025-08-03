#nullable enable

namespace Emotion.Game.Systems.UI;

public class HorizontalPanelSeparator : UIBaseWindow
{
    public float SeparationPercent = 0.5f;

    public HorizontalPanelSeparator()
    {
        MinSizeX = 10;
    }
}
