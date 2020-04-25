#region Using

using System.Collections.Generic;
using System.Collections.ObjectModel;
using Emotion.Common;
using Emotion.Standard.Logging;
using Emotion.Standard.XML;

#endregion

namespace Emotion.Standard.TMX.Layer
{
    public class TmxLayer : ITmxElement
    {
        public string Name { get; private set; }

        public int Width { get; private set; }
        public int Height { get; private set; }

        public double Opacity { get; private set; }
        public bool Visible { get; private set; }
        public float OffsetX { get; private set; }
        public float OffsetY { get; private set; }

        public Collection<TmxLayerTile> Tiles { get; private set; }
        public Dictionary<string, string> Properties { get; private set; }

        public TmxLayer(XMLReader xLayer, int width, int height)
        {
            Name = xLayer.Attribute("name");
            Width = width;
            Height = height;

            Opacity = xLayer.AttributeDoubleN("opacity") ?? 1.0;
            Visible = xLayer.AttributeBoolN("visible") ?? true;
            OffsetX = (float) xLayer.AttributeDouble("offsetx");
            OffsetY = (float) xLayer.AttributeDouble("offsety");

            Properties = TmxHelpers.GetPropertyDict(xLayer.Element("properties"));

            // Not a layer which contains tiles.
            if (width == 0) return;
            XMLReader xData = xLayer.Element("data");
            string encoding = xData.Attribute("encoding");
            Tiles = new Collection<TmxLayerTile>();
            switch (encoding)
            {
                case "csv":
                {
                    string csvData = xData.CurrentContents();
                    foreach (string s in csvData.Split(','))
                    {
                        uint gid = uint.Parse(s.Trim());
                        Tiles.Add(new TmxLayerTile(gid));
                    }

                    break;
                }
                case null:
                {
                    foreach (XMLReader e in xData.Elements("tile"))
                    {
                        uint gid = e.AttributeUInt("gid");
                        Tiles.Add(new TmxLayerTile(gid));
                    }

                    break;
                }
                default:
                    Engine.Log.Warning($"Unknown tmx layer encoding {encoding}", MessageSource.TMX);
                    return;
            }
        }
    }
}