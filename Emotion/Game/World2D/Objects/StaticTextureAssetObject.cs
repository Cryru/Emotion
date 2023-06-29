#region Using

using System.Threading.Tasks;
using Emotion.Game.World2D.EditorHelpers;
using Emotion.Graphics;
using Emotion.IO;

#endregion

#nullable enable

namespace Emotion.Game.World2D;

public class StaticTextureAssetObject : GameObject2D
{
	[AssetFileName] public string? AssetFile;

	private TextureAsset? _asset;

	public override async Task LoadAssetsAsync()
	{
		if (string.IsNullOrEmpty(AssetFile)) return;

		_asset = await Engine.AssetLoader.GetAsync<TextureAsset>(AssetFile);
	}

	protected override void RenderInternal(RenderComposer c)
	{
		c.RenderSprite(Position, Size, Tint, _asset?.Texture);
	}
}