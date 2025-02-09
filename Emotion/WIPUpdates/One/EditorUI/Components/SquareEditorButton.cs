#nullable enable

using Emotion.UI;

namespace Emotion.WIPUpdates.One.EditorUI.Components;

public class SquareEditorButton : EditorButton
{
    public bool ShowOutline = true;

    public SquareEditorButton() : base()
    {
    }

    public SquareEditorButton(string name) : base(name)
    {
    }

    public override void AttachedToController(UIController controller)
    {
        Paddings = new Primitives.Rectangle(3, 3, 3, 3);
        MinSizeX = 30;
        MinSizeY = 30;

        _label.AnchorAndParentAnchor = UIAnchor.CenterCenter;

        base.AttachedToController(controller);
    }

    protected override void AfterRenderChildren(RenderComposer c)
    {
        base.AfterRenderChildren(c);

        if (ShowOutline)
            c.RenderOutline(Bounds, Color.White * 0.5f, 1 * GetScale());
    }
}
