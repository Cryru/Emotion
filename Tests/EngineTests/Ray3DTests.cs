#region Using

using Emotion.Primitives;
using Emotion.Testing;
using System;
using System.Numerics;

#endregion

namespace Tests.EngineTests;

public class Ray3DTests : ProxyRenderTestingScene
{
    #region IntersectWithPlane

    [Test]
    public void PlaneIntersection_HitsPlane()
    {
        var ray = new Ray3D(new Vector3(0, 0, 10), new Vector3(0, 0, -1));
        Vector3 hit = ray.IntersectWithPlane(new Vector3(0, 0, 1), Vector3.Zero);

        Assert.True(hit.Z == 0);
        Assert.True(hit.X == 0);
        Assert.True(hit.Y == 0);
    }

    [Test]
    public void PlaneIntersection_ParallelRay_ReturnsZero()
    {
        var ray = new Ray3D(new Vector3(0, 0, 5), new Vector3(1, 0, 0));
        Vector3 hit = ray.IntersectWithPlane(new Vector3(0, 0, 1), Vector3.Zero);

        Assert.True(hit == Vector3.Zero);
    }

    [Test]
    public void PlaneIntersection_Angled()
    {
        // 45 degree ray hitting the XY plane - hit point should be at Z=0, X=10
        var ray = new Ray3D(new Vector3(0, 0, 10), Vector3.Normalize(new Vector3(1, 0, -1)));
        Vector3 hit = ray.IntersectWithPlane(new Vector3(0, 0, 1), Vector3.Zero);

        Assert.True(MathF.Abs(hit.Z) < 0.001f);
        Assert.True(MathF.Abs(hit.X - 10f) < 0.001f);
        Assert.True(MathF.Abs(hit.Y) < 0.001f);
    }

    [Test]
    public void PlaneIntersection_OffsetPlane()
    {
        // Plane at Z=5, ray coming from Z=10
        var ray = new Ray3D(new Vector3(0, 0, 10), new Vector3(0, 0, -1));
        Vector3 hit = ray.IntersectWithPlane(new Vector3(0, 0, 1), new Vector3(0, 0, 5));

        Assert.True(MathF.Abs(hit.Z - 5f) < 0.001f);
    }

    [Test]
    public void PlaneIntersection_RayOnPlane_ReturnsZero()
    {
        // Ray starting on the plane and going along it - parallel, should return zero
        var ray = new Ray3D(new Vector3(0, 0, 0), new Vector3(1, 0, 0));
        Vector3 hit = ray.IntersectWithPlane(new Vector3(0, 0, 1), Vector3.Zero);

        Assert.True(hit == Vector3.Zero);
    }

    #endregion

    #region IntersectsTriangle

    [Test]
    public void IntersectsTriangle_DirectHit()
    {
        var ray = new Ray3D(new Vector3(0, 0, 5), new Vector3(0, 0, -1));
        var tri = new Triangle(
            new Vector3(-1, -1, 0),
            new Vector3(1, -1, 0),
            new Vector3(0, 1, 0)
        );

        bool hit = ray.IntersectsTriangle(tri, out float distance);
        Assert.True(hit);
        Assert.True(distance == 5f);
    }

    [Test]
    public void IntersectsTriangle_Miss()
    {
        var ray = new Ray3D(new Vector3(5, 5, 5), new Vector3(0, 0, -1));
        var tri = new Triangle(
            new Vector3(-1, -1, 0),
            new Vector3(1, -1, 0),
            new Vector3(0, 1, 0)
        );

        bool hit = ray.IntersectsTriangle(tri, out float _);
        Assert.True(!hit);
    }

    [Test]
    public void IntersectsTriangle_BehindRay_NoHit()
    {
        var ray = new Ray3D(new Vector3(0, 0, -5), new Vector3(0, 0, -1));
        var tri = new Triangle(
            new Vector3(-1, -1, 0),
            new Vector3(1, -1, 0),
            new Vector3(0, 1, 0)
        );

        bool hit = ray.IntersectsTriangle(tri, out float _);
        Assert.True(!hit);
    }

    [Test]
    public void IntersectsTriangle_Parallel_NoHit()
    {
        var ray = new Ray3D(new Vector3(0, 0, 5), new Vector3(1, 0, 0));
        var tri = new Triangle(
            new Vector3(-1, -1, 0),
            new Vector3(1, -1, 0),
            new Vector3(0, 1, 0)
        );

        bool hit = ray.IntersectsTriangle(tri, out float _);
        Assert.True(!hit);
    }

