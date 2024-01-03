#nullable enable

#region Using

using Emotion.Editor.EditorHelpers;
using Emotion.Game.ThreeDee.Editor;
using Emotion.Game.World;
using Emotion.Game.World3D.Objects;
using System.Threading.Tasks;

#endregion

namespace Emotion.Game.World3D;

public class Map3D : BaseMap
{
    public LightModel LightModel = new();
    public bool RenderShadowMap = false;

    public override List<Type> GetValidObjectTypes()
    {
        List<Type>? types = EditorUtility.GetTypesWhichInherit<GameObject3D>();

        // Editor only
        types.Remove(typeof(TranslationGizmo));
        types.Remove(typeof(InfiniteGrid));

        return types;
    }

    protected override async Task InitAsyncInternal()
    {
        await Task.Run(Engine.Renderer.MeshEntityRenderer.EnsureAssetsLoaded);
        await base.InitAsyncInternal();
    }

    public static Comparison<BaseGameObject> ObjectComparison = ObjectSort; // Prevent delegate allocation

    protected static int ObjectSort(BaseGameObject x, BaseGameObject y)
    {
        var camera = Engine.Renderer.Camera;
        float distToA = Vector3.Distance(camera.Position, x.Position);
        float distToB = Vector3.Distance(camera.Position, y.Position);
        return MathF.Sign(distToB - distToA);
    }

    public override void Render(RenderComposer c)
    {
        if (!Initialized) return;

        c.MeshEntityRenderer.StartScene(c);
        // (todo: in the future the shadow frustums will require separate ones, but we could process all into pass buckets)
        foreach (var obj in ObjectsEnum())
        {
            obj.Render(c);
        }
        c.MeshEntityRenderer.EndScene(c, this);








        //if (RenderShadowMap)
        //{
        //    // todo: cull objects per cascade
        //    int cascades = c.RenderStream.MeshRenderer.GetShadowMapCascadeCount();
        //    for (var cascIdx = 0; cascIdx < cascades; cascIdx++)
        //    {
        //        c.RenderStream.MeshRenderer.StartRenderShadowMap(cascIdx, c, LightModel);

        //        for (var i = 0; i < _renderQuery.Count; i++)
        //        {
        //            GameObject3D obj = _renderQuery[i];
        //            obj.Render(c);
        //        }

        //        c.RenderStream.MeshRenderer.EndRenderShadowMap(c);
        //    }
        //}

        //var anyTransparent = false;
        //for (var i = 0; i < _renderQuery.Count; i++)
        //{
        //    GameObject3D obj = _renderQuery[i];
        //    if (obj.IsTransparent())
        //    {
        //        anyTransparent = true;
        //        continue;
        //    }

        //    obj.Render(c);
        //}

        //if (anyTransparent)
        //{
        //    // Transparent objects are first sorted by distance from the camera, so they
        //    // can be drawn from furthest to closest and alpha blend with each other.
        //    //
        //    // Then they are drawn twice - first without writing to the color buffer
        //    // so they can populate the depth buffer with the highest depth, and then
        //    // drawn normally. This will prevent faces of one object from occluding other
        //    // faces in that same object, since they will be depth clipped.

        //    _renderQuery.Sort(ObjectComparison);
        //    c.ToggleRenderColor(false);
        //    for (var i = 0; i < _renderQuery.Count; i++)
        //    {
        //        GameObject3D obj = _renderQuery[i];
        //        if (!obj.IsTransparent()) continue;
        //        obj.Render(c);
        //    }

        //    c.ToggleRenderColor(true);
        //    for (var i = 0; i < _renderQuery.Count; i++)
        //    {
        //        GameObject3D obj = _renderQuery[i];
        //        if (!obj.IsTransparent()) continue;
        //        obj.Render(c);
        //    }
        //}
    }
}