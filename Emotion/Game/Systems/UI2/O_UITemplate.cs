#nullable enable

using Emotion.Game.Systems.UI;

namespace Emotion.Game.Systems.UI2;

public class O_UITemplate : GameDataObject
{
    public UIBaseWindow Window = new UIContainer()
    {
        Visuals =
        {
            Color = Color.White
        }
    };
}