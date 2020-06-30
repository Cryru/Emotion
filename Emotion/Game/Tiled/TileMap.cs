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
using Emotion.IO;
using Emotion.Primitives;
using Emotion.Standard.Logging;
using Emotion.Standard.TMX;
using Emotion.Standard.TMX.Layer;
using Emotion.Standard.TMX.Object;
using Emotion.Standard.XML;
using Emotion.Utility;

#endregion

namespace Emotion.Game.Tiled
{
    public class TileMap<T> : TransformRenderable, IDisposable where T : TransformRenderable
    {
        #region Properties

        /// <summary>
        /// The range of the map to render. If null then the camera range will be used.
        /// </summary>
        public Rectangle? Clip;

        /// <summary>
        /// The TiledSharp object the map is using.
        /// </summary>
        public TmxMap TiledMap { get; protected set; }

        /// <summary>
        /// Loaded tileset textures.
        /// </summary>
        public List<TextureAsset> Tilesets { get; private set; } = new List<TextureAsset>();

        /// <summary>
        /// The size of a tile in pixels.
        /// </summary>
        public Vector2 TileSize
        {
            get => new Vector2(TiledMap.TileWidth, TiledMap.TileHeight);
        }

        /// <summary>
        /// The size of the map in tiles.
        /// </summary>
        public Vector2 SizeInTiles
        {
            get => new Vector2(TiledMap.Width, TiledMap.Height);
        }

        /// <summary>
        /// The size of the map in the world.
        /// </summary>
        public Vector2 WorldSize
        {
            get => new Vector2(TiledMap.Width * TiledMap.TileWidth, TiledMap.Height * TiledMap.TileHeight);
        }

        /// <summary>
        /// Holds the objects created by the object factory (if any).
        /// </summary>
        public QuadTree<T> Objects { get; protected set; } = new QuadTree<T>(Rectangle.Empty);

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
        protected string _tilesetFolder;

        /// <summary>
        /// Cached render data for tiles per layer.
        /// </summary>
        protected VertexData[][] _cachedTileRenderData;

        /// <summary>
        /// Reusable memory for querying the quad tree.
        /// </summary>
        protected List<T> _quadTreeQueryMemory = new List<T>();

        /// <summary>
        /// The clip rect doesn't have to be tile aligned, so we expand it with a safe area to ensure all visible tiles are
        /// covered.
        /// This is the number of tiles for tiles and 25 * SafeArea pixels for objects.
        /// </summary>
        public int SafeArea = 2;

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
        public TileMap(TextAsset mapFile) : base(Vector2.Zero, Vector2.One)
        {
            if (mapFile == null) return;
            Reset(mapFile);
        }

        /// <summary>
        /// Create a new map object from a Tiled map.
        /// </summary>
        /// <param name="mapPath">The path to the map.</param>
        public TileMap(string mapPath) : base(Vector2.Zero, Vector2.One)
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
                if (tileSetFolder[^1] == '/') tileSetFolder = tileSetFolder.Substring(0, tileSetFolder.Length - 1);
            }

