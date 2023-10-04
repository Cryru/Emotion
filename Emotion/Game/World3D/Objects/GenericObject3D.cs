#nullable enable

#region Using

using System.Threading.Tasks;
using Emotion.Editor;
using Emotion.IO;

#endregion

namespace Emotion.Game.World3D.Objects;

public class GenericObject3D : GameObject3D
{
	[AssetFileName<MeshAsset>]
	public string? EntityPath;

	private string? _setAnimationToOnLoad = null;

	public override async Task LoadAssetsAsync()
	{
		await base.LoadAssetsAsync();
		if (string.IsNullOrEmpty(EntityPath)) return;

		var asset = await Engine.AssetLoader.GetAsync<MeshAsset>(EntityPath);
		Entity = asset?.Entity;

	}

	public override void Init()
	{
		base.Init();

		if (_setAnimationToOnLoad != null)
		{
			SetAnimation(_setAnimationToOnLoad);
			_setAnimationToOnLoad = null;
		}
	}

	public override void SetAnimation(string? name)
    {
		if (ObjectState == World2D.ObjectState.None && name != null)
		{
			_setAnimationToOnLoad = name;
			return;
        }

        base.SetAnimation(name);
    }
}