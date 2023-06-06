using Emotion.Game.World2D.EditorHelpers;
using Emotion.UI;

namespace Emotion.Editor.EditorHelpers;

#nullable enable

public class EditorScrollBar : UIScrollbar
{
    public EditorScrollBar()
    {
        DefaultSelectorColor = MapEditorColorPalette.ButtonColor;
        SelectorMouseInColor = MapEditorColorPalette.ActiveButtonColor;
        WindowColor = Color.Black * 0.5f;
        Anchor = UIAnchor.TopRight;
        ParentAnchor = UIAnchor.TopRight;
        MinSize = new Vector2(5, 0);
        MaxSize = new Vector2(5, 9999);
    }
}