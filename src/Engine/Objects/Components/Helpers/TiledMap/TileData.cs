using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoulEngine.Objects.Components.Helpers
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
        #region "Privates ;)"
        /// <summary>
        /// The map object this tile belongs to.
        /// </summary>
        private TiledMap originMap;
        /// <summary>
        /// The X coordinate of the tile in map space.
        /// </summary>
        private int X = -1;
        /// <summary>
        /// The Y coordinate of the tile, in map space.
        /// </summary>
        private int Y = -1;
        /// <summary>
        /// The X coordinate of the tile in world space.
        /// </summary>
        private int XWorld = -1;
        /// <summary>
        /// The Y coordinate of the tile in world space.
        /// </summary>
        private int YWorld = -1;
        /// <summary>
        /// The tileset this tile belongs to.
        /// </summary>
        private int tileSet = -1;
        /// <summary>
        /// The ID of the tile image within the tileset.
        /// </summary>
        private int tileImage = -1;
        /// <summary>
        /// The layer the tile is on.
        /// </summary>
        private int layer = -1;
        /// <summary>
        /// A list of all objects on this tile.
        /// </summary>
        private List<TileObject> tileObjects;
        #endregion
        #region "Accessors"
        public TiledMap OriginMap
        {
            get
            {
                return originMap;
            }
        }
        public int TileSet
        {
            get
            {
                return tileSet;
            }
        }
        public int TileImage
        {
            get
            {
                return tileImage;
            }
        }
        public int Layer
        {
            get
            {
                return layer;
            }
        }
        public Vector2 Location
        {
            get
            {
                return new Vector2(X, Y);
            }
        }
        public Vector2 WorldLocation
        {
            get
            {
                return new Vector2(XWorld, YWorld);
            }
        }
        public List<TileObject> TileObjects
        {
            get
            {
                return tileObjects;
            }
        }
        #endregion

        /// <summary>
        /// Initializes a tiledata object.
        /// </summary>
        /// <param name="_originMap">The map the tile is from.</param>
        /// <param name="_Location">The location in tile space.</param>
        /// <param name="_WLocation">The location in world space.</param>
        /// <param name="_tileSet">The tileset this tile is from.</param>
        /// <param name="_tileImage">The image id within the tileset.</param>
        /// <param name="_layer">The layer the tile is on.</param>
        /// <param name="_tileObjects">A list of objects on this tile.</param>
        public TileData(TiledMap _originMap, Vector2 _Location, Vector2 _WLocation, int _tileSet, int _tileImage, int _layer, List<TileObject> _tileObjects)
        {
            originMap = _originMap;
            X = (int)_Location.X;
            Y = (int)_Location.Y;
            XWorld = (int)_WLocation.X;
            YWorld = (int)_WLocation.Y;
            tileSet = _tileSet;
            tileImage = _tileImage;
            layer = _layer;
            tileObjects = _tileObjects;
        }

        /// <summary>
        /// Prints all data properties as a string.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return "[TileData]\nTileLoc:" + Location.ToString() +
                "\nWLoc:" + WorldLocation.ToString() +
                "\nLayer:" + layer +
                "\nImage: " + tileImage +
                " on tileSet: " + tileSet +
                "\n{Objects:}\n" + string.Join("\n", tileObjects);
        }
    }
}
