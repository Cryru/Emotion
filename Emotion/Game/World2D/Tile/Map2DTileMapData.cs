#region Using

using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Emotion.Common.Serialization;
using Emotion.Graphics.Batches;
using Emotion.Graphics.Data;
using Emotion.IO;
using Emotion.Utility;

#endregion

#nullable enable

namespace Emotion.Game.World2D.Tile
{
    // Terminology
    //
    // layer - Each layer contains a set of tiles.
    // t - A single tile in a layer
    //      tId - The absolute id of the tile within all tilesets combined.
    //      tRect - The location the [t] should be drawn to.
    // ts - A tileset object. Each one contains a set of [t].
    //      tsId - The index of the tileset in the Tilesets array.
    //      tsOffset - The [tId] within the scope of the current tileset.
    //          Example: If you have two tilesets with 100 tiles each, the first tile in the second tileset
    //          will have a tId of 101, and a tsOffset of 1, while the first tile in the first tileset will have
    //          a tId of 1 and a tsOffset of 1.
    // ti - The tile image, within the [ts] which represents the image of [tsOffset].
    //      tiUV - The rectangle where the [ti] is located within the [ts] texture.

    /// <summary>
    /// Handles tilemap data representation and management for Map2D.
    /// This class also handles all calculated runtime data for layers and tilesets, while those classes
    /// themselves hold only data that is considered immutable.
    /// </summary>
    public sealed class Map2DTileMapData
    {
        public const uint FLIPPED_HORIZONTALLY_FLAG = 0x80000000;
        public const uint FLIPPED_VERTICALLY_FLAG = 0x40000000;
        public const uint FLIPPED_DIAGONALLY_FLAG = 0x20000000;

        public Vector2 TileSize = new Vector2(16);
        public List<Map2DTileMapLayer> Layers = new();
        public List<Map2DTileset> Tilesets = new();

        public Color BackgroundColor = new(0);

        #region Runtime State

        /// <summary>
        /// How big the entire map is, in tiles.
        /// </summary>
        [DontSerialize]
        public Vector2 SizeInTiles;

        /// <summary>
        /// Runtime data for each tileset.
        /// </summary>
        private TilesetRuntimeData[]? _tilesetRuntime;

        /// <summary>
        /// Cached render data for tiles per layer.
        /// </summary>
        private VertexData[]?[]? _cachedTileRenderData;

        /// <summary>
        /// Cached render data for tiles per layer.
        /// </summary>
        private Texture[]?[]? _cachedTileTextures;

