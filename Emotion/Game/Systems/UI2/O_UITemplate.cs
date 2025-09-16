#nullable enable

namespace Emotion.Game.Systems.UI2;

public class O_UITemplate : GameDataObject
{
    public O_UIBaseWindow Window = new O_UIBaseWindow()
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