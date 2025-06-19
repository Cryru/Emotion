namespace Emotion.Primitives;

public struct Frustum
{
    public Plane Far;
    public Plane Near;

    public Plane Top;
    public Plane Bottom;
    public Plane Left;
    public Plane Right;

    public Frustum(Matrix4x4 viewProjectionMatrix)
    {
        Left = Plane.Normalize(new Plane(
                         viewProjectionMatrix.M14 + viewProjectionMatrix.M11,
                         viewProjectionMatrix.M24 + viewProjectionMatrix.M21,
                         viewProjectionMatrix.M34 + viewProjectionMatrix.M31,
                         viewProjectionMatrix.M44 + viewProjectionMatrix.M41
                ));

        Right = Plane.Normalize(new Plane(
                         viewProjectionMatrix.M14 - viewProjectionMatrix.M11,
                         viewProjectionMatrix.M24 - viewProjectionMatrix.M21,
                         viewProjectionMatrix.M34 - viewProjectionMatrix.M31,
                         viewProjectionMatrix.M44 - viewProjectionMatrix.M41
                ));


        Bottom = Plane.Normalize(new Plane(
                        viewProjectionMatrix.M14 + viewProjectionMatrix.M12,
                        viewProjectionMatrix.M24 + viewProjectionMatrix.M22,
                        viewProjectionMatrix.M34 + viewProjectionMatrix.M32,
                        viewProjectionMatrix.M44 + viewProjectionMatrix.M42
                ));

        Top = Plane.Normalize(new Plane(
                        viewProjectionMatrix.M14 - viewProjectionMatrix.M12,
                        viewProjectionMatrix.M24 - viewProjectionMatrix.M22,
                        viewProjectionMatrix.M34 - viewProjectionMatrix.M32,
                        viewProjectionMatrix.M44 - viewProjectionMatrix.M42
                ));

        Near = Plane.Normalize(new Plane(
                        viewProjectionMatrix.M13,
                        viewProjectionMatrix.M23,
                        viewProjectionMatrix.M33,
                        viewProjectionMatrix.M43
                ));

        Far = Plane.Normalize(new Plane(
                        viewProjectionMatrix.M14 - viewProjectionMatrix.M13,
                        viewProjectionMatrix.M24 - viewProjectionMatrix.M23,
                        viewProjectionMatrix.M34 - viewProjectionMatrix.M33,
                        viewProjectionMatrix.M44 - viewProjectionMatrix.M43
                ));
    }

    public bool IntersectsOrContainsSphere(Sphere sphere)
    {
        float distance = Plane.DotCoordinate(Top, sphere.Origin);
        if (distance < -sphere.Radius) return false; // Behind this plane
        if (MathF.Abs(distance) < sphere.Radius) return true; // Intersects plane

        distance = Plane.DotCoordinate(Bottom, sphere.Origin);
        if (distance < -sphere.Radius) return false;
        if (MathF.Abs(distance) < sphere.Radius) return true;

        distance = Plane.DotCoordinate(Left, sphere.Origin);
        if (distance < -sphere.Radius) return false;
        if (MathF.Abs(distance) < sphere.Radius) return true;

        distance = Plane.DotCoordinate(Right, sphere.Origin);
        if (distance < -sphere.Radius) return false;
        if (MathF.Abs(distance) < sphere.Radius) return true;

        distance = Plane.DotCoordinate(Near, sphere.Origin);
        if (distance < -sphere.Radius) return false;
        if (MathF.Abs(distance) < sphere.Radius) return true;

        distance = Plane.DotCoordinate(Far, sphere.Origin);
        if (distance < -sphere.Radius) return false;
        if (MathF.Abs(distance) < sphere.Radius) return true;

        return true; // Sphere is entirely in front of all planes (inside)
    }

    public bool IntersectsOrContainsCube(Cube cube)
    {
        (Vector3 min, Vector3 max) = cube.GetMinMax();

        Span<Plane> planes = new Plane[] { Top, Bottom, Left, Right, Near, Far };
        foreach (Plane plane in planes)
        {
            Vector3 positiveVertex = new Vector3(
                plane.Normal.X >= 0 ? max.X : min.X,
                plane.Normal.Y >= 0 ? max.Y : min.Y,
                plane.Normal.Z >= 0 ? max.Z : min.Z
            );

            Vector3 negativeVertex = new Vector3(
                plane.Normal.X >= 0 ? min.X : max.X,
                plane.Normal.Y >= 0 ? min.Y : max.Y,
                plane.Normal.Z >= 0 ? min.Z : max.Z
            );

            if (Plane.DotCoordinate(plane, positiveVertex) < 0)
                return false;
        }

        return true;
    }

    #region Get Corners

