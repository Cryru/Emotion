#region Using

using System.Threading.Tasks;
using Emotion.Editor;
using Emotion.Graphics;
using Emotion.Graphics.Assets;


#endregion

#nullable enable

namespace Emotion.Game.World2D.Objects;

public class StaticTextureAssetObject : GameObject2D
{
    [AssetFileName<TextureAsset>] public string? AssetFile;

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