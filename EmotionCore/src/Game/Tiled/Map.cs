// Emotion - https://github.com/Cryru/Emotion

#region Using

using System;
using System.Collections.Generic;
using System.IO;
using Emotion.Graphics;
using Emotion.Graphics.GLES;
using Emotion.IO;
using Emotion.Primitives;
using TiledSharp;

#endregion

namespace Emotion.Game.Tiled
{
    public class Map : TransformRenderable
    {
        #region Properties

        /// <summary>
        /// The TiledSharp object the map is using.
        /// </summary>
        public TmxMap TiledMap { get; protected set; }

        #endregion

        public List<Texture> Tilesets { get; private set; } = new List<Texture>();
        protected List<AnimatedTile> _animatedTiles = new List<AnimatedTile>();
        private AssetLoader _assetLoader;
        private bool _loaded;

        /// <summary>
        /// Create a new map object from a Tiled map.
        /// </summary>
        /// <param name="position">The position of the Map.</param>
        /// <param name="size">The size of the map. Leave at 0,0 to scale automatically.</param>
        /// <param name="assetLoader">The asset loader to use to load map and tileset assets.</param>
        /// <param name="mapPath">The path to the map.</param>
        /// <param name="tileSetFolder">The path to the folder containing the tilesets. No slash needed at the end.</param>
        public Map(Vector3 position, Vector2 size, AssetLoader assetLoader, string mapPath, string tileSetFolder) : base(position, size)
        {
            _assetLoader = assetLoader;

            // Check if no map is provided.
            if (mapPath == "") return;

            // Reset with the constructor parameters.
            Reset(mapPath, tileSetFolder);
        }

        public void Reset(string mapPath, string tileSetFolder)
        {
            // Reset loading flag.
            _loaded = false;

            // Reset holders.
            Tilesets.Clear();
            _animatedTiles.Clear();
            TiledMap = null;

            // Check if no map is provided.
            if (mapPath == "") return;

            // Load the map from the data as a stream.
            using (MemoryStream mapFileStream = new MemoryStream(_assetLoader.Get<Other>(mapPath).Content))
            {
                TiledMap = new TmxMap(mapFileStream);
            }

            // Load all map tilesets.
            foreach (TmxTileset tileset in TiledMap.Tilesets)
            {
                string tilesetFile = tileset.Image.Source;
                // Cut out the last slash if any.
                if (tilesetFile.IndexOf('/') != -1) tilesetFile = tilesetFile.Substring(tilesetFile.LastIndexOf('/'));
                if (tilesetFile.IndexOf('\\') != -1) tilesetFile = tilesetFile.Substring(tilesetFile.LastIndexOf('\\'));

                Texture temp = _assetLoader.Get<Texture>(tileSetFolder + '/' + tilesetFile);
                Tilesets.Add(temp);
            }

            // Find animated tiles.
            CacheAnimatedTiles();

            // Set default size if none set.
            if (Width == 0 && Height == 0) Size = new Vector2(TiledMap.Width * TiledMap.TileWidth, TiledMap.Height * TiledMap.TileHeight);

            // Set loading flag.
            _loaded = true;
        }

        /// <summary>
        /// Update the map. Processes animated tiles.
        /// </summary>
        /// <param name="frameTime">The time passed since the last frame.</param>
        public void Update(float frameTime)
        {
            // Update animated tiles.
            UpdateAnimatedTiles(frameTime);
        }

