#nullable enable

#region Using

using Emotion.Game.World3D;
using Emotion.Graphics.Data;
using Emotion.Graphics.ThreeDee;

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

    public bool IntersectWithMeshLocalSpace(Mesh mesh, out Vector3 collisionPoint, out Vector3 normal, out int triangleIndex)
    {
        collisionPoint = Vector3.Zero;
        normal = Vector3.Zero;
        triangleIndex = -1;

        var closestDistance = float.MaxValue;
        var intersectionFound = false;

        ushort[] meshIndices = mesh.Indices;
        VertexDataWithNormal[] vertices = mesh.VerticesONE;

        for (var i = 0; i < meshIndices.Length; i += 3)
        {
            ushort idx1 = meshIndices[i];
            ushort idx2 = meshIndices[i + 1];
            ushort idx3 = meshIndices[i + 2];

            Vector3 p1 = vertices[idx1].Vertex;
            Vector3 p2 = vertices[idx2].Vertex;
            Vector3 p3 = vertices[idx3].Vertex;

            Vector3 triangleNormal = Vector3.Normalize(Vector3.Cross(p2 - p1, p3 - p1));

            if (!IntersectWithTriangle(p1, p2, p3, triangleNormal, out float t)) continue;

            if (t < closestDistance)
            {
                IntersectWithTriangle(p1, p2, p3, triangleNormal, out float _);
                closestDistance = t;
                normal = triangleNormal;
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
                IntersectWithTriangle(p1, p2, p3, triangleNormal, out float _);
                closestDistance = t;
                normal = triangleNormal;
                triangleIndex = i;
                intersectionFound = true;
            }
        }

        if (intersectionFound) collisionPoint = Start + Direction * closestDistance;

        return intersectionFound;
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

    public bool IntersectWithCube(Cube cube, out Vector3 intersectionPoint)
    {
        intersectionPoint = Vector3.Zero;

        Vector3 minBounds = cube.Origin - cube.HalfExtents;
        Vector3 maxBounds = cube.Origin + cube.HalfExtents;

        // Calculate the inverse direction to simplify calculations
        Vector3 invDirection = new Vector3(1.0f / Direction.X, 1.0f / Direction.Y, 1.0f / Direction.Z);

        // Calculate the intersection with each slab along the x-axis
        float minX = (minBounds.X - Start.X) * invDirection.X;
        float maxX = (maxBounds.X - Start.X) * invDirection.X;

        // Swap if needed
        if (minX > maxX)
            (minX, maxX) = (maxX, minX);

        // Calculate the intersection with each slab along the y-axis
        float minY = (minBounds.Y - Start.Y) * invDirection.Y;
        float maxY = (maxBounds.Y - Start.Y) * invDirection.Y;

        // Swap if needed
        if (minY > maxY)
            (minY, maxY) = (maxY, minY);

        // Check for no intersection along the x or y axis
        if (minX > maxY || minY > maxX)
            return false;

        // Update tMin and tMax to consider the intersection along the y-axis
        minX = Math.Max(minX, minY);
        maxX = Math.Min(maxX, maxY);

        // Calculate the intersection with each slab along the z-axis
        float minZ = (minBounds.Z - Start.Z) * invDirection.Z;
        float maxZ = (maxBounds.Z - Start.Z) * invDirection.Z;

        // Swap if needed
        if (minZ > maxZ)
            (minZ, maxZ) = (maxZ, minZ);

        // Check for no intersection along the z-axis
        if (minX > maxZ || minZ > maxX)
            return false;

        // Update tMin and tMax to consider the intersection along the z-axis
        minX = Math.Max(minX, minZ);
        maxX = Math.Min(maxX, maxZ);

        // Check if the intersection is behind the ray starting point
        if (maxX < 0)
            return false;

        // Calculate the intersection point
        intersectionPoint = Start + minX * Direction;

        return true;
    }
}