#region Using

using Emotion.Graphics;

#endregion

namespace Emotion.UI;

public class UISolidColor : UIBaseWindow
{
    protected override bool RenderInternal(RenderComposer c)
    {
        c.RenderSprite(Position, Size, _calculatedColor);
        return true;
    }
}

public class UISolidOutline : UIBaseWindow
{
    protected override void AfterRenderChildren(RenderComposer c)
    {
        base.AfterRenderChildren(c);
        c.RenderRectOutline(Bounds, _calculatedColor, 1 * GetScale());
    }
}