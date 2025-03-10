#nullable enable

#pragma warning disable CS9080 // Use of variable in this context may expose referenced variables outside of their declaration scope

using Emotion.Standard.Reflector.Handlers;
using Emotion.Standard.Reflector;
using System.Text;
using Emotion.Standard.OptimizedStringReadWrite;

namespace Emotion.Serialization.JSON;

public struct JSONConfig
{
    public bool Pretty = true;
    public int Indentation = 2;

    public JSONConfig()
    {
    }
}

public static class JSONSerialization
{
    private static bool DEBUG = true;

    public static T? From<T>(string json)
    {
        return From<T>(json.AsSpan());
    }

    public static T? From<T>(ReadOnlySpan<char> jsonDataUtf16)
    {
        var reader = new ValueStringReader(jsonDataUtf16);
        return From<T>(ref reader);
    }

    public static T? From<T>(ReadOnlySpan<byte> jsonDataUtf8)
    {
        var reader = new ValueStringReader(jsonDataUtf8);
        return From<T>(ref reader);
    }

    public unsafe static T? From<T>(ref ValueStringReader reader)
    {
        ReflectorTypeHandlerBase<T>? typeHandler = ReflectorEngine.GetTypeHandler<T>();
        if (typeHandler == null) return default;

        IGenericReflectorComplexTypeHandler? complexHandler = typeHandler as IGenericReflectorComplexTypeHandler;
        if (complexHandler == null) return default;

        Type requestedType = typeof(T);
        Span<char> readMemory = stackalloc char[512];
        if (!reader.MoveCursorToNextOccuranceOfChar('{')) return default;

        object? result = ReadObject(ref reader, readMemory, complexHandler);
        return (T?)result;
    }

    private static object? ReadObject(
        ref ValueStringReader reader,
        Span<char> scratchMemory,
        IGenericReflectorComplexTypeHandler? objHandler
    )
    {
        char c = reader.ReadNextChar();
        if (c != '{') return null;

        object? obj = null;
        if (objHandler != null)
            obj = objHandler.CreateNew();

        bool firstLoop = true;

        // Read key-value pairs until closing of object.
        while (true)
        {
            // Go to next key-value (if any)
            char nextNonWhitespace = reader.MoveCursorToNextOccuranceOfNotWhitespace();
            if (nextNonWhitespace == '}')
            {
                reader.ReadNextChar();
                break;
            }
            else if (firstLoop && nextNonWhitespace == '\"')
            {
                // Thats fine - it's the first tag
            }
            else if (nextNonWhitespace != ',')
            {
                return null;
            }
            firstLoop = false;

            // Start reading tag
            if (!reader.MoveCursorToNextOccuranceOfChar('\"')) return null;
            reader.ReadNextChar();
            int charsWritten = reader.ReadToNextOccuranceofChar('\"', scratchMemory);
            if (charsWritten == 0) return null;

            Span<char> nextTag = scratchMemory.Slice(0, charsWritten);
            ComplexTypeHandlerMember? member = null;

            if (objHandler != null)
            {
                if (nextTag.SequenceEqual("nodes"))
                {
                    bool a = true;
                }

                // If the first character is lowercase (as is usually the case with JSON)
                // attempt to get a member first by making it uppercased.
                bool uppercased = false;
                if (nextTag[0] > (char)90)
                {
                    nextTag[0] -= (char)32;
                    uppercased = true;
                }

                int tagNameHash = nextTag.GetStableHashCode();
                member = objHandler.GetMemberByName(tagNameHash);

                // Ok try to read it now lowercased
                if (member == null && uppercased)
                {
                    nextTag[0] += (char)32;
                    tagNameHash = nextTag.GetStableHashCode();
                    member = objHandler.GetMemberByName(tagNameHash);
                    uppercased = false;
                }
            }

            // Go to value delim
            if (!reader.MoveCursorToNextOccuranceOfChar(':')) return null;
            reader.ReadNextChar();

            IGenericReflectorTypeHandler? memberHandler = member?.GetTypeHandler();
            object? readValue = ReadJSONValue(ref reader, scratchMemory, memberHandler);
            if (member != null && obj != null)
                member.SetValueInComplexObject(obj, readValue);
        }

        return obj;
    }