    [Test]
    public void IntersectsTriangle_EdgeCases()
    {
        var tri = new Triangle(
            new Vector3(-1, -1, 0),
            new Vector3(1, -1, 0),
            new Vector3(0, 1, 0)
        );

        // Hit near each corner - should still count as a hit
        bool hitNearA = new Ray3D(new Vector3(-0.9f, -0.9f, 5), new Vector3(0, 0, -1)).IntersectsTriangle(tri, out float _);
        bool hitNearB = new Ray3D(new Vector3(0.9f, -0.9f, 5), new Vector3(0, 0, -1)).IntersectsTriangle(tri, out float _);
        bool hitNearC = new Ray3D(new Vector3(0f, 0.9f, 5), new Vector3(0, 0, -1)).IntersectsTriangle(tri, out float _);

        Assert.True(hitNearA);
        Assert.True(hitNearB);
        Assert.True(hitNearC);

        // Just outside each edge - should miss
        bool missOutsideAB = new Ray3D(new Vector3(0f, -1.1f, 5), new Vector3(0, 0, -1)).IntersectsTriangle(tri, out float _);
        bool missOutsideAC = new Ray3D(new Vector3(-1f, 0.5f, 5), new Vector3(0, 0, -1)).IntersectsTriangle(tri, out float _);

        Assert.True(!missOutsideAB);
        Assert.True(!missOutsideAC);
    }

    [Test]
    public void IntersectsTriangle_Degenerate_NoHit()
    {
        // Collinear points
        var tri = new Triangle(
            new Vector3(0, 0, 0),
            new Vector3(1, 0, 0),
            new Vector3(2, 0, 0)
        );

        var ray = new Ray3D(new Vector3(0, 0, 5), new Vector3(0, 0, -1));
        bool hit = ray.IntersectsTriangle(tri, out float _);
        Assert.True(!hit);
    }

    #endregion

    #region IntersectWithSphere

    [Test]
    public void SphereIntersection_DirectHit()
    {
        var ray = new Ray3D(new Vector3(0, 0, 10), new Vector3(0, 0, -1));
        var sphere = new Sphere(Vector3.Zero, 1f);

        bool hit = ray.IntersectWithSphere(sphere, out Vector3 p1, out Vector3 p2);
        Assert.True(hit);
        Assert.True(MathF.Abs(p1.Z - 1f) < 0.001f);
        Assert.True(MathF.Abs(p2.Z + 1f) < 0.001f);
    }

    [Test]
    public void SphereIntersection_Miss()
    {
        var ray = new Ray3D(new Vector3(5, 5, 10), new Vector3(0, 0, -1));
        var sphere = new Sphere(Vector3.Zero, 1f);

        bool hit = ray.IntersectWithSphere(sphere, out Vector3 _, out Vector3 _);
        Assert.True(!hit);
    }

    [Test]
    public void SphereIntersection_RayInsideSphere()
    {
        var ray = new Ray3D(Vector3.Zero, new Vector3(0, 0, 1));
        var sphere = new Sphere(Vector3.Zero, 5f);

        // p1 (near) should be zero since ray starts inside, p2 (far) should be the exit point
        bool hit = ray.IntersectWithSphere(sphere, out Vector3 p1, out Vector3 p2);
        Assert.True(hit);
        Assert.True(p1 == Vector3.Zero);
        Assert.True(MathF.Abs(p2.Z - 5f) < 0.001f);
    }

    [Test]
    public void SphereIntersection_BehindRay_NoHit()
    {
        // Sphere entirely behind the ray origin
        var ray = new Ray3D(new Vector3(0, 0, 10), new Vector3(0, 0, 1));
        var sphere = new Sphere(Vector3.Zero, 1f);

        bool hit = ray.IntersectWithSphere(sphere, out Vector3 _, out Vector3 _);
        Assert.True(!hit);
    }

    [Test]
    public void SphereIntersection_Tangent()
    {
        // Ray just grazes the sphere - discriminant near zero, should still hit
        var ray = new Ray3D(new Vector3(1, 0, 10), new Vector3(0, 0, -1));
        var sphere = new Sphere(Vector3.Zero, 1f);

        bool hit = ray.IntersectWithSphere(sphere, out Vector3 p1, out Vector3 p2);
        Assert.True(hit);
        // Both points should be approximately the same (tangent touch)
        Assert.True(Vector3.Distance(p1, p2) < 0.001f);
    }

