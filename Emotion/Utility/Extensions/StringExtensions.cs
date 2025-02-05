#nullable enable

using System.Runtime.InteropServices;
using System.Security.Cryptography;

namespace System;

public static class StringExtensions
{
    private static MD5 _md5Hasher = MD5.Create();

    public static int GetStableHashCode(this string str)
    {
        ReadOnlySpan<byte> stringAsBytes = MemoryMarshal.Cast<char, byte>(str);
        return stringAsBytes.GetStableHashCode();
    }

    public static int GetStableHashCodeASCII(this string str)
    {
        Span<byte> asciiBytes = stackalloc byte[str.Length];
        int bytes = System.Text.Encoding.ASCII.GetBytes(str, asciiBytes);
        ReadOnlySpan<byte> stringAsASCIIBytes = asciiBytes.Slice(0, bytes);
        return stringAsASCIIBytes.GetStableHashCode();
    }
}