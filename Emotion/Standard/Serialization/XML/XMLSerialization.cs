#nullable enable

#pragma warning disable CS9080 // Use of variable in this context may expose referenced variables outside of their declaration scope

using Emotion.Standard.DataStructures.OptimizedStringReadWrite;
using Emotion.Standard.Reflector;
using Emotion.Standard.Reflector.Handlers.Base;
using Emotion.Standard.Reflector.Handlers.Interfaces;
using System.Text;
using System.Xml.XPath;

namespace Emotion.Standard.Serialization.XML;

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
        Span<char> readMemory = stackalloc char[128];

        // Read first tag
        int charsWritten = reader.ReadXMLTag(readMemory, out bool closing);
        if (charsWritten == 0 || closing) return default;

        bool isXmlHeader = readMemory[0] == '?';
        if (isXmlHeader)
        {
            charsWritten = reader.ReadXMLTag(readMemory, out closing);
            if (charsWritten == 0 || closing) return default;
        }

        Span<char> nextTag = readMemory.Slice(0, charsWritten);
        IGenericReflectorTypeHandler? typeHandler = ReflectorEngine.GetTypeHandlerByName(nextTag);
        if (typeHandler == null) return default;

        if (!typeHandler.IsTypeAssignableTo(requestedType))
        {
            Engine.Log.Warning($"Tried to deserialize into {requestedType.Name} type but the document is of type {typeHandler.Type.Name}", MessageSource.XML);
            return default;
        }

        return typeHandler.ParseFromXML<T>(ref reader);
    }

    public static string To<T>(in T obj)
    {
        return To(obj, new XMLConfig());
    }

    public static string To<T>(in T obj, XMLConfig config)
    {
        var builder = new StringBuilder();
        var reader = new ValueStringWriter(builder);
        if (To(obj, config, ref reader) == -1) return string.Empty;
        return builder.ToString();
    }

    public static int To<T>(in T obj, XMLConfig config, Span<char> xmlDataUtf16)
    {
        var reader = new ValueStringWriter(xmlDataUtf16);
        return To(obj, config, ref reader);
    }

    public static int To<T>(in T obj, XMLConfig config, Span<byte> xmlDataUtf8)
    {
        var reader = new ValueStringWriter(xmlDataUtf8);
        return To(obj, config, ref reader);
    }

    public static int To<T>(in T obj, XMLConfig config, ref ValueStringWriter writer)
    {
        return To(obj?.GetType() ?? typeof(T), obj, config, ref writer);
    }

    public static int To<T>(Type typ, in T obj, XMLConfig config, ref ValueStringWriter writer)
    {
        if (config.UseXMLHeader)
        {
            writer.WriteString(XMLHeader);

            if (config.Pretty)
                writer.WriteChar('\n');
        }

        IGenericReflectorTypeHandler? typeHandler = ReflectorEngine.GetTypeHandler(typ);
        if (typeHandler == null) return 0;

        writer.SetIndentSize(config.Indentation);
        typeHandler.WriteAsXML(obj, ref writer, true, config);

        return writer.CharsWritten;
    }
}
