namespace Emotion.Primitives;

public struct Triangle
{
    public Vector3 A;
    public Vector3 B;
    public Vector3 C;

    public LineSegment Base { get => new LineSegment(B.ToVec2(), C.ToVec2()); }
    public Vector3 Apex { get => A; }

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
}
