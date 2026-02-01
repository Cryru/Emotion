#nullable enable

#region Using

using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;

#endregion

namespace Emotion.Standard;

/// <summary>
/// Various helper functions.
/// </summary>
public static class Helpers
{
    /// <summary>
    /// The assemblies of the program running (game, engine, main method containing assembly etc).
    /// Used to find embedded resources from, types referenced in serialization etc.
    /// </summary>
    public static Assembly[] AssociatedAssemblies = Array.Empty<Assembly>();

    public static void AddAssociatedAssembly(Assembly? ass)
    {
        if (ass == null) return;
        if (AssociatedAssemblies.IndexOf(ass) != -1) return;
        AssociatedAssemblies = AssociatedAssemblies.AddToArray(ass);
    }

    /// <summary>
    /// Regex for capturing Windows line endings.
    /// </summary>
    private static readonly Regex _newlineRegex = new Regex("\r\n", RegexOptions.Compiled);

    /// <summary>
    /// Replaces windows new lines with unix new lines.
    /// </summary>
    public static string NormalizeNewLines(string source)
    {
        return _newlineRegex.Replace(source, "\n");
    }

    /// <summary>
    /// Safely parses the text to an int. If the parse fails returns a default value.
    /// </summary>
    /// <param name="text">The text to parse to an int.</param>
    /// <param name="invalidValue">The value to return if the parsing fails. 0 by default.</param>
    /// <returns>The text parsed as an int, or a default value.</returns>
    public static int StringToInt(string text, int invalidValue = 0)
    {
        bool parsed = int.TryParse(text, out int result);
        return parsed ? result : invalidValue;
    }

    /// <summary>
    /// Set the seed of the global random generator.
    /// </summary>
    public static void SetRandomGenSeed(int seed)
    {
        LocalRand.SetRandomGenSeed(seed);
    }

    /// <summary>
    /// Set the seed of the global random generator.
    /// </summary>
    public static void SetRandomGenSeed(string seed)
    {
        LocalRand.SetRandomGenSeed(seed);
    }

    /// <summary>
    /// Returns a randomly generated number between two numbers, inclusive on both ends.
    /// </summary>
    /// <param name="min">The lowest number that can be generated.</param>
    /// <param name="max">The highest number that can be generated.</param>
    public static int GenerateRandomNumber(int min = 0, int max = 100)
    {
        return LocalRand.Int(min, max);
    }

    /// <summary>
    /// Returns a randomly generated float between 0 and 1
    /// </summary>
    public static float GenerateRandomFloat()
    {
        return LocalRand.Float();
    }

    /// <summary>
    /// Returns a random item from the array.
    /// </summary>
    public static T? GetRandomArrayItem<T>(T[] array)
    {
        return LocalRand.ArrayItem(array);
    }

    /// <inheritdoc cref="GetRandomArrayItem{T}(T[])" />
    public static T? GetRandomArrayItem<T>(IList<T> array)
    {
        return LocalRand.ArrayItem(array);
    }

    /// <inheritdoc cref="GetRandomArrayItem{T}(T[])" />

    public static T? GetRandomArrayItem<T>(T[] array, Random rng)
    {
        if (array.Length == 0) return default;

        int rand = rng.Next(0, array.Length);
        return array[rand];
    }

    /// <inheritdoc cref="GetRandomArrayItem{T}(T[])" />

    public static T? GetRandomArrayItem<T>(IList<T> array, Random rng, bool eject = false)
    {
        if (array.Count == 0) return default;

        int rand = rng.Next(0, array.Count);
        T item = array[rand];
        if (eject) array.RemoveAt(rand);
        return item;
    }

