using Emotion.Game.World.Editor;
using Emotion.UI;
using System.ComponentModel;
using System.Reflection.Emit;

namespace Emotion.WIPUpdates.One.EditorUI.Components;

public enum LabelStyle
{
    NormalEditor,
    MapEditor
}

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

    public static EditorLabel GetLabel(LabelStyle style, string txt)
    {
        var label = new EditorLabel(txt);

        switch (style)
        {
            case LabelStyle.MapEditor:
                label.OutlineColor = Color.Black;
                label.OutlineSize = 2;
                label.FontSize = 23;
                break;
        }

        return label;
    }
}
