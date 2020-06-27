﻿#region Using

using System.Collections.Generic;
using System.Numerics;
using Emotion.Primitives;
using Emotion.Standard.XML;

#endregion

namespace Emotion.Standard.TMX.Object
{
    public class TmxObject : ITmxElement
    {
        public int Id { get; private set; }
        public string Name { get; private set; }
        public TmxObjectType ObjectType { get; private set; }
        public string Type { get; private set; }
        public float X { get; private set; }
        public float Y { get; private set; }
        public float Width { get; private set; }
        public float Height { get; private set; }
        public float Rotation { get; private set; }
        public bool Visible { get; private set; }
        public Dictionary<string, string> Properties { get; private set; }

        // Image object.
        public int? Gid { get; private set; }
        public bool HorizontalFlip;
        public bool VerticalFlip;
        public bool DiagonalFlip;

        // Text object
        public TmxText Text { get; private set; }

        // Polygon object.
        public List<Vector2> Points { get; private set; }

        // Polyline object.
        public List<LineSegment> Lines { get; private set; }

        public TmxObject(XMLReader xObject)
        {
            Id = xObject.AttributeInt("id");
            Name = xObject.Attribute("name") ?? string.Empty;
            X = xObject.AttributeFloat("x");
            Y = xObject.AttributeFloat("y");
            Width = xObject.AttributeFloat("width");
            Height = xObject.AttributeFloat("height");
            Type = xObject.Attribute("type") ?? string.Empty;
            Visible = xObject.AttributeBoolN("visible") ?? true;
            Rotation = xObject.AttributeFloat("rotation");

            // Assess object type and assign appropriate content
            uint? rawGid = xObject.AttributeUIntN("gid");
            if (rawGid != null) Gid = TmxHelpers.GetGidFlags((uint) rawGid, out HorizontalFlip, out VerticalFlip, out DiagonalFlip);

            XMLReader xEllipse = xObject.Element("ellipse");
            XMLReader xPolygon = xObject.Element("polygon");
            XMLReader xPolyline = xObject.Element("polyline");

            if (Gid != null)
            {
                ObjectType = TmxObjectType.Image;
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
                List<Vector2> points = ParsePoints(xPolyline);
                Lines = new List<LineSegment>(points.Count - 1);
                for (var i = 0; i < points.Count - 1; i++)
                {
                    Lines.Add(new LineSegment(points[i], points[i + 1]));
                }

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

        public List<Vector2> ParsePoints(XMLReader xPoints)
        {
            var points = new List<Vector2>();

            string pointString = xPoints.Attribute("points");
            string[] pointStringPair = pointString.Split(' ');
            foreach (string s in pointStringPair)
            {
                Vector2 pt = TmxHelpers.GetObjectPoint(s);

                // Point coordinates are relative to the object.
                pt.X += X;
                pt.Y += Y;

                points.Add(pt);
            }

            return points;
        }
    }
}