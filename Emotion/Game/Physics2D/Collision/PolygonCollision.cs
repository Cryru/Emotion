#region Using

using System.Diagnostics;
using System.Numerics;
using Emotion.Game.Physics2D.Actors;
using Emotion.Game.Physics2D.Shape;
using Emotion.Primitives;
using Emotion.Utility;

#endregion

namespace Emotion.Game.Physics2D.Collision
{
    /// <summary>
    /// Code from Velcro Physics
    /// </summary>
    public static class PolygonCollision
    {
        /// <summary>
        /// Compute the collision manifold between two polygons.
        /// </summary>
        public static void CollidePolygons(ref Manifold manifold, PolygonShape polyA, PhysicsTransform xfA, PolygonShape polyB, PhysicsTransform xfB)
        {
            // Find edge normal of max separation on A - return if separating axis is found
            // Find edge normal of max separation on B - return if separation axis is found
            // Choose reference edge as min(minA, minB)
            // Find incident edge
            // Clip

            manifold.PointCount = 0;
            float totalRadius = polyA.Radius + polyB.Radius;

            float separationA = FindMaxSeparation(out int edgeA, polyA, xfA, polyB, xfB);
            if (separationA > totalRadius)
                return;

            float separationB = FindMaxSeparation(out int edgeB, polyB, xfB, polyA, xfA);
            if (separationB > totalRadius)
                return;

            // reference, incident
            PolygonShape poly1, poly2;
            PhysicsTransform xf1, xf2;
            int edge1; // reference edge
            bool flip;

            if (separationB > separationA + 0.1f * PhysicsConfig.LinearSlop)
            {
                poly1 = polyB;
                poly2 = polyA;
                xf1 = xfB;
                xf2 = xfA;
                edge1 = edgeB;
                manifold.Type = ManifoldType.FaceB;
                flip = true;
            }
            else
            {
                poly1 = polyA;
                poly2 = polyB;
                xf1 = xfA;
                xf2 = xfB;
                edge1 = edgeA;
                manifold.Type = ManifoldType.FaceA;
                flip = false;
            }

            FixedArray2<ClipVertex> incidentEdge = FindIncidentEdge(poly1, xf1, edge1, poly2, xf2);

            Vector2[] vertices1 = poly1.Polygon.Vertices;
            int count1 = vertices1.Length;

            int iv1 = edge1;
            int iv2 = edge1 + 1 < count1 ? edge1 + 1 : 0;

            Vector2 v11 = vertices1[iv1];
            Vector2 v12 = vertices1[iv2];

            Vector2 localTangent = v12 - v11;
            localTangent.Normalize();

            Vector2 localNormal = Maths.Cross2D(localTangent, 1.0f);
            Vector2 planePoint = 0.5f * (v11 + v12);

            Vector2 tangent = xf1.RotateVector(localTangent);
            Vector2 normal = Maths.Cross2D(tangent, 1.0f);

            v11 = xf1.TransformVector(v11);
            v12 = xf1.TransformVector(v12);

            // Face offset.
            float frontOffset = Vector2.Dot(normal, v11);

            // Side offsets, extended by polytope skin thickness.
            float sideOffset1 = -Vector2.Dot(tangent, v11) + totalRadius;
            float sideOffset2 = Vector2.Dot(tangent, v12) + totalRadius;

            // Clip incident edge against extruded edge1 side edges.

            // Clip to box side 1
            FixedArray2<ClipVertex> clipPoints1 = ClipSegmentToLine(ref incidentEdge, -tangent, sideOffset1, iv1, out int pointCount);

            if (pointCount < 2)
                return;

            // Clip to negative box side 1
            FixedArray2<ClipVertex> clipPoints2 = ClipSegmentToLine(ref clipPoints1, tangent, sideOffset2, iv2, out pointCount);

            if (pointCount < 2)
                return;

            // Now clipPoints2 contains the clipped points.
            manifold.LocalNormal = localNormal;
            manifold.LocalPoint = planePoint;

            var manifoldPoints = 0;
            for (var i = 0; i < manifold.Points.Length; i++)
            {
                float separation = Vector2.Dot(normal, clipPoints2[i].V) - frontOffset;
                if (separation > totalRadius) continue;

                ManifoldPoint cp = manifold.Points[manifoldPoints];
                cp.LocalPoint = xf2.TransformTransposeVector(clipPoints2[i].V);
                cp.Id = clipPoints2[i].Id;

                if (flip)
                {
                    // Swap features
                    ContactFeature cf = cp.Id.ContactFeature;
                    cp.Id.ContactFeature.IndexA = cf.IndexB;
                    cp.Id.ContactFeature.IndexB = cf.IndexA;
                    cp.Id.ContactFeature.TypeA = cf.TypeB;
                    cp.Id.ContactFeature.TypeB = cf.TypeA;
                }

                manifold.Points[manifoldPoints] = cp;
                manifoldPoints++;
            }

            manifold.PointCount = manifoldPoints;
        }

