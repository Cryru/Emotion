#nullable enable

using Emotion.Core.Systems.IO;
using Emotion.Editor.EditorUI.Components;
using Emotion.Editor.EditorUI.ObjectPropertiesEditorHelpers;
using Emotion.Graphics.Assets;

namespace Emotion.Game.World.TwoDee;

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
    public TextureReference Texture = new();
    public List<SpriteAnimationFramePoint> Points = new();

    public Vector2 AttachOffset;

    public Rectangle UV;

    private Vector2 GetPartTextureSize()
    {
        Texture? texture = Texture.GetObject();
        if (texture != null)
            return texture.Size;
        return Vector2.One;
    }

    public Vector2 GetCalculatedOrigin(SpriteAnimationBodyPart part)
    {
        Rectangle uv;
        if (UV.IsEmpty)
            uv = new Primitives.Rectangle(0, 0, GetPartTextureSize());
        else
            uv = UV;

        Vector2 spot = new Rectangle(0, 0, uv.Size).GetRectangleAnchorSpot(part.AttachSpot);
        //if (flipX) attachOffset.X = -attachOffset.X;
        return -spot + AttachOffset;
    }

    public Rectangle GetBoundingRect(SpriteAnimationBodyPart part)
    {
        Vector2 size;
        if (UV.IsEmpty)
            size = GetPartTextureSize();
        else
            size = UV.Size;

        Vector2 origin = new Rectangle(0, 0, size).GetRectangleAnchorSpot(part.AttachSpot);
        origin = -origin + AttachOffset;

        Rectangle r = new Rectangle(0, 0, size);
        r.Position += origin;
        return r;
    }

    public override string ToString()
    {
        return $"Frame {Texture}";
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
            Margins = new Rectangle(0, 10, 0, 0)
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

        while (ve.State == Systems.UI.UIWindowState.Open)
        {
            yield return Texture.PerformLoading(null, null);

            Texture? texture = Texture.GetObject();
            if (texture == null) continue;

            ve.SetValue(GetCalculatedOrigin(part));
        }
    }

    #endregion
}

