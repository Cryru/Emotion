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

    public readonly Vector3 IntersectWithPlane(Vector3 planeNormal, Vector3 planePoint)
    {
        // Check if the ray is parallel to the plane
        float dot = Vector3.Dot(Direction, planeNormal);
        if (MathF.Abs(dot) < float.Epsilon) return Vector3.Zero;

        float distance = Vector3.Dot(planeNormal, planePoint - Start) / dot;
        return Start + distance * Direction;
    }

    public readonly bool IntersectWithObject(GameObject obj,
        MeshComponent meshComponent,
        out Mesh? collidedMesh,
        out Vector3 collisionPoint,
        out Vector3 normal,
        out int triangleIndex,
        bool closest = false
    )
    {
        collidedMesh = null;
        collisionPoint = Vector3.Zero;
        normal = Vector3.Zero;
        triangleIndex = -1;

        return meshComponent.IntersectRay(this, out collidedMesh, out collisionPoint, out normal, out triangleIndex, closest);
    }

    public bool IntersectWithVertices(ushort[] indices, VertexDataAllocation vertices, out Vector3 collisionPoint, out Vector3 normal, out int triangleIndex)
    {
        return IntersectWithVertices(indices, indices.Length, vertices, out collisionPoint, out normal, out triangleIndex);
    }

    public bool IntersectWithVertices<TIndex>(TIndex[] indices, int indicesUsed, VertexDataAllocation vertices, out Vector3 collisionPoint, out Vector3 normal, out int triangleIndex)
        where TIndex : INumber<TIndex>
    {
        collisionPoint = Vector3.Zero;
        normal = Vector3.Zero;
        triangleIndex = -1;

        if (!vertices.Format.Built) return false;
        if (!vertices.Format.HasPosition) return false;

        var closestDistance = float.MaxValue;
        var intersectionFound = false;

        for (var i = 0; i < indicesUsed; i += 3)
        {
            TIndex idx1 = indices[i];
            TIndex idx2 = indices[i + 1];
            TIndex idx3 = indices[i + 2];

            Triangle triangle = vertices.GetTriangleAtIndices<TIndex>(idx1, idx2, idx3);

            if (!IntersectsTriangle(triangle, out float t)) continue;

            if (t < closestDistance)
            {
                closestDistance = t;
                normal = triangle.Normal;
                triangleIndex = i;
                intersectionFound = true;
            }
        }

        if (intersectionFound) collisionPoint = Start + Direction * closestDistance;

        return intersectionFound;
    }

    public readonly bool IntersectsTriangle(Triangle tri, out float distance)
    {
        distance = 0;

        Vector3 edge1 = tri.B - tri.A;
        Vector3 edge2 = tri.C - tri.A;

        Vector3 h = Vector3.Cross(Direction, edge2);
        float a = Vector3.Dot(edge1, h);

        // Ray is parallel to triangle, or triangle is degenerate
        if (MathF.Abs(a) < float.Epsilon) return false;

        float f = 1f / a;
        Vector3 s = Start - tri.A;
        float u = f * Vector3.Dot(s, h);
        if (u < 0 || u > 1) return false;

        Vector3 q = Vector3.Cross(s, edge1);
        float v = f * Vector3.Dot(Direction, q);
        if (v < 0 || u + v > 1) return false;

        distance = f * Vector3.Dot(edge2, q);
        return distance >= 0;
    }

    public readonly bool IntersectWithSphere(Sphere sphere, out Vector3 entryPoint, out Vector3 exitPoint)
    {
        entryPoint = Vector3.Zero;
        exitPoint = Vector3.Zero;

        Vector3 sphereToRay = Start - sphere.Origin;

        // Quadratic equation
        float a = Vector3.Dot(Direction, Direction);
        float halfB = Vector3.Dot(sphereToRay, Direction);
        float c = Vector3.Dot(sphereToRay, sphereToRay) - sphere.Radius * sphere.Radius;

        float discriminant = halfB * halfB - a * c;
        if (discriminant < 0) return false; // no intersections

        float sqrtD = MathF.Sqrt(discriminant);
        float invA = 1f / a;

        float tNear = (-halfB - sqrtD) * invA;
        float tFar = (-halfB + sqrtD) * invA;
        if (tNear < 0 && tFar < 0) return false;

        entryPoint = tNear >= 0 ? Start + tNear * Direction : Vector3.Zero;
        exitPoint = tFar >= 0 ? Start + tFar * Direction : Vector3.Zero;

        return true;
    }

    public bool IntersectWithCube(Cube cube, out Vector3 intersectionPoint, out Vector3 surfaceNormal)
    {
        intersectionPoint = Vector3.Zero;
        surfaceNormal = Vector3.Zero;

        Vector3 invDirection = Vector3.One / Direction;

        Vector3 t1 = (cube.Origin - cube.HalfExtents - Start) * invDirection;
        Vector3 t2 = (cube.Origin + cube.HalfExtents - Start) * invDirection;

        Vector3 tMins = Vector3.Min(t1, t2);
        Vector3 tMaxs = Vector3.Max(t1, t2);

        float tMin = MathF.Max(MathF.Max(tMins.X, tMins.Y), tMins.Z);
        float tMax = MathF.Min(MathF.Min(tMaxs.X, tMaxs.Y), tMaxs.Z);

        if (tMax < 0 || tMin > tMax)
            return false;

        float t = tMin >= 0 ? tMin : tMax;
        intersectionPoint = Start + t * Direction;

        if (t == tMins.X)
            surfaceNormal = new Vector3(invDirection.X < 0 ? 1 : -1, 0, 0);
        else if (t == tMins.Y)
            surfaceNormal = new Vector3(0, invDirection.Y < 0 ? 1 : -1, 0);
        else
            surfaceNormal = new Vector3(0, 0, invDirection.Z < 0 ? 1 : -1);

        return true;
    }
}