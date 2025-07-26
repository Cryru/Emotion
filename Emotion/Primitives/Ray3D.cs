#nullable enable

#region Using

using Emotion.Game.World3D;
using Emotion.Graphics.Data;
using Emotion.Graphics.ThreeDee;
using Emotion.WIPUpdates.One.Work;
using Emotion.WIPUpdates.Rendering;

#endregion

namespace Emotion.Primitives;

/// <summary>
/// A struct representing a ray.
/// </summary>
public struct Ray3D
{
    public Vector3 Start;
    public Vector3 Direction;

    /// <summary>
    /// Create a ray from a starting position, direction and length.
    /// </summary>
    /// <param name="start">The ray's start.</param>
    /// <param name="direction">The direction of the ray.</param>
    public Ray3D(Vector3 start, Vector3 direction)
    {
        Start = start;
        Direction = direction;
    }

    public Vector3 IntersectWithPlane(Vector3 planeNormal, Vector3 planePoint)
    {
        // Check if the ray is parallel to the plane
        float dot = Vector3.Dot(Direction, planeNormal);
        if (dot == 0) return Vector3.Zero;

        float distance = Vector3.Dot(planeNormal, planePoint - Start) / dot;
        return Start + distance * Direction;
    }

    /// <summary>
    /// Returns whether the ray intersects the specified GameObject3D.
    /// Animated objects will be intersected according to the state of their vertices at their last
    /// CacheVerticesForCollision call. Keep in mind that applying the animation is slow.
    /// </summary>
    public bool IntersectWithObject(GameObject3D obj, out Mesh? collidedMesh, out Vector3 collisionPoint, out Vector3 normal, out int triangleIndex, bool closest = false)
    {
        collidedMesh = null;
        collisionPoint = Vector3.Zero;
        normal = Vector3.Zero;
        triangleIndex = -1;

        Mesh[]? meshes = obj.Entity?.Meshes;
        if (meshes == null) return false;

        Sphere boundSphere = obj.BoundingSphere;
        if (!IntersectWithSphere(boundSphere, out Vector3 _, out Vector3 _)) return false;

        // Used when closest == true
        float closestDist = float.MaxValue;
        Vector3 closestNormal = Vector3.Zero;
        Vector3 closestCollisionPoint = Vector3.Zero;
        int closestTriangleIndex = 0;

        for (var i = 0; i < meshes.Length; i++)
        {
            Mesh mesh = meshes[i];
            if (IntersectWithObjectMesh(obj, i, out collisionPoint, out normal, out triangleIndex))
            {
                if (closest)
                {
                    float distance = Vector3.Distance(Start, collisionPoint);
                    if (closestDist == float.MaxValue || distance < closestDist)
                    {
                        closestDist = distance;
                        collidedMesh = mesh;

                        closestCollisionPoint = collisionPoint;
                        closestNormal = normal;
                        closestTriangleIndex = triangleIndex;
                    }
                }
                else
                {
                    collidedMesh = mesh;
                    return true;
                }
            }
        }

        if (closest && collidedMesh != null)
        {
            collisionPoint = closestCollisionPoint;
            normal = closestNormal;
            triangleIndex = closestTriangleIndex;

            return true;
        }

        return false;
    }

    public bool IntersectWithObject(MapObjectMesh obj, out Mesh? collidedMesh, out Vector3 collisionPoint, out Vector3 normal, out int triangleIndex, bool closest = false)
    {
        collidedMesh = null;
        collisionPoint = Vector3.Zero;
        normal = Vector3.Zero;
        triangleIndex = -1;

        Mesh[] meshes = obj.MeshEntity.Meshes;
        if (meshes == null) return false;

        Sphere boundSphere = obj.BoundingSphere;
        if (!IntersectWithSphere(boundSphere, out Vector3 _, out Vector3 _)) return false;

        // Used when closest == true
        float closestDist = float.MaxValue;
        Vector3 closestNormal = Vector3.Zero;
        Vector3 closestCollisionPoint = Vector3.Zero;
        int closestTriangleIndex = 0;

        for (var i = 0; i < meshes.Length; i++)
        {
            Mesh mesh = meshes[i];
            if (IntersectWithObjectMesh(obj, i, out collisionPoint, out normal, out triangleIndex))
            {
                if (closest)
                {
                    float distance = Vector3.Distance(Start, collisionPoint);
                    if (closestDist == float.MaxValue || distance < closestDist)
                    {
                        closestDist = distance;
                        collidedMesh = mesh;

                        closestCollisionPoint = collisionPoint;
                        closestNormal = normal;
                        closestTriangleIndex = triangleIndex;
                    }
                }
                else
                {
                    collidedMesh = mesh;
                    return true;
                }
            }
        }

        if (closest && collidedMesh != null)
        {
            collisionPoint = closestCollisionPoint;
            normal = closestNormal;
            triangleIndex = closestTriangleIndex;

            return true;
        }

        return false;
    }

