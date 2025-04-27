#nullable enable

#pragma warning disable CS9080 // Use of variable in this context may expose referenced variables outside of their declaration scope

using Emotion.Standard.Reflector.Handlers;
using Emotion.Standard.Reflector;
using Emotion.Utility;
using System.Buffers;
using System.Text.Json;
using System.Diagnostics.CodeAnalysis;
using Emotion.Standard.Reflector.Handlers.Base;

namespace Emotion.Serialization.JSON;

public static class JSONSerialization
{
    // worst case scneario a single utf16 character could be expanded to 3 utf8 bytes.
    private const int MAX_STACKALLOC = 256;
    private const int UTF16_TO_8_MAX_FACTOR = 3;

    public static T? From<T>(string json)
    {
        return From<T>(json.AsSpan());
    }

    public static T? From<T>([StringSyntax(StringSyntaxAttribute.Json)] ReadOnlySpan<byte> utf8Data)
    {
        T? val = default;

        try
        {
            Utf8JsonReader reader = new Utf8JsonReader(utf8Data);
            val = InternalFrom<T>(ref reader);
        }
        catch
        {
            // Prevent crashing
        }

        return val;
    }

    public static T? From<T>([StringSyntax(StringSyntaxAttribute.Json)] ReadOnlySpan<char> utf16str)
    {
        byte[]? poolArray = null;
        Span<byte> utf8 =
            // Use stack memory if the string is not huge.
            utf16str.Length <= (MAX_STACKALLOC / UTF16_TO_8_MAX_FACTOR) ? stackalloc byte[MAX_STACKALLOC] :
            // Use a pooled array to avoid GC.
            poolArray = ArrayPool<byte>.Shared.Rent(utf16str.Length * UTF16_TO_8_MAX_FACTOR);

        T? val = default;

        try
        {
            int actualByteCount = Helpers.UTF8Encoder.GetBytes(utf16str, utf8);
            utf8 = utf8.Slice(0, actualByteCount);

            Utf8JsonReader reader = new Utf8JsonReader(utf8);
            val = InternalFrom<T>(ref reader);
        }
        finally
        {
            // Make sure array is returned and prevent crashing
            if (poolArray != null)
            {
                utf8.Clear();
                ArrayPool<byte>.Shared.Return(poolArray);
            }
        }

        return val;
    }

    private static T? InternalFrom<T>(ref Utf8JsonReader reader)
    {
        ReflectorTypeHandlerBase<T>? typeHandler = ReflectorEngine.GetTypeHandler<T>();
        if (typeHandler == null) return default;

        ComplexTypeHandler<T>? complexHandler = typeHandler as ComplexTypeHandler<T>;
        if (complexHandler == null) return default;

        return complexHandler.ParseFromJSON(ref reader);
    }

    public static string? To<T>(T obj)
    {
        // temp
        return System.Text.Json.JsonSerializer.Serialize<T>(obj);
    }
}