    public static T? GetWeightedRandomArrayItem<T>(IList<(int weight, T obj)> weights, Random? rng = null, bool eject = false, IList<T>? exceptions = null, IList<T>? whiteList = null)
    {
        if (weights.Count == 0) return default;
        if (weights.Count == 1) return weights[0].obj;

        rng ??= new Random();

        int total = 0;
        for (int i = 0; i < weights.Count; i++)
        {
            if (exceptions != null && exceptions.IndexOf(weights[i].obj) != -1) continue;

            total += weights[i].weight;
        }

        if (total == 0) return default;

        int rand = rng.Next(0, total);
        int sum = 0;
        for (int i = 0; i < weights.Count; i++)
        {
            (int weight, T obj) = weights[i];
            if (exceptions != null && exceptions.IndexOf(weights[i].obj) != -1) continue;
            if (whiteList != null && whiteList.IndexOf(weights[i].obj) == -1) continue;

            if (weight == 0) continue;

            sum += weight;
            if (rand < sum)
            {
                if (eject) weights.RemoveAt(i);
                return obj;
            }
        }

        Assert(false, "No weighted random?");
        return weights[0].obj;
    }

    private static Dictionary<string, int> _repeatRandomMap = new();

    /// <summary>
    /// Returns a randomly generated number which will be
    /// different to the number that was provided last time for this id.
    /// </summary>
    public static int GenerateRandomNumber(string idForUnique, int min = 0, int max = 100)
    {
        int num = LocalRand.Int(min, max);
        if (_repeatRandomMap.TryGetValue(idForUnique, out int lastRolled) && num == lastRolled)
        {
            num++;
            if (num == max) num = min;
        }

        _repeatRandomMap[idForUnique] = num;
        return num;
    }

    /// <summary>
    /// Compare two byte arrays. If the first n bytes of the "compare" array match, returns true.
    /// Where n is the length of the "compare" array.
    /// </summary>
    /// <param name="bytes">The bytes to compare against.</param>
    /// <param name="compare">The bytes to compare.</param>
    public static bool BytesEqual(ReadOnlySpan<byte> bytes, ReadOnlySpan<byte> compare)
    {
        // ReSharper disable once LoopCanBeConvertedToQuery
        for (var i = 0; i < compare.Length; i++)
        {
            if (compare[i] != bytes[i]) return false;
        }

        return true;
    }

    private static int _oneKb = 1000;
    private static int _oneMb = 1000 * _oneKb;
    private static int _oneGb = 1000 * _oneMb;

    /// <summary>
    /// Format a byte amount as a human readable byte size.
    /// ex. 1200 = 1.20 KB
    /// </summary>
    /// <param name="bytes"></param>
    /// <returns></returns>
    public static string FormatByteAmountAsString(long bytes)
    {
        if (bytes < _oneKb / 2) return $"{bytes} B";
        if (bytes < _oneMb / 2) return $"{(float) bytes / _oneKb:0.00} Kb";
        if (bytes < _oneGb / 2) return $"{(float) bytes / _oneMb:0.00} Mb";
        return $"{(float) bytes / _oneGb:0.00} Gb";
    }

    public enum ByteName
    {
        Byte,
        Kilobyte,
        Megabyte,
        Gigabyte
    }

    public static double GetNumberInByteFormat(double srcNum, ByteName srcFormat, ByteName dstFormat)
    {
        double bytes = srcNum;
        switch(srcFormat)
        {
            case ByteName.Gigabyte:
                bytes = srcNum * _oneGb;
                break;
            case ByteName.Megabyte:
                bytes = srcNum * _oneMb;
                break;
            case ByteName.Kilobyte:
                bytes = srcNum * _oneKb;
                break;
        }

        switch(dstFormat)
        {
            case ByteName.Gigabyte:
                return bytes / _oneGb;
            case ByteName.Megabyte:
                return bytes / _oneMb;
            case ByteName.Kilobyte:
                return bytes / _oneKb;
        }

        return bytes;
    }

    /// <summary>
    /// Get the average value in a float array.
    /// </summary>
    public static float GetArrayAverage(float[] arr)
    {
        float sum = 0;
        for (var i = 0; i < arr.Length; i++)
        {
            sum += arr[i];
        }

        return sum / arr.Length;
    }

