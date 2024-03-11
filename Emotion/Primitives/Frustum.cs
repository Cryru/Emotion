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
}
