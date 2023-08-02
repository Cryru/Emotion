#nullable enable

#region Using

using System.Threading.Tasks;
using Emotion.Game.World.SceneControl;
using Emotion.Game.World2D;
using Emotion.IO;

#endregion

namespace Emotion.Game.World.Editor;

public abstract class WorldBaseEditorGeneric<T> : WorldBaseEditor where T : Map2D
{
	public new T? CurrentMap
	{
		get => (T?) base.CurrentMap;
	}

	protected IWorldAwareScene<T> _scene;

	protected WorldBaseEditorGeneric(IWorldAwareScene<T> scene, Type mapType) : base(mapType)
	{
		_scene = scene;
	}

	protected override BaseMap? GetCurrentSceneMap()
	{
		return _scene.GetCurrentMap();
	}

	protected override Task ChangeSceneMapAsync(BaseMap map)
	{
		return map is T mapT ? _scene.ChangeMapAsync(mapT) : Task.CompletedTask;
	}

	protected override XMLAssetMarkerClass GetCurrentMapAsXMLAsset(BaseMap map)
	{
		return XMLAsset<T>.CreateFromContent((T) map);
	}
}