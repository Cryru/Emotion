#region Using

using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Emotion.Common;
using Emotion.Common.Serialization;
using Emotion.Graphics;
using Emotion.Graphics.Batches;
using Emotion.Graphics.Data;
using Emotion.Graphics.Objects;
using Emotion.IO;
using Emotion.Primitives;
using Emotion.Utility;

#endregion

#nullable enable

namespace Emotion.Game.World2D
{
    // Terminology
    //
    // layer - The map layer currently drawing.
    // t - The tile currently drawing from [layer]. 
    //      tId - The id of the tile within all tilesets combined.
    //      tRect - The location [t] should be drawn to.
    // ts - The tileset of [t]. 
    //      tsId - The id of [ts] within the collection.
    //      tsOffset - The [tId] within the scope of the current tileset. An id and image within the map containing the ti.
    // ti - The tile image, within the [ts] which represents the image of [tsOffset].
    //      tiUv - The rectangle where the [ti] is located within the [ts] texture.

    /// <summary>
    /// Represents a single tile layer.
    /// </summary>
    public class Map2DTileMapLayer
    {
        public const uint FLIPPED_HORIZONTALLY_FLAG = 0x80000000;
        public const uint FLIPPED_VERTICALLY_FLAG = 0x40000000;
        public const uint FLIPPED_DIAGONALLY_FLAG = 0x20000000;

        [SerializeNonPublicGetSet]
        public string Name { get; protected set; }

        [SerializeNonPublicGetSet]
        public string StringData { get; protected set; }

        public bool Visible { get; set; } = true;

        protected uint[]? _readDataCached;

