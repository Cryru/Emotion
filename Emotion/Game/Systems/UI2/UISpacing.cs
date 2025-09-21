#nullable enable

namespace Emotion.Game.Systems.UI2;

public struct UISpacing
{
    public Vector2 TopLeft;
    public Vector2 BottomRight;

    public UISpacing(float left, float top, float right, float bottom)
    {
        TopLeft = new Vector2(left, top);
        BottomRight = new Vector2(right, bottom);
    }

    public override string ToString()
    {
        return $"{TopLeft}:{BottomRight}";
    }
}