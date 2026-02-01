#nullable enable

namespace Emotion.ThirdParty;

public record struct XoshiroState(ulong S0, ulong S1, ulong S2, ulong S3);

public static class Xoshiro256
{
    public static XoshiroState Initialize(ulong seed)
    {
        return new XoshiroState(
            SplitMix64(ref seed),
            SplitMix64(ref seed),
            SplitMix64(ref seed),
            SplitMix64(ref seed)
        );
    }

    public static ulong NextUint64(ref XoshiroState state)
    {
        ulong result = RotateLeft(state.S1 * 5, 7) * 9;
        ulong t = state.S1 << 17;

        state.S2 ^= state.S0;
        state.S3 ^= state.S1;
        state.S1 ^= state.S2;
        state.S0 ^= state.S3;

        state.S2 ^= t;
        state.S3 = RotateLeft(state.S3, 45);

        return result;
    }

    public static double NextDouble(ref XoshiroState state)
    {
        return (NextUint64(ref state) >> 11) * (1.0 / (1UL << 53));
    }

    public static int NextInRange(ref XoshiroState state, int min, int max)
    {
        max++; // Inclusive max

        return min + (int)(NextDouble(ref state) * (max - min));
    }

    private static ulong RotateLeft(ulong value, int offset)
    {
        return (value << offset) | (value >> (64 - offset));
    }

    private static ulong SplitMix64(ref ulong state)
    {
        ulong z = (state += 0x9E3779B97F4A7C15);
        z = (z ^ (z >> 30)) * 0xBF58476D1CE4E5B9;
        z = (z ^ (z >> 27)) * 0x94D049BB133111EB;
        return z ^ (z >> 31);
    }
}