    public bool IntersectWithVertices(ushort[] indices, VertexDataAllocation vertices, out Vector3 collisionPoint, out Vector3 normal, out int triangleIndex)
    {
        collisionPoint = Vector3.Zero;
        normal = Vector3.Zero;
        triangleIndex = -1;

        if (!vertices.Format.Built) return false;
        if (!vertices.Format.HasPosition) return false;

        var closestDistance = float.MaxValue;
        var intersectionFound = false;

        for (var i = 0; i < indices.Length; i += 3)
        {
            ushort idx1 = indices[i];
            ushort idx2 = indices[i + 1];
            ushort idx3 = indices[i + 2];

            Triangle triangle = vertices.GetTriangleAtIndices(idx1, idx2, idx3);
            Vector3 triangleNormal = triangle.Normal;

            if (!IntersectWithTriangle(triangle.A, triangle.B, triangle.C, triangleNormal, out float t)) continue;

            if (t < closestDistance)
            {
                closestDistance = t;
                normal = triangleNormal;
                triangleIndex = i;
                intersectionFound = true;
            }
        }

        if (intersectionFound) collisionPoint = Start + Direction * closestDistance;

        return intersectionFound;
    }

    private bool IntersectWithObjectMesh(MapObjectMesh obj, int meshIdx, out Vector3 collisionPoint, out Vector3 normal, out int triangleIndex)
    {
        collisionPoint = Vector3.Zero;
        normal = Vector3.Zero;
        triangleIndex = -1;

        Mesh[] meshes = obj.MeshEntity.Meshes;
        Mesh mesh = meshes[meshIdx];

        var closestDistance = float.MaxValue;
        var intersectionFound = false;

        ushort[] meshIndices = mesh.Indices;
        VertexData[] verts = mesh.Vertices;

        Matrix4x4 modelMatrixInverse = obj.GetModelMatrix().Inverted();

        // Tranform the ray to the inverse of the model matrix so we don't have to transform
        // each vertex of the mesh.
        Ray3D transformedRay = new Ray3D(
            Vector3.Transform(Start, modelMatrixInverse),
            Vector3.Transform(Direction, obj.GetModelMatrixRotation().Inverted())
        );

        for (var i = 0; i < meshIndices.Length; i += 3)
        {
            ushort idx1 = meshIndices[i];
            ushort idx2 = meshIndices[i + 1];
            ushort idx3 = meshIndices[i + 2];

            Vector3 v1 = verts[idx1].Vertex;
            Vector3 v2 = verts[idx2].Vertex;
            Vector3 v3 = verts[idx3].Vertex;

            Triangle tri = new Triangle(v1, v2, v3);
            if (!transformedRay.IntersectsTriangle(tri, out float t)) continue;

            if (t < closestDistance)
            {
                closestDistance = t;
                normal = tri.Normal;
                triangleIndex = i;
                intersectionFound = true;
            }
        }

        if (intersectionFound) collisionPoint = Start + Direction * closestDistance;

        return intersectionFound;
    }

