﻿#region Using

using System.Collections.Generic;
using System.Collections.ObjectModel;
using Emotion.Standard.TMX.Layer;
using Emotion.Standard.XML;

#endregion

namespace Emotion.Standard.TMX
{
    public class TmxTilesetTile
    {
        public int Id { get; private set; }
        public Collection<TmxTerrain> TerrainEdges { get; private set; }
        public double Probability { get; private set; }
        public string Type { get; private set; }
        public string Image { get; private set; }

        public Dictionary<string, string> Properties { get; private set; }
        public TmxList<TmxObjectLayer> ObjectGroups { get; private set; }
        public Collection<TmxAnimationFrame> AnimationFrames { get; private set; }

        // Human-readable aliases to the Terrain markers
        public TmxTerrain TopLeft
        {
            get => TerrainEdges[0];
        }

        public TmxTerrain TopRight
        {
            get => TerrainEdges[1];
        }

        public TmxTerrain BottomLeft
        {
            get => TerrainEdges[2];
        }

        public TmxTerrain BottomRight
        {
            get => TerrainEdges[3];
        }

        public TmxTilesetTile(XMLReader xTile, TmxList<TmxTerrain> terrains)
        {
            Id = xTile.AttributeInt("id");

            TerrainEdges = new Collection<TmxTerrain>();

            string strTerrain = xTile.Attribute("terrain") ?? ",,,";
            foreach (string v in strTerrain.Split(','))
            {
                bool success = int.TryParse(v, out int result);
                TmxTerrain edge = success ? terrains[result] : null;
                TerrainEdges.Add(edge);

                // TODO: Assert that TerrainEdges length is 4
            }

            Probability = xTile.AttributeDoubleN("probability") ?? 1.0;
            Type = xTile.Attribute("type");
            Image = xTile.Element("image").CurrentContents();

            ObjectGroups = new TmxList<TmxObjectLayer>();
            foreach (XMLReader e in xTile.Elements("objectgroup"))
            {
                ObjectGroups.Add(new TmxObjectLayer(e));
            }

            AnimationFrames = new Collection<TmxAnimationFrame>();
            if (xTile.Element("animation") != null)
                foreach (XMLReader e in xTile.Element("animation").Elements("frame"))
                {
                    AnimationFrames.Add(new TmxAnimationFrame(e));
                }

            Properties = TmxHelpers.GetPropertyDict(xTile.Element("properties"));
        }
    }
}