        #endregion

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
        public void RenderLayer(RenderComposer composer, int layerIdx, Rectangle clipVal)
        {
            VertexData[]? renderCache = _cachedTileRenderData?[layerIdx];
            Texture[]? textureCache = _cachedTileTextures?[layerIdx];
            if (renderCache == null || textureCache == null) return;

            var yStart = (int)Maths.Clamp(MathF.Floor(clipVal.Y / TileSize.Y), 0, SizeInTiles.Y);
            var yEnd = (int)Maths.Clamp(MathF.Ceiling(clipVal.Bottom / TileSize.Y), 0, SizeInTiles.Y);
            var xStart = (int)Maths.Clamp(MathF.Floor(clipVal.X / TileSize.X), 0, SizeInTiles.X);
            var xEnd = (int)Maths.Clamp(MathF.Ceiling(clipVal.Right / TileSize.X), 0, SizeInTiles.X);

            for (int y = yStart; y < yEnd; y++)
            {
                var yIdx = (int)(y * SizeInTiles.X);
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
        /// <param name="c">The renderer</param>
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
        public int GetMapLayerIdxByName(string layerName)
        {
            for (var i = 0; i < Layers.Count; i++)
            {
                if (Layers[i].Name == layerName) return i;
            }

            return -1;
        }

        /// <summary>
        /// Get a reference to a tile map layer by its name.
        /// </summary>
        public Map2DTileMapLayer? GetMapLayerByName(string layerName)
        {
            for (var i = 0; i < Layers.Count; i++)
            {
                var layer = Layers[i];
                if (layer.Name == layerName) return layer;
            }

            return null;
        }

        /// <summary>
        /// Get the render cache of the specified map layer. Allows you to
        /// modify the way tiles in the layer are rendered.
        /// </summary>
        public VertexData[]? GetMapLayerRenderCache(string layerName)
        {
            if (_cachedTileRenderData == null) return null;

            int mapLayerIdx = GetMapLayerIdxByName(layerName);
            return mapLayerIdx == -1 ? null : _cachedTileRenderData[mapLayerIdx];
        }

        /// <summary>
        /// Get the texture cache of the specified map layer. Allows you to modify
        /// textures per tile.
        /// </summary>
        public Texture[]? GetMapLayerTextureCache(string layerName)
        {
            if (_cachedTileTextures == null) return null;

            int mapLayerIdx = GetMapLayerIdxByName(layerName);
            return mapLayerIdx == -1 ? null : _cachedTileTextures[mapLayerIdx];
        }

        /// <summary>
        /// Converts a twp dimensional tile coordinate to a one dimensional index.
        /// </summary>
        public int GetTile1DFromTile2D(Vector2 coordinate)
        {
            var top = (int)coordinate.Y;
            var left = (int)coordinate.X;

            return (int)(left + SizeInTiles.X * top);
        }

        /// <summary>
        /// Converts a one dimensional tile index to a two dimensional coordinate.
        /// </summary>
        public Vector2 GetTile2DFromTile1D(int coordinate)
        {
            var x = (int)(coordinate % SizeInTiles.X);
            var y = (int)(coordinate / SizeInTiles.X);
            return coordinate >= SizeInTiles.X * SizeInTiles.Y ? Vector2.Zero : new Vector2(x, y);
        }

        /// <summary>
        /// Returns the tile coordinate of the tile at the specified coordinates.
        /// </summary>
        /// <param name="location">The coordinates in world space you want to sample.</param>
        /// <returns>The id of a singular tile in which the provided coordinates lay.</returns>
        public Vector2 GetTilePosOfWorldPos(Vector2 location)
        {
            var left = (int)Math.Max(0, location.X / TileSize.X);
            var top = (int)Math.Max(0, location.Y / TileSize.Y);

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
            tsOffset = (int)tId;

            if (_tilesetRuntime == null) return 0;

            for (var i = 0; i < Tilesets.Count; i++)
            {
                Map2DTileset? tileSet = Tilesets[i];
                if (tileSet == null) continue;

                var runtimeData = _tilesetRuntime[i];
                int firstTile = runtimeData.FirstTid;

                // Check if the id we need is beyond the current tileset.
                if (tId < firstTile) break;
                tsId = i;

                if (tsId > 0)
                    tsOffset = (int)tId - firstTile;
                else
                    tsOffset = (int)tId - 1;
            }

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
            return GetUVFromTileImageIdAndTileset((uint)tsOffset, tsId);
        }

        /// <summary>
        /// Returns the UV of the specified tid in the specified tileset.
        /// </summary>
        /// <param name="tId">The texture id to parse.</param>
        /// <param name="tsId">The tileset id containing the texture id.</param>
        /// <returns>The UV of the tid within the tsId.</returns>
        public Rectangle GetUVFromTileImageIdAndTileset(uint tId, int tsId)
        {
            Map2DTileset? ts = tsId < Tilesets.Count ? Tilesets[tsId] : null;
            if (ts == null || _tilesetRuntime == null) return Rectangle.Empty;

            var runtimeData = _tilesetRuntime[tsId];
            int widthInTiles = (int)runtimeData.SizeInTiles.X;

            // Get tile image properties.
            int tIdInt = (int)tId;
            int tiColumn = tIdInt % widthInTiles;
            var tiRow = (int)(tIdInt / (float)widthInTiles);
            var tiRect = new Rectangle(TileSize.X * tiColumn, TileSize.Y * tiRow, TileSize);

            // Add margins and spacing.
            tiRect.X += ts.Margin;
            tiRect.Y += ts.Margin;
            tiRect.X += ts.Spacing * tiColumn;
            tiRect.Y += ts.Spacing * tiRow;
            return tiRect;
        }

        public int GetTsIdFromTilesetRef(Map2DTileset? ts)
        {
            if (ts == null) return -1;

            for (int i = 0; i < Tilesets.Count; i++)
            {
                if (Tilesets[i] == ts) return i;
            }

            return -1;
        }

        #endregion

        #region Runtime State

        private struct TilesetPatchData
        {
            public int MoveAfterTid; // Move all tiles that use tid's after this number
            public int OffsetAmount;
        }

        private List<TilesetPatchData>? _patchesNeeded;

        private class TilesetRuntimeData
        {
            public TextureAsset? Texture;
            public Vector2 SizeInTiles;
            public int FirstTid;
        }

        public async Task InitRuntimeState(Map2D map)
        {
            var mapSize = map.MapSize;
            SizeInTiles = Vector2.Max(Vector2.One, (mapSize / TileSize).Floor());

            await LoadTilesetRuntimeData();
            LoadTileLayerRuntimeData();
            LoadRuntimeRenderCache();
        }

        private async Task LoadTilesetRuntimeData()
        {
            if (Tilesets.Count == 0) return;

            _tilesetRuntime = new TilesetRuntimeData[Tilesets.Count];

            // Start loading of all of them at the same time.
            var assets = new Task<TextureAsset?>[Tilesets.Count];
            for (var i = 0; i < assets.Length; i++)
            {
                Map2DTileset? tileSet = Tilesets[i];
                string? tilesetFile = tileSet?.AssetFile;
                if (tileSet == null || tilesetFile == null)
                {
                    assets[i] = Task.FromResult((TextureAsset?)null);
                    continue;
                }

                assets[i] = Engine.AssetLoader.GetAsync<TextureAsset>(tilesetFile);
            }

            // ReSharper disable once CoVariantArrayConversion
            await Task.WhenAll(assets);

            // Calculate all runtime data and populate the runtime data array.
            int tIdOffset = 1;
            int patchCheckOffset = 0;
            for (var i = 0; i < assets.Length; i++)
            {
                TextureAsset? textureAsset = assets[i].IsCompletedSuccessfully ? assets[i].Result : null;
                Vector2 textureAssetSize = textureAsset?.Texture.Size ?? Vector2.One;

                var tileset = Tilesets[i];
                TilesetRuntimeData data = new TilesetRuntimeData();
                data.Texture = textureAsset;
                data.SizeInTiles = Vector2.Max(Vector2.One,
                    (
                        (textureAssetSize - new Vector2(tileset.Margin))
                        /
                        (TileSize + new Vector2(tileset.Spacing))
                    ).Round()); // Round to coincide behavior with Tiled
                data.FirstTid = tIdOffset;
                _tilesetRuntime[i] = data;

                int tilesInTileset = (int)(data.SizeInTiles.X * data.SizeInTiles.Y);
                if (tileset.TilesetFirstTidExpected == -1) // Tiles in tileset not recorded.
                {
                    tileset.TilesetFirstTidExpected = tIdOffset;
                }
                else if (tileset.TilesetFirstTidExpected + patchCheckOffset != tIdOffset) // Tileset size changed or a tileset was removed, fix tiles.
                {
                    _patchesNeeded ??= new List<TilesetPatchData>();

                    int change = tIdOffset - tileset.TilesetFirstTidExpected;
                    patchCheckOffset += change;

                    _patchesNeeded.Add(new TilesetPatchData()
                    {
                        MoveAfterTid = tileset.TilesetFirstTidExpected,
                        OffsetAmount = change
                    });
                    tileset.TilesetFirstTidExpected = tIdOffset;
                }

                tIdOffset += tilesInTileset;
            }
        }

        private void LoadTileLayerRuntimeData()
        {
            int totalTilesInMap = (int)(SizeInTiles.X * SizeInTiles.Y);
            int newStride = (int)SizeInTiles.X;

            // Ensure all layers have a tile reference for each tile in the map.
            // If the map was shrunk and the layer has more tiles,
            // we don't really care.
            for (int i = 0; i < Layers.Count; i++)
            {
                var layer = Layers[i];
                uint[] unpackedData = layer.GetUnpackedTileData();

                if (unpackedData.Length < totalTilesInMap) // map size doesn't match layer size
                {
                    if (layer.DataStride == -1) // No stride recorded, we can't really guess :P
                    {
                        Array.Resize(ref unpackedData, totalTilesInMap);
                        layer.SetUnpackedTileData(unpackedData);
                    }
                    else // Resize the map intelligently.
                    {
                        uint[] newData = new uint[totalTilesInMap];

                        // Copy old data and retain map size.
                        int layerStride = layer.DataStride;
                        for (int t = 0; t < unpackedData.Length; t++)
                        {
                            int column = t / layerStride;
                            int row = t - (column * layerStride);

                            newData[column * newStride + row] = unpackedData[t];
                        }

                        layer.SetUnpackedTileData(newData);
                    }

                    Engine.Log.Warning($"Resized layer {layer.Name} due to changes in the map size.", "TileMap");
                }

                layer.DataStride = newStride;
            }

            // Check if we should apply any data patches to the layers in order to
            // correct tid from moved tilesets.
            if (_patchesNeeded != null)
            {
                for (int l = 0; l < Layers.Count; l++)
                {
                    var layer = Layers[l];

                    // Gather changes from all patches
                    int[] changesMatrix = new int[totalTilesInMap];
                    for (int p = 0; p < _patchesNeeded.Count; p++)
                    {
                        var patch = _patchesNeeded[p];

                        for (int i = 0; i < totalTilesInMap; i++)
                        {
                            var tId = GetTileData(layer, i);
                            if (tId > patch.MoveAfterTid)
                            {
                                changesMatrix[i] += patch.OffsetAmount;
                            }
                        }
                    }

                    // Apply them at once.
                    for (int i = 0; i < totalTilesInMap; i++)
                    {
                        var tId = GetTileData(layer, i);
                        SetTileData(layer, i, (uint) (tId + changesMatrix[i]));
                    }
                }

                // Log applied patches
                for (int p = 0; p < _patchesNeeded.Count; p++)
                {
                    var patch = _patchesNeeded[p];
                    Engine.Log.Warning($"Offset tiles with tId after {patch.MoveAfterTid} by {patch.OffsetAmount} due to tileset changes.", "TileMap");
                }
                _patchesNeeded = null;
            }
        }

        // Render information is cached as to prevent having to
        // do lookups each frame.
        private void LoadRuntimeRenderCache()
        {
            if (Tilesets.Count == 0) return;
            Assert(_tilesetRuntime != null);

            _cachedTileRenderData = null;
            _cachedTileTextures = null;
            if (Layers.Count == 0) return;

            var tileColumns = (int)SizeInTiles.X;
            var tileRows = (int)SizeInTiles.Y;
            int totalTileSize = tileRows * tileColumns;

            _cachedTileRenderData = new VertexData[Layers.Count][];
            _cachedTileTextures = new Texture[Layers.Count][];
            for (var layerIdx = 0; layerIdx < Layers.Count; layerIdx++)
            {
                var currentCache = new VertexData[totalTileSize * 4];
                var currentTextureCache = new Texture[totalTileSize];

                int totalTiles = tileRows * tileColumns;
                for (int tileIdx = 0; tileIdx < totalTiles; tileIdx++)
                {
                    CalculateRenderCacheForTile(layerIdx, tileIdx, currentCache, currentTextureCache);
                }

                _cachedTileRenderData[layerIdx] = currentCache;
                _cachedTileTextures[layerIdx] = currentTextureCache;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void CalculateRenderCacheForTile(int layerIdx, int tileIdx, Span<VertexData> layerCache, Texture[] textureCache)
        {
            Map2DTileMapLayer layer = Layers[layerIdx];
            GetTileData(layer, tileIdx, out uint tId, out bool flipX, out bool flipY, out bool _);

            Span<VertexData> tileData = layerCache.Slice(tileIdx * 4, 4);

            // Get the id of the tile, and if empty skip it
            if (tId == 0)
            {
                for (var i = 0; i < tileData.Length; i++)
                {
                    tileData[i].Color = 0;
                }

                return;
            }

            // Find which tileset the tId belongs in.
            Rectangle tiUv = GetUvFromTileImageId(tId, out int tsId);

            // Calculate dimensions of the tile.
            Vector2 tileIdx2D = GetTile2DFromTile1D(tileIdx);
            Vector2 position = tileIdx2D * TileSize;
            var v3 = new Vector3(position, layerIdx);
            var c = new Color(255, 255, 255, (int)(layer.Opacity * 255));

            var tilesetTexture = GetTilesetTexture(tsId);
            if (tilesetTexture != null) textureCache[tileIdx] = tilesetTexture;

            // Write to tilemap mesh.
            VertexData.SpriteToVertexData(tileData, v3, TileSize, c, tilesetTexture, tiUv, flipX, flipY);
        }

        /// <summary>
        /// Get the tile id of a specific tile index in a specific tile layer.
        /// </summary>
        public uint GetTileData(Map2DTileMapLayer? layer, int tileIdx)
        {
            GetTileData(layer, tileIdx, out uint tId, out bool _, out bool _, out bool _);
            return tId;
        }

        public void GetTileData(Map2DTileMapLayer? layer, int tileIdx, out uint tid, out bool flipX, out bool flipY, out bool flipD)
        {
            tid = 0;
            flipX = false;
            flipY = false;
            flipD = false;

            if (layer == null) return;
            var unpackedData = layer.GetUnpackedTileData();

            if (tileIdx < 0 || tileIdx >= unpackedData.Length) return;

            uint data = unpackedData[tileIdx];
            flipX = (data & FLIPPED_HORIZONTALLY_FLAG) != 0;
            flipY = (data & FLIPPED_VERTICALLY_FLAG) != 0;
            flipD = (data & FLIPPED_DIAGONALLY_FLAG) != 0;
            tid = data & ~(FLIPPED_HORIZONTALLY_FLAG | FLIPPED_VERTICALLY_FLAG | FLIPPED_DIAGONALLY_FLAG);
        }

        public void SetTileData(Map2DTileMapLayer? layer, int tileIdx, uint tId)
        {
            if (layer == null) return;
            var unpackedData = layer.GetUnpackedTileData();

            if (tileIdx < 0 || tileIdx >= unpackedData.Length) return;
            unpackedData[tileIdx] = tId;

            // Recalculate the render cache for this tile.
            if (_cachedTileRenderData == null) return;
            AssertNotNull(_cachedTileTextures);

            var layerIdx = Layers.IndexOf(layer);
            if (layerIdx == -1) return;

            VertexData[]? cacheForThisLayer = _cachedTileRenderData[layerIdx];
            Texture[]? textureCacheForThisLayer = _cachedTileTextures[layerIdx];

            if (cacheForThisLayer == null || textureCacheForThisLayer == null) return;
            CalculateRenderCacheForTile(layerIdx, tileIdx, cacheForThisLayer, textureCacheForThisLayer);
        }

        public Vector2 GetTilesetSizeInTiles(Map2DTileset? ts)
        {
            if (ts == null) return Vector2.Zero;
            if (_tilesetRuntime == null) return Vector2.Zero;

            var tsId = GetTsIdFromTileset(ts);
            TilesetRuntimeData data = _tilesetRuntime[tsId];
            return data.SizeInTiles;
        }

        public Texture? GetTilesetTexture(Map2DTileset? ts)
        {
            if (ts == null) return null;
            if (_tilesetRuntime == null) return null;

            int tsId = GetTsIdFromTileset(ts);
            TilesetRuntimeData data = _tilesetRuntime[tsId];
            return data.Texture?.Texture;
        }

        public Texture? GetTilesetTexture(int tsId)
        {
            if (tsId > Tilesets.Count - 1) return null;
            if (_tilesetRuntime == null) return null;

            TilesetRuntimeData data = _tilesetRuntime[tsId];
            return data.Texture?.Texture;
        }

        public int GetTilesetTidOffset(Map2DTileset? ts)
        {
            if (ts == null) return 0;
            if (_tilesetRuntime == null) return 0;

            int tsId = GetTsIdFromTileset(ts);
            TilesetRuntimeData data = _tilesetRuntime[tsId];
            return data.FirstTid;
        }

        public int GetTsIdFromTileset(Map2DTileset? ts)
        {
            if (ts == null) return 0;
            return Tilesets.IndexOf(ts);
        }

        #endregion
    }
}