using Emotion.IO;
using Emotion.WIPUpdates.One.EditorUI.Components;
using Emotion.WIPUpdates.One.EditorUI.ObjectPropertiesEditorHelpers;

#nullable enable

namespace Emotion.Game.TwoDee;

public class SpriteAnimationFrame : IObjectEditorExtendedFunctionality<SpriteAnimationFrame>
{
    public int TextureId;
    public Rectangle UV;
    public RectangleAnchor AnchorSpot = RectangleAnchor.CenterCenter;
    public Vector2 AnchorOffset;

    public Vector2 GetCalculatedAnchor(Texture texture, out Rectangle uv)
    {
        uv = !UV.IsEmpty ? UV : new Rectangle(0, 0, texture.Size);
        Vector2 spot = new Rectangle(0, 0, uv.Size).GetRectangleAnchorSpot(AnchorSpot);
        return spot - AnchorOffset;
    }

    public override string ToString()
    {
        return $"Frame {TextureId} - {UV}";
    }

    #region Editor Functionality

    public void OnAfterEditorsSpawn(ComplexObjectEditor<SpriteAnimationFrame> editor)
    {
        if (editor.ParentObject is not SpriteAnimation anim) return;

        editor.EditorList.AddChild(new EditorLabel("==== Read Only ====")
        {
            Margins = new Primitives.Rectangle(0, 10, 0, 0)
        });

        VectorEditor anchorDisplay = new VectorEditor(2);
        anchorDisplay.SetValue(new Vector2(5, 5));
        editor.EditorList.AddChild(TypeEditor.WrapWithLabel(
            "Calculated Offset:",
            anchorDisplay
        ));
        Engine.CoroutineManager.StartCoroutine(UpdateCalculatedAnchorRoutine(anim, anchorDisplay));
    }

    private IEnumerator UpdateCalculatedAnchorRoutine(SpriteAnimation anim, VectorEditor ve)
    {
        yield return null;

        while (ve.Controller != null)
        {
            yield return null;

            SerializableAsset<TextureAsset>? textureHandle = anim.Textures.SafelyGet(TextureId);
            if (textureHandle == null) continue;

            TextureAsset? textureAsset = textureHandle.Get();
            if (textureAsset == null) continue;

            ve.SetValue(GetCalculatedAnchor(textureAsset.Texture, out _));
        }

        yield return null;
    }

    #endregion
}

