#nullable enable

using System.Globalization;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;

namespace Emotion.WIPUpdates.Grids;

public static class NumericDataPacking
{
    public static T[] UnpackData<T, TNumeric>(ReadOnlySpan<char> data)
        where T : struct
        where TNumeric : struct, INumber<TNumeric>
    {
#if DEBUG
        Assert(Unsafe.SizeOf<TNumeric>() == Unsafe.SizeOf<T>());
#endif

        // First pass - Count characters, including packed.
        var chars = 0;
        var lastSepIdx = 0;
        var charCount = 1;
        for (var i = 0; i < data.Length; i++)
        {
            char c = data[i];
            if (c == 'x')
            {
                ReadOnlySpan<char> sinceLast = data.Slice(lastSepIdx, i - lastSepIdx);
                if (int.TryParse(sinceLast, out int countPacked)) charCount = countPacked;
            }
            else if (c == ',')
            {
                chars += charCount;
                charCount = 1;
                lastSepIdx = i + 1;
            }
        }

        chars += charCount;

        // Second pass, unpack.
        // (we support unpacking as a type that can be reinterpret cast to a numeric type)
        T[] unpackedDataUnderlyingType = new T[chars];
        Span<T> unpackedDataUnderlyingSpan = (Span<T>)unpackedDataUnderlyingType;
        Span<TNumeric> unpackedData = MemoryMarshal.Cast<T, TNumeric>(unpackedDataUnderlyingSpan);

        var arrayPtr = 0;
        lastSepIdx = 0;
        charCount = 1;
        for (var i = 0; i < data.Length; i++)
        {
            char c = data[i];
            if (c == 'x')
            {
                ReadOnlySpan<char> sinceLast = data.Slice(lastSepIdx, i - lastSepIdx);
                if (int.TryParse(sinceLast, out int countPacked))
                {
                    charCount = countPacked;
                    lastSepIdx = i + 1;
                }
            }
            else if (c == ',' || i == data.Length - 1)
            {
                // Dumping last character, pretend the index is after the string so we
                // read the final char below.
                if (i == data.Length - 1) i++;

                // Get tile value.
                ReadOnlySpan<char> sinceLast = data.Slice(lastSepIdx, i - lastSepIdx);
                TNumeric.TryParse(sinceLast, System.Globalization.NumberStyles.Any, CultureInfo.InvariantCulture, out TNumeric value);

                for (var j = 0; j < charCount; j++)
                {
                    unpackedData[arrayPtr] = value;
                    arrayPtr++;
                }

                charCount = 1;
                lastSepIdx = i + 1;
            }
        }

        return unpackedDataUnderlyingType;
    }

    public static string PackData<T, TNumeric>(Span<T> dataT)
        where T : struct
        where TNumeric : struct, INumber<TNumeric>
    {
#if DEBUG
        Assert(Unsafe.SizeOf<TNumeric>() == Unsafe.SizeOf<T>());
#endif

        if (dataT.Length == 0) return string.Empty;

        Span<TNumeric> data = MemoryMarshal.Cast<T, TNumeric>(dataT);
        var b = new StringBuilder(data.Length * 2 + data.Length - 1);

        TNumeric lastNumber = data[0];
        uint lastNumberCount = 1;
        var firstAppended = false;
        for (var i = 1; i <= data.Length; i++)
        {
            // There is an extra loop to dump last number.
            TNumeric num = TNumeric.Zero;
            if (i != data.Length)
            {
                num = data[i];
                // Same number as before, increment counter.
                if (num == lastNumber)
                {
                    lastNumberCount++;
                    continue;
                }
            }

            if (firstAppended) b.Append(',');
            if (lastNumberCount == 1)
            {
                // "0"
                b.Append(lastNumber);
            }
            else
            {
                // "2x0" = "0, 0"
                b.Append(lastNumberCount);
                b.Append('x');
                b.Append(lastNumber);
            }

            lastNumber = num;
            lastNumberCount = 1;
            firstAppended = true;
        }

        return b.ToString();
    }
}
