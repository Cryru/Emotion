using Emotion.Game.World.ThreeDee;
using Emotion.Game.World.TwoDee;

#nullable enable

namespace Emotion.Graphics;

// This API provides functionality for rendering an entity on its own.
// When rendering objects in a map they will be batched and processed, this is used for
// editors, UI, or other special cases.

public partial class Renderer
{
    public void RenderEntityStandalone(
        SpriteEntity entity,
        SpriteEntityMetaState state,
        Matrix4x4 modelMatrix
    )
    {
        PushModelMatrix(modelMatrix);
        int partCount = state.GetPartCount();
        for (int i = 0; i < partCount; i++)
        {
            if (!state.GetRenderData(i, out Texture texture, out Rectangle uv, out Vector2 anchor))
                continue;
            RenderSprite(anchor.ToVec3(), uv.Size, Color.White, texture, uv);
        }
        PopModelMatrix();
    }

    public void RenderEntityStandalone(
       MeshEntity entity,
       MeshEntityMetaState state,
       Vector3 position
    )
    {

    }
}
