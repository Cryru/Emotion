#region Using

using System.Linq;
using Emotion.Common;
using Emotion.Standard.Logging;
using Emotion.Standard.XML;

#endregion

namespace Emotion.Standard.TMX.Layer
{
    public class TmxGroupedLayers : TmxLayer
    {
        public TmxList<TmxLayer> Layers { get; private set; }

        public TmxList<TmxLayer> TileLayers { get; private set; }
        public TmxList<TmxObjectLayer> ObjectGroups { get; private set; }
        public TmxList<TmxImageLayer> ImageLayers { get; private set; }
        public TmxList<TmxGroupedLayers> Groups { get; private set; }

        public TmxGroupedLayers(XMLReader xGroup, int width, int height) : base(xGroup, width, height)
        {
            Layers = new TmxList<TmxLayer>();
            TileLayers = new TmxList<TmxLayer>();
            ObjectGroups = new TmxList<TmxObjectLayer>();
            ImageLayers = new TmxList<TmxImageLayer>();
            Groups = new TmxList<TmxGroupedLayers>();
            foreach (XMLReader e in xGroup.Elements().Where(x => x.Name == "layer" || x.Name == "objectgroup" || x.Name == "imagelayer" || x.Name == "group"))
            {
                TmxLayer layer;
                switch (e.Name)
                {
                    case "layer":
                        var tileLayer = new TmxLayer(e, width, height);
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
                        var group = new TmxGroupedLayers(e, width, height);
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