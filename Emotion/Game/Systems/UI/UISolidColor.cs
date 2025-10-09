#nullable enable

namespace Emotion.Game.Systems.UI;

public class UISolidColor : UIBaseWindow
{
    protected override bool RenderInternal(Renderer c)
    {
        c.RenderSprite(Position, Size, _calculatedColor);
        return true;
    }
}

public class UISolidOutline : UIBaseWindow
{
    //protected override void AfterRenderChildren(Renderer c)
    //{
    //    base.AfterRenderChildren(c);
    //    c.RenderRectOutline(Bounds, _calculatedColor, 1 * GetScale());
    //}
}