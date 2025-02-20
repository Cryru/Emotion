﻿using Emotion.Editor;
using System.Text;

#nullable enable

namespace Emotion.Game.World.Grid;

public class PackedNumericMapGrid<T> : MapGrid<T> where T : INumber<T>
{
    [DontShowInEditor]
    public string DataPacked { get => PackData(_data); set => _data = UnpackData(value); }

    //public int DataStride { get; set; } = -1;

    #region Data Packing

    protected T[] UnpackData(string data)
    {
        // First pass - Count characters, including packed.
        var chars = 0;
        var lastSepIdx = 0;
        var charCount = 1;
        for (var i = 0; i < data.Length; i++)
        {
            char c = data[i];
            if (c == 'x')
            {
                ReadOnlySpan<char> sinceLast = data.AsSpan(lastSepIdx, i - lastSepIdx);
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
        var unpackedData = new T[chars];
        var arrayPtr = 0;
        lastSepIdx = 0;
        charCount = 1;
        for (var i = 0; i < data.Length; i++)
        {
            char c = data[i];
            if (c == 'x')
            {
                ReadOnlySpan<char> sinceLast = data.AsSpan(lastSepIdx, i - lastSepIdx);
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
                ReadOnlySpan<char> sinceLast = data.AsSpan(lastSepIdx, i - lastSepIdx);
                T.TryParse(sinceLast, System.Globalization.NumberStyles.Any, null, out T? value);
                AssertNotNull(value);

                for (var j = 0; j < charCount; j++)
                {
                    unpackedData[arrayPtr] = value;
                    arrayPtr++;
                }

                charCount = 1;
                lastSepIdx = i + 1;
            }
        }

        return unpackedData;
    }

    protected string PackData(T[]? data)
    {
        if (data == null || data.Length == 0) return "";

        var b = new StringBuilder(data.Length * 2 + data.Length - 1);

        T lastNumber = data[0];
        uint lastNumberCount = 1;
        var firstAppended = false;
        for (var i = 1; i <= data.Length; i++)
        {
            // There is an extra loop to dump last number.
            T num = T.Zero;
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

            if (firstAppended) b.Append(",");
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

    #endregion
}
