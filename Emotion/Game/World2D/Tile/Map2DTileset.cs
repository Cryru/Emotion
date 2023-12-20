#region Using


#endregion

#nullable enable

namespace Emotion.Game.World2D.Tile
{
    /// <summary>
    /// Represents a tileset image that contains all the tiles used by a tile layer.
    /// </summary>
    public class Map2DTileset
    {
        public string AssetFile;
        public int FirstTileId;

        public float Spacing = 0f;
        public float Margin = 0f;

        public override string ToString()
        {
            return AssetFile ?? "No Tileset Texture";
        }
    }
}