    [Test]
    public void SphereIntersection_PointsAreOnSphere()
    {
        // Both intersection points should lie on the sphere surface
        var ray = new Ray3D(new Vector3(0, 0, 10), new Vector3(0, 0, -1));
        var sphere = new Sphere(Vector3.Zero, 2f);

        ray.IntersectWithSphere(sphere, out Vector3 p1, out Vector3 p2);

        float d1 = Vector3.Distance(p1, sphere.Origin);
        float d2 = Vector3.Distance(p2, sphere.Origin);
        Assert.True(MathF.Abs(d1 - sphere.Radius) < 0.001f);
        Assert.True(MathF.Abs(d2 - sphere.Radius) < 0.001f);
    }

    #endregion

    #region IntersectWithCube

    [Test]
    public void CubeIntersection_DirectHit()
    {
        var ray = new Ray3D(new Vector3(0, 0, 10), new Vector3(0, 0, -1));
        var cube = new Cube(Vector3.Zero, new Vector3(1, 1, 1));

        bool hit = ray.IntersectWithCube(cube, out Vector3 point, out Vector3 normal);
        Assert.True(hit);
        Assert.True(MathF.Abs(point.Z - 1f) < 0.001f);
        Assert.True(normal.Z != 0);
    }

    [Test]
    public void CubeIntersection_Miss()
    {
        var ray = new Ray3D(new Vector3(5, 5, 10), new Vector3(0, 0, -1));
        var cube = new Cube(Vector3.Zero, new Vector3(1, 1, 1));

        bool hit = ray.IntersectWithCube(cube, out Vector3 _, out Vector3 _);
        Assert.True(!hit);
    }

    [Test]
    public void CubeIntersection_RayInsideCube()
    {
        var ray = new Ray3D(Vector3.Zero, new Vector3(0, 0, 1));
        var cube = new Cube(Vector3.Zero, new Vector3(5, 5, 5));

        bool hit = ray.IntersectWithCube(cube, out Vector3 _, out Vector3 _);
        Assert.True(hit);
    }

    [Test]
    public void CubeIntersection_NormalFace()
    {
        var ray = new Ray3D(new Vector3(0, 0, 10), new Vector3(0, 0, -1));
        var cube = new Cube(Vector3.Zero, new Vector3(1, 1, 1));

        ray.IntersectWithCube(cube, out Vector3 _, out Vector3 normal);
        Assert.True(normal.Z != 0);
        Assert.True(normal.X == 0 && normal.Y == 0);
    }

    [Test]
    public void CubeIntersection_BehindRay_NoHit()
    {
        var ray = new Ray3D(new Vector3(0, 0, 10), new Vector3(0, 0, 1));
        var cube = new Cube(Vector3.Zero, new Vector3(1, 1, 1));

        bool hit = ray.IntersectWithCube(cube, out Vector3 _, out Vector3 _);
        Assert.True(!hit);
    }

    [Test]
    public void CubeIntersection_AllFaceNormals()
    {
        var cube = new Cube(Vector3.Zero, new Vector3(1, 1, 1));

        // Hit each face and verify the normal points outward on the correct axis
        Assert.True(new Ray3D(new Vector3(0, 0, 10), new Vector3(0, 0, -1)).IntersectWithCube(cube, out _, out Vector3 n) && n.Z > 0); // +Z face
        Assert.True(new Ray3D(new Vector3(0, 0, -10), new Vector3(0, 0, 1)).IntersectWithCube(cube, out _, out n) && n.Z < 0); // -Z face
        Assert.True(new Ray3D(new Vector3(10, 0, 0), new Vector3(-1, 0, 0)).IntersectWithCube(cube, out _, out n) && n.X > 0); // +X face
        Assert.True(new Ray3D(new Vector3(-10, 0, 0), new Vector3(1, 0, 0)).IntersectWithCube(cube, out _, out n) && n.X < 0); // -X face
        Assert.True(new Ray3D(new Vector3(0, 10, 0), new Vector3(0, -1, 0)).IntersectWithCube(cube, out _, out n) && n.Y > 0); // +Y face
        Assert.True(new Ray3D(new Vector3(0, -10, 0), new Vector3(0, 1, 0)).IntersectWithCube(cube, out _, out n) && n.Y < 0); // -Y face
    }

    [Test]
    public void CubeIntersection_HitPointOnSurface()
    {
        // The hit point should lie exactly on the cube's surface
        var ray = new Ray3D(new Vector3(0, 0, 10), new Vector3(0, 0, -1));
        var cube = new Cube(Vector3.Zero, new Vector3(2, 2, 2));

        ray.IntersectWithCube(cube, out Vector3 point, out Vector3 _);

        // Point should be on the +Z face at Z=2
        Assert.True(MathF.Abs(point.Z - 2f) < 0.001f);
        Assert.True(MathF.Abs(point.X) < 0.001f);
        Assert.True(MathF.Abs(point.Y) < 0.001f);
    }

    #endregion
}