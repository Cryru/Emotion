#nullable enable

namespace Emotion.Game.Systems.UI;

[Obsolete("Use UIBaseWindow with Visuals.BackgroundColor")]
public class UISolidColor : UIBaseWindow
{
    protected override bool RenderInternal(Renderer c)
    {
        //c.RenderSprite(Position, Size, _calculatedColor);
        return true;
    }
}