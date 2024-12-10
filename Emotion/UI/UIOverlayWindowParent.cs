
namespace Emotion.UI;

public class UIOverlayWindowParent : UIBaseWindow
{
    public UIOverlayWindowParent()
    {
        OrderInParent = 99;
    }

    protected override void AfterRenderChildren(RenderComposer c)
    {
        base.AfterRenderChildren(c);
        Controller.RenderOverlayChildren(this, c);
    }
}
