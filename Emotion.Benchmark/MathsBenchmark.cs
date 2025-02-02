using BenchmarkDotNet.Attributes;
using Emotion.Utility;

namespace Emotion.Benchmark;

[MemoryDiagnoser]
public class MathsBenchmark
{
    [Benchmark]
    public float ClampWithMinMax()
    {
        float value = Helpers.GenerateRandomFloat();
        value = MathF.Min(value, 1f);
        value = MathF.Max(value, 0f);
        return value;
    }

    [Benchmark]
    public float ClampWithIfs()
    {
        float value = Helpers.GenerateRandomFloat();
        if (value < 0f)
            return 0f;

        return value > 1f ? 1f : value;
    }
}
