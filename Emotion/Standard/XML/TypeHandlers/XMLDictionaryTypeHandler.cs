#region Using

using System;
using System.Collections;

#endregion

namespace Emotion.Standard.XML.TypeHandlers
{
    public class XMLDictionaryTypeHandler : XMLArrayTypeHandler
    {
        private Type _dictGenericType;

        public XMLDictionaryTypeHandler(Type type, XMLTypeHandler elementTypeHandler) : base(type, elementTypeHandler.Type, elementTypeHandler)
        {
            // Make a dictionary type with the arguments of the Key/Value generic element type.
            Type genericTypeDef = Type.GetGenericTypeDefinition();
            _dictGenericType = genericTypeDef.MakeGenericType(elementTypeHandler.Type.GetGenericArguments());
        }

        public override object Deserialize(XMLReader input)
        {
            var dict = (IDictionary) Activator.CreateInstance(_dictGenericType);
            var keyValueHandler = (XMLKeyValueTypeHandler) _elementTypeHandler;

            int depth = input.Depth;
            input.GoToNextTag();
            input.ReadTagWithoutAttribute();
            while (input.Depth >= depth && !input.Finished)
            {
                keyValueHandler.DeserializeKeyValue(input, out object key, out object value);
                dict.Add(key, value);
                input.GoToNextTag();
                input.ReadTagWithoutAttribute();
            }

            return dict;
        }
    }
}