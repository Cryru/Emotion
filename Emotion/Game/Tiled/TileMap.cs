#region Using

using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;
using Emotion.Common;
using Emotion.Common.Threading;
using Emotion.Game.QuadTree;
using Emotion.Graphics;
using Emotion.Graphics.Batches;
using Emotion.Graphics.Data;
using Emotion.Graphics.Objects;
using Emotion.IO;
using Emotion.Primitives;
using Emotion.Standard.Logging;
using Emotion.Standard.TMX;
using Emotion.Standard.TMX.Layer;
using Emotion.Standard.TMX.Object;
using Emotion.Standard.XML;
using Emotion.Utility;

#endregion

#nullable enable

namespace Emotion.Game.Tiled
{
    public class TileMap<T> : TileMap where T : TransformRenderable
    {
        /// <summary>
        /// Holds the objects created by the object factory (if any).
        /// </summary>
        public QuadTree<T> Objects { get; protected set; } = new QuadTree<T>(Rectangle.Empty);

        /// <summary>
        /// Reusable memory for querying the quad tree.
        /// </summary>
        protected List<T> _quadTreeQueryMemory = new List<T>();

        public TileMap(TextAsset? mapFile) : base(mapFile)
        {
        }

        public TileMap(string? mapPath) : base(mapPath)
        {
        }

        protected void CreateObjectInternal(TmxObject objDef, int layerId)
        {
            TextureAsset? asset = null;
            Rectangle? uv = null;
            if (objDef.Gid != null)
            {
                uv = GetUvFromTileImageId(objDef.Gid.Value, out int tsId);
                if (tsId >= 0 && tsId < Tilesets.Count) asset = Tilesets[tsId];
            }

            T? factoryObject = CreateObject(objDef, asset, uv, layerId);
            if (factoryObject != null) Objects.Add(factoryObject);
        }

        protected virtual bool ShouldSpawnObjectLayer(int layerId)
        {
            return true;
        }

        protected override void MapPostLoad()
        {
            Objects.Reset(new Rectangle(0, 0, WorldSize));

            // Construct all objects.
            if (TiledMap!.ObjectLayers.Count > 0)
                // For each layer with objects.
                for (var i = 0; i < TiledMap.ObjectLayers.Count; i++)
                {
                    if (!ShouldSpawnObjectLayer(i)) continue;

                    // For each object.
                    for (var j = 0; j < TiledMap.ObjectLayers[i].Objects.Count; j++)
                    {
                        TmxObject objDef = TiledMap.ObjectLayers[i].Objects[j];
                        CreateObjectInternal(objDef, i);
                    }
                }

            // Construct all objects associated with tiles. These are usually collisions.
            for (var i = 0; i < TiledMap.TileLayers.Count; i++)
            {
                TmxLayer layer = TiledMap.TileLayers[i];
                for (var t = 0; t < layer.Tiles.Count; t++)
                {
                    TmxLayerTile tile = layer.Tiles[t];
                    int tId = tile.Gid;
                    if (tId == 0) continue; // Quick out for empty tiles.

                    int tsId = GetTilesetIdFromTid(tId, out int tsOffset);
                    TmxTileset ts = TiledMap.Tilesets[tsId];
                    TmxTilesetTile? tileData = ts?.Tiles.GetValueOrDefault(tsOffset);
                    if (tileData?.ObjectGroups == null) continue;
                    foreach (TmxObjectLayer groups in tileData.ObjectGroups)
                    {
                        for (var o = 0; o < groups.Objects.Count; o++)
                        {
                            TmxObject obj = groups.Objects[o];
                            if (string.IsNullOrEmpty(obj.Type)) obj.Type = "TileObject";
                            // Patch in position of the current tile.
                            Vector2 coord = GetTile2DFromTile1D(t, i);
                            TmxObject clone = obj.Clone(); // Don't pollute obj def
                            clone.X = obj.X + layer.OffsetX + coord.X * TiledMap.TileWidth;
                            clone.Y = obj.Y + layer.OffsetY + coord.Y * TiledMap.TileHeight;
                            CreateObjectInternal(clone, -i);
                        }
                    }
                }
            }

            base.MapPostLoad();
        }

