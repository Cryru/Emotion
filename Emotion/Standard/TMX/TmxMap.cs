#region Using

using System.Linq;
using Emotion.Common;
using Emotion.IO;
using Emotion.Primitives;
using Emotion.Standard.Logging;
using Emotion.Standard.TMX.Enum;
using Emotion.Standard.TMX.Layer;
using Emotion.Standard.XML;

#endregion

#nullable enable

namespace Emotion.Standard.TMX
{
    public class TmxMap
    {
        public string? Version { get; private set; }
        public string? TiledVersion { get; private set; }
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

        public TmxList<TmxTileset> Tilesets { get; private set; } = new TmxList<TmxTileset>();
        public TmxList<TmxLayer> TileLayers { get; private set; } = new TmxList<TmxLayer>();
        public TmxList<TmxObjectLayer> ObjectLayers { get; private set; } = new TmxList<TmxObjectLayer>();
        public TmxList<TmxImageLayer> ImageLayers { get; private set; } = new TmxList<TmxImageLayer>();
        public TmxList<TmxGroupedLayers> Groups { get; private set; } = new TmxList<TmxGroupedLayers>();
        public TmxProperties? Properties { get; private set; }

        public TmxList<TmxLayer> Layers { get; private set; } = new TmxList<TmxLayer>();

        public TmxMap(XMLReader reader, string? filePath = null)
        {
            XMLReader? xMap = reader.Element("map");
            if (xMap == null) return;

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

            foreach (XMLReader e in xMap.Elements("tileset"))
            {
                string? fileSource = e.Attribute("source");
                int firstGid = e.AttributeInt("firstgid");

                // Check if external file.
                if (!string.IsNullOrEmpty(fileSource))
                {
                    if (filePath != null) fileSource = AssetLoader.GetNonRelativePath(filePath, fileSource);
                    var textAsset = Engine.AssetLoader.Get<TextAsset>(fileSource);
                    if (textAsset?.Content == null)
                    {
                        Engine.Log.Warning("Couldn't load external tileset.", MessageSource.TMX);
                        continue;
                    }

                    var externalReader = new XMLReader(textAsset.Content);
                    XMLReader? tileSetElement = externalReader.Element("tileset");
                    if (tileSetElement == null) continue;
                    Tilesets.Add(new TmxTileset(firstGid, tileSetElement));
                    continue;
                }

                Tilesets.Add(new TmxTileset(firstGid, e));
            }

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
                        var objectLayer = new TmxObjectLayer(e);
                        layer = objectLayer;
                        ObjectLayers.Add(objectLayer);
                        break;
                    case "imagelayer":
                        var imageLayer = new TmxImageLayer(e);
                        layer = imageLayer;
                        ImageLayers.Add(imageLayer);
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