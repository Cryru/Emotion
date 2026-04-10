#nullable enable

using Emotion.ThirdParty;
using System.Runtime.CompilerServices;

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
    /// <inheritdoc cref="LocalRand.ArrayItem"/>
    public static T? ArrayItem<T>(T[] array)
    {
        return ArrayItemInternal<T, T[]>(array);
    }

    /// <inheritdoc cref="LocalRand.ArrayItem"/>
    public static T? ArrayItem<T>(List<T> array)
    {
        return ArrayItemInternal<T, List<T>>(array);
    }

    /// <inheritdoc cref="LocalRand.ArrayItem"/>
    public static T? ArrayItem<T>(IReadOnlyList<T> array)
    {
        return ArrayItemInternal<T, IReadOnlyList<T>>(array);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static T? ArrayItemInternal<T, TList>(TList array)
        where TList : IReadOnlyList<T>
    {
        if (array.Count == 0) return default;

        int rand = Int(0, array.Count - 1);
        return array[rand];
    }

    /// <inheritdoc cref="LocalRand.ArrayItem"/>
    public static T? ArrayItemExcept<T>(T?[] array, params ReadOnlySpan<T?> exceptions)
    {
        return ArrayItemExceptInternal(array, exceptions);
    }

    /// <inheritdoc cref="LocalRand.ArrayItem"/>
    public static T? ArrayItemExcept<T>(List<T?> array, params ReadOnlySpan<T?> exceptions)
    {
        return ArrayItemExceptInternal(array, exceptions);
    }

    /// <inheritdoc cref="LocalRand.ArrayItem"/>
    public static T? ArrayItemExcept<T>(IReadOnlyList<T?> array, params ReadOnlySpan<T?> exceptions)
    {
        return ArrayItemExceptInternal(array, exceptions);
    }

    /// <inheritdoc cref="LocalRand.ArrayItem"/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static T? ArrayItemExceptInternal<T, TList>(TList array, params ReadOnlySpan<T?> exceptions)
        where TList : IReadOnlyList<T?>
    {
        int total = array.Count;
        if (total == 0) return default;

        int start = Int(0, total - 1);

        for (int i = 0; i < total; i++)
        {
            int currentIndex = (start + i) % total;
            T? picked = array[currentIndex];

            bool foundInExceptions = false;
            for (int ii = 0; ii < exceptions.Length; ii++)
            {
                T? exception = exceptions[ii];
                if (EqualityComparer<T>.Default.Equals(picked, exception))
                {
                    foundInExceptions = true;
                    break;
                }
            }
            if (!foundInExceptions)
                return picked;
        }

        // All items are present in the exceptions?
        return default;
    }

    public static T? ListItemAndRemove<T>(IList<T> list, bool keepOrder = false)
    {
        if (list.Count == 0) return default;

        int rand = Int(0, list.Count - 1);
        T item = list[rand];

        if (keepOrder)
        {
            list.RemoveAt(rand);
        }
        else
        {
            (list[^1], list[rand]) = (list[rand], list[^1]);
            list.RemoveAt(list.Count - 1);
        }
        
        return item;
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
        Engine.Multiplayer.LockStepVerify($"RandFloat {tag} - {rolled}");
        return rolled;
    }

    /// <inheritdoc cref="LocalRand.Int"/>
    public static int Int(string tag, int min, int max)
    {
        int rolled = Xoshiro256.NextInRange(ref _randomState, min, max);
        Engine.Multiplayer.LockStepVerify($"RandInt {tag} - {rolled}");

        return rolled;
    }

    /// <inheritdoc cref="LocalRand.ArrayItem"/>
    public static T? ArrayItem<T>(string tag, T[] array)
    {
        return ArrayItemInternal<T, T[]>(tag, array);
    }

    /// <inheritdoc cref="LocalRand.ArrayItem"/>
    public static T? ArrayItem<T>(string tag, List<T> array)
    {
        return ArrayItemInternal<T, List<T>>(tag, array);
    }

    /// <inheritdoc cref="LocalRand.ArrayItem"/>
    public static T? ArrayItem<T>(string tag, IReadOnlyList<T> array)
    {
        return ArrayItemInternal<T, IReadOnlyList<T>>(tag, array);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static T? ArrayItemInternal<T, TList>(string tag, TList array)
        where TList : IReadOnlyList<T>
    {
        if (array.Count == 0) return default;

        int rand = Int(tag, 0, array.Count - 1);
        return array[rand];
    }

    /// <inheritdoc cref="LocalRand.ArrayItem"/>
    public static T? ArrayItemExcept<T>(string tag, T?[] array, params ReadOnlySpan<T?> exceptions)
    {
        return ArrayItemExceptInternal(tag, array, exceptions);
    }

    /// <inheritdoc cref="LocalRand.ArrayItem"/>
    public static T? ArrayItemExcept<T>(string tag, List<T?> array, params ReadOnlySpan<T?> exceptions)
    {
        return ArrayItemExceptInternal(tag, array, exceptions);
    }

    /// <inheritdoc cref="LocalRand.ArrayItem"/>
    public static T? ArrayItemExcept<T>(string tag, IReadOnlyList<T?> array, params ReadOnlySpan<T?> exceptions)
    {
        return ArrayItemExceptInternal(tag, array, exceptions);
    }

    /// <inheritdoc cref="LocalRand.ArrayItem"/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static T? ArrayItemExceptInternal<T, TList>(string tag, TList array, params ReadOnlySpan<T?> exceptions)
        where TList : IReadOnlyList<T?>
    {
        int total = array.Count;
        if (total == 0)
            return default;

        int start = Int(tag, 0, total - 1);

        for (int i = 0; i < total; i++)
        {
            int currentIndex = (start + i) % total;
            T? picked = array[currentIndex];

            bool foundInExceptions = false;
            for (int ii = 0; ii < exceptions.Length; ii++)
            {
                T? exception = exceptions[ii];
                if (EqualityComparer<T>.Default.Equals(picked, exception))
                {
                    foundInExceptions = true;
                    break;
                }
            }
            if (!foundInExceptions)
                return picked;
        }

        // All items are present in the exceptions?
        return default;
    }

    public unsafe static T? ArrayItemFiltered<T>(string tag, List<T> array, delegate*<T, bool> filterFunc)
    {
        int total = array.Count;
        if (total == 0)
            return default;

        int start = Int(tag, 0, total - 1);
        for (int i = 0; i < total; i++)
        {
            int currentIndex = (start + i) % total;
            T? picked = array[currentIndex];
            if (filterFunc(picked))
                return picked;
        }

        return default;
    }
}