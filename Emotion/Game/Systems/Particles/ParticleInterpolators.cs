#nullable enable

namespace Emotion.Game.Systems.Particles;

public struct ColorAtTime
{
    public float PercentOfLifetime;
    public Color Color;

    public ColorAtTime(float percent, Color color)
    {
        PercentOfLifetime = percent;
        Color = color;
    }
}

public struct SizeAtTime
{
    public float PercentOfLifetime;
    public Vector2 Size;

    public SizeAtTime(float percent, Vector2 size)
    {
        PercentOfLifetime = percent;
        Size = size;
    }
}
