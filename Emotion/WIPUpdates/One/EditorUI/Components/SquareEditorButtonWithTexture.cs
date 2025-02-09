#nullable enable

using Emotion.Game.World.Editor;
using Emotion.UI;

namespace Emotion.WIPUpdates.One.EditorUI.Components;

public class SquareEditorButtonWithTexture : SquareEditorButton
{
    public UITexture Texture { get; private set; }

    public SquareEditorButtonWithTexture(string texturePath) : base()
    {
        ShowOutline = false;

        var texture = new UITexture()
        {
            TextureFile = texturePath,
            Smooth = true,
            RenderSize = new Vector2(24),
            AnchorAndParentAnchor = UIAnchor.CenterCenter,
            IgnoreParentColor = true
        };
        AddChild(texture);
        Texture = texture;
    }

    protected override void RecalculateButtonColor()
    {
        base.RecalculateButtonColor();
        Texture.WindowColor = Enabled ? MapEditorColorPalette.TextColor : MapEditorColorPalette.TextColor * 0.5f;
    }
}