    private static bool ReadString(
        ref ValueStringReader reader,
        Span<char> scratchMemory,
        IGenericReflectorComplexTypeHandler? parentObjHandler
    )
    {
        return true;
    }

    private static bool ReadArray(
        ref ValueStringReader reader,
        Span<char> scratchMemory,
        IGenericReflectorTypeHandler? typeHandler
    )
    {
        char c = reader.ReadNextChar();
        if (c != '[') return false;

        bool firstLoop = true;

        while (true)
        {
            char nextNonWhitespace = reader.MoveCursorToNextOccuranceOfNotWhitespace();
            if (nextNonWhitespace == ']')
            {
                reader.ReadNextChar();
                break;
            }
            else if (firstLoop)
            {
                // That's ok - it's the first one so no need for a comma
            }
            else if (nextNonWhitespace == ',')
            {
                reader.ReadNextChar(); // Skip comma (todo: trailing comma?)
            }
            else 
            {
                return false;
            }
            firstLoop = false;

            object? readValue = ReadJSONValue(ref reader, scratchMemory, typeHandler);
         
        }

        return true;
    }

    private static object? ReadJSONValue(
        ref ValueStringReader reader,
        Span<char> scratchMemory,
        IGenericReflectorTypeHandler? typeHandlerOfHolder
    )
    {
        // Peak the next character that isnt whitespace
        char charAfterWhitespace = reader.MoveCursorToNextOccuranceOfNotWhitespace();

        // Object value opened
        if (charAfterWhitespace == '{')
        {
            IGenericReflectorComplexTypeHandler? handler = typeHandlerOfHolder as IGenericReflectorComplexTypeHandler;
            return ReadObject(ref reader, scratchMemory, handler);
        }
        // Array value opened
        else if (charAfterWhitespace == '[')
        {
            ReadArray(ref reader, scratchMemory, typeHandlerOfHolder);
        }
        // String value opened
        else if (charAfterWhitespace == '\"')
        {
            reader.ReadNextChar();
            int charsWritten = reader.ReadToNextOccuranceofChar('\"', scratchMemory);
            if (charsWritten == 0) return null;

            // todo: big strings will fuck us up :/
            Span<char> stringContent = scratchMemory.Slice(0, charsWritten);
            bool a = true;

            char closing = reader.ReadNextChar();
            if (closing != '\"') return null;
        }
        else if (char.IsNumber(charAfterWhitespace) || charAfterWhitespace == '-')
        {
            int writeChar = 0;
            scratchMemory[0] = charAfterWhitespace;
            writeChar++;
            reader.ReadNextChar();

            while (true)
            {
                char c = reader.PeekCurrentChar();
                if (char.IsNumber(c) || c == '.' || c == 'e' || c == '-')
                {
                    scratchMemory[writeChar] = c;
                    writeChar++;

                    reader.ReadNextChar();
                }
                else
                {
                    break;
                }
            }

            // In JS (which where JSON is mostly used) the number type is double.
            if (!double.TryParse(scratchMemory.Slice(0, writeChar), out double resultVal)) resultVal = 0;
            return resultVal;
        }
        else if (charAfterWhitespace == 't' || charAfterWhitespace == 'f') // true or false
        {
            reader.ReadToNextOccuranceofChar('e', scratchMemory);
            reader.ReadNextChar(); // next
            return charAfterWhitespace == 't';
        }

        return null;
    }

    public static string To<T>(T obj)
    {
        return To<T>(obj, new JSONConfig());
    }

    public static string To<T>(T obj, JSONConfig config)
    {
        var builder = new StringBuilder();
        var reader = new ValueStringWriter(builder);
        if (To(obj, config, ref reader) == -1) return string.Empty;
        return builder.ToString();
    }

    public static int To<T>(T obj, JSONConfig config, Span<char> jsonDataUtf16)
    {
        var reader = new ValueStringWriter(jsonDataUtf16);
        return To(obj, config, ref reader);
    }

    public static int To<T>(T obj, JSONConfig config, Span<byte> jsonDataUtf8)
    {
        var reader = new ValueStringWriter(jsonDataUtf8);
        return To(obj, config, ref reader);
    }

    public static int To<T>(T obj, JSONConfig config, ref ValueStringWriter writer)
    {
        return 0;
    }
}
