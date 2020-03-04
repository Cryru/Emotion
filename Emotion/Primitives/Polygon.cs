#region Using

using Emotion.Utility;
using System.Numerics;

#endregion

namespace Emotion.Primitives
{
    public class Polygon
    {
        public Vector3[] Vertices;

        public Polygon(int vCount)
        {
            Vertices = new Vector3[vCount];
        }

        public Polygon(params Vector3[] vertices)
        {
            Vertices = vertices;
        }

        /// <summary>
        /// Whether the polygon contains a point.
        /// </summary>
        /// <param name="point">The point to check.</param>
        /// <returns>Whether the point is inside the polygon.</returns>
        public bool Contains(Vector2 point)
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
            var clone = (Polygon) poly.MemberwiseClone();
            clone.Transform(mat);
            return clone;
        }

        #region Constructors

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
    }
}