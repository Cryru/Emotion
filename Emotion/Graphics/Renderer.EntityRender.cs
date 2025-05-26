using Emotion.Game.ThreeDee;
using Emotion.Game.TwoDee;
using Emotion.Graphics.ThreeDee;

#nullable enable

namespace Emotion.Graphics;

// This API provides functionality for rendering an entity on its own.
// When rendering objects in a map they will be batched and processed, this is used for
// editors, UI, or other special cases.

public partial class RenderComposer
{
    public void RenderEntityStandalone(
        SpriteEntity entity,
        SpriteEntityMetaState state,
        Vector3 position
    )
    {
        if (entity.PixelArt)
            position = position.Round();

        int partCount = state.GetPartCount();
        for (int i = 0; i < partCount; i++)
        {
            state.GetRenderData(i, out Texture texture, out Rectangle uv, out Vector2 anchor);
            RenderSprite(position + anchor.ToVec3(), uv.Size, Color.White, texture, uv);
        }
    }

    public void RenderEntityStandalone(
       MeshEntity entity,
       MeshEntityMetaState state,
       Vector3 position
    )
    {

    }
}
