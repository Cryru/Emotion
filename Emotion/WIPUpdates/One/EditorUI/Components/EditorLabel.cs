using Emotion.Game.World.Editor;
using Emotion.UI;

namespace Emotion.WIPUpdates.One.EditorUI.Components;

public class EditorLabel : UIRichText
{
    public EditorLabel()
    {
        WindowColor = MapEditorColorPalette.TextColor;
        FontSize = MapEditorColorPalette.EditorButtonTextSize;
        IgnoreParentColor = true;
        Anchor = UIAnchor.CenterLeft;
        ParentAnchor = UIAnchor.CenterLeft;
    }

    public EditorLabel(string txt) : this()
    {
        Text = txt;
    }
}
