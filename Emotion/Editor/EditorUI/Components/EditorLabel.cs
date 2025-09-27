#nullable enable

using Emotion.Game.Systems.UI;
using Emotion.Game.Systems.UI2;

namespace Emotion.Editor.EditorUI.Components;

public enum LabelStyle
{
    NormalEditor,
    MapEditor
}

public class EditorLabel : NewUIText
{
    public EditorLabel()
    {
        WindowColor = EditorColorPalette.TextColor;
        FontSize = EditorColorPalette.EditorButtonTextSize;
        IgnoreParentColor = true;
        Layout.Anchor = UIAnchor.CenterLeft;
        Layout.ParentAnchor = UIAnchor.CenterLeft;
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
