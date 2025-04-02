using Emotion.Utility;

namespace Emotion.Primitives;

public struct Triangle
{
    public static Triangle Invalid = new Triangle(Vector3.Zero, Vector3.Zero, Vector3.Zero);

    public Vector3 A;
    public Vector3 B;
    public Vector3 C;

    public LineSegment Base { get => new LineSegment(B.ToVec2(), C.ToVec2()); }

    public Vector3 Apex { get => A; }

    public Vector3 Normal { get => Vector3.Normalize(Vector3.Cross(B - A, C - A)); }

    public bool Valid { get => A != Vector3.Zero || B != Vector3.Zero || C != Vector3.Zero; }

    public Triangle(Vector3 a, Vector3 b, Vector3 c)
    {
        A = a; 
        B = b; 
        C = c;
    }

    public void RenderOutline(RenderComposer c, Color? color = null, float thickness = 1)
    {
        c.RenderLine(A, B, color ?? Color.White, thickness);
        c.RenderLine(B, C, color ?? Color.White, thickness);
        c.RenderLine(C, A, color ?? Color.White, thickness);
    }

    public bool IsPoint2DInTriangle(Vector2 point, float tolerance = Maths.EPSILON)
    {
        // Compute barycentric coordinates
        float denominator = (B.Y - C.Y) * (A.X - C.X) + (C.X - B.X) * (A.Y - C.Y);
        float alpha = ((B.Y - C.Y) * (point.X - C.X) + (C.X - B.X) * (point.Y - C.Y)) / denominator;
        float beta = ((C.Y - A.Y) * (point.X - C.X) + (A.X - C.X) * (point.Y - C.Y)) / denominator;
        float gamma = 1.0f - alpha - beta;

        return alpha >= -tolerance &&
            beta >= -tolerance &&
            gamma >= -tolerance;
    }
}
