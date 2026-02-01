#nullable enable

using Emotion.Network.LockStep;
using Emotion.Network.New.Base;
using Emotion.ThirdParty;

namespace Emotion.Standard;

public static class RandHelpers
{
    public static ulong GenerateRandomSeed()
    {
        Random random = new Random();
        double randDouble = random.NextDouble();
        return (ulong)(ulong.MaxValue * randDouble);
    }
}

public static class LocalRand
{
    private static XoshiroState _randomState = Xoshiro256.Initialize(RandHelpers.GenerateRandomSeed());

    /// <summary>
    /// Set the seed of the global random generator.
    /// </summary>
    public static void SetRandomGenSeed(string seed)
    {
        _randomState = Xoshiro256.Initialize((ulong)seed.GetStableHashCode());
    }

    /// <summary>
    /// Set the seed of the global random generator.
    /// </summary>
    public static void SetRandomGenSeed(int seed)
    {
        _randomState = Xoshiro256.Initialize((ulong)seed);
    }

    /// <summary>
    /// Returns a randomly generated float between 0 and 1
    /// </summary>
    public static float Float()
    {
        return (float)Xoshiro256.NextDouble(ref _randomState);
    }

    /// <summary>
    /// Returns a randomly generated number between two numbers, inclusive on both ends.
    /// </summary>
    /// <param name="min">The lowest number that can be generated.</param>
    /// <param name="max">The highest number that can be generated.</param>
    public static int Int(int min, int max)
    {
        return Xoshiro256.NextInRange(ref _randomState, min, max);
    }

    /// <summary>
    /// Returns a random item from the array.
    /// </summary>
    public static T? ArrayItem<T>(T[] array)
    {
        if (array.Length == 0) return default;

        int rand = Int(0, array.Length - 1);
        return array[rand];
    }

    /// <inheritdoc cref="LocalRand.ArrayItem"/>
    public static T? ArrayItem<T>(IList<T> array)
    {
        if (array.Count == 0) return default;

        int rand = Int(0, array.Count- 1);
        return array[rand];
    }
}

public static class NetworkRand
{
    private static XoshiroState _randomState = Xoshiro256.Initialize(RandHelpers.GenerateRandomSeed());

    public static void SetNetworkRandState(ulong seed)
    {
        _randomState = Xoshiro256.Initialize(seed);
    }

    /// <inheritdoc cref="LocalRand.Float"/>
    public static float Float(string tag)
    {
        float rolled = (float)Xoshiro256.NextDouble(ref _randomState);
        Engine.Multiplayer.SendMessageToServer(NetworkMessageType.LockStepVerify, new LockStepVerify($"RandFloat {tag} - {rolled}"));
        return rolled;
    }

    /// <inheritdoc cref="LocalRand.Int"/>
    public static int Int(string tag, int min, int max)
    {
        int rolled = Xoshiro256.NextInRange(ref _randomState, min, max);
        Engine.Multiplayer.SendMessageToServer(NetworkMessageType.LockStepVerify, new LockStepVerify($"RandInt {tag} - {rolled}"));
        return rolled;
    }

    /// <inheritdoc cref="LocalRand.ArrayItem"/>
    public static T? ArrayItem<T>(string tag, T[] array)
    {
        if (array.Length == 0) return default;

        int rand = Int(tag, 0, array.Length - 1);
        return array[rand];
    }

    /// <inheritdoc cref="LocalRand.ArrayItem"/>
    public static T? ArrayItem<T>(string tag, IList<T> array)
    {
        if (array.Count == 0) return default;

        int rand = Int(tag, 0, array.Count - 1);
        return array[rand];
    }
}