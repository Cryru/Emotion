#nullable enable

using Emotion.Game.Systems.UI;

namespace Emotion.Editor.EditorUI.Components;

public class EditorScrollArea : UIScrollArea
{
    public EditorScrollArea()
    {
        Paddings = new Rectangle(5, 5, 5, 5);
    }
    protected override bool RenderInternal(Renderer c)
    {
        c.RenderRectOutline(Bounds, EditorColorPalette.ButtonColor, 3 * GetScale());
        return base.RenderInternal(c);
    }
}
