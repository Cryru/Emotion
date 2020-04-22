#region Using

using System;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Text;
using Emotion.Common;
using Emotion.Standard.Logging;

#endregion

namespace Emotion.Standard.XML.TypeHandlers
{
    public class XMLKeyValueTypeHandler : XMLTypeHandler
    {
        private Lazy<XMLFieldHandler> _keyHandler;
        private Lazy<XMLFieldHandler> _valueHandler;

        private object _keyDefault;
        private object _valueDefault;

        [SuppressMessage("ReSharper", "PossibleNullReferenceException")]
        public XMLKeyValueTypeHandler(Type type) : base(type)
        {
            PropertyInfo keyProperty = Type.GetProperty("Key");
            _keyHandler = new Lazy<XMLFieldHandler>(() => XMLHelpers.ResolveFieldHandler(keyProperty.PropertyType, new XMLReflectionHandler(keyProperty)));
            PropertyInfo valueProperty = Type.GetProperty("Value");
            _valueHandler = new Lazy<XMLFieldHandler>(() => XMLHelpers.ResolveFieldHandler(valueProperty.PropertyType, new XMLReflectionHandler(valueProperty)));

            Type opaqueKeyType = XMLHelpers.GetOpaqueType(keyProperty.PropertyType, out bool opaque);
            _keyDefault = opaque && opaqueKeyType.IsValueType ? Activator.CreateInstance(opaqueKeyType) : null;
            Type opaqueValueType = XMLHelpers.GetOpaqueType(valueProperty.PropertyType, out opaque);
            _valueDefault = opaque && opaqueValueType.IsValueType ? Activator.CreateInstance(opaqueValueType) : null;
        }

        public override bool Serialize(object obj, StringBuilder output, int indentation = 1, XMLRecursionChecker recursionChecker = null, string fieldName = null)
        {
            if (obj == null) return false;

            fieldName ??= TypeName;
            output.AppendJoin(XMLFormat.IndentChar, new string[indentation]);
            output.Append($"<{fieldName}>\n");

            XMLFieldHandler keyHandler = _keyHandler.Value;
            XMLFieldHandler valueHandler = _valueHandler.Value;

            keyHandler.TypeHandler.Serialize(keyHandler.ReflectionInfo.GetValue(obj), output, indentation + 1, recursionChecker, "Key");
            valueHandler.TypeHandler.Serialize(valueHandler.ReflectionInfo.GetValue(obj), output, indentation + 1, recursionChecker, "Value");

            output.AppendJoin(XMLFormat.IndentChar, new string[indentation]);
            output.Append($"</{fieldName}>\n");
            return true;
        }

        public override object Deserialize(XMLReader input)
        {
            // This type is expected to be encountered only in the dictionary, where it use DeserializeKeyValue
            throw new NotImplementedException();
        }

        public void DeserializeKeyValue(XMLReader input, out object key, out object value)
        {
            key = _keyDefault;
            value = _valueDefault;

            int depth = input.Depth;
            input.GoToNextTag();
            while (input.Depth >= depth && !input.Finished)
            {
                XMLTypeHandler handler = XMLHelpers.GetDerivedTypeHandlerFromXMLTag(input, out string currentTag);
                switch (currentTag)
                {
                    case "Key":
                        if (handler == null) handler = _keyHandler.Value.TypeHandler;
                        key = handler.Deserialize(input);
                        break;
                    case "Value":
                        if (handler == null) handler = _valueHandler.Value.TypeHandler;
                        value = handler.Deserialize(input);
                        break;
                    default:
                        Engine.Log.Warning($"Unknown deserialization tag in KVP - {currentTag}.", MessageSource.XML);
                        return;
                }

                input.GoToNextTag();
            }
        }
    }
}