    public void GetCorners(Span<Vector3> fillCorners)
    {
        fillCorners[0] = PlaneIntersection(Far, Bottom, Left);  // Bottom-left (near plane)
        fillCorners[1] = PlaneIntersection(Far, Bottom, Right); // Bottom-right (near plane)
        fillCorners[2] = PlaneIntersection(Near, Bottom, Right);// Top-left (near plane)
        fillCorners[3] = PlaneIntersection(Near, Bottom, Left); // Top-right (near plane)

        fillCorners[4] = PlaneIntersection(Far, Top, Left);     // Bottom-left (far plane)
        fillCorners[5] = PlaneIntersection(Far, Top, Right);    // Bottom-right (far plane)
        fillCorners[6] = PlaneIntersection(Near, Top, Right);   // Top-right (far plane)
        fillCorners[7] = PlaneIntersection(Near, Top, Left);    // Top-left (far plane)
    }

    // Helper to find the intersection point of three planes
    private Vector3 PlaneIntersection(Plane p1, Plane p2, Plane p3)
    {
        Vector3 normal1 = p1.Normal;
        Vector3 normal2 = p2.Normal;
        Vector3 normal3 = p3.Normal;
        float dot = Vector3.Dot(normal1, Vector3.Cross(normal2, normal3));

        Vector3 point = (-p1.D * Vector3.Cross(normal2, normal3) -
                         p2.D * Vector3.Cross(normal3, normal1) -
                         p3.D * Vector3.Cross(normal1, normal2)) / dot;

        return point;
    }

    public static void GetCameraFrustumSidePlanes(Span<Vector3> frustumCorners, Span<Vector3> sidePlaneA, Span<Vector3> sidePlaneB)
    {
        sidePlaneA[0] = frustumCorners[0];
        sidePlaneA[1] = frustumCorners[3];
        sidePlaneA[2] = frustumCorners[7];
        sidePlaneA[3] = frustumCorners[4];

        sidePlaneB[0] = frustumCorners[1];
        sidePlaneB[1] = frustumCorners[2];
        sidePlaneB[2] = frustumCorners[6];
        sidePlaneB[3] = frustumCorners[5];
    }

    public static void GetCameraFrustumNearAndFarPlanes(Span<Vector3> frustumCorners, Span<Vector3> sidePanelNear, Span<Vector3> sidePlaneFar)
    {
        sidePanelNear[0] = frustumCorners[0];
        sidePanelNear[1] = frustumCorners[1];
        sidePanelNear[2] = frustumCorners[2];
        sidePanelNear[3] = frustumCorners[3];

        sidePlaneFar[0] = frustumCorners[4];
        sidePlaneFar[1] = frustumCorners[5];
        sidePlaneFar[2] = frustumCorners[6];
        sidePlaneFar[3] = frustumCorners[7];
    }

    #endregion

    public void Render(RenderComposer c, Color colOutline, Color colFill)
    {
        Span<Vector3> corners = stackalloc Vector3[8];
        GetCorners(corners);

        Span<Vector3> sideA = stackalloc Vector3[4];
        Span<Vector3> sideB = stackalloc Vector3[4];
        Span<Vector3> sideNear = stackalloc Vector3[4];
        Span<Vector3> sideFar = stackalloc Vector3[4];

        Frustum.GetCameraFrustumSidePlanes(corners, sideA, sideB);
        Frustum.GetCameraFrustumNearAndFarPlanes(corners, sideNear, sideFar);

        c.RenderQuad(sideA, colFill);
        c.RenderQuad(sideB, colFill);
        c.RenderQuad(sideNear, colFill);
        c.RenderQuad(sideFar, colFill);

        // Far plane
        c.RenderLine(corners[0], corners[1], colOutline, 0.15f);
        c.RenderLine(corners[4], corners[5], colOutline, 0.15f);

        // Far plane x Right Plane
        c.RenderLine(corners[0], corners[4], colOutline, 0.15f);

        // Left plane
        c.RenderLine(corners[1], corners[2], colOutline, 0.15f);
        c.RenderLine(corners[5], corners[6], colOutline, 0.15f);

        // Near plane
        c.RenderLine(corners[2], corners[3], colOutline, 0.15f);
        c.RenderLine(corners[6], corners[7], colOutline, 0.15f);

        // Near plane x Left Plane
        c.RenderLine(corners[2], corners[6], colOutline, 0.15f);

        // Near Plane x Right Plane
        c.RenderLine(corners[3], corners[7], colOutline, 0.15f);

        // Right plane
        c.RenderLine(corners[3], corners[0], colOutline, 0.15f);
        c.RenderLine(corners[7], corners[4], colOutline, 0.15f);

        // Far plane X Left Plane
        c.RenderLine(corners[1], corners[5], colOutline, 0.15f);
    }
}
