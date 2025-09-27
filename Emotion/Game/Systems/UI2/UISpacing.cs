#nullable enable

namespace Emotion.Game.Systems.UI2;

public struct UISpacing
{
    public IntVector2 TopLeft;
    public IntVector2 BottomRight;

    public UISpacing(int left, int top, int right, int bottom)
    {
        TopLeft = new IntVector2(left, top);
        BottomRight = new IntVector2(right, bottom);
    }

    public override string ToString()
    {
        return $"{TopLeft}:{BottomRight}";
    }
}