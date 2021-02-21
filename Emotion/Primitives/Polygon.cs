#region Using

using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Emotion.Graphics;
using Emotion.Utility;

#endregion

namespace Emotion.Primitives
{
    public class Polygon : IShape
    {
        public Vector3[] Vertices;

        /// <summary>
        /// The rectangle bounding this polygon.
        /// </summary>
        public Rectangle Bounds2D
        {
            get => BoundingRectangleOfPolygon(Vertices);
        }

        #region Constructors

        public Polygon(int vCount)
        {
            Vertices = new Vector3[vCount];
        }

        public Polygon(params Vector3[] vertices)
        {
            Vertices = vertices;
        }

        public Polygon(List<Vector3> vertices)
        {
            Vertices = vertices.ToArray();
        }

        public Polygon(IReadOnlyList<Vector2> vertices)
        {
            Vertices = new Vector3[vertices.Count];
            for (var i = 0; i < vertices.Count; i++)
            {
                Vertices[i] = vertices[i].ToVec3();
            }
        }

        public static Polygon FromRectangle(Rectangle rect)
        {
            var verts = new Vector3[4];
            verts[0] = rect.Position.ToVec3();
            verts[1] = (rect.Position + new Vector2(rect.Width, 0)).ToVec3();
            verts[2] = (rect.Position + rect.Size).ToVec3();
            verts[3] = (rect.Position + new Vector2(0, rect.Height)).ToVec3();

            return new Polygon(verts);
        }

        #endregion

        /// <summary>
        /// Ensure this polygon's vertices is counter clockwise.
        /// </summary>
        public void EnsureCounterClockwise()
        {
            if (0.0f > Area()) Array.Reverse(Vertices);
        }

        /// <summary>
        /// Removes duplicate vertices one after another and the looping vertex.
        /// </summary>
        public void CleanupPolygon()
        {
            var newVertices = new List<Vector3>();

            // Remove duplicates.
            ref Vector3 previous = ref Vertices[0];
            newVertices.Add(previous);
            for (var i = 1; i < Vertices.Length; i++)
            {
                ref Vector3 current = ref Vertices[i];
                if (current != previous) newVertices.Add(current);
                previous = ref current;
            }

            // Check if looping back.
            if (Vertices[^1] == Vertices[0]) newVertices.RemoveAt(newVertices.Count - 1);

            Vertices = newVertices.ToArray();
        }

        /// <summary>
        /// Whether the polygon contains a point.
        /// </summary>
        /// <param name="point">The point to check.</param>
        /// <returns>Whether the point is inside the polygon.</returns>
        public bool Contains(ref Vector2 point)
        {
            var result = false;
            int j = Vertices.Length - 1;
            for (var i = 0; i < Vertices.Length; i++)
            {
                Vector3 vert = Vertices[i];
                Vector3 vertJ = Vertices[j];
                if (vert.Y < point.Y && vertJ.Y >= point.Y || vertJ.Y < point.Y && vert.Y >= point.Y)
                    if (vert.X + (point.Y - vert.Y) / (vertJ.Y - vert.Y) * (vertJ.X - vert.X) < point.X)
                        result = !result;
                j = i;
            }

            return result;
        }

        /// <summary>
        /// Returns the area of the polygon.
        /// </summary>
        /// <returns>The area of the polygon.</returns>
        public float Area()
        {
            var a = 0.0f;
            for (int p = Vertices.Length - 1, q = 0; q < Vertices.Length; p = q++)
            {
                a += Vertices[p].X * Vertices[q].Y - Vertices[q].X * Vertices[p].Y;
            }

            return a * 0.5f;
        }

        /// <summary>
        /// Get the 2d line segments between the points on the polygon.
        /// </summary>
        /// <returns></returns>
        public List<LineSegment> Get2DContour()
        {
            var newList = new List<LineSegment>();
            Get2DContour(ref newList);
            return newList;
        }

        /// <summary>
        /// Get the 2d line segments between the points on the polygon.
        /// </summary>
        /// <param name="memory">Memory to re-use.</param>
        /// <returns></returns>
        public void Get2DContour(ref List<LineSegment> memory)
        {
            memory.Clear();
            for (var i = 0; i < Vertices.Length - 1; i++)
            {
                memory.Add(new LineSegment(Vertices[i].ToVec2(), Vertices[i + 1].ToVec2()));
            }

            memory.Add(new LineSegment(Vertices[0].ToVec2(), Vertices[^1].ToVec2()));
        }