            _tilesetFolder = tileSetFolder;
            Reload();
        }

        protected virtual void LoadTilesets()
        {
            if (TiledMap.Tilesets.Count == 0) return;

            // Don't load the assets in parallel if running on the draw thread. This might cause a deadlock as assets will wait on
            // the draw thread to wake up in order to upload data.
            bool parallel = !GLThread.IsGLThread();

            var assets = new Task<TextureAsset>[TiledMap.Tilesets.Count];
            for (var i = 0; i < assets.Length; i++)
            {
                string tilesetFile = TiledMap.Tilesets[i].Source;
                if (string.IsNullOrEmpty(tilesetFile)) continue;
                tilesetFile = AssetLoader.NameToEngineName(tilesetFile);
                if (tilesetFile[0] == '/') tilesetFile = tilesetFile.Substring(1);

                string assetPath = AssetLoader.JoinPath(_tilesetFolder, tilesetFile);
                if (parallel)
                    assets[i] = Engine.AssetLoader.GetAsync<TextureAsset>(assetPath);
                else
                    assets[i] = Task.FromResult(Engine.AssetLoader.Get<TextureAsset>(assetPath));
            }

            // ReSharper disable once CoVariantArrayConversion
            Task.WaitAll(assets);

            for (var i = 0; i < assets.Length; i++)
            {
                if (assets[i] != null && assets[i].IsCompletedSuccessfully && assets[i].Result != null)
                    Tilesets.Add(assets[i].Result);
                else
                    Tilesets.Add(null);
            }
        }

        /// <summary>
        /// Reload the currently loaded map.
        /// </summary>
        public void Reload()
        {
            TmxMap map = TiledMap;
            MapUnloadInternal();
            TiledMap = map;
            ResetInternal(map);
        }

        /// <summary>
        /// Reset the tile map with another map and tileset. If an empty string is provided the map is reset to an unloaded state.
        /// </summary>
        /// <param name="mapPath">The path to the new map.</param>
        public void Reset(string mapPath)
        {
            Reset(mapPath != null ? Engine.AssetLoader.Get<TextAsset>(mapPath) : null);
        }

        /// <summary>
        /// Reset the tile map with another map and tileset. If an empty string is provided the map is reset to an unloaded state.
        /// </summary>
        /// <param name="mapFile">The new map file.</param>
        public virtual void Reset(TextAsset mapFile)
        {
            MapUnloadInternal();

            // Check if no map is provided.
            if (mapFile == null) return;

            // Load the map from the data as a stream.
            try
            {
                TiledMap = new TmxMap(new XMLReader(mapFile.Content));
            }
            catch (Exception ex)
            {
                Engine.Log.Warning($"Couldn't parse tilemap - {ex}.", MessageSource.Other);
                return;
            }

            if (string.IsNullOrEmpty(_tilesetFolder)) _tilesetFolder = AssetLoader.GetDirectoryName(mapFile.Name);

            ResetInternal(TiledMap);
        }

        protected void ResetInternal(TmxMap map)
        {
            if (map == null) return;

            MapPreLoad();

            // Load all map tilesets.
            PerfProfiler.ProfilerEventStart("TileMap: Loading Tilesets", "Loading");
            LoadTilesets();
            PerfProfiler.ProfilerEventEnd("TileMap: Loading Tilesets", "Loading");

            // Find animated tiles.
            CacheAnimatedTiles();

            // Construct all objects.
            if (TiledMap.ObjectLayers != null && TiledMap.ObjectLayers.Count > 0)
            {
                Objects.Reset(new Rectangle(0, 0, WorldSize));

                // For each layer with objects.
                for (var i = 0; i < TiledMap.ObjectLayers.Count; i++)
                {
                    // For each object.
                    for (var j = 0; j < TiledMap.ObjectLayers[i].Objects.Count; j++)
                    {
                        TmxObject objDef = TiledMap.ObjectLayers[i].Objects[j];
                        TextureAsset asset = null;
                        Rectangle? uv = null;
                        if (objDef.Gid != null)
                        {
                            uv = GetUvFromTileImageId(objDef.Gid.Value, out int tsId);
                            if (tsId > 0 && tsId < Tilesets.Count) asset = Tilesets[tsId];
                        }

                        T factoryObject = CreateObject(objDef, asset, uv, i);
                        if (factoryObject != null) Objects.Add(factoryObject);
                    }
                }
            }

            // Construct render cache.
            _cachedTileRenderData = null;
            if (TiledMap.TileLayers != null && TiledMap.TileLayers.Count > 0)
            {
                _cachedTileRenderData = new VertexData[TiledMap.TileLayers.Count][];
                for (var i = 0; i < TiledMap.TileLayers.Count; i++)
                {
                    _cachedTileRenderData[i] = SetupRenderCacheForTileLayer(i, TiledMap.TileLayers[i]);
                }
            }

            // Set loading flag.
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

        private VertexData[] SetupRenderCacheForTileLayer(int layerIdx, TmxLayer layer)
        {
            PerfProfiler.ProfilerEventStart($"TileMap: RenderCache for layer {layerIdx}", "Loading");

            if (!layer.Visible || layer.Width == 0 || layer.Height == 0 || layer.Tiles == null) return null;

            var currentCache = new VertexData[layer.Width * layer.Height * 4];
            var dataSpan = new Span<VertexData>(currentCache);

            for (var y = 0; y < layer.Height; y++)
            {
                int yIdx = y * layer.Width;

                for (var x = 0; x < layer.Width; x++)
                {
                    int tileIdx = yIdx + x;
                    Span<VertexData> tileData = dataSpan.Slice(tileIdx * 4, 4);

                    // Get the id of the tile.
                    int tId = layer.Tiles[tileIdx].Gid;

                    // If the tile is empty skip it.
                    if (tId == 0)
                    {
                        for (var i = 0; i < tileData.Length; i++)
                        {
                            tileData[i].Tid = -1;
                        }

                        continue;
                    }

                    // Find which tileset the tId belongs in.
                    Rectangle tiUv = GetUvFromTileImageId(tId, out int tsId);
                    TmxTileset ts = TiledMap.Tilesets[tsId];

                    // Encode the tileset id as a texture id.
                    for (var i = 0; i < tileData.Length; i++)
                    {
                        tileData[i].Tid = tsId;
                    }

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

                    var c = new Color(255, 255, 255, (int) (layer.Opacity * 255));
                    TextureAsset tileSet = Tilesets[tsId];
                    VertexData.SpriteToVertexData(tileData, v3, size, c, tileSet?.Texture, tiUv, layer.Tiles[tileIdx].HorizontalFlip, layer.Tiles[tileIdx].VerticalFlip);
                }
            }

            PerfProfiler.ProfilerEventEnd($"TileMap: RenderCache for layer {layerIdx}", "Loading");
            return currentCache;
        }

        public override void Render(RenderComposer composer)
        {
            RenderTileLayerRange(composer);
            RenderObjects(composer);
        }

        /// <summary>
        /// Render all tile layers in the specified index range.
        /// </summary>
        /// <param name="composer"></param>
        /// <param name="start">The layer to start from, inclusive.</param>
        /// <param name="end">The layer to render to, exclusive. -1 for until end</param>
        public void RenderTileLayerRange(RenderComposer composer, int start = 0, int end = -1)
        {
            // Check if anything is loaded.
            if (TiledMap == null || !_loaded) return;
            end = end == -1 ? TiledMap.TileLayers.Count : end;

            Rectangle clipRect = Clip ?? composer.Camera.GetWorldBoundingRect();

            for (int layer = start; layer < end; layer++)
            {
                PerfProfiler.FrameEventStart($"TileMap: Mapping layer {layer}");
                RenderLayer(composer, layer, clipRect);
                PerfProfiler.FrameEventEnd($"TileMap: Mapping layer {layer}");
            }
        }

        private void RenderLayer(RenderComposer composer, int idx, Rectangle clipVal)
        {
            VertexData[] renderCache = _cachedTileRenderData[idx];
            if (renderCache == null) return;

            TmxLayer layer = TiledMap.TileLayers[idx];
            var yStart = (int) Maths.Clamp(MathF.Floor(clipVal.Y / TiledMap.TileHeight) - SafeArea, 0, layer.Height);
            var yEnd = (int) Maths.Clamp(yStart + MathF.Ceiling(clipVal.Height / TiledMap.TileHeight) + SafeArea * 2, 0, layer.Height);
            var xStart = (int) Maths.Clamp(MathF.Floor(clipVal.X / TiledMap.TileWidth) - SafeArea, 0, layer.Width);
            var xEnd = (int) Maths.Clamp(xStart + MathF.Ceiling(clipVal.Width / TiledMap.TileWidth) + SafeArea * 2, 0, layer.Width);

            for (int y = yStart; y < yEnd; y++)
            {
                int yIdx = y * layer.Width;
                for (int x = xStart; x < xEnd; x++)
                {
                    int tileIdx = (yIdx + x) * 4;
                    if (renderCache[tileIdx].Tid == -1) continue;

                    SpriteBatchBase<VertexData> batch = composer.GetBatch();
                    TextureAsset tileset = Tilesets[(int) renderCache[tileIdx].Tid];
                    Span<VertexData> vertices = batch.GetData(tileset?.Texture);
                    float tid = vertices[0].Tid;

                    for (var i = 0; i < vertices.Length; i++)
                    {
                        vertices[i] = renderCache[tileIdx + i];
                        vertices[i].Tid = tid;
                    }
                }
            }
        }

        public void RenderObjects(RenderComposer composer)
        {
            // Check if anything is loaded.
            if (Objects == null) return;
            QueryObjectsToRender();
            PerfProfiler.FrameEventStart("TileMap: Objects");
            for (var i = 0; i < _quadTreeQueryMemory.Count; i++)
            {
                _quadTreeQueryMemory[i].Render(composer);
            }

            PerfProfiler.FrameEventEnd("TileMap: Objects");
        }

        protected virtual void QueryObjectsToRender()
        {
            _quadTreeQueryMemory.Clear();
            Rectangle clipRect = Clip ?? Engine.Renderer.Camera.GetWorldBoundingRect();
            clipRect = clipRect.Inflate(SafeArea * 25, SafeArea * 25);
            Objects.GetObjects(clipRect, ref _quadTreeQueryMemory);
            _quadTreeQueryMemory.Sort(ObjectSort);
        }

        #endregion

        #region Animated Tiles

        /// <summary>
        /// Cached all animated tiles in memory and tracks their animations.
        /// </summary>
        private void CacheAnimatedTiles()
        {
            foreach (TmxTileset tileset in TiledMap.Tilesets)
            {
                // Check if the tileset has animated tiles.
                if (tileset.Tiles.Count <= 0) continue;

                // Cache them all.
                foreach (KeyValuePair<int, TmxTilesetTile> animatedTile in tileset.Tiles)
                {
                    _animatedTiles.Add(new AnimatedTile(animatedTile.Value.Id, animatedTile.Value.AnimationFrames));
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
        protected int GetTile1DFromTile2D(Vector2 coordinate)
        {
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
        protected Vector2 GetTile2DFromTile1D(int coordinate, int layer = 0)
        {
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
            if (layer < 0 && layer > TiledMap.Layers.Count - 1 || coordinate > TiledMap.Layers[layer].Tiles.Count || coordinate < 0) return -1;

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
        /// The image id is relative to the tileset's id.
        /// </summary>
        /// <param name="coordinate">The 21D coordinate to lookup the tile id of.</param>
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
            var left = (int) Math.Max(0, (location.X - X) / (TiledMap.TileWidth * Size.X));
            var top = (int) Math.Max(0, (location.Y - Y) / (TiledMap.TileHeight * Size.Y));

            return new Vector2(left, top);
        }

        /// <summary>
        /// Get the bounds of a tile from its tile coordinate.
        /// </summary>
        /// <param name="coordinate">The tile coordinate.</param>
        /// <param name="layer">The layer the tile is on.</param>
        /// <returns>The in-world position of the tile.</returns>
        public Rectangle GetTileBounds(Vector2 coordinate, int layer = 0)
        {
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

        protected virtual T CreateObject(TmxObject objDef, TextureAsset image, Rectangle? uv, int layerId)
        {
            return null;
        }

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

        protected virtual int ObjectSort(T x, T y)
        {
            return MathF.Sign(x.Position.Z - y.Position.Z);
        }

        /// <summary>
        /// Called when the map is reloaded or disposed of.
        /// Internal resources will be cleared separately.
        /// </summary>
        protected virtual void MapUnloading()
        {
            Objects.Reset(Rectangle.Empty);

            // Dispose of old tilesets.
            if (Tilesets.Count <= 0) return;
            foreach (TextureAsset tileset in Tilesets)
            {
                Engine.AssetLoader.Destroy(tileset.Name);
            }
        }

        #endregion
    }
}