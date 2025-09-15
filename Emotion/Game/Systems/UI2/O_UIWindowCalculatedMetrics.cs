#nullable enable

namespace Emotion.Game.Systems.UI2;

public class O_UIWindowCalculatedMetrics
{
    public Vector3 Position;
    public Vector2 Size;

    public override string ToString()
    {
        return $"{Position} x {Size}";
    }
}