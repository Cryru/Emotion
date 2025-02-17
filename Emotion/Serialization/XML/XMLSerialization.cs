#nullable enable

#pragma warning disable CS9080 // Use of variable in this context may expose referenced variables outside of their declaration scope

using Emotion.Standard.Reflector.Handlers;
using Emotion.Standard.Reflector;
using System.Text;
using Emotion.Standard.OptimizedStringReadWrite;

namespace Emotion.Serialization.XML;

public struct XMLConfig
{
    public bool UseXMLHeader = true;
    public bool Pretty = true;
    public int Indentation = 2;

    public XMLConfig()
    {
    }
}

public static class XMLSerialization
{
    public static string XMLHeader = "<?xml version=\"1.0\"?>";

    public static T? From<T>(string xml)
    {
        return From<T>(xml.AsSpan());
    }

    public static T? From<T>(ReadOnlySpan<char> xmlDataUtf16)
    {
        var reader = new ValueStringReader(xmlDataUtf16);
        return From<T>(ref reader);
    }

    public static T? From<T>(ReadOnlySpan<byte> xmlDataUtf8)
    {
        var reader = new ValueStringReader(xmlDataUtf8);
        return From<T>(ref reader);
    }

    public unsafe static T? From<T>(ref ValueStringReader reader)
    {
        Type requestedType = typeof(T);

        if (!reader.MoveCursorToNextOccuranceOfChar('<')) return default;

        bool hasXMLHeader = reader.PeekNextChar() == '?';
        if (hasXMLHeader)
        {
            if (!reader.MoveCursorToNextOccuranceOfChar('>')) return default;
            if (!reader.MoveCursorToNextOccuranceOfChar('<')) return default;
        }

        // Start reading tag.
        {
            char c = reader.ReadNextChar();
            if (c != '<') return default;
        }

        // Read tag content
        Span<char> readMemory = stackalloc char[128];
        int charsWritten = reader.ReadToNextOccuranceofChar('>', readMemory);
        if (charsWritten == 0) return default;

        Span<char> nextTag = readMemory.Slice(0, charsWritten);
        IGenericReflectorTypeHandler? typeHandler = ReflectorEngine.GetTypeHandlerByName(nextTag);
        if (typeHandler == null) return default;

        if (!typeHandler.IsTypeAssignableTo(requestedType))
        {
            Engine.Log.Warning($"Tried to deserialize into {requestedType.Name} type but the document is of type {typeHandler.Type.Name}", "XML");
            return default;
        }

        // Fast path for primitives
        if (typeHandler.CanGetOrParseValueAsString)
        {
            // Read end of this tag
            char c = reader.ReadNextChar();
            if (c != '>') return default;

            // Read value of data between tags.
            charsWritten = reader.ReadToNextOccuranceofChar('<', readMemory);
            if (charsWritten == 0) return default;

            Span<char> tagValue = readMemory.Slice(0, charsWritten);

            if (!typeHandler.ParseValueFromStringGeneric(tagValue, out object? resp)) return default;
            return (T?) resp;
        }

        return default;
    }

    public static string To<T>(T obj)
    {
        return To<T>(obj, new XMLConfig());
    }

    public static string To<T>(T obj, XMLConfig config)
    {
        var builder = new StringBuilder();
        var reader = new ValueStringWriter(builder);
        if (To(obj, config, ref reader) == -1) return string.Empty;
        return builder.ToString();
    }

    public static int To<T>(T obj, XMLConfig config, Span<char> xmlDataUtf16)
    {
        var reader = new ValueStringWriter(xmlDataUtf16);
        return To(obj, config, ref reader);
    }

    public static int To<T>(T obj, XMLConfig config, Span<byte> xmlDataUtf8)
    {
        var reader = new ValueStringWriter(xmlDataUtf8);
        return To(obj, config, ref reader);
    }

    public static int To<T>(T obj, XMLConfig config, ref ValueStringWriter writer)
    {
        if (config.UseXMLHeader)
        {
            if (!writer.WriteString(XMLHeader)) return -1;
            if (config.Pretty && !writer.WriteChar('\n')) return -1;
        }

        ReflectorTypeHandlerBase<T>? typeHandler = ReflectorEngine.GetTypeHandler<T>();
        if (typeHandler == null) return -1;

        if (!writer.WriteChar('<')) return -1;
        if (!writer.WriteString(typeHandler.Type.Name)) return -1;
        if (!writer.WriteChar('>')) return -1;

        // The base type can be written as a string directly!
        if (typeHandler.CanGetOrParseValueAsString)
        {
            bool written = typeHandler.WriteValueAsString(ref writer, obj);
            if (!written) return -1;
        }
        else if (typeHandler is IGenericReflectorComplexTypeHandler complexTypeHandler)
        {
            bool written = WriteComplexType(complexTypeHandler, ref writer, config, obj, config.Indentation);
            if (!written) return -1;
        }

        if (!writer.WriteString("</")) return -1;
        if (!writer.WriteString(typeHandler.Type.Name)) return -1;
        if (!writer.WriteChar('>')) return -1;

        return writer.BytesWritten;
    }

    private static bool WriteComplexType(IGenericReflectorComplexTypeHandler complexTypeHandler, ref ValueStringWriter writer, XMLConfig config, object? obj, int indent)
    {
        if (obj == null)
        {
            Assert(false, "Unhandled null");
            return true;
        }

        ComplexTypeHandlerMember[] members = complexTypeHandler.GetMembers();
        foreach (ComplexTypeHandlerMember member in members)
        {
            IGenericReflectorTypeHandler? memberTypeHandler = member.GetTypeHandler();
            if (memberTypeHandler == null) continue;

            if (config.Pretty)
            {
                if (!writer.WriteChar('\n')) return false;

                for (int i = 0; i < indent; i++)
                {
                    if (!writer.WriteChar(' ')) return false;
                }
            }

            if (!writer.WriteChar('<')) return false;
            if (!writer.WriteString(member.Name)) return false;

            bool read = member.GetValueFromComplexObject(obj, out object? memberVal);
            Assert(read, $"Couldn't read member {member.Name}");
            if (memberVal == null)
            {
                if (!writer.WriteChar('/')) return false;
                if (!writer.WriteChar('>')) return false;
                if (config.Pretty && !writer.WriteChar('\n')) return false;

                continue;
            }

            if (!writer.WriteChar('>')) return false;

            if (memberTypeHandler.CanGetOrParseValueAsString)
            {
                bool writtenMember = member.WriteValueAsStringFromComplexObject(ref writer, obj);
                Assert(writtenMember);
            }
            else
            {
                Assert(memberTypeHandler is IGenericReflectorComplexTypeHandler);

                var complexMemberHandler = (IGenericReflectorComplexTypeHandler)memberTypeHandler;
                WriteComplexType(complexMemberHandler, ref writer, config, memberVal, indent + config.Indentation);

                if (config.Pretty)
                {
                    for (int i = 0; i < indent; i++)
                    {
                        if (!writer.WriteChar(' ')) return false;
                    }
                }
            }

            if (!writer.WriteString("</")) return false;
            if (!writer.WriteString(member.Name)) return false;
            if (!writer.WriteChar('>')) return false;

            if (config.Pretty && !writer.WriteChar('\n')) return false;
        }

        return true;
    }
}
