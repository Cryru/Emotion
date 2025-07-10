
namespace Emotion.UI;

public class UIOverlayWindowParent : UIBaseWindow
{
    public bool NoClip;

    public UIOverlayWindowParent()
    {
        OrderInParent = 99;
    }

    protected override void AfterRenderChildren(RenderComposer c)
    {
        base.AfterRenderChildren(c);

        if (NoClip)
        {
            Rectangle? clip = c.CurrentState.ClipRect;
            c.SetClipRect(null);

            Controller.RenderOverlayChildren(this, c);

            c.SetClipRect(clip);
        }
        else
        {
            Controller.RenderOverlayChildren(this, c);
        }
    }
}
