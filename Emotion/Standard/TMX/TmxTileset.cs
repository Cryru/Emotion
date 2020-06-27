#region Using

using System.Collections.Generic;
using System.Numerics;
using Emotion.Standard.XML;

#endregion

namespace Emotion.Standard.TMX
{
    public class TmxTileset : ITmxElement
    {
        public int FirstGid { get; private set; }
        public string Name { get; private set; }
        public int TileWidth { get; private set; }
        public int TileHeight { get; private set; }
        public int Spacing { get; private set; }
        public int Margin { get; private set; }
        public int? Columns { get; private set; }
        public int? TileCount { get; private set; }
        public string Source { get; private set; }

        public Dictionary<int, TmxTilesetTile> Tiles { get; private set; }
        public Vector2 TileOffset { get; private set; }
        public Dictionary<string, string> Properties { get; private set; }
        public TmxList<TmxTerrain> Terrains { get; private set; }

        public TmxTileset(XMLReader xTileset)
        {
            FirstGid = xTileset.AttributeInt("firstgid");
            Name = xTileset.Attribute("name");
            TileWidth = xTileset.AttributeInt("tilewidth");
            TileHeight = xTileset.AttributeInt("tileheight");
            Spacing = xTileset.AttributeIntN("spacing") ?? 0;
            Margin = xTileset.AttributeInt("margin");
            Columns = xTileset.AttributeIntN("columns");
            TileCount = xTileset.AttributeIntN("tilecount");
            TileOffset = TmxHelpers.GetVector2(xTileset.Element("tileoffset"));

            XMLReader image = xTileset.Element("image");
            if (image != null) Source = image.Attribute("source");

            Terrains = new TmxList<TmxTerrain>();
            XMLReader xTerrainType = xTileset.Element("terraintypes");
            if (xTerrainType != null)
                foreach (XMLReader e in xTerrainType.Elements("terrain"))
                {
                    Terrains.Add(new TmxTerrain(e));
                }

            Tiles = new Dictionary<int, TmxTilesetTile>();
            foreach (XMLReader xTile in xTileset.Elements("tile"))
            {
                var tile = new TmxTilesetTile(xTile, Terrains);
                Tiles[tile.Id] = tile;
            }

            Properties = TmxHelpers.GetPropertyDict(xTileset.Element("properties"));
        }
    }
}