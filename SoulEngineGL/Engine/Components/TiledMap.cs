// SoulEngine - https://github.com/Cryru/SoulEngine

#region Using

using System.IO;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Soul.Engine.ECS;
using TiledSharp;

#endregion

namespace Soul.Engine.Components
{
    public class TiledMap : ComponentBase
    {
        public TmxMap Map;
        public bool HasUpdated = true;
        public RenderTarget2D CachedRender;
        public Texture2D Tileset;

        /// <summary>
        /// The path to the map file.
        /// </summary>
        public string MapPath
        {
            get => _mapPath;
            set => _mapPath = value.Replace('/', '?').Replace('\\', '?').Replace('?', Path.DirectorySeparatorChar);
        }

        private string _mapPath;

        /// <summary>
        /// Returns the data for the selected tile coordinate.
        /// </summary>
        /// <param name="tileCoordinate">The one dimensional coordinate of the tile.</param>
        /// <param name="layer">The layer of the tile.</param>
        public TileData GetTileDataFromCoordinate(int tileCoordinate, int layer)
        {
            // Check if layer is out of bounds.
            if (layer > Map.Layers.Count - 1 || tileCoordinate > Map.Layers[layer].Tiles.Count || tileCoordinate < 0)
                return new TileData(this, new Vector2(), new Vector2(), -1, -1, -1);

            // Get the GID of the tile.
            int gId = Map.Layers[layer].Tiles[tileCoordinate].Gid;

            // Get the tileset for this layer.
            int tilesetId = 0;
            int imageId = gId;
            for (int t = 0; t < Map.Tilesets.Count; t++)
            {
                // Check if the current tile is beyond the first tileset.
                if (gId > Map.Tilesets[t].FirstGid)
                {
                    tilesetId = t;

                    if (t > 0) imageId -= Map.Tilesets[t].FirstGid - t;
                }
                else
                {
                    break;
                }
            }

            // Get the location as a vector 2.
            Vector2 location = TileLocationAsVector2(tileCoordinate);
            Vector2 worldLocation = TileCoordinateToWorldLocation(tileCoordinate);

            return new TileData(this, location, worldLocation, tilesetId, imageId, layer);
        }

        /// <summary>
        /// Returns the data for the selected tile coordinate.
        /// </summary>
        /// <param name="tileCoordinate">The two dimensional coordinate of the tile.</param>
        /// <param name="layer">The layer of the tile.</param>
        public TileData GetTileDataFromCoordinate(Vector2 tileCoordinate, int layer)
        {
            return GetTileDataFromCoordinate(TileLocationAsInt(tileCoordinate), layer);
        }

        #region Converters

        /// <summary>
        /// Returns the single dimension coordinate of a tile from its two dimensional coordinate.
        /// </summary>
        public Vector2 TileLocationAsVector2(int tileCoordinate)
        {
            // Check if no map.
            if (Map == null || tileCoordinate < 0) return Vector2.Zero;

            // Check if out of range.
            if (tileCoordinate >= Map.Layers[0].Tiles.Count) return Vector2.Zero;

            return new Vector2(Map.Layers[0].Tiles[tileCoordinate].X, Map.Layers[0].Tiles[tileCoordinate].Y);
        }

        /// <summary>
        /// Returns the two dimensional coordinate of a tile from its single dimensional coordinate.
        /// </summary>
        public int TileLocationAsInt(Vector2 tileCoordinate)
        {
            // Check if invalid map.
            if (Map == null) return -1;

            // Find the index of the item that has the same X and Y values as the ones we are looking for.
            return Map.Layers[0].Tiles.IndexOf(Map.Layers[0].Tiles.ToList()
                .Find(x => x.X == tileCoordinate.X && x.Y == tileCoordinate.Y));
        }

        /// <summary>
        /// Returns the pixel location of a tile coordinate.
        /// </summary>
        public Vector2 TileCoordinateToWorldLocation(int tileCoordinate)
        {
            // Check if no map.
            if (Map == null || tileCoordinate < 0) return Vector2.Zero;

            if (tileCoordinate >= Map.Layers[0].Tiles.Count) return Vector2.Zero;

            // Get the X and Y of the tile.
            float xTile = Parent.X + Map.Layers[0].Tiles[tileCoordinate].X * GetWarpedTileSize().X;
            float yTile = Parent.Y + Map.Layers[0].Tiles[tileCoordinate].Y * GetWarpedTileSize().Y;
            // The location of the object, plus the tile's actual size warped through the scale.

            return new Vector2(xTile, yTile);
        }

        /// <summary>
        /// Converts a pixel location to a tile coordinate.
        /// </summary>
        public int WorldLocationToTileCoordinate(Vector2 worldCoordinate)
        {
            // Check if no map.
            if (Map == null) return -1;

            // Assign a selector.
            Rectangle selector = new Rectangle(worldCoordinate.ToPoint(), new Point(1, 1));

            // Run through all tiles until the selector is hit by a tile.
            for (int i = 0; i < Map.Layers[0].Tiles.Count; i++)
            {
                Vector2 tileWorldLocation = TileCoordinateToWorldLocation(i);

                if (selector.Intersects(new Rectangle(tileWorldLocation.ToPoint(), GetWarpedTileSize().ToPoint())))
                    return i;
            }

            // If not return an invalid value.
            return -1;
        }

        /// <summary>
        /// Returns the size of tiles as warped through the current size of the map object.
        /// </summary>
        /// <returns></returns>
        public Vector2 GetWarpedTileSize()
        {
            // Check if invalid map.
            switch (Map)
            {
                case null:
                    return Vector2.Zero;
                default:
                    //  Calculate warp scale.
                    float xScale = Parent.Width / (Map.Width * Map.TileWidth);
                    float yScale = Parent.Height / (Map.Height * Map.TileHeight);

                    // Get the X and Y of the tile.
                    float warpedWidth = Map.TileWidth * xScale;
                    float warpedHeight = Map.TileHeight * yScale;

                    return new Vector2(warpedWidth, warpedHeight);
            }
        }

        #endregion
    }
}