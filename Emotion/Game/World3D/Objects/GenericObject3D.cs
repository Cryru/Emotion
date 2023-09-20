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

	public override async Task LoadAssetsAsync()
	{
		await base.LoadAssetsAsync();
		if (string.IsNullOrEmpty(EntityPath)) return;

		var asset = await Engine.AssetLoader.GetAsync<MeshAsset>(EntityPath);
		Entity = asset?.Entity;
	}
}