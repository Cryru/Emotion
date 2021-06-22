#region Using

using System;
using System.Text;
using Emotion.Common;
using Emotion.Standard.Logging;
using Emotion.Standard.XML.TypeHandlers;
using Emotion.Utility;

#endregion

namespace Emotion.Standard.XML
{
    public static class XMLFormat
    {
        public static string IndentChar = "  ";

        public static string To<T>(T obj)
        {
            Type type = typeof(T);
            var output = new StringBuilder();
            output.Append("<?xml version=\"1.0\"?>\n");

            XMLTypeHandler handler = XMLHelpers.GetTypeHandler(type);
            if (handler == null) return null;
            handler.Serialize(obj, output);
            return output.ToString();
        }

        public static T From<T>(string xml)
        {
            if (string.IsNullOrEmpty(xml)) return default;
            Type type = typeof(T);

            // This is called first in order to index the type.
            XMLTypeHandler requestedType = XMLHelpers.GetTypeHandler(type);

            var reader = new XMLReader(xml);
            reader.GoToNextTag();

            // Check if the handler is supposed to be of an inherited type. Otherwise use the handler of the requested type.
            XMLTypeHandler handler = XMLHelpers.GetInheritedTypeHandlerFromXMLTag(reader, out string tag) ?? requestedType;
            Type headerTagType = XMLHelpers.GetTypeByName(tag);
            if (headerTagType == null || headerTagType != type)
            {
                Engine.Log.Warning($"Tried to deserialize a document with header type {headerTagType}({tag}) while expecting inherited from {type}.", MessageSource.XML);
                return default;
            }

            // Verify whether document is of an assignable type.
            if (!type.IsAssignableFrom(handler.Type))
            {
                Engine.Log.Warning($"Tried to deserialize a document of type {handler.TypeName} while expecting inherited from {type}.", MessageSource.XML);
                return default;
            }

            return (T) handler.Deserialize(reader);
        }

        public static T From<T>(ReadOnlyMemory<byte> data)
        {
            ReadOnlySpan<byte> span = data.Span;
            Encoding encoding = Helpers.GuessStringEncoding(span);
            return From<T>(encoding.GetString(span));
        }
    }
}