        protected virtual void QueryObjectsToRender(List<T>? memory = null)
        {
            memory ??= _quadTreeQueryMemory;
            memory.Clear();
            Rectangle clipRect = Clip ?? Engine.Renderer.Camera.GetWorldBoundingRect();
            Objects.GetObjects(ref clipRect, memory);
            memory.Sort(ObjectSort);
        }

        /// <summary>
        /// Render all objects within the clip rect.
        /// </summary>
        public void RenderObjects(RenderComposer composer)
        {
            // Check if anything is loaded.
            QueryObjectsToRender();
            PerfProfiler.FrameEventStart("TileMap: Objects");
            for (var i = 0; i < _quadTreeQueryMemory.Count; i++)
            {
                _quadTreeQueryMemory[i].Render(composer);
            }

            PerfProfiler.FrameEventEnd("TileMap: Objects");
        }

        /// <inheritdoc />
        public override void Render(RenderComposer composer)
        {
            base.Render(composer);
            RenderObjects(composer);
        }

        protected override void MapUnloading()
        {
            Objects.Reset(Rectangle.Empty);
            base.MapUnloading();
        }

        #region Interface

        protected virtual T? CreateObject(TmxObject objDef, TextureAsset? image, Rectangle? uv, int layerId)
        {
            return null;
        }

        protected virtual int ObjectSort(T x, T y)
        {
            return MathF.Sign(x.Position.Z - y.Position.Z);
        }

        #endregion
    }

    public class TileMap : TransformRenderable, IDisposable
    {
        #region Properties

        /// <summary>
        /// The range of the map to render. If null then the camera range will be used.
        /// </summary>
        public Rectangle? Clip;

        /// <summary>
        /// The currently loaded map asset name. This is the engine path which AssetLoader.Get was called with.
        /// </summary>
        public string? FileName { get; protected set; }

        /// <summary>
        /// The TiledSharp object the map is using.
        /// </summary>
        public TmxMap? TiledMap { get; protected set; }

        /// <summary>
        /// Loaded tileset textures.
        /// Missing textures are null.
        /// </summary>
        public List<TextureAsset?> Tilesets { get; private set; } = new List<TextureAsset?>();

        /// <summary>
        /// The size of a tile in pixels.
        /// </summary>
        public Vector2 TileSize
        {
            get => TiledMap == null ? Vector2.Zero : new Vector2(TiledMap.TileWidth, TiledMap.TileHeight);
        }

        /// <summary>
        /// The size of the map in tiles.
        /// </summary>
        public Vector2 SizeInTiles
        {
            get => TiledMap == null ? Vector2.Zero : new Vector2(TiledMap.Width, TiledMap.Height);
        }

        /// <summary>
        /// The size of the map in the world.
        /// </summary>
        public Vector2 WorldSize
        {
            get => TiledMap == null ? Vector2.Zero : new Vector2(TiledMap.Width * TiledMap.TileWidth, TiledMap.Height * TiledMap.TileHeight);
        }

        #endregion

        /// <summary>
        /// Whether the map is loaded.
        /// </summary>
        protected bool _loaded;

        /// <summary>
        /// Animated tile meta data.
        /// </summary>
        protected List<AnimatedTile> _animatedTiles = new List<AnimatedTile>();

        /// <summary>
        /// The folder to load tilesets from.
        /// </summary>
        protected string? _tilesetFolder;

        /// <summary>
        /// Cached render data for tiles per layer. Non-tile layers are null.
        /// </summary>
        protected VertexData[]?[]? _cachedTileRenderData;

        /// <summary>
        /// Cached render data for tiles per layer. Non-tile layers are null.
        /// </summary>
        protected Texture[]?[]? _cachedTileTextures;


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
        /// Create a new map object from a Tiled map.
        /// </summary>
        /// <param name="mapFile">The file to load as the map.</param>
        public TileMap(TextAsset? mapFile) : base(Vector2.Zero, Vector2.One)
        {
            Reset(mapFile);
        }

