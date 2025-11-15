#nullable enable

using Emotion.Graphics.Camera;

namespace Emotion.Game.World;

public interface IMapGrid
{
    public string UniqueId { get; set; }

    public IEnumerator InitRoutine(GameMap.GridFriendAdapter mapAdapter);

    public void Done();

    public void Update(float dt);

    public void Render(GameMap map, Renderer r, CameraCullingContext culling);

    public void _Save(string folder)
    {

    }
}