    public bool IntersectWithObjectMesh(GameObject3D obj, int meshIdx, out Vector3 collisionPoint, out Vector3 normal, out int triangleIndex)
    {
        collisionPoint = Vector3.Zero;
        normal = Vector3.Zero;
        triangleIndex = -1;

        Mesh[]? meshes = obj.Entity?.Meshes;
        if (meshes == null) return false;
        Mesh mesh = meshes[meshIdx];

        var closestDistance = float.MaxValue;
        var intersectionFound = false;

        ushort[] meshIndices = mesh.Indices;

        Matrix4x4 matrix = obj.GetModelMatrix();
        for (var i = 0; i < meshIndices.Length; i += 3)
        {
            ushort idx1 = meshIndices[i];
            ushort idx2 = meshIndices[i + 1];
            ushort idx3 = meshIndices[i + 2];

            obj.GetMeshTriangleForCollision(meshIdx, idx1, idx2, idx3, out Vector3 p1, out Vector3 p2, out Vector3 p3);

            p1 = Vector3.Transform(p1, matrix);
            p2 = Vector3.Transform(p2, matrix);
            p3 = Vector3.Transform(p3, matrix);

            Vector3 triangleNormal = Vector3.Normalize(Vector3.Cross(p2 - p1, p3 - p1));

            if (!IntersectWithTriangle(p1, p2, p3, triangleNormal, out float t)) continue;

            if (t < closestDistance)
            {
                closestDistance = t;
                normal = triangleNormal;
                triangleIndex = i;
                intersectionFound = true;
            }
        }

        if (intersectionFound) collisionPoint = Start + Direction * closestDistance;

        return intersectionFound;
    }

    public bool IntersectsTriangle(Triangle tri, out float distance)
    {
        // Check if the ray is parallel to the triangle
        Vector3 normal = tri.Normal;
        float normalRayDot = Vector3.Dot(normal, Direction);
        if (Math.Abs(normalRayDot) < float.Epsilon)
        {
            distance = 0;
            return false;
        }

        // Calculate the intersection point
        Vector3 rayToTriangle = tri.A - Start;
        distance = Vector3.Dot(rayToTriangle, normal) / normalRayDot;

        // The intersection point is behind the ray's origin
        if (distance < 0)
            return false;

        // Calculate the barycentric coordinates of the intersection point
        Vector3 edge1 = tri.B - tri.A;
        Vector3 edge2 = tri.C - tri.A;

        // Calculate the area of the triangle.
        // The magnitude of the cross product is equal to twice the area of the triangle
        float triangleArea = 0.5f * Vector3.Cross(edge1, edge2).Length();

        // Degenerate triangle, area is too small.
        if (triangleArea < 0.001f) return false;

        Vector3 intersectionPoint = Start + distance * Direction;
        Vector3 c = intersectionPoint - tri.A;
        float d00 = Vector3.Dot(edge1, edge1);
        float d01 = Vector3.Dot(edge1, edge2);
        float d11 = Vector3.Dot(edge2, edge2);
        float denom = d00 * d11 - d01 * d01;

        float u = (d11 * Vector3.Dot(edge1, c) - d01 * Vector3.Dot(edge2, c)) / denom;
        float v = (d00 * Vector3.Dot(edge2, c) - d01 * Vector3.Dot(edge1, c)) / denom;

        // Check if the intersection point is inside the triangle
        return u >= 0 && v >= 0 && u + v <= 1;
    }

    public bool IntersectWithTriangle(Vector3 p1, Vector3 p2, Vector3 p3, Vector3 normal, out float distance)
    {
        // Check if the ray is parallel to the triangle
        float normalRayDot = Vector3.Dot(normal, Direction);
        if (Math.Abs(normalRayDot) < float.Epsilon)
        {
            distance = 0;
            return false;
        }

        // Calculate the intersection point
        Vector3 rayToTriangle = p1 - Start;
        distance = Vector3.Dot(rayToTriangle, normal) / normalRayDot;

        // The intersection point is behind the ray's origin
        if (distance < 0)
            return false;

        // Calculate the barycentric coordinates of the intersection point
        Vector3 edge1 = p2 - p1;
        Vector3 edge2 = p3 - p1;

        // Calculate the area of the triangle.
        // The magnitude of the cross product is equal to twice the area of the triangle
        float triangleArea = 0.5f * Vector3.Cross(edge1, edge2).Length();

        // Degenerate triangle, area is too small.
        if (triangleArea < 0.001f) return false;

        Vector3 intersectionPoint = Start + distance * Direction;
        Vector3 c = intersectionPoint - p1;
        float d00 = Vector3.Dot(edge1, edge1);
        float d01 = Vector3.Dot(edge1, edge2);
        float d11 = Vector3.Dot(edge2, edge2);
        float denom = d00 * d11 - d01 * d01;

        float u = (d11 * Vector3.Dot(edge1, c) - d01 * Vector3.Dot(edge2, c)) / denom;
        float v = (d00 * Vector3.Dot(edge2, c) - d01 * Vector3.Dot(edge1, c)) / denom;

        // Check if the intersection point is inside the triangle
        return u >= 0 && v >= 0 && u + v <= 1;
    }