        /// <summary>
        /// Transform all vertices of the polygon.
        /// </summary>
        /// <param name="mat">The matrix to transform with.</param>
        public Polygon Transform(Matrix4x4 mat)
        {
            for (var i = 0; i < Vertices.Length; i++)
            {
                Vertices[i] = Vector3.Transform(Vertices[i], mat);
            }

            return this;
        }

        /// <summary>
        /// Multiply a polygon with a matrix.
        /// </summary>
        /// <param name="poly">The polygon to multiply.</param>
        /// <param name="mat">The matrix to multiply with.</param>
        /// <returns>The polygon multiplied.</returns>
        public static Polygon Transform(Polygon poly, Matrix4x4 mat)
        {
            var clone = new Polygon(poly.Vertices.ToArray());
            clone.Transform(mat);
            return clone;
        }

        /// <summary>
        /// Turn the polygon's vertices into triangles.
        /// </summary>
        /// <returns>This polygon triangulated.</returns>
        public Polygon Triangulate()
        {
            // Check if the triangle described by points A, B, C contains pointCheck.
            static bool InsideTriangle(Vector3 pointA, Vector3 pointB, Vector3 pointC, Vector3 pointCheck)
            {
                Vector3 a = pointC - pointB;
                Vector3 b = pointA - pointC;
                Vector3 c = pointB - pointA;

                Vector3 ap = pointCheck - pointA;
                Vector3 bp = pointCheck - pointB;
                Vector3 cp = pointCheck - pointC;

                Vector3 axbp = Vector3.Cross(a, bp);
                Vector3 cxap = Vector3.Cross(c, ap);
                Vector3 bxcp = Vector3.Cross(b, cp);

                return axbp.Z >= 0.0f && cxap.Z >= 0.0f && bxcp.Z >= 0.0f;
            }

            if (Vertices.Length < 3) return null;
            EnsureCounterClockwise();
            CleanupPolygon();

            // Remove nv-2 vertices, creating 1 triangle every time.
            var result = new List<Vector3>();
            var vertices = new List<Vector3>(Vertices);
            int count = 2 * vertices.Count;

            bool Snip(int u, int v, int w)
            {
                Vector3 a = vertices[u];
                Vector3 b = vertices[v];
                Vector3 c = vertices[w];

                if (Maths.EPSILON > (b.X - a.X) * (c.Y - a.Y) - (b.Y - a.Y) * (c.X - a.X)) return false;
                // ReSharper disable once LoopCanBeConvertedToQuery
                for (var p = 0; p < vertices.Count; p++)
                {
                    if (p == u || p == v || p == w) continue;
                    if (InsideTriangle(a, b, c, vertices[p])) return false;
                }

                return true;
            }

            for (var v = 0; vertices.Count > 2;)
            {
                // If we loop, it is probably a non-simple polygon.
                if (0 >= count--) return null;

                // Three consecutive vertices in current polygon, <u,v,w>
                // This is the current vertex and the two next ones.

                int u = v;
                if (vertices.Count <= u) u = 0;
                v = u + 1;
                if (vertices.Count <= v) v = 0;
                int w = v + 1;
                if (vertices.Count <= w) w = 0;

                if (!Snip(u, v, w)) continue;

                // The output triangle.
                result.Add(vertices[u]);
                result.Add(vertices[v]);
                result.Add(vertices[w]);

                // Remove v from the polygon.
                vertices.RemoveAt(v);

                // Reset error detection counter.
                count = 2 * vertices.Count;
            }

            return new Polygon(result);
        }

        /// <summary>
        /// Renders the polygon unwound.
        /// </summary>
        /// <param name="composer">The composer to render with.</param>
        /// <param name="color">The color of the polygon.</param>
        public void Render(RenderComposer composer, Color color)
        {
            composer.SetStencilTest(true);
            composer.StencilWindingStart();
            composer.ToggleRenderColor(false);

            composer.RenderVertices(Vertices, Color.White);

            composer.StencilWindingEnd();
            composer.ToggleRenderColor(true);

            composer.RenderSprite(Bounds2D, color);

            composer.SetStencilTest(false);
        }

