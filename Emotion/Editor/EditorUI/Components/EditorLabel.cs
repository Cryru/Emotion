#nullable enable

using Emotion.Graphics.Text;

namespace Emotion.Editor.EditorUI.Components;

public enum LabelStyle
{
    NormalEditor,
    MapEditor
}

public class EditorLabel : UIText
{
    public EditorLabel()
    {
        TextColor = EditorColorPalette.TextColor;
        FontSize = EditorColorPalette.EditorButtonTextSize;
        IgnoreParentColor = true;
        Layout.AnchorAndParentAnchor = UIAnchor.CenterLeft;
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
                label.Effect = TextEffect.Outline(Color.Black, 2);
                label.FontSize = 23;
                break;
        }

        return label;
    }
}
