#nullable enable

#pragma warning disable CS9080 // Use of variable in this context may expose referenced variables outside of their declaration scope

using Emotion.Standard.Reflector;
using System.Text;
using Emotion.Standard.Reflector.Handlers.Interfaces;
using Emotion.Standard.Reflector.Handlers.Base;
using Emotion.Utility.OptimizedStringReadWrite;

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

        return typeHandler.ParseFromXML<T>(ref reader);
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

        typeHandler.WriteAsXML(obj, ref writer, true, config, 0);

        return writer.BytesWritten;
    }
}
