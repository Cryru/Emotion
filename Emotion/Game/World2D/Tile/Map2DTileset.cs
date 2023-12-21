#region Using


#endregion

#nullable enable

namespace Emotion.Game.World2D.Tile
{
    /// <summary>
    /// Represents a tileset texture that can be referenced by layers to build up a tilemap.
    /// Note that modifying the number of tiles present in such texture (meaning changing its size)
    /// will offset all tiles in maps that use this tileset.
    /// </summary>
    public class Map2DTileset
    {
        public string? AssetFile;
        public float Spacing = 0f;
        public float Margin = 0f;

        public override string ToString()
        {
            return AssetFile ?? "No Tileset Texture";
        }
    }
}