        public Map2DTileMapLayer(string name, ReadOnlySpan<uint> data)
        {
            Name = name;

            var b = new StringBuilder(data.Length * 2 + data.Length - 1);

            uint lastNumber = data[0];
            uint lastNumberCount = 1;
            var firstAppended = false;
            for (var i = 1; i <= data.Length; i++)
            {
                // There is an extra loop to dump last number.
                uint num = 0;
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

            StringData = b.ToString();
        }

        // Serialization constructor
        protected Map2DTileMapLayer()
        {
            Name = "";
            StringData = "";
        }

        private uint[] UnpackData()
        {
            // First pass - Count characters, including packed.
            var chars = 0;
            var lastSepIdx = 0;
            var charCount = 1;
            for (var i = 0; i < StringData.Length; i++)
            {
                char c = StringData[i];
                if (c == 'x')
                {
                    ReadOnlySpan<char> sinceLast = StringData.AsSpan(lastSepIdx, i - lastSepIdx);
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
            var unpackedData = new uint[chars];
            var arrayPtr = 0;
            lastSepIdx = 0;
            charCount = 1;
            for (var i = 0; i < StringData.Length; i++)
            {
                char c = StringData[i];
                if (c == 'x')
                {
                    ReadOnlySpan<char> sinceLast = StringData.AsSpan(lastSepIdx, i - lastSepIdx);
                    if (int.TryParse(sinceLast, out int countPacked))
                    {
                        charCount = countPacked;
                        lastSepIdx = i + 1;
                    }
                }
                else if (c == ',' || i == StringData.Length - 1)
                {
                    // Dumping last character, pretend the index is after the string so we
                    // read the final char below.
                    if (i == StringData.Length - 1) i++;

                    // Get tile value.
                    ReadOnlySpan<char> sinceLast = StringData.AsSpan(lastSepIdx, i - lastSepIdx);
                    uint.TryParse(sinceLast, out uint value);

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

        public void GetTileData(int tileIdx, out uint tid, out bool flipX, out bool flipY, out bool flipD)
        {
            _readDataCached ??= UnpackData();

            if (tileIdx < 0 || tileIdx >= _readDataCached.Length)
            {
                tid = 0;
                flipX = false;
                flipY = false;
                flipD = false;
                return;
            }

            uint data = _readDataCached[tileIdx];
            flipX = (data & FLIPPED_HORIZONTALLY_FLAG) != 0;
            flipY = (data & FLIPPED_VERTICALLY_FLAG) != 0;
            flipD = (data & FLIPPED_DIAGONALLY_FLAG) != 0;
            tid = data & ~(FLIPPED_HORIZONTALLY_FLAG | FLIPPED_VERTICALLY_FLAG | FLIPPED_DIAGONALLY_FLAG);
        }
    }

    /// <summary>
    /// Represents a tileset image that contains all the tiles used by a tile layer.
    /// </summary>
    public class Map2DTileset
    {
        public string AssetFile;
        public int FirstTileId;

        public int Width;
        public float Spacing = 0f;
        public float Margin = 0f;

        public Map2DTileset(string assetFile, int firstTileId, int width)
        {
            AssetFile = assetFile;
            FirstTileId = firstTileId;
            Width = width;
        }

        // Serialization constructor
        protected Map2DTileset()
        {
            AssetFile = "";
            FirstTileId = 0;
        }
    }

    /// <summary>
    /// Handles tilemap data representation and management for Map2D
    /// </summary>
    public class Map2DTileMapData
    {
        public Map2DTileset?[] Tilesets;
        public Vector2 TileSize;
        public Vector2 SizeInTiles;
        public List<Map2DTileMapLayer> Layers = new();

        public Color BackgroundColor = new(0);

        #region Runtime State

        [DontSerialize] public TextureAsset?[]? TilesetsLoaded;

        /// <summary>
        /// Cached render data for tiles per layer.
        /// </summary>
        protected VertexData[]?[]? _cachedTileRenderData;

        /// <summary>
        /// Cached render data for tiles per layer.
        /// </summary>
        protected Texture[]?[]? _cachedTileTextures;

        #endregion

        public Map2DTileMapData(Vector2 tileSize, Vector2 sizeInTiles, int tileSetsCount)
        {
            TileSize = tileSize;
            SizeInTiles = sizeInTiles;
            Tilesets = new Map2DTileset[tileSetsCount];
        }

        // Serialization constructor
        protected Map2DTileMapData()
        {
            Tilesets = null!;
        }

        /// <summary>
        /// Load tileset assets frm the AssetManager. Called by Map2D during Init.
        /// </summary>
        public async Task LoadTileDataAsync()
        {
            if (Tilesets.Length == 0) return;

            var assets = new Task<TextureAsset?>[Tilesets.Length];
            for (var i = 0; i < assets.Length; i++)
            {
                Map2DTileset? tileSet = Tilesets[i];
                if (tileSet == null)
                {
                    assets[i] = Task.FromResult((TextureAsset?) null);
                    continue;
                }

                string tilesetFile = tileSet.AssetFile;
                assets[i] = Engine.AssetLoader.GetAsync<TextureAsset>(tilesetFile);
            }

            // ReSharper disable once CoVariantArrayConversion
            await Task.WhenAll(assets);

            TilesetsLoaded = new TextureAsset[assets.Length];
            for (var i = 0; i < assets.Length; i++)
            {
                TilesetsLoaded[i] = assets[i].IsCompletedSuccessfully ? assets[i].Result : null;
            }

            CreateRenderCache();
        }

        private void CreateRenderCache()
        {
            Debug.Assert(TilesetsLoaded != null);

            _cachedTileRenderData = null;
            _cachedTileTextures = null;
            if (Layers.Count == 0) return;

            var tileColumns = (int) SizeInTiles.X;
            var tileRows = (int) SizeInTiles.Y;
            int totalTileSize = tileRows * tileColumns;

            _cachedTileRenderData = new VertexData[Layers.Count][];
            _cachedTileTextures = new Texture[Layers.Count][];
            for (var layerIdx = 0; layerIdx < Layers.Count; layerIdx++)
            {
                Map2DTileMapLayer layer = Layers[layerIdx];

                var currentCache = new VertexData[totalTileSize * 4];
                var currentTextureCache = new Texture[totalTileSize];
                var dataSpan = new Span<VertexData>(currentCache);

                for (var y = 0; y < tileRows; y++)
                {
                    int yIdx = y * tileColumns;
                    for (var x = 0; x < tileColumns; x++)
                    {
                        int tileIdx = yIdx + x;
                        Span<VertexData> tileData = dataSpan.Slice(tileIdx * 4, 4);

                        layer.GetTileData(tileIdx, out uint tId, out bool flipX, out bool flipY, out bool _);

                        // Get the id of the tile, and if empty skip it
                        if (tId == 0)
                        {
                            for (var i = 0; i < tileData.Length; i++)
                            {
                                tileData[i].Color = 0;
                            }

                            continue;
                        }

                        // Find which tileset the tId belongs in.
                        Rectangle tiUv = GetUvFromTileImageId(tId, out int tsId);

                        // Calculate dimensions of the tile.
                        Vector2 position = new Vector2(x, y) * TileSize;
                        var v3 = new Vector3(position, layerIdx);
                        var c = new Color(255, 255, 255); //layer.Opacity * 255);
                        TextureAsset? tileSetAsset = TilesetsLoaded[tsId];
                        if (tileSetAsset != null) currentTextureCache[tileIdx] = tileSetAsset.Texture;

                        // Write to tilemap mesh.
                        VertexData.SpriteToVertexData(tileData, v3, TileSize, c, tileSetAsset?.Texture, tiUv, flipX, flipY);
                    }
                }

                _cachedTileRenderData[layerIdx] = currentCache;
                _cachedTileTextures[layerIdx] = currentTextureCache;
            }
        }

        /// <summary>
        /// Render all tile layers in the specified index range.
        /// </summary>
        /// <param name="composer"></param>
        /// <param name="clipRect">A rectangle bound containing the part of space to render.</param>
        /// <param name="start">The layer to start from, inclusive.</param>
        /// <param name="end">The layer to render to, exclusive. -1 for until end</param>
        public void RenderTileLayerRange(RenderComposer composer, Rectangle clipRect, int start = 0, int end = -1)
        {
            end = end == -1 ? Layers.Count : end;
            for (int layerId = start; layerId < end; layerId++)
            {
                Map2DTileMapLayer layer = Layers[layerId];
                if (!layer.Visible) continue;
                RenderLayer(composer, layerId, clipRect);
            }
        }

        /// <summary>
        /// Render a particular tile layer.
        /// </summary>
        public virtual void RenderLayer(RenderComposer composer, int layerIdx, Rectangle clipVal)
        {
            VertexData[]? renderCache = _cachedTileRenderData?[layerIdx];
            Texture[]? textureCache = _cachedTileTextures?[layerIdx];
            if (renderCache == null || textureCache == null) return;

            var yStart = (int) Maths.Clamp(MathF.Floor(clipVal.Y / TileSize.Y), 0, SizeInTiles.Y);
            var yEnd = (int) Maths.Clamp(MathF.Ceiling(clipVal.Bottom / TileSize.Y), 0, SizeInTiles.Y);
            var xStart = (int) Maths.Clamp(MathF.Floor(clipVal.X / TileSize.X), 0, SizeInTiles.X);
            var xEnd = (int) Maths.Clamp(MathF.Ceiling(clipVal.Right / TileSize.X), 0, SizeInTiles.X);

            for (int y = yStart; y < yEnd; y++)
            {
                var yIdx = (int) (y * SizeInTiles.X);
                for (int x = xStart; x < xEnd; x++)
                {
                    int tileIdx = yIdx + x;
                    int tileVertexIdx = tileIdx * 4;
                    if (renderCache[tileVertexIdx].Color == 0) continue;

                    Span<VertexData> vertices = composer.RenderStream.GetStreamMemory(4, BatchMode.Quad, textureCache[tileIdx]);
                    for (var i = 0; i < vertices.Length; i++)
                    {
                        vertices[i] = renderCache[tileVertexIdx + i];
                    }
                }
            }
        }

        /// <summary>
        /// Render the entire tilemap.
        /// </summary>
        /// <param name="c"></param>
        /// <param name="clipRect">A rectangle bound containing the part of space to render.</param>
        public void RenderTileMap(RenderComposer c, Rectangle clipRect)
        {
            c.RenderSprite(clipRect, BackgroundColor);
            RenderTileLayerRange(c, clipRect);
        }

        #region External Helpers

        /// <summary>
        /// Get the map layer index by name.
        /// </summary>
        public int GetMapLayerByName(string layerName)
        {
            for (var i = 0; i < Layers.Count; i++)
            {
                if (Layers[i].Name == layerName) return i;
            }

            return -1;
        }

        /// <summary>
        /// Get the render cache of the specified map layer. Allows you to
        /// modify the way tiles in the layer are rendered.
        /// </summary>
        public VertexData[]? GetMapLayerRenderCache(string layerName)
        {
            if (_cachedTileRenderData == null) return null;

            int mapLayerIdx = GetMapLayerByName(layerName);
            return mapLayerIdx == -1 ? null : _cachedTileRenderData[mapLayerIdx];
        }

        /// <summary>
        /// Get the texture cache of the specified map layer. Allows you to modify
        /// textures per tile.
        /// </summary>
        public Texture[]? GetMapLayerTextureCache(string layerName)
        {
            if (_cachedTileTextures == null) return null;

            int mapLayerIdx = GetMapLayerByName(layerName);
            return mapLayerIdx == -1 ? null : _cachedTileTextures[mapLayerIdx];
        }

        /// <summary>
        /// Converts a twp dimensional tile coordinate to a one dimensional index.
        /// </summary>
        public int GetTile1DFromTile2D(Vector2 coordinate)
        {
            var top = (int) coordinate.Y;
            var left = (int) coordinate.X;

            return (int) (left + SizeInTiles.X * top);
        }

        /// <summary>
        /// Converts a one dimensional tile index to a two dimensional coordinate.
        /// </summary>
        public Vector2 GetTile2DFromTile1D(int coordinate)
        {
            var x = (int) (coordinate % SizeInTiles.X);
            var y = (int) (coordinate / SizeInTiles.X);
            return coordinate >= SizeInTiles.X * SizeInTiles.Y ? Vector2.Zero : new Vector2(x, y);
        }

        /// <summary>
        /// Returns the tile coordinate of the tile at the specified coordinates.
        /// </summary>
        /// <param name="location">The coordinates in world space you want to sample.</param>
        /// <returns>The id of a singular tile in which the provided coordinates lay.</returns>
        public Vector2 GetTilePosOfWorldPos(Vector2 location)
        {
            var left = (int) Math.Max(0, location.X / TileSize.X);
            var top = (int) Math.Max(0, location.Y / TileSize.Y);

            return new Vector2(left, top);
        }

        /// <summary>
        /// Get the world render position of a tile.
        /// </summary>
        public Vector3 GetWorldPosOfTile(Vector2 tilePos, int layerIdx)
        {
            if (_cachedTileRenderData == null || layerIdx == -1) return Vector3.Zero;

            VertexData[]? renderCache = _cachedTileRenderData[layerIdx];
            if (renderCache == null) return Vector3.Zero;

            int tileIdx = GetTile1DFromTile2D(tilePos);
            if (tileIdx == -1) return Vector3.Zero;

            VertexData firstVertexOfTile = renderCache[tileIdx * 4];
            return firstVertexOfTile.Vertex;
        }

        #endregion

        #region Internal Helpers

        /// <summary>
        /// Returns the index of the tileset the tid belongs to.
        /// </summary>
        /// <param name="tId">The tid to find.</param>
        /// <param name="tsOffset">The offset within the tileset. This is the tid relative to the tileset.</param>
        /// <returns>The tsId that contains the tId.</returns>
        public int GetTilesetIdFromTid(uint tId, out int tsOffset)
        {
            var tsId = 0;
            tsOffset = (int) tId;

            for (var i = 0; i < Tilesets.Length; i++)
            {
                Map2DTileset? tileSet = Tilesets[i];
                if (tileSet == null) continue;

                // Check if the id we need is beyond the current tileset.
                if (tId < tileSet.FirstTileId) break;
                tsId = i;
            }

            if (tsId > 0) tsOffset -= Tilesets[tsId]!.FirstTileId - 1;
            tsOffset -= 1;

            return tsId;
        }

        /// <summary>
        /// Returns the UV and tileset id of the specified tid.
        /// </summary>
        /// <param name="tId">The texture id to parse.</param>
        /// <param name="tsId">The tileset id containing the texture id.</param>
        /// <returns>The UV of the tid within the tsId.</returns>
        public Rectangle GetUvFromTileImageId(uint tId, out int tsId)
        {
            tsId = GetTilesetIdFromTid(tId, out int tsOffset);
            return GetUVFromTileImageIdAndTileset(tsOffset, tsId);
        }

        /// <summary>
        /// Returns the UV of the specified tid in the specified tileset.
        /// </summary>
        /// <param name="tId">The texture id to parse.</param>
        /// <param name="tsId">The tileset id containing the texture id.</param>
        /// <returns>The UV of the tid within the tsId.</returns>
        public Rectangle GetUVFromTileImageIdAndTileset(int tId, int tsId)
        {
            Map2DTileset? ts = Tilesets[tsId];
            if (ts == null) return Rectangle.Empty;

            // Get tile image properties.
            int tiColumn = tId % ts.Width;
            var tiRow = (int) (tId / (float) ts.Width);
            var tiRect = new Rectangle(TileSize.X * tiColumn, TileSize.Y * tiRow, TileSize);

            // Add margins and spacing.
            tiRect.X += ts.Margin;
            tiRect.Y += ts.Margin;
            tiRect.X += ts.Spacing * tiColumn;
            tiRect.Y += ts.Spacing * tiRow;
            return tiRect;
        }

        #endregion
    }
}