        /// <summary>
        /// Draw the map using the specified renderer.
        /// </summary>
        /// <param name="renderer">The renderer to use to draw the map.</param>
        public override void Render(Renderer renderer)
        {
            // Flush the buffer.
            renderer.RenderFlush();

            // Check if anything is loaded.
            if (TiledMap == null || !_loaded) return;

            // layer - The map layer currently drawing.
            // t - The tile currently drawing from [layer]. 
            //      tId - The id of the tile within all tilesets combined.
            //      tRect - The location [t] should be drawn to.
            // ts - The tileset of [t]. 
            //      tsId - The id of [ts] within the collection.
            //      tsOffset - The [tId] within the scope of the current tileset. An id and image within the map containing the ti.
            // ti - The tile image, within the [ts] which represents the image of [tsOffset].
            //      tiUv - The rectangle where the [ti] is located within the [ts] texture.

            // Go through all map layers.
            foreach (TmxLayer layer in TiledMap.Layers)
            {
                // Skip the layer if not visible.
                if (!layer.Visible) continue;

                // Go through all tiles on the layer.
                for (int t = 0; t < layer.Tiles.Count; t++)
                {
                    // Get the id of the tile.
                    int tId = layer.Tiles[t].Gid;

                    // If the tile is empty skip it.
                    if (tId == 0) continue;

                    // Find which tileset the tId belongs in.
                    Rectangle tiUv = GetUvFromTileImageId(tId, out int tsId);
                    TmxTileset ts = TiledMap.Tilesets[tsId];

                    // Get tile properties.
                    int tX = t % TiledMap.Width * TiledMap.TileWidth;
                    int tY = (int) ((float) Math.Floor(t / (double) TiledMap.Width) * TiledMap.TileHeight);

                    // Get the location of the tile.
                    Rectangle tRect = new Rectangle(tX, tY, ts.TileWidth, ts.TileHeight);

                    // Modify for map size.
                    float ratioDifferenceX = Width / (TiledMap.TileWidth * TiledMap.Width);
                    float ratioDifferenceY = Height / (TiledMap.TileHeight * TiledMap.Height);
                    tRect.Width *= ratioDifferenceX;
                    tRect.Height *= ratioDifferenceY;
                    tRect.X *= ratioDifferenceX;
                    tRect.Y *= ratioDifferenceY;

                    // Check if visible rectangle exists.
                    renderer.RenderQueue(tRect.LocationZ(0), tRect.Size, new Color(255, 255, 255, (int) (layer.Opacity * 255)), Tilesets[tsId], tiUv);
                }
            }

            // Flush the rendered tiles.
            renderer.RenderFlush();
        }

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
                foreach (TmxTilesetTile animatedTile in tileset.Tiles)
                {
                    _animatedTiles.Add(new AnimatedTile(animatedTile.Id, animatedTile.AnimationFrames));
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

        #region API

        /// <summary>
        /// Returns the layer id of the layer with the specified name, or -1 if invalid.
        /// </summary>
        /// <param name="name">The layer name to check.</param>
        /// <returns>The id of the layer matching the specified name or -1 if none found.</returns>
        public int GetLayerIdFromName(string name)
        {
            for (int i = 0; i < TiledMap.Layers.Count; i++)
            {
                if (string.Equals(TiledMap.Layers[i].Name, name, StringComparison.CurrentCultureIgnoreCase)) return i;
            }

            return -1;
        }

        /// <summary>
        /// </summary>
        /// <param name="coordinate"></param>
        /// <param name="layer"></param>
        /// <returns></returns>
        public int GetTileImageIdInLayer(int coordinate, int layer)
        {
            // Check if layer is out of bounds.
            if (layer > TiledMap.Layers.Count - 1 || coordinate > TiledMap.Layers[layer].Tiles.Count || coordinate < 0) return -1;

            //Get the GID of the tile.
            int tId = TiledMap.Layers[layer].Tiles[coordinate].Gid;

            // Find the id of tile within the tileset.
            int tsOffset = tId;
            for (int t = 0; t < TiledMap.Tilesets.Count; t++)
            {
                // Check if the current tile is beyond the first tileset.
                if (tId < TiledMap.Tilesets[t].FirstGid) break;
                if (t > 0) tsOffset -= TiledMap.Tilesets[t].FirstGid - t;
            }

            return tsOffset;
        }

        #endregion

        #region Bounds Functions

        /// <summary>
        /// Returns the pixel bounds of the tile from its one dimensional coordinate.
        /// </summary>
        /// <param name="coordinate">The tile's one dimensional coordinate.</param>
        /// <param name="layer">The layer the tile is on.</param>
        /// <returns>The pixel bounds within the map rendering of the tile..</returns>
        public Rectangle GetTileBoundsFrom1D(int coordinate, int layer = 0)
        {
            // Check if out of range.
            if (coordinate > TiledMap.Width * TiledMap.Height) return Rectangle.Empty;

            Vector2 scale = new Vector2(Width / (TiledMap.TileWidth * TiledMap.Width), Height / (TiledMap.TileHeight * TiledMap.Height));

            // Check if out of range, and if not return the tile location from the id.
            return new Rectangle(
                X + TiledMap.Layers[layer].Tiles[coordinate].X * (TiledMap.TileWidth * scale.X),
                Y + TiledMap.Layers[layer].Tiles[coordinate].Y * (TiledMap.TileHeight * scale.Y),
                TiledMap.TileWidth * scale.X,
                TiledMap.TileHeight * scale.Y
            );
        }

        /// <summary>
        /// Returns the one dimensional coordinate of the tile at the specified coordinates.
        /// </summary>
        /// <param name="location">The coordinates in world space you want to sample.</param>
        /// <returns>The id of a singular tile in which the provided coordinates lay.</returns>
        public int GetTile1DFromBounds(Vector2 location)
        {
            Vector2 scale = new Vector2(Width / (TiledMap.TileWidth * TiledMap.Width), Height / (TiledMap.TileHeight * TiledMap.Height));
            int left = (int) Math.Max(0, (location.X - X) / (TiledMap.TileWidth * scale.X));
            int top = (int) Math.Max(0, (location.Y - Y) / (TiledMap.TileHeight * scale.Y));

            return GetTile1DFromTile2D(new Vector2(left, top));
        }

        /// <summary>
        /// Converts the two dimensional tile coordinate to a one dimensional one.
        /// </summary>
        /// <param name="coordinate">The coordinate to convert.</param>
        /// <returns>A one dimensional tile coordinate.</returns>
        public int GetTile1DFromTile2D(Vector2 coordinate)
        {
            int top = (int) coordinate.Y;
            int left = (int) coordinate.X;

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
            return new Vector2(TiledMap.Layers[layer].Tiles[coordinate].X, TiledMap.Layers[layer].Tiles[coordinate].Y);
        }

        /// <summary>
        /// Returns the UV and tileset id of the specified tid.
        /// </summary>
        /// <param name="tId">The texture id to parse.</param>
        /// <param name="tsId">The tileset id containing the texture id.</param>
        /// <returns>The UV of the tid within the tsId.</returns>
        public Rectangle GetUvFromTileImageId(int tId, out int tsId)
        {
            // Find which tileset the tId belongs in.
            tsId = 0;
            int tsOffset = tId;
            for (int i = 0; i < TiledMap.Tilesets.Count; i++)
            {
                // Check if the id we need is beyond the current tileset.
                if (tId < TiledMap.Tilesets[i].FirstGid) break;
                tsId = i;
            }

            if (tsId > 0) tsOffset -= TiledMap.Tilesets[tsId].FirstGid - 1;
            tsOffset -= 1;
            TmxTileset ts = TiledMap.Tilesets[tsId];

            // Check if the current tileset has animated tiles.
            if (ts.Tiles.Count > 0)
                foreach (AnimatedTile cachedTile in _animatedTiles)
                {
                    // In that case set the tsOffset to the current frame.
                    if (cachedTile.Id == tsOffset) tsOffset = cachedTile.FrameId;
                }

            // Get tile image properties.
            int tiColumn = tsOffset % (ts.Columns ?? 0);
            int tiRow = (int) (tsOffset / (double) (ts.Columns ?? 0));
            Rectangle tiRect = new Rectangle(ts.TileWidth * tiColumn, ts.TileHeight * tiRow, ts.TileWidth, ts.TileHeight);

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