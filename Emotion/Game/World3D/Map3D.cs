#nullable enable

#region Using

using Emotion.Editor.EditorHelpers;
using Emotion.Game.ThreeDee.Editor;
using Emotion.Game.World;
using Emotion.Game.World.Grid;
using Emotion.Game.World3D.Objects;
using System.Threading.Tasks;

#endregion

namespace Emotion.Game.World3D;

public class Map3D : BaseMap
{
    public LightModel LightModel = new();

    public Map3D(Vector2 size, string mapName = "Unnamed Map") : base(size, mapName)
    {
    }

    // Serialization constructor
    protected Map3D()
    {
    }

    public override List<Type> GetValidObjectTypes()
    {
        List<Type>? types = EditorUtility.GetTypesWhichInherit<GameObject3D>();

        // Editor only
        types.Remove(typeof(TranslationGizmo));
        types.Remove(typeof(InfiniteGrid));

        // System
        types.Remove(typeof(TerrainChunkObject));

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

        base.Render(c);

        c.MeshEntityRenderer.StartScene(c);
        // todo: frustum culling
        // todo: in the future the shadow frustums will require separate ones
        // todo: maybe a better solution would be to pass the map into the renderer and have it extract
        // the objects and frustum call (this would also prob mean that .Render() is useless)
        foreach (var obj in ObjectsEnum())
        {
            obj.Render(c);
        }
        c.MeshEntityRenderer.EndScene(c, this);
    }
}