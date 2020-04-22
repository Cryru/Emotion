﻿#region Using

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

            // Handler is gotten first so it can index type names.
            XMLTypeHandler handler = XMLHelpers.GetTypeHandler(type);
            if (handler == null) return default;

            var reader = new XMLReader(xml);

            // Verify first tag is the provided type.
            reader.GoToNextTag();
            string currentTag = reader.ReadTagWithoutAttribute();
            Type tagType = XMLHelpers.GetTypeByName(currentTag);
            if (tagType != null && tagType == type) return (T) handler.Deserialize(reader);
            Engine.Log.Warning($"Serializing XML of type {tagType}({currentTag}) while expecting {type}.", MessageSource.XML);
            return default;
        }

        public static T From<T>(byte[] data)
        {
            return From<T>(Helpers.GuessStringEncoding(data));
        }
    }
}