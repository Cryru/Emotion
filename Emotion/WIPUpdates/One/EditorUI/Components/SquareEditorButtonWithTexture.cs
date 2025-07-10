#nullable enable

using Emotion.Game.World.Editor;
using Emotion.UI;

namespace Emotion.WIPUpdates.One.EditorUI.Components;

public class SquareEditorButtonWithTexture : SquareEditorButton
{
    public UITexture Texture { get; private set; }

    public Color IconColor = MapEditorColorPalette.TextColor;

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
    }

    protected override void RecalculateButtonColor()
    {
        base.RecalculateButtonColor();
        Texture.WindowColor = Enabled ? IconColor : IconColor * 0.5f;
    }
}
