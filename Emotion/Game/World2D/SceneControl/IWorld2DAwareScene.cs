#nullable enable

#region Using

using System.Threading.Tasks;

#endregion

namespace Emotion.Game.World2D.SceneControl;

/// <summary>
/// Scene that is aware of Map2D stuff.
/// The map instance passed will always be of the same type as the map loaded in the current scene.
/// Usually you would inherit World2DScene (instead of Scene) but you could also implement your own
/// using this interface.
/// </summary>
public interface IWorld2DAwareScene
{
	public Map2D GetCurrentMap();
	public Task ChangeMapAsync(Map2D map);
}