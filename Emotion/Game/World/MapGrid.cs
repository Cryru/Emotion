using Emotion.Common.Serialization;
using Emotion.Editor;
using Emotion.IO;
using Emotion.Utility;
using OpenGL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

#nullable enable

namespace Emotion.Game.World;

public abstract class MapGrid
{
    public string Name { get; set; } = "Untitled";

    public Vector2 GridSizeInTiles { get; set; } = new Vector2(1f);

    public Vector2 TileSize { get; set; } = new Vector2(1f);

    public bool AutoCenterOnMapOrigin { get; set; } = true;

    public Vector2 WorldOffset { get; set; }

    public virtual Task LoadAsync(BaseMap map)
    {
        return Task.CompletedTask;
    }

    public virtual void DebugRender(RenderComposer c)
    {

    }

    public virtual void Render(RenderComposer c)
    {

    }

    public void FillToMapSize(Vector2 mapSize)
    {
        Vector2 currentGridSize = GridSizeInTiles * TileSize;
        if (currentGridSize.X >= mapSize.X && currentGridSize.Y >= mapSize.Y) return;

        // todo: resize
    }

    public Vector2 WorldToGrid(Vector3 wPos)
    {
        return Vector2.Zero;
    }

    public Vector3 GridToWorld(Vector2 gPos)
    {
        return Vector3.Zero;
    }

    public Vector2 GetCoordinate2DFrom1D(int oneD)
    {
        return Vector2.Zero;
    }

    public int GetCoordinate1DFrom2D(Vector2 twoD)
    {
        return 0;
    }
}

public abstract class MapGrid<T> : MapGrid
{
    protected T[] _data = Array.Empty<T>();

    public virtual T GetValueInTile(Vector2 pos)
    {
        if (_data.Length == 0) return default!;

        int index = GetCoordinate1DFrom2D(pos);
        return _data[index];
    }

    public virtual void SetValueInTile(Vector2 pos, T value)
    {

    }

    public override void DebugRender(RenderComposer c)
    {
        for (int x = 0; x < GridSizeInTiles.X; x++)
        {
            for (int y = 0; y < GridSizeInTiles.Y; y++)
            {
                Vector2 gridTile = new Vector2(x, y);


            }
        }
    }
}

public class TextureSourceGrid : MapGrid<byte>
{
    [AssetFileName<TextureAsset>]
    public string? DataTexturePath;

    protected TextureSourceGridAsset? _texture;
    protected Vector2 _textureSize;

    [DontSerialize]
    protected class TextureSourceGridAsset : TextureAsset
    {
        public Vector2 Size;
        public byte[]? PixelData;

        protected override void UploadTexture(Vector2 size, byte[] pixels, bool flipped, PixelFormat pixelFormat)
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
        GridSizeInTiles = _textureSize;

        await base.LoadAsync(map);
    }
}

public class NumericMapGrid<T> : MapGrid<T> where T : INumber<T>
{
    [DontShowInEditor]
    public string Data { get => PackData(_data); set => _data = UnpackData(value); }

    public int DataStride { get; set; } = -1;

    #region Data Packing

    protected T[] UnpackData(string data)
    {
        // First pass - Count characters, including packed.
        var chars = 0;
        var lastSepIdx = 0;
        var charCount = 1;
        for (var i = 0; i < data.Length; i++)
        {
            char c = data[i];
            if (c == 'x')
            {
                ReadOnlySpan<char> sinceLast = data.AsSpan(lastSepIdx, i - lastSepIdx);
                if (int.TryParse(sinceLast, out int countPacked)) charCount = countPacked;
            }
            else if (c == ',')
            {
                chars += charCount;
                charCount = 1;
                lastSepIdx = i + 1;
            }
        }

        chars += charCount;

        // Second pass, unpack.
        var unpackedData = new T[chars];
        var arrayPtr = 0;
        lastSepIdx = 0;
        charCount = 1;
        for (var i = 0; i < data.Length; i++)
        {
            char c = data[i];
            if (c == 'x')
            {
                ReadOnlySpan<char> sinceLast = data.AsSpan(lastSepIdx, i - lastSepIdx);
                if (int.TryParse(sinceLast, out int countPacked))
                {
                    charCount = countPacked;
                    lastSepIdx = i + 1;
                }
            }
            else if (c == ',' || i == data.Length - 1)
            {
                // Dumping last character, pretend the index is after the string so we
                // read the final char below.
                if (i == data.Length - 1) i++;

                // Get tile value.
                ReadOnlySpan<char> sinceLast = data.AsSpan(lastSepIdx, i - lastSepIdx);
                T.TryParse(sinceLast, System.Globalization.NumberStyles.Any, null, out T? value);
                AssertNotNull(value);

                for (var j = 0; j < charCount; j++)
                {
                    unpackedData[arrayPtr] = value;
                    arrayPtr++;
                }

                charCount = 1;
                lastSepIdx = i + 1;
            }
        }

        return unpackedData;
    }

    protected string PackData(T[]? data)
    {
        if (data == null || data.Length == 0) return "";

        var b = new StringBuilder(data.Length * 2 + data.Length - 1);

        T lastNumber = data[0];
        uint lastNumberCount = 1;
        var firstAppended = false;
        for (var i = 1; i <= data.Length; i++)
        {
            // There is an extra loop to dump last number.
            T num = T.Zero;
            if (i != data.Length)
            {
                num = data[i];
                // Same number as before, increment counter.
                if (num == lastNumber)
                {
                    lastNumberCount++;
                    continue;
                }
            }

            if (firstAppended) b.Append(",");
            if (lastNumberCount == 1)
            {
                // "0"
                b.Append(lastNumber);
            }
            else
            {
                // "2x0" = "0, 0"
                b.Append(lastNumberCount);
                b.Append('x');
                b.Append(lastNumber);
            }

            lastNumber = num;
            lastNumberCount = 1;
            firstAppended = true;
        }

        return b.ToString();
    }

    #endregion
}
