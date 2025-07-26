using Emotion.Common.Serialization;
using Emotion.Editor;
using Emotion.Graphics.Assets;
using Emotion.Utility;
using OpenGL;
using System.Threading.Tasks;

#nullable enable

namespace Emotion.Game.World.Grid;

public class TextureSourceGrid : MapGrid<byte>
{
    [AssetFileName<TextureAsset>]
    public string? DataTexturePath;

    protected TextureSourceGridAsset? _texture;
    protected Vector2 _textureSize;

    public TextureSourceGrid(string texturePath)
    {
        DataTexturePath = texturePath;
    }

    // serialization constructor
    protected TextureSourceGrid()
    {

    }

    [DontSerialize]
    protected class TextureSourceGridAsset : TextureAsset
    {
        public Vector2 Size;
        public byte[]? PixelData;

        protected override void UploadTexture(Vector2 size, byte[] pixels, bool flipped, PixelFormat pixelFormat, bool rentedMemory)
        {
            Size = size;

            Assert(!flipped);

            if (pixelFormat == PixelFormat.Red)
            {
                PixelData = pixels;
            }
            else if (pixelFormat == PixelFormat.Bgra || pixelFormat == PixelFormat.Rgba)
            {
                //ImageUtil.FlipImageY(pixels, (int)size.Y);
                if (pixelFormat == PixelFormat.Bgra)
                    ImageUtil.BgraToRgba(pixels, true);
                PixelData = ImageUtil.RgbaToA(pixels, true);
            }
            else
            {
                Assert(false, "Unknown texture source grid pixel format.");
            }
        }
    }

    public override async Task LoadAsync(BaseMap map)
    {
        if (string.IsNullOrEmpty(DataTexturePath)) return;

        _texture = await Engine.AssetLoader.GetAsync<TextureSourceGridAsset>(DataTexturePath);
        if (_texture == null) return;

        _data = _texture.PixelData;
        _textureSize = _texture.Size;

        TileSize = new Vector2(1f);
        SizeInTiles = _textureSize;

        await base.LoadAsync(map);
    }
}
