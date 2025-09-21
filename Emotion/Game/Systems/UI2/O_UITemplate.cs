#nullable enable

using Emotion.Game.Systems.UI;

namespace Emotion.Game.Systems.UI2;

public class O_UITemplate : GameDataObject
{
    public UIBaseWindow Window = new UIBaseWindow()
    {
        Layout =
        {
            SizingX = UISizing.Grow(),
            SizingY = UISizing.Grow()
        },
        Visuals =
        {
            Color = Color.White
        }
    };
}