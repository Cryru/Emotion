#nullable enable

#region Using

using Emotion.Game.World.Components;
using Emotion.Game.World.ThreeDee;
using Emotion.Graphics.Data;

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

    // todo
    public bool IntersectWithObject(GameObject obj, MeshComponent meshComponent,
        out Mesh? collidedMesh, out Vector3 collisionPoint, out Vector3 normal, out int triangleIndex, bool closest = false)
    {
        collidedMesh = null;
        collisionPoint = Vector3.Zero;
        normal = Vector3.Zero;
        triangleIndex = -1;

        return meshComponent.IntersectRay(this, out collidedMesh, out collisionPoint, out normal, out triangleIndex, closest);
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
        if (triangleArea < 0.0001f) return false;

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