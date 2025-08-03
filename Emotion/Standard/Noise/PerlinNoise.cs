#nullable enable

using Emotion.ThirdParty.IcariaNoise;

namespace Emotion.Standard.Noise;

/// <summary>
/// An object used to sample Perlin Noise, implemented via Icaria Noise's GradientNoise.
/// </summary>
public struct PerlinNoise
{
    private int _seed;

    public PerlinNoise(int? seed = null)
    {
        if (seed == null)
            _seed = (int)(int.MaxValue * Helpers.GenerateRandomFloat());
        else
            _seed = seed.Value;
    }

    public float Sample2D(Vector2 pos)
    {
        return IcariaNoise.GradientNoise(pos.X, pos.Y, _seed);
    }

    public float Sample2D(float x, float y)
    {
        return IcariaNoise.GradientNoise(x, y, _seed);
    }

    public float Sample2D(int x, int y)
    {
        // Perlin noise reguires non-int coordinates!
        return IcariaNoise.GradientNoise(x + 0.5f, y + 0.5f, _seed);
    }
}
