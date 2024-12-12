#nullable enable

using Emotion.Standard.Reflector.Handlers;
using Emotion.Standard.Reflector;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Emotion.Utility;
using Emotion.Standard.OptimizedStringReadWrite;

namespace Emotion.Serialization.XML;

public struct XMLConfig
{
    public bool UseXMLHeader = true;

    public XMLConfig()
    {
    }
}

public static class XMLSerialize
{
    public static string XMLHeader = "<?xml version=\"1.0\"?>\n";

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


        //ReflectorTypeHandlerBase<T>? typeHandler = ReflectorEngine.GetTypeHandler<T>();
        //if (typeHandler == null) return default;

        //typeHandler.

        //if (typeHandler.CanGetOrParseValueAsString)
        //{
        //    typeHandler.ParseValueFromString(reader);
        //}
    }

    public static T? From<T>(ref ValueStringReader reader)
    {
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
        bool written;
        if (config.UseXMLHeader)
        {
            written = writer.WriteString(XMLHeader);
            if (!written) return -1;
        }

        ReflectorTypeHandlerBase<T>? typeHandler = ReflectorEngine.GetTypeHandler<T>();
        if (typeHandler == null) return -1;

        written = writer.WriteChar('<');
        if (!written) return -1;

        written = writer.WriteString(typeHandler.Type.Name);
        if (!written) return -1;

        written = writer.WriteChar('>');
        if (!written) return -1;

        // The base type can be written as a string directly!
        if (typeHandler.CanGetOrParseValueAsString)
        {
            written = typeHandler.WriteValueAsString(ref writer, obj);
            if (!written) return -1;
        }
        else if (typeHandler is IGenericReflectorComplexTypeHandler complexTypeHandler)
        {
            ComplexTypeHandlerMember[] members = complexTypeHandler.GetMembers();
            foreach (ComplexTypeHandlerMember member in members)
            {
                IGenericReflectorTypeHandler? memberTypeHandler = member.GetTypeHandler();
                if (memberTypeHandler == null) continue;

                written = writer.WriteString("\n  <");
                if (!written) return -1;

                written = writer.WriteString(member.Name);
                if (!written) return -1;

                written = writer.WriteChar('>');
                if (!written) return -1;

                if (member.WriteValueFromComplexObject(ref writer, obj))
                {

                }
                else
                {

                }

                written = writer.WriteString("</");
                if (!written) return -1;

                written = writer.WriteString(member.Name);
                if (!written) return -1;

                written = writer.WriteString(">\n");
                if (!written) return -1;
            }
        }

        written = writer.WriteString("</");
        if (!written) return -1;

        written = writer.WriteString(typeHandler.Type.Name);
        if (!written) return -1;

        written = writer.WriteChar('>');
        if (!written) return -1;

        return writer.BytesWritten;
    }
}
