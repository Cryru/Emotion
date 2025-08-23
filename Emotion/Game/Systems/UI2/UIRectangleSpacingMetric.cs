#nullable enable

namespace Emotion.Game.Systems.UI2;

public struct UIRectangleSpacingMetric
{
    public Vector2 TopLeft;
    public Vector2 BottomRight;

    public UIRectangleSpacingMetric(float left, float top, float right, float bottom)
    {
        TopLeft = new Vector2(left, top);
        BottomRight = new Vector2(right, bottom);
    }
}