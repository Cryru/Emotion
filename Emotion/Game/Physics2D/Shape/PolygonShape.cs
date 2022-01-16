#region Using

using System.Diagnostics;
using System.Numerics;
using Emotion.Game.Physics2D.Actors;
using Emotion.Game.Physics2D.Collision;
using Emotion.Primitives;
using Emotion.Utility;

#endregion

namespace Emotion.Game.Physics2D.Shape
{
    public class PolygonShape : ShapeBase
    {
        public static float PolygonRadius = 2.0f * PhysicsConfig.LinearSlop;

        public Polygon Polygon { get; protected set; }
        public Vector2[] Normals;

        public PolygonShape(Polygon polygon, float density) : base(density)
        {
            Radius = PolygonRadius;
            Polygon = polygon;
            Polygon.CleanupPolygon(0.5f * PhysicsConfig.LinearSlop);
            Normals = Polygon.GetNormals();
            ComputeMassData();
        }

        public override Rectangle GetBounds(PhysicsTransform transform)
        {
            // Translate vertices with body data.
            var temp = new Vector2[Polygon.Vertices.Length];
            Polygon.Vertices.CopyTo(temp, 0);
            for (var i = 0; i < temp.Length; i++)
            {
                temp[i] = transform.TransformVector(temp[i]);
            }

            // Get bound and add a bit to it.
            Rectangle bounds = Polygon.BoundingRectangleOfPolygon(temp);
            var margins = new Vector2(Radius);
            bounds.Position -= margins;
            bounds.Size += margins;
            return bounds;
        }

        protected sealed override void ComputeMassData()
        {
            // Polygon mass, centroid, and inertia.
            // Let rho be the polygon density in mass per unit area.
            // Then:
            // mass = rho * int(dA)
            // centroid.x = (1/mass) * rho * int(x * dA)
            // centroid.y = (1/mass) * rho * int(y * dA)
            // I = rho * int((x*x + y*y) * dA)
            //
            // We can compute these integrals by summing all the integrals
            // for each triangle of the polygon. To evaluate the integral
            // for a single triangle, we make a change of variables to
            // the (u,v) coordinates of the triangle:
            // x = x0 + e1x * u + e2x * v
            // y = y0 + e1y * u + e2y * v
            // where 0 <= u && 0 <= v && u + v <= 1.
            //
            // We integrate u from [0,1-v] and then v from [0,1].
            // We also need to use the Jacobian of the transformation:
            // D = cross(e1, e2)
            //
            // Simplification: triangle centroid = (1/3) * (p1 + p2 + p3)
            //
            // The rest of the derivation is handled by computer algebra.

            // Early exit as polygons with 0 density does not have any properties.
            if (Density <= 0 || !Polygon.IsClean)
                return;

            //Velcro: Consolidated the calculate centroid and mass code to a single method.
            Vector2 center = Vector2.Zero;
            var area = 0.0f;
            var I = 0.0f;

            // Get a reference point for forming triangles.
            // Use the first vertex to reduce round-off errors.
            const float inv3 = 1.0f / 3.0f;
            Vector2[] vertices = Polygon.Vertices;
            int count = vertices.Length;
            Vector2 s = vertices[0];
            for (var i = 0; i < count; ++i)
            {
                // Triangle vertices.
                Vector2 e1 = vertices[i] - s;
                Vector2 e2 = i + 1 < count ? vertices[i + 1] - s : vertices[0] - s;

                float d = Maths.Cross2D(e1, e2);

                float triangleArea = 0.5f * d;
                area += triangleArea;

                // Area weighted centroid
                center += triangleArea * inv3 * (e1 + e2);

                float ex1 = e1.X, ey1 = e1.Y;
                float ex2 = e2.X, ey2 = e2.Y;

                float intx2 = ex1 * ex1 + ex2 * ex1 + ex2 * ex2;
                float inty2 = ey1 * ey1 + ey2 * ey1 + ey2 * ey2;

                I += 0.25f * inv3 * d * (intx2 + inty2);
            }

            //The area is too small for the engine to handle.
            Debug.Assert(area > Maths.EPSILON);

            // We save the area
            MassData.Area = area;

            // Total mass
            MassData.Mass = Density * area;

            // Center of mass
            center *= 1.0f / area;
            MassData.Centroid = center + s;

            // Inertia tensor relative to the local origin (point s).
            MassData.Inertia = Density * I;

            // Shift to center of mass then to original body origin.
            MassData.Inertia += MassData.Mass * (Vector2.Dot(MassData.Centroid, MassData.Centroid) - Vector2.Dot(center, center));
        }

        public override ContactType GetContactType(ShapeBase b)
        {
            if (b is PolygonShape) return ContactType.Polygon;
            return ContactType.NotSupported;
        }
    }
}