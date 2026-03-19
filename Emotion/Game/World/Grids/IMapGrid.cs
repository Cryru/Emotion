#nullable enable

using Emotion.Core.Systems.IO;
using Emotion.Graphics.Camera;

namespace Emotion.Game.World.Grids;

public interface IMapGrid
{
    public string UniqueId { get; set; }

    public IEnumerator InitRoutine(GameMap.GridFriendAdapter mapAdapter);

    public void Done();

    public void Update(float dt); // todo: IUpdatableMapGrid

    public void Render(GameMap map, Renderer r, CameraCullingContext culling); // todo: IRenderableMapGrid

    internal bool _Save(string folder)
    {
        return true;
    }

    internal IEnumerator _LoadRoutine(string folder, Asset ass)
    {
        yield break;
    }

    internal IMapGrid Copy()
    {
        // todo :D
        return null!;
    }
}