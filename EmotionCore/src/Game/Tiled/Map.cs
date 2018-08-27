// Emotion - https://github.com/Cryru/Emotion

#region Using

using System;
using System.Collections.Generic;
using System.IO;
using Emotion.GLES;
using Emotion.IO;
using Emotion.Primitives;
using TiledSharp;

#endregion

namespace Emotion.Game.Tiled
{
    public class Map : Transform
    {
        #region Properties

        /// <summary>
        /// The TiledSharp object the map is using.
        /// </summary>
        public TmxMap TiledMap { get; protected set; }

        /// <summary>
        /// The rectangle to render from the whole map. If empty the whole map is rendered.
        /// </summary>
        public Rectangle VisibleRectangle { get; set; }

        #endregion

        protected List<Texture> _tilesets = new List<Texture>();
        protected List<AnimatedTile> _animatedTiles = new List<AnimatedTile>();
        private AssetLoader _assetLoader;

        /// <summary>
        /// Create a new map object from a Tiled map.
        /// </summary>
        /// <param name="mapBounds">The location and size to draw the map at.</param>
        /// <param name="assetLoader">The asset loader to use to load map and tileset assets.</param>
        /// <param name="mapPath">The path to the map.</param>
        /// <param name="tileSetFolder">The path to the folder containing the tilesets. No slash needed at the end.</param>
        public Map(Rectangle mapBounds, AssetLoader assetLoader, string mapPath, string tileSetFolder) : base(mapBounds)
        {
            _assetLoader = assetLoader;

            // Check if no map is provided.
            if (mapPath == "") return;

            // Reset with the constructor parameters.
            Reset(mapPath, tileSetFolder);
        }

        public void Reset(string mapPath, string tileSetFolder)
        {
            // Reset holders.
            _tilesets.Clear();
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
                _tilesets.Add(temp);
            }

            // Find animated tiles.
            CacheAnimatedTiles();

            // Set default size if none set.
            if (Bounds.Width == 0 && Bounds.Height == 0) Bounds.Size = new Vector2(TiledMap.Width * TiledMap.TileWidth, TiledMap.Height * TiledMap.TileHeight);
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
        public void Draw(Renderer renderer)
        {
            // Check if anything is loaded.
            if (TiledMap == null) return;

            // layer - The map layer currently drawing.
            // t - The tile currently drawing from [layer]. 
            //      tId - The id of the tile within all tilesets combined.
            //      tRect - The location [t] should be drawn to.
            // ts - The tileset of [t]. 
            //      tsId - The id of [ts] within the collection.
            //      tsOffset - The [tId] within the scope of the current tileset. An id and image within the map containing the ti.
            // ti - The tile image, within the [ts] which represents the image of [tsOffset].
            //      tiColumn - The Y coordinate of the [ti] within the [ts].
            //      tiRow - The X coordinate of the [ti] within the [ts].
            //      tiRect - The rectangle where the [ti] is located within the [ts] texture.

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
                    Rectangle tiRect = GetUvFromTileImageId(tId, out int tsId);
                    TmxTileset ts = TiledMap.Tilesets[tsId];

                    // Get tile properties.
                    int tX = t % TiledMap.Width * TiledMap.TileWidth;
                    int tY = (int)((float)Math.Floor(t / (double)TiledMap.Width) * TiledMap.TileHeight);

                    // Get the location of the tile.
                    Rectangle tRect = new Rectangle(tX, tY, ts.TileWidth, ts.TileHeight);

                    // Modify for map size.
                    float ratioDifferenceX = Bounds.Width / (TiledMap.TileWidth * TiledMap.Width);
                    float ratioDifferenceY = Bounds.Height / (TiledMap.TileHeight * TiledMap.Height);
                    tRect.Width *= ratioDifferenceX;
                    tRect.Height *= ratioDifferenceY;
                    tRect.X *= ratioDifferenceX;
                    tRect.Y *= ratioDifferenceY;

                    // Add map position.
                    tRect.X += Bounds.X;
                    tRect.Y += Bounds.Y;

                    // Check if visible rectangle exists.
                    if (VisibleRectangle == Rectangle.Empty || VisibleRectangle.Intersects(tRect))
                        renderer.DrawTexture(_tilesets[tsId], tRect, tiRect, new Color(255, 255, 255, (int)(layer.Opacity * 255)));
                }
            }
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
            int tiRow = (int)(tsOffset / (double)(ts.Columns ?? 0));
            Rectangle tiRect = new Rectangle(ts.TileWidth * tiColumn, ts.TileHeight * tiRow, ts.TileWidth, ts.TileHeight);

            // Add margins and spacing.
            tiRect.X += ts.Margin;
            tiRect.Y += ts.Margin;
            tiRect.X += ts.Spacing * tiColumn;
            tiRect.Y += ts.Spacing * tiRow;

            return tiRect;
        }

        #endregion

        #region Bounds Functions

        /// <summary>
        /// Returns the pixel bounds of the tile from its id.
        /// </summary>
        /// <param name="coordinate">The tile coordinate.</param>
        /// <param name="layer">The layer the tile is on.</param>
        /// <returns>The pixel bounds within the map rendering of the tile..</returns>
        public Rectangle GetTileBoundsFromId(int coordinate, int layer = 0)
        {
            Vector2 scale = new Vector2(Bounds.Width / (TiledMap.TileWidth * TiledMap.Width), Bounds.Height / (TiledMap.TileHeight * TiledMap.Height));

            // Check if out of range, and if not return the tile location from the id.
            return new Rectangle(
                Bounds.X + TiledMap.Layers[layer].Tiles[coordinate].X * (TiledMap.TileWidth * scale.X),
                Bounds.Y + TiledMap.Layers[layer].Tiles[coordinate].Y * (TiledMap.TileHeight * scale.Y),
                TiledMap.TileWidth * scale.X,
                TiledMap.TileHeight * scale.Y
            );
        }

        /// <summary>
        /// Returns the id of the tile at the specified coordinates.
        /// </summary>
        /// <param name="location">The coordinates in world space you want to sample.</param>
        /// <returns>The id of a singular tile in which the provided coordinates lay.</returns>
        public int GetTileIdFromBounds(Vector2 location)
        {
            int left = (int)Math.Max(0, location.X / 32);
            int top = (int)Math.Max(0, location.Y / 32);

            return left + top * TiledMap.Width;
        }

        /// <summary>
        /// Returns a two dimensional coordinate representing the provided tile coordinate.
        /// </summary>
        /// <param name="coordinate">The tile coordinate.</param>
        /// <param name="layer">The layer the tile is on.</param>
        /// <returns>The two dimensional coordinate equivalent of the one dimensional coordinate provided.</returns>
        public Vector2 GetTile2DFromId(int coordinate, int layer = 0)
        {
            return new Vector2(TiledMap.Layers[layer].Tiles[coordinate].X, TiledMap.Layers[layer].Tiles[coordinate].Y);
        }

        #endregion
    }
}