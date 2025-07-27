#nullable enable

using Emotion.UI;

namespace Emotion.WIPUpdates.One.EditorUI.Components;

public class SquareEditorButton : EditorButton
{
    public bool ShowOutline = true;

    public SquareEditorButton() : base()
    {
        Paddings = new Primitives.Rectangle(3, 3, 3, 3);
        MinSizeX = 30;
        MinSizeY = 30;
        _label.AnchorAndParentAnchor = UIAnchor.CenterCenter;
    }

    public SquareEditorButton(string name) : base(name)
    {
        Paddings = new Primitives.Rectangle(3, 3, 3, 3);
        MinSizeX = 30;
        MinSizeY = 30;
        _label.AnchorAndParentAnchor = UIAnchor.CenterCenter;
    }

    protected override void AfterRenderChildren(RenderComposer c)
    {
        base.AfterRenderChildren(c);

        if (ShowOutline)
            c.RenderRectOutline(Bounds, Color.White * 0.5f, 1 * GetScale());
    }
}
