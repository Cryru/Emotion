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
    /// A Tiled map object.
    /// </summary>
    public class TileObject
    {
        #region "Declarations"
        public string Name;
        public string Type;
        public Dictionary<string, string> Properties = new Dictionary<string, string>();
        public int Location;
        #endregion

        /// <summary>
        /// Initializes a tile object.
        /// </summary>
        /// <param name="Name">The name of the object.</param>
        /// <param name="Properties">The object's properties.</param>
        /// <param name="X">The X location, will be used to calculate tile coordinate.</param>
        /// <param name="Y">The Y location, will be used to calculate tile coordinate.</param>
        public TileObject(string Name, Dictionary<string, string> Properties, string Type, int X, int Y, Vector2 TileSize, int mapWidth)
        {
            //Assign properties.
            this.Name = Name;
            this.Properties = Properties;
            this.Type = Type;

            //Calculate location.
            X /= (int)TileSize.X;
            Y /= (int)TileSize.Y;

            Location = Y * mapWidth + X % mapWidth;
        }
    }
}