        /// <summary>
        /// Create a new map object from a Tiled map.
        /// </summary>
        /// <param name="mapPath">The path to the map.</param>
        public TileMap(string? mapPath) : base(Vector2.Zero, Vector2.One)
        {
            // Check if no map is provided.
            if (mapPath == string.Empty) return;
            Reset(mapPath);
        }

        /// <summary>
        /// Override the folder where tilesets will be resolved.
        /// This will reload all currently loaded tilesets (and objects).
        /// </summary>
        public void SetTilesetFolder(string tileSetFolder)
        {
            if (!string.IsNullOrEmpty(tileSetFolder))
            {
                tileSetFolder = AssetLoader.NameToEngineName(tileSetFolder);
                if (tileSetFolder[^1] == '/') tileSetFolder = tileSetFolder[..^1];
            }

            _tilesetFolder = tileSetFolder;
            Reload();
        }

        protected virtual void LoadTilesets()
        {
            if (TiledMap?.Tilesets == null) return;
            if (TiledMap.Tilesets.Count == 0) return;

            string tileSetFolder = FileName != null ? _tilesetFolder ?? AssetLoader.GetDirectoryName(FileName) : "";

            // Don't load the assets in parallel if running on the draw thread. This might cause a deadlock as assets will wait on
            // the draw thread to wake up in order to upload data.
            bool parallel = !GLThread.IsGLThread();

            var assets = new Task<TextureAsset?>[TiledMap.Tilesets.Count];
            for (var i = 0; i < assets.Length; i++)
            {
                string? tilesetFile = TiledMap.Tilesets[i]?.Source;
                if (string.IsNullOrEmpty(tilesetFile)) continue;
                tilesetFile = AssetLoader.NameToEngineName(tilesetFile);
                if (tilesetFile[0] == '/') tilesetFile = tilesetFile[1..];

                string assetPath = AssetLoader.GetNonRelativePath(tileSetFolder, tilesetFile);
                if (parallel)
                    assets[i] = Engine.AssetLoader.GetAsync<TextureAsset>(assetPath);
                else
                    assets[i] = Task.FromResult(Engine.AssetLoader.Get<TextureAsset>(assetPath));
            }

            // ReSharper disable once CoVariantArrayConversion
            Task.WaitAll(assets);

            for (var i = 0; i < assets.Length; i++)
            {
                Tilesets.Add(assets[i].IsCompletedSuccessfully ? assets[i].Result : null);
            }
        }

        /// <summary>
        /// Reload the currently loaded map.
        /// </summary>
        public void Reload()
        {
            if (TiledMap == null) return;

            TmxMap map = TiledMap;
            MapUnloadInternal();
            TiledMap = map;
            ResetInternal();
        }

        /// <summary>
        /// Reset the tile map with another map and tileset. If an empty string is provided the map is reset to an unloaded state.
        /// </summary>
        /// <param name="mapPath">The path to the new map.</param>
        public void Reset(string? mapPath)
        {
            Reset(mapPath != null ? Engine.AssetLoader.Get<TextAsset>(mapPath) : null);
        }

        /// <summary>
        /// Reset the tile map with another map and tileset. If an empty string is provided the map is reset to an unloaded state.
        /// </summary>
        /// <param name="mapFile">The new map file.</param>
        public virtual void Reset(TextAsset? mapFile)
        {
            MapUnloadInternal();

            // Check if no map is provided.
            if (mapFile == null) return;

            // Load the map from the data as a stream.
            try
            {
                FileName = mapFile.Name;
                TiledMap = new TmxMap(new XMLReader(mapFile.Content), mapFile.Name);
            }
            catch (Exception ex)
            {
                Engine.Log.Warning($"Couldn't parse tilemap - {ex}.", MessageSource.Other);
                return;
            }

            ResetInternal();
        }