        /// <summary>
        /// Find the bounding rectangle of a polygon.
        /// </summary>
        /// <param name="vertices">The vertices which make up the polygon.</param>
        /// <returns>The bounding rectangle of the polygon.</returns>
        public static Rectangle BoundingRectangleOfPolygon(params Vector3[] vertices)
        {
            var minX = float.MaxValue;
            var maxX = float.MinValue;
            var minY = float.MaxValue;
            var maxY = float.MinValue;

            for (var i = 0; i < vertices.Length; i++)
            {
                float x = vertices[i].X;
                float y = vertices[i].Y;
                minX = Math.Min(minX, x);
                maxX = Math.Max(maxX, x);
                minY = Math.Min(minY, y);
                maxY = Math.Max(maxY, y);
            }

            float width = maxX - minX;
            float height = maxY - minY;

            return new Rectangle(minX, minY, width, height);
        }

        /// <summary>
        /// Find the bounding rectangle of a polygon.
        /// </summary>
        /// <param name="vertices">The vertices which make up the polygon.</param>
        /// <returns>The bounding rectangle of the polygon.</returns>
        public static Rectangle BoundingRectangleOfPolygon(params Vector2[] vertices)
        {
            var minX = float.MaxValue;
            var maxX = float.MinValue;
            var minY = float.MaxValue;
            var maxY = float.MinValue;

            for (var i = 0; i < vertices.Length; i++)
            {
                float x = vertices[i].X;
                float y = vertices[i].Y;
                minX = Math.Min(minX, x);
                maxX = Math.Max(maxX, x);
                minY = Math.Min(minY, y);
                maxY = Math.Max(maxY, y);
            }

            float width = maxX - minX;
            float height = maxY - minY;

            return new Rectangle(minX, minY, width, height);
        }

        /// <summary>
        /// Find the bounding rectangle of a line segment contour.
        /// </summary>
        /// <param name="lines">The lines which make up the contour.</param>
        /// <returns>The bounding rectangle of the contour.</returns>
        public static Rectangle BoundingRectangleOfContour(params LineSegment[] lines)
        {
            var minX = float.MaxValue;
            var maxX = float.MinValue;
            var minY = float.MaxValue;
            var maxY = float.MinValue;

            float x = lines[0].Start.X;
            float y = lines[0].Start.Y;
            minX = Math.Min(minX, x);
            maxX = Math.Max(maxX, x);
            minY = Math.Min(minY, y);
            maxY = Math.Max(maxY, y);

            for (var i = 0; i < lines.Length; i++)
            {
                x = lines[i].End.X;
                y = lines[i].End.Y;
                minX = Math.Min(minX, x);
                maxX = Math.Max(maxX, x);
                minY = Math.Min(minY, y);
                maxY = Math.Max(maxY, y);
            }

            float width = maxX - minX;
            float height = maxY - minY;

            return new Rectangle(minX, minY, width, height);
        }

        // -----

        public bool Contains(Vector2 point)
        {
            return Contains(ref point);
        }

        #region Shape API

        public Vector2 Center
        {
            get => throw new NotImplementedException();
            set => throw new NotImplementedException();
        }

        public IShape CloneShape()
        {
            return new Polygon(Vertices);
        }

        public bool Intersects(ref LineSegment line)
        {
            throw new NotImplementedException();
        }

        public Vector2 GetIntersectionPoint(ref LineSegment line)
        {
            throw new NotImplementedException();
        }

        public bool ContainsInclusive(ref Vector2 point)
        {
            return Contains(ref point);
        }

        public bool Contains(ref Rectangle rect)
        {
            Rectangle rectBound = Bounds2D;
            return rectBound.Contains(rect);
        }

        public bool ContainsInclusive(ref Rectangle rect)
        {
            Rectangle rectBound = Bounds2D;
            return rectBound.ContainsInclusive(rect);
        }

        public bool Intersects(ref Rectangle rect)
        {
            Rectangle rectBound = Bounds2D;
            if (!rectBound.Intersects(rect)) return false;

            if (Contains(rect.TopLeft) ||
                Contains(rect.TopRight) ||
                Contains(rect.BottomLeft) ||
                Contains(rect.BottomRight)) return true;

            List<LineSegment> contour = Get2DContour();
            LineSegment[] rectContour = rect.GetLineSegments();

            for (var i = 0; i < rectContour.Length; i++)
            {
                ref LineSegment rectSegment = ref rectContour[i];
                for (var j = 0; j < contour.Count; j++)
                {
                    LineSegment polySegment = contour[j];
                    if (polySegment.Intersects(ref rectSegment)) return true;
                }
            }

            return false;
        }

        #endregion
    }
}