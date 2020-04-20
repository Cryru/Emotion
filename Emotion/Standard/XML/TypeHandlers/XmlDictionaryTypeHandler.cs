#region Using

using System;
using System.Collections;
using Emotion.Common;
using Emotion.Standard.Logging;

#endregion

namespace Emotion.Standard.XML.TypeHandlers
{
    public class XMLDictionaryTypeHandler : XMLArrayTypeHandler
    {
        private Type _dictGenericType;

        public XMLDictionaryTypeHandler(Type type, Type elementType) : base(type, elementType)
        {
            Type genericTypeDef = Type.GetGenericTypeDefinition();
            _dictGenericType = genericTypeDef.MakeGenericType(elementType.GetGenericArguments());
        }

        public override object Deserialize(XMLReader input)
        {
            var dict = (IDictionary) Activator.CreateInstance(_dictGenericType);
            var keyValueHandler = (XMLKeyValueTypeHandler) XMLHelpers.GetTypeHandler(_elementTypeHandler.TypeHandler.Type);

            int depth = input.Depth;
            input.GoToNextTag();
            input.ReadTag(out string typeAttribute);
            while (input.Depth >= depth && !input.Finished)
            {
                XMLTypeHandler handler = _elementTypeHandler.TypeHandler;
                if (typeAttribute != null)
                {
                    Type derivedType = XMLHelpers.GetTypeByName(typeAttribute);
                    if (derivedType == null)
                    {
                        Engine.Log.Warning($"Couldn't find derived type of name {typeAttribute} in array.", MessageSource.XML);
                        return null;
                    }

                    handler = XMLHelpers.GetTypeHandler(derivedType);
                }

                object newObj = handler.Deserialize(input);
                dict.Add(keyValueHandler.GetKey(newObj), keyValueHandler.GetValue(newObj));
                input.GoToNextTag();
                input.ReadTag(out typeAttribute);
            }

            return dict;
        }
    }
}