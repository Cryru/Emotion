using Emotion.IO;
using Emotion.WIPUpdates.One.EditorUI.Components;
using Emotion.WIPUpdates.One.EditorUI.ObjectPropertiesEditorHelpers;

#nullable enable

namespace Emotion.Game.TwoDee;

public class SpriteAnimationFramePoint
{
    public string Name = string.Empty;

    public Vector2 OriginOffset;

    public bool RelativeToPartOrigin = false;

    public override string ToString()
    {
        return Name;
    }
}

public class SpriteAnimationFrame : IObjectEditorExtendedFunctionality<SpriteAnimationFrame>
{
    public SerializableAsset<TextureAsset> Texture = new();
    public List<SpriteAnimationFramePoint> Points = new();

    public Vector2 AttachOffset;

    public Rectangle UV;

    public Vector2 GetCalculatedOrigin(SpriteAnimationBodyPart part)
    {
        Rectangle uv;
        if (UV.IsEmpty)
        {
            TextureAsset texture = Texture.Get();
            if (texture.Loaded)
                uv = new Primitives.Rectangle(0, 0, texture.Texture.Size);
            else
                uv = Rectangle.Empty;
        }
        else
        {
            uv = UV;
        }

        Vector2 spot = new Rectangle(0, 0, uv.Size).GetRectangleAnchorSpot(part.AttachSpot);
        return -spot + AttachOffset;
    }

    public override string ToString()
    {
        return $"Frame {Texture.Name}";
    }

    #region Editor Functionality

    public void OnAfterEditorsSpawn(ComplexObjectEditor<SpriteAnimationFrame> editor)
    {
        ObjectPropertyWindow? objectEditor = editor.GetParentOfKind<ObjectPropertyWindow>();
        if (objectEditor == null) return;

        SpriteAnimation? anim = objectEditor.GetParentObjectOfObjectOfKind<SpriteAnimation>(this);
        if (anim == null) return;

        SpriteAnimationBodyPart? bodyPart = objectEditor.GetParentObjectOfObjectOfKind<SpriteAnimationBodyPart>(this);
        if (bodyPart == null) return;

        editor.EditorList.AddChild(new EditorLabel("==== Read Only ====")
        {
            Margins = new Primitives.Rectangle(0, 10, 0, 0)
        });

        VectorEditor anchorDisplay = new VectorEditor(2);
        anchorDisplay.SetValue(new Vector2(5, 5));
        editor.EditorList.AddChild(TypeEditor.WrapWithLabel(
            "Calculated Origin:",
            anchorDisplay
        ));
        Engine.CoroutineManager.StartCoroutine(UpdateCalculatedAnchorRoutine(bodyPart, anchorDisplay));
    }

    private IEnumerator UpdateCalculatedAnchorRoutine(SpriteAnimationBodyPart part, VectorEditor ve)
    {
        yield return null;

        while (ve.Controller != null)
        {
            yield return null;

            TextureAsset textureAsset = Texture.Get();
            if (!textureAsset.Loaded) continue;

            ve.SetValue(GetCalculatedOrigin(part).Round());
        }

        yield return null;
    }

    #endregion
}

