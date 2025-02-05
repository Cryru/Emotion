#nullable enable

using Standart.Hash.xxHash;
using System.Runtime.InteropServices;

namespace System;

public static class SpanExtensions
{
    public static unsafe string AsString(this ReadOnlySpan<char> source)
    {
        string result = new string(' ', source.Length);
        fixed (char* dest = result, src = &MemoryMarshal.GetReference(source))
        {
            for (int i = 0; i < source.Length; i++)
            {
                *(dest + i) = *(src + i);
            }
        }

#if DEBUG
        // todo: can this be .ToString()?
        var str = source.ToString();
        Assert(str == result);
#endif

        return result;
    }

    public static int GetStableHashCode(this ReadOnlySpan<byte> span)
    {
        return (int)xxHash32.ComputeHash(span, span.Length);
    }
}