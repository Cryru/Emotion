#region Using

#endregion

namespace Emotion.Primitives;

/// <summary>
/// A struct representing a ray.
/// </summary>
public struct Ray2D
{
    public Vector2 Start;
    public Vector2 Direction;

    /// <summary>
    /// Create a ray from a starting position, direction and length.
    /// </summary>
    /// <param name="start">The ray's start.</param>
    /// <param name="direction">The direction of the ray.</param>
    public Ray2D(Vector2 start, Vector2 direction)
    {
        Start = start;
        Direction = direction.SafeNormalize();
    }

    public bool IntersectWithRectangle(Rectangle rect, out Vector2 collisionPoint)
    {
        collisionPoint = Vector2.Zero;
        if (Direction == Vector2.Zero) return false;

        rect.GetMinMaxPoints(out Vector2 rectMin, out Vector2 rectMax);

        Vector2 inverseDir = new Vector2(1f) / Direction;

        Vector2 tNear = (rectMin - Start) * inverseDir;
        Vector2 tFar = (rectMax - Start) * inverseDir;

        Vector2 tMin = Vector2.Min(tNear, tFar);
        Vector2 tMax = Vector2.Max(tNear, tFar);

        // Find the largest tMin (entry point) and smallest tMax (exit point)
        float tEntry = Math.Max(tMin.X, tMin.Y);
        float tExit = Math.Min(tMax.X, tMin.Y);

        // If tExit < 0, the rectangle is behind the ray
        // If tEntry > tExit, there is no intersection
        if (tExit < 0 || tEntry > tExit || float.IsNaN(tEntry))
            return false;

        collisionPoint = Start + tEntry * Direction;
        return true;
    }
}