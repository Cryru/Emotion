#region Using

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Numerics;
using Emotion.Standard.TMX.Layer;
using Emotion.Standard.XML;

#endregion

namespace Emotion.Standard.TMX.Object
{
    public class TmxObject : ITmxElement
    {
        // Many TmxObjectTypes are distinguished by null values in fields
        // It might be smart to subclass TmxObject
        public int Id { get; private set; }
        public string Name { get; private set; }
        public TmxObjectType ObjectType { get; private set; }
        public string Type { get; private set; }
        public double X { get; private set; }
        public double Y { get; private set; }
        public double Width { get; private set; }
        public double Height { get; private set; }
        public double Rotation { get; private set; }
        public TmxLayerTile Tile { get; private set; }
        public bool Visible { get; private set; }
        public TmxText Text { get; private set; }

        public Collection<Vector2> Points { get; private set; }
        public Dictionary<string, string> Properties { get; private set; }

        public TmxObject(XMLReader xObject)
        {
            Id = xObject.AttributeInt("id");
            Name = xObject.Attribute("name") ?? string.Empty;
            X = xObject.AttributeDouble("x");
            Y = xObject.AttributeDouble("y");
            Width = xObject.AttributeDouble("width");
            Height = xObject.AttributeDouble("height");
            Type = xObject.Attribute("type") ?? string.Empty;
            Visible = xObject.AttributeBoolN("visible") ?? true;
            Rotation = xObject.AttributeDouble("rotation");

            // Assess object type and assign appropriate content
            uint? xGid = xObject.AttributeUIntN("gid");
            XMLReader xEllipse = xObject.Element("ellipse");
            XMLReader xPolygon = xObject.Element("polygon");
            XMLReader xPolyline = xObject.Element("polyline");

            if (xGid != null)
            {
                Tile = new TmxLayerTile((uint) xGid, Convert.ToInt32(Math.Round(X)), Convert.ToInt32(Math.Round(Y)));
                ObjectType = TmxObjectType.Tile;
            }
            else if (xEllipse != null)
            {
                ObjectType = TmxObjectType.Ellipse;
            }
            else if (xPolygon != null)
            {
                Points = ParsePoints(xPolygon);
                ObjectType = TmxObjectType.Polygon;
            }
            else if (xPolyline != null)
            {
                Points = ParsePoints(xPolyline);
                ObjectType = TmxObjectType.Polyline;
            }
            else
            {
                ObjectType = TmxObjectType.Basic;
            }

            XMLReader xText = xObject.Element("text");
            if (xText != null) Text = new TmxText(xText);

            Properties = TmxHelpers.GetPropertyDict(xObject.Element("properties"));
        }

        public static Collection<Vector2> ParsePoints(XMLReader xPoints)
        {
            var points = new Collection<Vector2>();

            string pointString = xPoints.Attribute("points");
            string[] pointStringPair = pointString.Split(' ');
            foreach (string s in pointStringPair)
            {
                Vector2 pt = TmxHelpers.GetObjectPoint(s);
                points.Add(pt);
            }

            return points;
        }
    }
}