        protected void ResetInternal()
        {
            if (TiledMap == null) return;

            MapPreLoad();

            // Load all map tilesets.
            PerfProfiler.ProfilerEventStart("TileMap: Loading Tilesets", "Loading");
            LoadTilesets();
            PerfProfiler.ProfilerEventEnd("TileMap: Loading Tilesets", "Loading");

            // Find animated tiles.
            CacheTilesetData();

            // Construct render cache.
            _cachedTileRenderData = null;
            _cachedTileTextures = null;
            if (TiledMap.TileLayers.Count > 0)
            {
                _cachedTileRenderData = new VertexData[TiledMap.Layers.Count][];
                _cachedTileTextures = new Texture[TiledMap.Layers.Count][];
                for (var i = 0; i < TiledMap.Layers.Count; i++)
                {
                    SetupRenderCacheForTileLayer(i);
                }
            }

            // Set loaded flag.
            _loaded = true;

            MapPostLoad();
        }

        /// <summary>
        /// Update the map. Processes animated tiles.
        /// </summary>
        /// <param name="frameTime">The time passed since the last frame.</param>
        public virtual void Update(float frameTime)
        {
            // Update animated tiles.
            UpdateAnimatedTiles(frameTime);
        }

        #region Rendering

        private void SetupRenderCacheForTileLayer(int layerIdx)
        {
            if (TiledMap == null) return;

            TmxLayer? layer = TiledMap.Layers[layerIdx];
            if (!layer.Visible || layer.Width == 0 || layer.Height == 0 || layer.Tiles == null) return;

            PerfProfiler.ProfilerEventStart($"TileMap: RenderCache for layer {layerIdx}", "Loading");

            var currentCache = new VertexData[layer.Width * layer.Height * 4];
            var currentTextureCache = new Texture[layer.Width * layer.Height];
            var dataSpan = new Span<VertexData>(currentCache);

            for (var y = 0; y < layer.Height; y++)
            {
                int yIdx = y * layer.Width;
                for (var x = 0; x < layer.Width; x++)
                {
                    int tileIdx = yIdx + x;
                    Span<VertexData> tileData = dataSpan.Slice(tileIdx * 4, 4);

                    // Get the id of the tile, and if empty skip it
                    int tId = layer.Tiles[tileIdx].Gid;
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
                    TmxTileset ts = TiledMap.Tilesets[tsId];

                    // Calculate dimensions of the tile.
                    var position = new Vector2(x * TiledMap.TileWidth, y * TiledMap.TileHeight);
                    var size = new Vector2(ts.TileWidth, ts.TileHeight);

                    // Add offset.
                    position.X += layer.OffsetX;
                    position.Y += layer.OffsetY;

                    // Scale
                    size.X *= Width;
                    size.Y *= Height;
                    position.X *= Width;
                    position.Y *= Height;

                    // Calculate Z
                    var v3 = new Vector3(position, CalculateZOrder(position, size, new Vector2(x, y), layerIdx, tId, tsId));

                    // Calculate tint and texture.
                    var c = new Color(255, 255, 255, (int) (layer.Opacity * 255));
                    TextureAsset? tileSet = Tilesets[tsId];
                    if (tileSet != null) currentTextureCache![tileIdx] = tileSet.Texture;

                    // Write to tilemap mesh.
                    VertexData.SpriteToVertexData(tileData, v3, size, c, tileSet?.Texture, tiUv, layer.Tiles[tileIdx].HorizontalFlip, layer.Tiles[tileIdx].VerticalFlip);
                }
            }

            PerfProfiler.ProfilerEventEnd($"TileMap: RenderCache for layer {layerIdx}", "Loading");
            _cachedTileRenderData![layerIdx] = currentCache;
            _cachedTileTextures![layerIdx] = currentTextureCache;
        }

        public override void Render(RenderComposer composer)
        {
            RenderTileLayerRange(composer, 0, -1, true);
        }

