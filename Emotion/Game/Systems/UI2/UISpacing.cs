#nullable enable

namespace Emotion.Game.Systems.UI2;

public struct UISpacing
{
    public IntVector2 LeftTop;
    public IntVector2 RightBottom;

    public UISpacing(int left, int top, int right, int bottom)
    {
        LeftTop = new IntVector2(left, top);
        RightBottom = new IntVector2(right, bottom);
    }

    public override string ToString()
    {
        return $"{LeftTop}:{RightBottom}";
    }
}