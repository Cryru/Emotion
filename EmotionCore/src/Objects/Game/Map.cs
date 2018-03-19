// Emotion - https://github.com/Cryru/Emotion

#region Using

using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using Emotion.Assets;
using Emotion.Engine;
using Emotion.Modules;
using Emotion.Objects.Bases;
using TiledSharp;

#endregion

namespace Emotion.Objects.Game
{
    public class Map : Transform
    {
        #region Properties

        /// <summary>
        /// The number of tiles per layer.
        /// </summary>
        public int TilesPerLayer
        {
            get => TiledMap.Layers[0].Tiles.Count;
        }

        #endregion

        protected TmxMap TiledMap;
        protected List<Texture> Tilesets;

        /// <summary>
        /// Create a new map object from a Tiled map.
        /// </summary>
        /// <param name="assetLoader">The asset loader to use to load map and tileset assets.</param>
        /// <param name="mapPath">The path to the map.</param>
        /// <param name="tileSetFolder">The path to the folder containing the tilesets. No slash needed at the end.</param>
        public Map(AssetLoader assetLoader, string mapPath, string tileSetFolder) : base(new Rectangle(0, 0, 0, 0))
        {
            // Load the map from the data as a stream.
            using (MemoryStream mapFileStream = new MemoryStream(assetLoader.GetFile(mapPath)))
            {
                TiledMap = new TmxMap(mapFileStream);
            }

            Tilesets = new List<Texture>();

            // Load all map tilesets.
            foreach (TmxTileset tileset in TiledMap.Tilesets)
            {
                string tilesetFile = tileset.Image.Source;
                tilesetFile = tilesetFile.Substring(tilesetFile.LastIndexOf('/'));

                Texture temp = assetLoader.LoadTexture(tileSetFolder + '/' + tilesetFile);
                Tilesets.Add(temp);
            }

            // Calculate size.
            Bounds.Size = new Size(TiledMap.Width * TiledMap.TileWidth, TiledMap.Height * TiledMap.TileHeight);
        }

        /// <summary>
        /// Draw the map using the specified renderer.
        /// </summary>
        /// <param name="renderer">The renderer to use to draw the map.</param>
        public void Draw(Renderer renderer)
        {
            // t - The current tile. A rectangle within pixel space and an id within the ts.
            // ts - Current tileset. An id and image within the map containing the ti.
            // tst - The current tilesets tiles. Ts data about the t objects it contains.
            // ti - The tile image, a rectangle within the ts which represents the image of t.

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

                    // Get the tileset of the layer.
                    int tsId = 0;
                    int tsOffset = tId;
                    int tsLoopLast = 0;

                    for (int ts = 0; ts < TiledMap.Tilesets.Count; ts++)
                    {
                        // Check if the id we need is beyond the current tileset.
                        if (tId < TiledMap.Tilesets[ts].FirstGid) break;

                        tsId = ts;
                        tsLoopLast = ts;
                    }

                    if (tsLoopLast > 0) tsOffset -= TiledMap.Tilesets[tsLoopLast].FirstGid - 1;

                    TmxTileset tsObject = TiledMap.Tilesets[tsId];

                    // Get tileset properties.
                    int tstWidth = tsObject.TileWidth;
                    int tstHeight = tsObject.TileHeight;
                    int tsColumns = tsObject.Columns ?? 0;

                    // Get tile image properties.
                    int tiFrame = tsOffset - 1;
                    int tiColumn = tiFrame % tsColumns;
                    int tiRow = (int) (tiFrame / (double) tsColumns);
                    Rectangle tiRect = new Rectangle(tstWidth * tiColumn, tstHeight * tiRow, tstWidth, tstHeight);

                    // Get tile properties.
                    int tX = t % TiledMap.Width * TiledMap.TileWidth;
                    int tY = (int) ((float) Math.Floor(t / (double) TiledMap.Width) * TiledMap.TileHeight);

                    // Add margins and spacing.
                    tiRect.X += tsObject.Margin;
                    tiRect.Y += tsObject.Margin;
                    tiRect.X += tsObject.Spacing * tiColumn;
                    tiRect.Y += tsObject.Spacing * tiRow;

                    // Get the location of the tile.
                    Rectangle tRect = new Rectangle(tX, tY, tstWidth, tstHeight);

                    // Add map position.
                    tRect.X += Bounds.X;
                    tRect.Y += Bounds.Y;

                    // Draw.
                    Tilesets[tsId].SetAlpha((byte) (layer.Opacity * 255));
                    renderer.DrawTexture(Tilesets[tsId], tRect, tiRect);
                }
            }
        }

        #region Functions

        /// <summary>
        /// Returns the layer id of the layer with the specified name, or -1 if invalid.
        /// </summary>
        /// <param name="name">The layer name to check.</param>
        /// <returns>The id of the layer matching the specified name or -1 if none found.</returns>
        public int GetLayerByName(string name)
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
        public int GetTileIdInLayer(int coordinate, int layer)
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

        #region PixelLocators

        /// <summary>
        /// Returns the pixel bounds of the tile from its id.
        /// </summary>
        /// <param name="coordinate">The tile coordinate.</param>
        /// <param name="layer">The layer the tile is on.</param>
        /// <returns>The pixel bounds within the map rendering of the tile.</returns>
        public Rectangle TileBoundsFromId(int coordinate, int layer = 0)
        {
            // Check if out of range, and if not return the tile location from the id.
            return coordinate >= TiledMap.Layers[layer].Tiles.Count
                ? Rectangle.Empty
                : new Rectangle(
                    Bounds.X + TiledMap.Layers[layer].Tiles[coordinate].X * TiledMap.TileWidth,
                    Bounds.Y + TiledMap.Layers[layer].Tiles[coordinate].Y * TiledMap.TileHeight,
                    TiledMap.TileWidth,
                    TiledMap.TileHeight
                );
        }

        #endregion
    }
}