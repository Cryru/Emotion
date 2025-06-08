#nullable enable

using Emotion.ThirdParty.FastNoiseLite;

namespace Emotion.Utility.Noise;

public struct SimplexNoise
{
    private int _seed;

    public SimplexNoise(int? seed = null)
    {
        if (seed == null)
            _seed = (int)(int.MaxValue * Helpers.GenerateRandomFloat());
        else
            _seed = seed.Value;
    }

    public float Sample2D(Vector2 pos)
    {
        return FastNoiseLite.SimpleSimplex(pos.X, pos.Y, _seed);
    }

    public float Sample2D(float x, float y)
    {
        return FastNoiseLite.SimpleSimplex(x, y, _seed);
    }

    public float Sample2D(int x, int y)
    {
        // Perlin noise reguires non-int coordinates!
        return FastNoiseLite.SimpleSimplex(x + 0.5f, y + 0.5f, _seed);
    }
}