        /// <summary>
        /// Render all tile layers in the specified index range.
        /// </summary>
        /// <param name="composer"></param>
        /// <param name="start">The layer to start from, inclusive.</param>
        /// <param name="end">The layer to render to, exclusive. -1 for until end</param>
        /// <param name="renderBackground">Whether to render a solid color background first.</param>
        public void RenderTileLayerRange(RenderComposer composer, int start = 0, int end = -1, bool renderBackground = false)
        {
            // Check if anything is loaded.
            if (TiledMap == null || !_loaded) return;
            end = end == -1 ? TiledMap.Layers.Count : end;

            Rectangle clipRect = Clip ?? composer.Camera.GetWorldBoundingRect();
            if (renderBackground) composer.RenderSprite(clipRect, TiledMap.BackgroundColor);

            for (int layer = start; layer < end; layer++)
            {
                PerfProfiler.FrameEventStart($"TileMap: Mapping layer {layer}");
                RenderLayer(composer, layer, clipRect);
                PerfProfiler.FrameEventEnd($"TileMap: Mapping layer {layer}");
            }
        }

        protected virtual void RenderLayer(RenderComposer composer, int layerIdx, Rectangle clipVal)
        {
            VertexData[]? renderCache = _cachedTileRenderData?[layerIdx];
            Texture[]? textureCache = _cachedTileTextures?[layerIdx];
            if (renderCache == null || textureCache == null || TiledMap == null) return;

            TmxLayer layer = TiledMap.Layers[layerIdx];
            if (!layer.Visible || layer.Width == 0 || layer.Height == 0 || layer.Tiles == null) return;

            var yStart = (int) Maths.Clamp(MathF.Floor(clipVal.Y / TiledMap.TileHeight), 0, layer.Height);
            var yEnd = (int) Maths.Clamp(MathF.Ceiling(clipVal.Bottom / TiledMap.TileHeight), 0, layer.Height);
            var xStart = (int) Maths.Clamp(MathF.Floor(clipVal.X / TiledMap.TileWidth), 0, layer.Width);
            var xEnd = (int) Maths.Clamp(MathF.Ceiling(clipVal.Right / TiledMap.TileWidth), 0, layer.Width);

            for (int y = yStart; y < yEnd; y++)
            {
                int yIdx = y * layer.Width;
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

        #endregion

        #region Animated Tiles

        /// <summary>
        /// Reads additional meta per tile for all tilesets.
        /// Caches all animated tiles.
        /// </summary>
        private void CacheTilesetData()
        {
            if (TiledMap == null) return;

            for (var layer = 0; layer < TiledMap.Tilesets.Count; layer++)
            {
                TmxTileset tileset = TiledMap.Tilesets[layer];

                // Check if the tileset has tile data.
                if (tileset.Tiles.Count <= 0) continue;

                foreach ((int _, TmxTilesetTile tileData) in tileset.Tiles)
                {
                    // Cache animated tiles.
                    if (tileData.AnimationFrames != null) _animatedTiles.Add(new AnimatedTile(tileData.Id, tileData.AnimationFrames));
                }
            }
        }

        /// <summary>
        /// Updates the animations of all cached animated tiles.
        /// </summary>
        /// <param name="time">The amount of time which has passed since the last update.</param>
        private void UpdateAnimatedTiles(float time)
        {
            foreach (AnimatedTile cachedTile in _animatedTiles)
            {
                cachedTile.Update(time);
            }
        }

        #endregion

        #region Internal API

        /// <summary>
        /// Converts the two dimensional tile coordinate to a one dimensional one.
        /// </summary>
        /// <param name="coordinate">The coordinate to convert.</param>
        /// <returns>A one dimensional tile coordinate.</returns>
        public int GetTile1DFromTile2D(Vector2 coordinate)
        {
            if (TiledMap == null) return -1;

            var top = (int) coordinate.Y;
            var left = (int) coordinate.X;

            return left + TiledMap.Width * top;
        }

        /// <summary>
        /// Returns a two dimensional coordinate representing the provided tile one dimensional coordinate.
        /// </summary>
        /// <param name="coordinate">The one dimensional tile coordinate.</param>
        /// <param name="layer">The layer the tile is on.</param>
        /// <returns>The two dimensional coordinate equivalent of the one dimensional coordinate provided.</returns>
        public Vector2 GetTile2DFromTile1D(int coordinate, int layer = 0)
        {
            if (TiledMap == null) return Vector2.Zero;

            TmxLayer tileLayer = TiledMap.Layers[layer];
            int x = coordinate % tileLayer.Width;
            int y = coordinate / tileLayer.Width;
            return coordinate >= tileLayer.Width * tileLayer.Height ? Vector2.Zero : new Vector2(x, y);
        }

        /// <summary>
        /// Returns the index of the tileset the tid belongs to.
        /// </summary>
        /// <param name="tid">The tid to find.</param>
        /// <param name="tsOffset">The offset within the tileset. This is the tid relative to the tileset.</param>
        /// <returns>The tsid of the tid.</returns>
        public int GetTilesetIdFromTid(int tid, out int tsOffset)
        {
            var tsId = 0;
            tsOffset = tid;

            if (TiledMap == null) return -1;
            for (var i = 0; i < TiledMap.Tilesets.Count; i++)
            {
                // Check if the id we need is beyond the current tileset.
                if (tid < TiledMap.Tilesets[i].FirstGid) break;
                tsId = i;
            }

            if (tsId > 0) tsOffset -= TiledMap.Tilesets[tsId].FirstGid - 1;
            tsOffset -= 1;

            return tsId;
        }

        /// <summary>
        /// Returns the UV and tileset id of the specified tid.
        /// </summary>
        /// <param name="tId">The texture id to parse.</param>
        /// <param name="tsId">The tileset id containing the texture id.</param>
        /// <returns>The UV of the tid within the tsId.</returns>
        public Rectangle GetUvFromTileImageId(int tId, out int tsId)
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
            if (TiledMap == null) return Rectangle.Empty;

            TmxTileset ts = TiledMap.Tilesets[tsId];

            // Check if the current tileset has animated tiles.
            if (ts.Tiles.Count > 0)
                foreach (AnimatedTile cachedTile in _animatedTiles.Where(cachedTile => cachedTile.Id == tId))
                {
                    tId = cachedTile.FrameId;
                }

            // Get tile image properties.
            int tiColumn = tId % (ts.Columns ?? 0);
            var tiRow = (int) (tId / (double) (ts.Columns ?? 0));
            var tiRect = new Rectangle(ts.TileWidth * tiColumn, ts.TileHeight * tiRow, ts.TileWidth, ts.TileHeight);

            // Add margins and spacing.
            tiRect.X += ts.Margin;
            tiRect.Y += ts.Margin;
            tiRect.X += ts.Spacing * tiColumn;
            tiRect.Y += ts.Spacing * tiRow;
            return tiRect;
        }

        #endregion

        #region API

        /// <summary>
        /// Returns the layer id of the layer with the specified name, or -1 if invalid.
        /// </summary>
        /// <param name="name">The layer name to check.</param>
        /// <returns>The id of the layer matching the specified name or -1 if none found.</returns>
        public int GetLayerIdFromName(string name)
        {
            if (TiledMap == null) return -1;

            for (var i = 0; i < TiledMap.Layers.Count; i++)
            {
                if (string.Equals(TiledMap.Layers[i].Name, name, StringComparison.CurrentCultureIgnoreCase)) return i;
            }

            return -1;
        }

        /// <summary>
        /// Get the image id of the tile in the coordinate.
        /// The image id is relative to the tileset's id.
        /// </summary>
        /// <param name="coordinate">The 1D coordinate to lookup the tile id of.</param>
        /// <param name="layer">The tile layer to check in.</param>
        /// <param name="tileSet">The id of the tile set in which the image id is.</param>
        /// <returns></returns>
        public int GetTileImageIdInLayer(int coordinate, int layer, out int tileSet)
        {
            // Check if layer is out of bounds.
            tileSet = -1;
            if (TiledMap?.Layers[layer].Tiles == null ||
                layer < 0 && layer > TiledMap.Layers.Count - 1 ||
                coordinate > TiledMap.Layers[layer].Tiles.Count ||
                coordinate < 0) return -1;

            // Get the GID of the tile.
            int tId = TiledMap.Layers[layer].Tiles[coordinate].Gid;
            if (tId == 0) return -1;

            // Find the id of tile within the tileset.
            for (var t = 0; t < TiledMap.Tilesets.Count; t++)
            {
                if (tId > TiledMap.Tilesets[t].FirstGid + TiledMap.Tilesets[t].TileCount) continue;
                tileSet = t;
                return tId - TiledMap.Tilesets[t].FirstGid;
            }

            return -1;
        }

        /// <summary>
        /// Get the image id of the tile in the coordinate.
        /// The image id is relative to the tileset.
        /// </summary>
        /// <param name="coordinate">The 2D coordinate to lookup the tile id of.</param>
        /// <param name="layer">The tile layer to check in.</param>
        /// <param name="tileSet">The id of the tile set in which the image id is.</param>
        /// <returns></returns>
        public int GetTileImageIdInLayer(Vector2 coordinate, int layer, out int tileSet)
        {
            int oneDCoord = GetTile1DFromTile2D(coordinate);
            return GetTileImageIdInLayer(oneDCoord, layer, out tileSet);
        }

        /// <summary>
        /// Returns the tile coordinate of the tile at the specified coordinates.
        /// </summary>
        /// <param name="location">The coordinates in world space you want to sample.</param>
        /// <returns>The id of a singular tile in which the provided coordinates lay.</returns>
        public Vector2 GetTileCoordinateFromLocation(Vector2 location)
        {
            if (TiledMap == null) return Vector2.Zero;

            var left = (int) Math.Max(0, (location.X - X) / (TiledMap.TileWidth * Size.X));
            var top = (int) Math.Max(0, (location.Y - Y) / (TiledMap.TileHeight * Size.Y));

            return new Vector2(left, top);
        }

        /// <summary>
        /// Get the world position of a tile. This is the position that includes the calculated Z value and
        /// is taken from the map's render cache.
        /// </summary>
        public Vector3 GetWorldPosOfTile(Vector2 tileCoordinate, int layerIdx)
        {
            if (layerIdx == -1 || _cachedTileRenderData == null) return Vector3.Zero;

            int tile1D = GetTile1DFromTile2D(tileCoordinate);
            if (tile1D == -1) return Vector3.Zero;
            VertexData[]? renderCache = _cachedTileRenderData[layerIdx];
            return renderCache?[tile1D * 4].Vertex ?? Vector3.Zero;
        }

        /// <summary>
        /// Get the bounds of a tile from its tile coordinate.
        /// </summary>
        /// <param name="coordinate">The tile coordinate.</param>
        /// <returns>The in-world position of the tile.</returns>
        public Rectangle GetTileBounds(Vector2 coordinate)
        {
            if (TiledMap == null) return Rectangle.Empty;

            // Check if out of range.
            if (coordinate.X > TiledMap.Width || coordinate.Y > TiledMap.Height) return Rectangle.Empty;

            // Check if out of range, and if not return the tile location from the id.
            return new Rectangle(
                X + (int) coordinate.X * (TiledMap.TileWidth * Size.X),
                Y + (int) coordinate.Y * (TiledMap.TileHeight * Size.Y),
                TiledMap.TileWidth * Size.X,
                TiledMap.TileHeight * Size.Y
            );
        }

        #endregion

        private void MapUnloadInternal()
        {
            MapUnloading();

            // Reset holders.
            _loaded = false;
            Tilesets.Clear();
            _animatedTiles.Clear();
            TiledMap = null;
        }

        public void Dispose()
        {
            MapUnloadInternal();
        }

        #region Override Interface

        protected virtual float CalculateZOrder(Vector2 tileWorldPos, Vector2 tileWorldSize, Vector2 tileCoordinate, int layerIdx, int imageId, int tilesetId)
        {
            return 0;
        }

        protected virtual void MapPreLoad()
        {
        }

        protected virtual void MapPostLoad()
        {
        }

        /// <summary>
        /// Called when the map is reloaded or disposed of.
        /// Internal resources will be cleared separately.
        /// </summary>
        protected virtual void MapUnloading()
        {
            // Dispose of tilesets. The tile map always assumed it owns the textures it loaded.
            foreach (TextureAsset? tileset in Tilesets)
            {
                if (tileset == null) continue;
                Engine.AssetLoader.Destroy(tileset.Name);
            }
        }

        #endregion
    }
}