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
        public Vector2[] Vertices;

        /// <summary>
        /// A clean polygon has at least 3 vertices, is non-degenerate, and has no without duplicate vertices.
        /// In addition the vertices are clockwise.
        /// </summary>
        public bool IsClean { get; private set; }

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
            Vertices = new Vector2[vCount];
        }

        public Polygon(params Vector2[] vertices)
        {
            Vertices = vertices;
        }

        public Polygon(IEnumerable<Vector2> vertices)
        {
            Vertices = vertices.ToArray();
        }

        public static Polygon FromRectangle(Rectangle rect)
        {
            var verts = new Vector2[4];
            verts[0] = rect.Position;
            verts[1] = rect.Position + new Vector2(rect.Width, 0);
            verts[2] = rect.Position + rect.Size;
            verts[3] = rect.Position + new Vector2(0, rect.Height);

            return new Polygon(verts) {IsClean = true};
        }

        #endregion

        /// <summary>
        /// Removes duplicate vertices one after another and the looping vertex.
        /// </summary>
        /// <param name="minDist">The minimum distance between points before they are considered duplicate.</param>
        public void CleanupPolygon(float minDist = 0.0f)
        {
            Debug.Assert(Vertices.Length >= 3);
            if (IsClean) return;

            float sqrtDist = minDist * minDist;
            var ps = new Vector2[Vertices.Length];
            var uniqueCount = 0;
            for (var i = 0; i < Vertices.Length; ++i)
            {
                Vector2 v = Vertices[i];

                var unique = true;
                for (var j = 0; j < uniqueCount; ++j)
                {
                    Vector2 temp = ps[j];
                    if (Vector2.DistanceSquared(v, temp) >= sqrtDist) continue;

                    unique = false;
                    break;
                }

                if (unique)
                {
                    ps[uniqueCount] = v;
                    uniqueCount++;
                }
            }

            // Polygon is degenerate.
            if (uniqueCount < 3) return;

            // Create the convex hull using the Gift wrapping algorithm
            // http://en.wikipedia.org/wiki/Gift_wrapping_algorithm

            // Find the right most point on the hull
            var i0 = 0;
            float x0 = ps[0].X;
            for (var i = 1; i < uniqueCount; ++i)
            {
                float x = ps[i].X;
                if (x > x0 || x == x0 && ps[i].Y < ps[i0].Y)
                {
                    i0 = i;
                    x0 = x;
                }
            }

            var hull = new List<int>();
            int ih = i0;
            while (true)
            {
                hull.Add(ih);

                var ie = 0;
                for (var j = 1; j < uniqueCount; ++j)
                {
                    if (ie == ih)
                    {
                        ie = j;
                        continue;
                    }

                    Vector2 r = ps[ie] - ps[ih];
                    Vector2 v = ps[j] - ps[ih];
                    float c = Maths.Cross2D(r, v);
                    if (c < 0.0f) ie = j;

                    // Check if co-linear
                    if (c == 0.0f && v.LengthSquared() > r.LengthSquared()) ie = j;
                }

                ih = ie;
                if (ie == i0) break;
            }

            // Polygon is degenerate.
            if (hull.Count < 3) return;

            // Copy vertices.
            Array.Resize(ref Vertices, hull.Count);
            for (var i = 0; i < hull.Count; ++i)
            {
                Vertices[i] = ps[hull[i]];
            }

            IsClean = true;
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
                Vector2 vert = Vertices[i];
                Vector2 vertJ = Vertices[j];
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
        /// Get the normal vectors of the polygon's vertices.
        /// </summary>
        public Vector2[] GetNormals()
        {
            int verticesCount = Vertices.Length;
            var normals = new Vector2[Vertices.Length];
            for (var i = 0; i < verticesCount; ++i)
            {
                int next = i + 1 < verticesCount ? i + 1 : 0;
                Vector2 edge = Vertices[next] - Vertices[i];
                Debug.Assert(edge.LengthSquared() > Maths.EPSILON * Maths.EPSILON);
                Vector2 temp = Maths.Cross2D(edge, 1.0f);
                normals[i] = Vector2.Normalize(temp);
            }

            return normals;
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
                memory.Add(new LineSegment(Vertices[i], Vertices[i + 1]));
            }

            memory.Add(new LineSegment(Vertices[0], Vertices[^1]));
        }

        /// <summary>
        /// Transform all vertices of the polygon.
        /// </summary>
        /// <param name="mat">The matrix to transform with.</param>
        public Polygon Transform(Matrix4x4 mat)
        {
            for (var i = 0; i < Vertices.Length; i++)
            {
                Vertices[i] = Vector2.Transform(Vertices[i], mat);
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
            static bool InsideTriangle(Vector2 pointA, Vector2 pointB, Vector2 pointC, Vector2 pointCheck)
            {
                Vector2 a = pointC - pointB;
                Vector2 b = pointA - pointC;
                Vector2 c = pointB - pointA;

                Vector2 ap = pointCheck - pointA;
                Vector2 bp = pointCheck - pointB;
                Vector2 cp = pointCheck - pointC;

                float axbp = Maths.Cross2D(a, bp);
                float cxap = Maths.Cross2D(c, ap);
                float bxcp = Maths.Cross2D(b, cp);

                return axbp >= 0.0f && cxap >= 0.0f && bxcp >= 0.0f;
            }

            if (Vertices.Length < 3) return null;
            CleanupPolygon();

            // Remove nv-2 vertices, creating 1 triangle every time.
            var result = new List<Vector2>();
            var vertices = new List<Vector2>(Vertices);
            int count = 2 * vertices.Count;

            bool Snip(int u, int v, int w)
            {
                Vector2 a = vertices[u];
                Vector2 b = vertices[v];
                Vector2 c = vertices[w];

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