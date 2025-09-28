#nullable enable

using Emotion.Game.Systems.UI;
using Emotion.Game.Systems.UI2;
using Emotion.Game.Systems.UI2.Editor;

namespace Emotion.Editor.EditorUI.Components;

public class SquareEditorButtonWithTexture : SquareEditorButton
{
    public UIPicture Texture { get; private set; }

    public Color IconColor = EditorColorPalette.TextColor;

    public SquareEditorButtonWithTexture(string texturePath, int size = 24, bool showBorder = false) : base()
    {
        if (!showBorder)
            Visuals.Border = 0;

        var texture = new UIPicture()
        {
            Texture = texturePath,
            Smooth = true,
            Layout =
            {
                SizingX = UISizing.Fixed(size),
                SizingY = UISizing.Fixed(size),
                AnchorAndParentAnchor = UIAnchor.CenterCenter,
            },
            IgnoreParentColor = true,
        };
        AddChild(texture);
        Texture = texture;

        RecalculateButtonColor();
    }

    protected override void RecalculateButtonColor()
    {
        base.RecalculateButtonColor();

        if (Texture == null) return; // RecalculateButtonColor is called in the base constructor
        Texture.ImageColor = Enabled ? IconColor : IconColor * 0.5f;
    }
}
