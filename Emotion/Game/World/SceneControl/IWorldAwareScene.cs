#nullable enable

#region Using

using System.Threading.Tasks;

#endregion

namespace Emotion.Game.World.SceneControl;

/// <summary>
/// Scene that is aware of world class stuff.
/// The map instance passed will always be of the same type as the map loaded in the current scene.
/// Usually you would inherit World2DScene/World3DScene (instead of Scene) but
/// you could also implement your own using this interface.
/// </summary>
public interface IWorldAwareScene<T> where T : BaseMap
{
	public T? GetCurrentMap();
	public Task ChangeMapAsync(T map);
}