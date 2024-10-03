#nullable enable

using Emotion.Standard.Reflector.Handlers;
using Emotion.Standard.Reflector;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Emotion.Utility;
using Emotion.Standard.ByteReadWrite;

namespace Emotion.Serialization.XML;

public static class XMLSerialize
{
    public static T? From<T>(string xml)
    {
        return From<T>(xml.AsSpan());
    }

    public static T? From<T>(ReadOnlySpan<char> xmlDataUtf16)
    {
        var reader = new ValueStringReaderReader(xmlDataUtf16);
        return default;
    }

    public static T? From<T>(ReadOnlySpan<byte> xmlDataUtf8)
    {
        var reader = new ValueStringReaderReader(xmlDataUtf8);
        return default;


        //ReflectorTypeHandlerBase<T>? typeHandler = ReflectorEngine.GetTypeHandler<T>();
        //if (typeHandler == null) return default;

        //typeHandler.

        //if (typeHandler.CanGetOrParseValueAsString)
        //{
        //    typeHandler.ParseValueFromString(reader);
        //}
    }
}
