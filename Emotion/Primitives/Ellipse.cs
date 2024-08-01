namespace Emotion.Primitives;

public struct Ellipse
{
    public float X;
    public float Y;
    public float RadiusX;
    public float RadiusY;

    public bool Intersects(Rectangle rect)
    {
        float closestX = Math.Clamp(X, rect.X, rect.Right);
        float closestY = Math.Clamp(Y, rect.Y, rect.Bottom);

        float distanceX = closestX - X;
        float distanceY = closestY - Y;

        float distanceXSquared = distanceX * distanceX;
        float distanceYSquared = distanceY * distanceY;
        float radiusXSquared = RadiusX * RadiusX;
        float radiusYSquared = RadiusY * RadiusY;

        return (distanceXSquared / radiusXSquared) + (distanceYSquared / radiusYSquared) <= 1;
    }

    public void RenderDebug(RenderComposer c, Color col)
    {
        c.RenderEllipse(new Vector3(X, Y, 0), new Vector2(RadiusX, RadiusY), col, true);
    }
}
