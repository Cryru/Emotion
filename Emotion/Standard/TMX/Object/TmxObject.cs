#region Using

using System.Collections.Generic;
using System.Numerics;
using Emotion.Primitives;
using Emotion.Standard.XML;
using Emotion.Utility;

#endregion

namespace Emotion.Standard.TMX.Object
{
    public class TmxObject : ITmxElement
    {
        public Vector2 Position
        {
            get => new Vector2(X, Y);
        }

        public Vector2 Size
        {
            get => new Vector2(Width, Height);
        }

        public int Id { get; private set; }
        public string Name { get; private set; }
        public TmxObjectType ObjectType { get; private set; }
        public string Type { get; set; }
        public float X { get; set; }
        public float Y { get; set; }
        public float Width { get; private set; }
        public float Height { get; private set; }
        public float Rotation { get; private set; }
        public bool Visible { get; private set; }
        public TmxProperties Properties { get; private set; }

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
                // In Tiled an image's X,Y coordinates represent the bottom-left corner of the image
                Y -= Height;
            }
            else if (xEllipse != null)
            {
                ObjectType = TmxObjectType.Ellipse;
            }
            else if (xPolygon != null)
            {
                Points = ParsePoints(xPolygon);
                ObjectType = TmxObjectType.Polygon;

                // Preprocess rotation in polygons.
                if (Rotation != 0)
                {
                    Vector3 origin = Position.ToVec3();
                    Matrix4x4 rotMatrix =
                        Matrix4x4.CreateTranslation(-origin) *
                        Matrix4x4.CreateRotationZ(Maths.DegreesToRadians(Rotation)) *
                        Matrix4x4.CreateTranslation(origin);
                    for (var i = 0; i < Points.Count; i++)
                    {
                        Points[i] = Vector2.Transform(Points[i], rotMatrix);
                    }

                    Rotation = 0;
                }
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
            if (pointString == null) return points;
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

        public TmxObject Clone()
        {
            return (TmxObject) MemberwiseClone();
        }
    }
}