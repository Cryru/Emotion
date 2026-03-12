#nullable enable

using Emotion.Game.World.Components;
using Emotion.Graphics.Camera;

namespace Emotion.Game.World.Systems;

public class SimpleSpriteRenderSystem : WorldSystem<SimpleSpriteComponent>, IWorldRenderSystem
{
    protected override void InitInternal()
    {

    }

    protected override void OnComponentListChanged()
    {
        Components.Sort(static (x, y) => MathF.Sign(x.Object.Z - y.Object.Z));
    }

    public void Render(Renderer r, in CameraCullingContext culling)
    {
        Rectangle cullRect = culling.Rect2D;
        Frustum cullFrustum = culling.Frustum;

        for (int i = 0; i < Components.Count; i++)
        {
            SimpleSpriteComponent component = Components[i];
            GameObject obj = component.Object;

            bool cull = false;
            if (!obj.Visible)
            {
                cull = true;
            }
            else if (!obj.AlwaysRender)
            {
                if (culling.Is2D)
                {
                    if (!obj.GetBoundingRect().Intersects(cullRect))
                        cull = true;
                }
                else
                {
                    if (!cullFrustum.IntersectsOrContainsCube(obj.GetBoundingCube()))
                        cull = true;
                }
            }
            if (cull) continue;

            r.PushModelMatrix(component.Object.GetModelMatrix());
            r.RenderSprite(component.CalculatedOffset, component.Texture);
            r.PopModelMatrix();
        }
    }
}
