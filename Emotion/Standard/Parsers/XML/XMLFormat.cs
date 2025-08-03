#nullable enable

#region Using

using System.Text;
using Emotion.Core.Systems.Logging;
using Emotion.Standard.Parsers.XML.TypeHandlers;

#endregion

namespace Emotion.Standard.Parsers.XML;

public static class XMLFormat
{
    public static string IndentChar = "  ";

    public static string To<T>(T obj)
    {
        Type type = typeof(T);
        var output = new StringBuilder();
        output.Append("<?xml version=\"1.0\"?>\n");

        XMLTypeHandler? handler = XMLHelpers.GetTypeHandler(type);
        if (handler == null) return string.Empty;

        var recursionChecker = new XMLRecursionChecker();
        recursionChecker.PushReference(obj, "main");
        handler.Serialize(obj, output, 1, recursionChecker);
        recursionChecker.PopReference(obj);

        return output.ToString();
    }

    public static T? From<T>(string xml)
    {
        if (string.IsNullOrEmpty(xml)) return default;
        Type requestedType = typeof(T);

        var reader = new XMLReader(xml);
        reader.GoToNextTag();

        // This is called first in order to index the type.
        XMLTypeHandler? requestedTypeHandler = XMLHelpers.GetTypeHandler(requestedType);
        if (requestedTypeHandler == null) return default;

        // Check if the handler is supposed to be of an inherited type based on the tag's type attribute.
        XMLTypeHandler? inheritingHandler = XMLHelpers.GetInheritedTypeHandlerFromXMLTag(reader, out string tag);

        XMLTypeHandler? handler;
        if (inheritingHandler == null)
        {
            // Try to find the type XML document via its document tag
            Type? headerTagType = XMLHelpers.GetTypeByNameWithTypeHint(requestedType, tag);
            if (headerTagType == null)
            {
                Engine.Log.Warning($"Tried to deserialize a document with header {tag} but found no such type that is assignable to {requestedType}.", MessageSource.XML);
                return default;
            }

            // Best case scenario it is the same type as the requested type.
            if (headerTagType == requestedType)
            {
                handler = requestedTypeHandler;
            }
            // It could be a type that inherits the requested type,
            // but didn't hit the inheritingHandler case since the document itself
            // was serialized as that higher order type (check comment in GetTypeByNameWithTypeHint)
            else if (headerTagType.IsAssignableTo(requestedType))
            {
                // then the handler is of the tag type.
                XMLTypeHandler? headerTypeHandler = XMLHelpers.GetTypeHandler(headerTagType);
                if (headerTypeHandler == null)
                {
                    Engine.Log.Warning($"Couldn't find type handler for type {headerTagType} gotten from document tag {tag}.", MessageSource.XML);
                    return default;
                }

                handler = headerTypeHandler;
            }
            else
            {
                Engine.Log.Warning($"Tried to deserialize a document with header type {headerTagType}({tag}) while expecting inherited from {requestedType}.", MessageSource.XML);
                return default;
            }
        }
        else
        {
            handler = inheritingHandler;
        }

        if (handler == null)
        {
            Engine.Log.Warning($"Couldn't find type handler for type {requestedType}.", MessageSource.XML);
            return default;
        }

        // Verify whether document is of an assignable type.
        if (!requestedType.IsAssignableFrom(handler.Type))
        {
            Engine.Log.Warning($"Tried to deserialize a document of type {handler.TypeName} while expecting inherited from {requestedType}.", MessageSource.XML);
            return default;
        }

        return (T?) handler.Deserialize(reader);
    }

    public static T? From<T>(ReadOnlyMemory<byte> data)
    {
        ReadOnlySpan<byte> span = data.Span;
        Encoding encoding = Helpers.GuessStringEncoding(span);
        return From<T>(encoding.GetString(span));
    }

    public static T? From<T>(ReadOnlySpan<byte> span)
    {
        Encoding encoding = Helpers.GuessStringEncoding(span);
        return From<T>(encoding.GetString(span));
    }

    public static T? From<T>(byte[] data)
    {
        Encoding encoding = Helpers.GuessStringEncoding(data);
        return From<T>(encoding.GetString(data));
    }
}