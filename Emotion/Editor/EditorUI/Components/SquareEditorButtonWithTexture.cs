#nullable enable

using Emotion.Game.Systems.UI;

namespace Emotion.Editor.EditorUI.Components;

public class SquareEditorButtonWithTexture : SquareEditorButton
{
    public UITexture Texture { get; private set; }

    public Color IconColor = EditorColorPalette.TextColor;

    public SquareEditorButtonWithTexture(string texturePath, int size = 24) : base()
    {
        ShowOutline = false;

        var texture = new UITexture()
        {
            TextureFile = texturePath,
            Smooth = true,
            RenderSize = new Vector2(size),
            AnchorAndParentAnchor = UIAnchor.CenterCenter,
            IgnoreParentColor = true
        };
        AddChild(texture);
        Texture = texture;

        RecalculateButtonColor();
    }

    protected override void RecalculateButtonColor()
    {
        base.RecalculateButtonColor();

        if (Texture == null) return; // RecalculateButtonColor is called in the base constructor
        Texture.WindowColor = Enabled ? IconColor : IconColor * 0.5f;
    }
}
