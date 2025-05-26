using Emotion.Game.World.Editor;
using Emotion.UI;

#nullable enable

namespace Emotion.WIPUpdates.One.EditorUI.Components;

public class EditorScrollArea : UIScrollArea
{
    public EditorScrollArea()
    {
        Paddings = new Primitives.Rectangle(5, 5, 5, 5);
    }
    protected override bool RenderInternal(RenderComposer c)
    {
        c.RenderRectOutline(Bounds, MapEditorColorPalette.ButtonColor, 3 * GetScale());
        return base.RenderInternal(c);
    }
}