    public bool IntersectWithSphere(Sphere sphere, out Vector3 intersectionPoint1, out Vector3 intersectionPoint2)
    {
        intersectionPoint1 = Vector3.Zero;
        intersectionPoint2 = Vector3.Zero;

        Vector3 sphereToRay = Start - sphere.Origin;

        // Calculate the coefficients for the quadratic equation
        float a = Vector3.Dot(Direction, Direction);
        float b = 2 * Vector3.Dot(sphereToRay, Direction);
        float c = Vector3.Dot(sphereToRay, sphereToRay) - sphere.Radius * sphere.Radius;

        // Calculate the discriminant, if negative - no intersections.
        float discriminant = b * b - 4 * a * c;
        if (discriminant < 0) return false;

        // Calculate the two possible values for t (intersection points)
        float t1 = (-b + (float)Math.Sqrt(discriminant)) / (2 * a);
        float t2 = (-b - (float)Math.Sqrt(discriminant)) / (2 * a);

        intersectionPoint1 = Start + t1 * Direction;
        intersectionPoint2 = Start + t2 * Direction;

        return true;
    }

    public bool IntersectWithCube(Cube cube, out Vector3 intersectionPoint, out Vector3 surfaceNormal)
    {
        intersectionPoint = Vector3.Zero;
        surfaceNormal = Vector3.Zero;

        Vector3 minBounds = cube.Origin - cube.HalfExtents;
        Vector3 maxBounds = cube.Origin + cube.HalfExtents;

        Vector3 invDirection = new Vector3(1.0f / Direction.X, 1.0f / Direction.Y, 1.0f / Direction.Z);

        // x slab
        float tx1 = (minBounds.X - Start.X) * invDirection.X;
        float tx2 = (maxBounds.X - Start.X) * invDirection.X;
        float tMinX = Math.Min(tx1, tx2);
        float tMaxX = Math.Max(tx1, tx2);

        // y slab
        float ty1 = (minBounds.Y - Start.Y) * invDirection.Y;
        float ty2 = (maxBounds.Y - Start.Y) * invDirection.Y;
        float tMinY = Math.Min(ty1, ty2);
        float tMaxY = Math.Max(ty1, ty2);

        // No intersection along the x or y axis
        if (tMinX > tMaxY || tMinY > tMaxX)
            return false;

        float tMin = Math.Max(tMinX, tMinY);
        float tMax = Math.Min(tMaxX, tMaxY);

        // z slab
        float tz1 = (minBounds.Z - Start.Z) * invDirection.Z;
        float tz2 = (maxBounds.Z - Start.Z) * invDirection.Z;
        float tMinZ = Math.Min(tz1, tz2);
        float tMaxZ = Math.Max(tz1, tz2);

        // No intersection along the z-axis
        if (tMin > tMaxZ || tMinZ > tMax)
            return false;

        tMin = Math.Max(tMin, tMinZ);
        tMax = Math.Min(tMax, tMaxZ);

        // Behind the ray starting point
        if (tMax < 0)
            return false;

        intersectionPoint = Start + tMin * Direction;

        if (tMin == tMinX)
            surfaceNormal = new Vector3(invDirection.X < 0 ? 1 : -1, 0, 0);
        else if (tMin == tMinY)
            surfaceNormal = new Vector3(0, invDirection.Y < 0 ? 1 : -1, 0);
        else if (tMin == tMinZ)
            surfaceNormal = new Vector3(0, 0, invDirection.Z < 0 ? 1 : -1);

        return true;
    }
}