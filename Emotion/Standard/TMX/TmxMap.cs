#region Using

using System.Collections.Generic;
using System.Linq;
using Emotion.Common;
using Emotion.Primitives;
using Emotion.Standard.Logging;
using Emotion.Standard.TMX.Enum;
using Emotion.Standard.TMX.Layer;
using Emotion.Standard.XML;

#endregion

namespace Emotion.Standard.TMX
{
    public class TmxMap
    {
        public string Version { get; private set; }
        public string TiledVersion { get; private set; }
        public int Width { get; private set; }
        public int Height { get; private set; }
        public int TileWidth { get; private set; }
        public int TileHeight { get; private set; }
        public int? HexSideLength { get; private set; }
        public Orientation Orientation { get; private set; }
        public StaggerAxis StaggerAxis { get; private set; }
        public StaggerIndex StaggerIndex { get; private set; }
        public RenderOrder RenderOrder { get; private set; }
        public Color BackgroundColor { get; private set; }
        public int? NextObjectId { get; private set; }

        public TmxList<TmxTileset> Tilesets { get; private set; }
        public TmxList<TmxLayer> TileLayers { get; private set; }
        public TmxList<TmxObjectLayer> ObjectGroups { get; private set; }
        public TmxList<TmxImageLayer> ImageLayers { get; private set; }
        public TmxList<TmxGroupedLayers> Groups { get; private set; }
        public Dictionary<string, string> Properties { get; private set; }

        public TmxList<TmxLayer> Layers { get; private set; }

        public TmxMap(XMLReader reader)
        {
            XMLReader xMap = reader.Element("map");
            Version = xMap.Attribute("version");
            TiledVersion = xMap.Attribute("tiledversion");

            Width = xMap.AttributeInt("width");
            Height = xMap.AttributeInt("height");
            TileWidth = xMap.AttributeInt("tilewidth");
            TileHeight = xMap.AttributeInt("tileheight");
            HexSideLength = xMap.AttributeIntN("hexsidelength");
            Orientation = xMap.AttributeEnum<Orientation>("orientation");
            StaggerAxis = xMap.AttributeEnum<StaggerAxis>("staggeraxis");
            StaggerIndex = xMap.AttributeEnum<StaggerIndex>("staggerindex");
            RenderOrder = xMap.AttributeEnum<RenderOrder>("renderorder");
            NextObjectId = xMap.AttributeIntN("nextobjectid");
            BackgroundColor = TmxHelpers.ParseTmxColor(xMap.Attribute("backgroundcolor"));

            Properties = TmxHelpers.GetPropertyDict(xMap.Element("properties"));

            Tilesets = new TmxList<TmxTileset>();
            foreach (XMLReader e in xMap.Elements("tileset"))
            {
                Tilesets.Add(new TmxTileset(e));
            }

            Layers = new TmxList<TmxLayer>();
            TileLayers = new TmxList<TmxLayer>();
            ObjectGroups = new TmxList<TmxObjectLayer>();
            ImageLayers = new TmxList<TmxImageLayer>();
            Groups = new TmxList<TmxGroupedLayers>();
            foreach (XMLReader e in xMap.Elements().Where(x => x.Name == "layer" || x.Name == "objectgroup" || x.Name == "imagelayer" || x.Name == "group"))
            {
                TmxLayer layer;
                switch (e.Name)
                {
                    case "layer":
                        var tileLayer = new TmxLayer(e, Width, Height);
                        layer = tileLayer;
                        TileLayers.Add(tileLayer);
                        break;
                    case "objectgroup":
                        var objectgroup = new TmxObjectLayer(e);
                        layer = objectgroup;
                        ObjectGroups.Add(objectgroup);
                        break;
                    case "imagelayer":
                        var imagelayer = new TmxImageLayer(e);
                        layer = imagelayer;
                        ImageLayers.Add(imagelayer);
                        break;
                    case "group":
                        var group = new TmxGroupedLayers(e, Width, Height);
                        layer = group;
                        Groups.Add(group);
                        break;
                    default:
                        Engine.Log.Warning($"Unknown TMX layer type {e.Name}.", MessageSource.TMX);
                        continue;
                }

                Layers.Add(layer);
            }
        }
    }
}