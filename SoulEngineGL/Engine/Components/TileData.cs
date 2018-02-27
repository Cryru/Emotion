// SoulEngine - https://github.com/Cryru/SoulEngine

#region Using

using Microsoft.Xna.Framework;

#endregion

namespace Soul.Engine.Components
{
    //////////////////////////////////////////////////////////////////////////////
    // SoulEngine - A game engine based on the MonoGame Framework.              //
    // Public Repository: https://github.com/Cryru/SoulEngine                   //
    //////////////////////////////////////////////////////////////////////////////
    /// <summary>
    /// Tile information for a TiledMap.
    /// </summary>
    public class TileData
    {
        #region Data

        /// <summary>
        /// The map object this tile belongs to.
        /// </summary>
        public TiledMap ParentMap {get; private set;}

        /// <summary>
        /// The X coordinate of the tile in map space.
        /// </summary>
        public int X {get; private set;}

        /// <summary>
        /// The Y coordinate of the tile, in map space.
        /// </summary>
        public int Y {get; private set;}

        /// <summary>
        /// The X coordinate of the tile in world space.
        /// </summary>
        public int XWorld {get; private set;}

        /// <summary>
        /// The Y coordinate of the tile in world space.
        /// </summary>
        public int YWorld {get; private set;}

        /// <summary>
        /// The tileset this tile belongs to.
        /// </summary>
        public int TileSet  {get; private set;}

        /// <summary>
        /// The ID of the tile image within the tileset.
        /// </summary>
        public int TileImage  {get; private set;}

        /// <summary>
        /// The layer the tile is on.
        /// </summary>
        public int Layer;

        #endregion

        /// <summary>
        /// Initializes a tiledata object.
        /// </summary>
        /// <param name="parentMap">The map the tile is from.</param>
        /// <param name="location">The location in tile space.</param>
        /// <param name="wLocation">The location in world space.</param>
        /// <param name="tileSet">The tileset this tile is from.</param>
        /// <param name="tileImage">The image id within the tileset.</param>
        /// <param name="layer">The layer the tile is on.</param>
        public TileData(TiledMap parentMap, Vector2 location, Vector2 wLocation, int tileSet, int tileImage,
            int layer)
        {
            ParentMap = parentMap;
            X = (int) location.X;
            Y = (int) location.Y;
            XWorld = (int) wLocation.X;
            YWorld = (int) wLocation.Y;
            TileSet = tileSet;
            TileImage = tileImage;
            Layer = layer;
        }
    }
}