#nullable enable

using Standart.Hash.xxHash;
using System.Runtime.InteropServices;
using System.Xml.Linq;

namespace Emotion.Standard.Extensions;

public static class SpanExtensions
{
    public static unsafe string AsLowerCaseString(this ReadOnlySpan<char> source)
    {
        Span<char> nameLower = stackalloc char[source.Length];
        source.ToLowerInvariant(nameLower);
        return nameLower.ToString();
    }

    public static int GetIndexOfFollowedByChar(this ReadOnlySpan<char> source, ReadOnlySpan<char> indexOf, char followedBy)
    {
        int searchUpTo = 0;
        int idx = 0;
        while (idx != -1)
        {
            idx = source.Slice(searchUpTo).IndexOf(indexOf);
            if (idx == -1)
                break;

            if (source[idx + indexOf.Length] == followedBy)
                return searchUpTo + idx;
            searchUpTo = idx + indexOf.Length;
        }

        return -1;
    }

    public static int GetStableHashCode(this Span<byte> span)
    {
        return (int)xxHash32.ComputeHash(span, span.Length);
    }

    public static int GetStableHashCode(this ReadOnlySpan<byte> span)
    {
        return (int)xxHash32.ComputeHash(span, span.Length);
    }

    public static int GetStableHashCode(this ReadOnlySpan<char> span)
    {
        ReadOnlySpan<byte> asBytes = MemoryMarshal.Cast<char, byte>(span);
        return (int)xxHash32.ComputeHash(asBytes, asBytes.Length);
    }

    public static int GetStableHashCode(this Span<char> span)
    {
        ReadOnlySpan<byte> asBytes = MemoryMarshal.Cast<char, byte>(span);
        return (int)xxHash32.ComputeHash(asBytes, asBytes.Length);
    }
}