        /// <summary>
        /// Find the max separation between poly1 and poly2 using edge normals from poly1.
        /// </summary>
        private static float FindMaxSeparation(out int edgeIndex, PolygonShape poly1, PhysicsTransform body1, PolygonShape poly2, PhysicsTransform body2)
        {
            Vector2[] verts1 = poly1.Polygon.Vertices;
            Vector2[] verts2 = poly2.Polygon.Vertices;
            int count1 = verts1.Length;
            int count2 = verts2.Length;

            Vector2[] normal1 = poly1.Normals;

            PhysicsTransform transform = body2.TransposeTransform(body1);

            var bestIndex = 0;
            float maxSeparation = -float.MaxValue;
            for (var i = 0; i < count1; ++i)
            {
                // Get poly1 normal in frame2.
                Vector2 n = transform.RotateVector(normal1[i]);
                Vector2 v1 = transform.TransformVector(verts1[i]);

                // Find deepest point for normal i.
                var si = float.MaxValue;
                for (var j = 0; j < count2; ++j)
                {
                    float sij = Vector2.Dot(n, verts2[j] - v1);
                    if (sij < si)
                        si = sij;
                }

                if (si > maxSeparation)
                {
                    maxSeparation = si;
                    bestIndex = i;
                }
            }

            edgeIndex = bestIndex;
            return maxSeparation;
        }

        private static FixedArray2<ClipVertex> FindIncidentEdge(PolygonShape poly1, PhysicsTransform body1, int edge1, PolygonShape poly2, PhysicsTransform body2)
        {
            Vector2[] normals1 = poly1.Normals;
            Vector2[] normals2 = poly2.Normals;

            Vector2[] vertices2 = poly2.Polygon.Vertices;
            int count2 = vertices2.Length;

            Debug.Assert(0 <= edge1 && edge1 < poly1.Polygon.Vertices.Length);

            // Get the normal of the reference edge in poly2's frame.
            Vector2 normal1 = body2.RotateTransposeVector(body1.RotateVector(normals1[edge1]));

            // Find the incident edge on poly2.
            var index = 0;
            var minDot = float.MaxValue;
            for (var i = 0; i < count2; ++i)
            {
                float dot = Vector2.Dot(normal1, normals2[i]);
                if (dot < minDot)
                {
                    minDot = dot;
                    index = i;
                }
            }

            // Build the clip vertices for the incident edge.
            int i1 = index;
            int i2 = i1 + 1 < count2 ? i1 + 1 : 0;

            var c = new FixedArray2<ClipVertex>();
            c.Value0.V = body2.TransformVector(vertices2[i1]);
            c.Value0.Id.ContactFeature.IndexA = (byte) edge1;
            c.Value0.Id.ContactFeature.IndexB = (byte) i1;
            c.Value0.Id.ContactFeature.TypeA = ContactFeatureType.Face;
            c.Value0.Id.ContactFeature.TypeB = ContactFeatureType.Vertex;

            c.Value1.V = body2.TransformVector(vertices2[i2]);
            c.Value1.Id.ContactFeature.IndexA = (byte) edge1;
            c.Value1.Id.ContactFeature.IndexB = (byte) i2;
            c.Value1.Id.ContactFeature.TypeA = ContactFeatureType.Face;
            c.Value1.Id.ContactFeature.TypeB = ContactFeatureType.Vertex;

            return c;
        }

        /// <summary>
        /// Clipping for contact manifolds.
        /// </summary>
        private static FixedArray2<ClipVertex> ClipSegmentToLine(ref FixedArray2<ClipVertex> vIn, Vector2 normal, float offset, int vertexIndexA, out int count)
        {
            var verts = new FixedArray2<ClipVertex>();
            count = 0;

            // Calculate the distance of end points to the line
            float distance0 = Vector2.Dot(normal, vIn.Value0.V) - offset;
            float distance1 = Vector2.Dot(normal, vIn.Value1.V) - offset;

            // If the points are behind the plane
            if (distance0 <= 0.0f) verts[count++] = vIn.Value0;
            if (distance1 <= 0.0f) verts[count++] = vIn.Value1;

            // If the points are on different sides of the plane
            if (distance0 * distance1 < 0.0f)
            {
                // Find intersection point of edge and plane
                float intersection = distance0 / (distance0 - distance1);

                ClipVertex cv = verts[count];
                cv.V = vIn.Value0.V + intersection * (vIn.Value1.V - vIn.Value0.V);

                // VertexA is hitting edgeB.
                cv.Id.ContactFeature.IndexA = (byte) vertexIndexA;
                cv.Id.ContactFeature.IndexB = vIn.Value0.Id.ContactFeature.IndexB;
                cv.Id.ContactFeature.TypeA = ContactFeatureType.Vertex;
                cv.Id.ContactFeature.TypeB = ContactFeatureType.Face;
                verts[count] = cv;

                count++;
            }

            return verts;
        }
    }
}