    public static bool AreObjectsEqual<T>(T a, T b)
    {
        return EqualityComparer<T>.Default.Equals(a, b);
    }

    /// <summary>
    /// Ensures the input string isn't contained in the taken set by
    /// appending an incrementing number to it until it doesn't match.
    /// </summary>
    public static string EnsureNoStringCollision(HashSet<string> takenSet, string input)
    {
        string testing = input;
        int counter = 1;
        while (takenSet.Contains(testing))
        {
            testing = $"{input}_{counter}";
            counter++;
        }

        return testing;
    }

    public static string GetRandomReadableString(int length)
    {
        Random randomness = new Random();
        StringBuilder builder = new StringBuilder();
        for (int i = 0; i < length; i++)
        {
            char ch = (char)randomness.NextInclusive(97, 122);
            if (randomness.NextInclusive(0, 1) == 0)
                ch = char.ToUpperInvariant(ch);
            builder.Append(ch);
        }
        return builder.ToString();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T[] SafeForEachArray<T>(T[]? arr)
    {
        if (arr == null) return Array.Empty<T>();
        return arr;
    }

    public static readonly UTF8Encoding UTF8Encoder = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false, throwOnInvalidBytes: false);

    // https://en.wikipedia.org/wiki/Byte_order_mark
    private static readonly byte[] Utf16Le = { 0xFF, 0xFE };
    private static readonly byte[] Utf8Le = { 0xEF, 0xBB, 0xBF };
    private static readonly byte[] Utf32Le = { 0xFF, 0xFE, 0, 0 };
    private static readonly byte[] Utf16Be = { 0xFE, 0xFF };

    // <?xml search
    private static readonly byte[] Utf16LeAlt = { 0x3C, 0, 0x3F, 0 };
    private static readonly byte[] Utf8LeAlt = { 0x3C, 0x3F, 0x78, 0x6D };
    private static readonly byte[] Utf32LeAlt = { 0x3C, 0, 0, 0 };
    private static readonly byte[] Utf16BeAlt = { 0, 0x3C, 00, 0x3F };

    /// <summary>
    /// Guess the string encoding of the data array.
    /// https://stackoverflow.com/questions/581318/c-sharp-detect-xml-encoding-from-byte-array
    /// </summary>
    public static Encoding GuessStringEncoding(ReadOnlySpan<byte> data)
    {
        return GuessStringEncoding(data, out byte[] _);
    }

    public static Encoding GuessStringEncoding(ReadOnlySpan<byte> data, out byte[] header)
    {
        // "utf-16" - Unicode UTF-16, little endian byte order
        if (data.StartsWith(Utf16LeAlt))
        {
            header = Array.Empty<byte>();
            return Encoding.Unicode;
        }

        if (data.Length >= 4 && data[2] == 0 ^ data[3] == 0 && data.StartsWith(Utf16Le))
        {
            header = Utf16Le;
            return Encoding.Unicode;
        }

        // "utf-8" - Unicode (UTF-8)
        if (data.StartsWith(Utf8LeAlt))
        {
            header = Array.Empty<byte>();
            return Encoding.UTF8;
        }

        if (data.StartsWith(Utf8Le))
        {
            header = Utf8Le;
            return Encoding.UTF8;
        }

        // "utf-32" - Unicode UTF-32, little endian byte order
        if (data.StartsWith(Utf32LeAlt))
        {
            header = Array.Empty<byte>();
            return Encoding.UTF32;
        }

        if (data.StartsWith(Utf32Le))
        {
            header = Array.Empty<byte>();
            return Encoding.UTF32;
        }

        // "unicodeFFFE" - Unicode UTF-16, big endian byte order
        if (data.StartsWith(Utf16BeAlt))
        {
            header = Array.Empty<byte>();
            return Encoding.BigEndianUnicode;
        }

        if (data.StartsWith(Utf16Be))
        {
            header = Utf16Be;
            return Encoding.BigEndianUnicode;
        }

        header = Array.Empty<byte>();
        return Encoding.UTF8;
    }
}