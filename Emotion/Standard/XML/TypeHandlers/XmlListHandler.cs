#region Using

using System;
using System.Collections;
using System.Collections.Generic;
using Emotion.Common;
using Emotion.Standard.Logging;

#endregion

namespace Emotion.Standard.XML.TypeHandlers
{
    public class XmlListHandler : XmlArrayTypeHandler
    {
        public XmlListHandler(Type type, Type elementType) : base(type, elementType)
        {
        }

        public override object Deserialize(XmlReader input)
        {
            var backingArr = new List<object>();

            int depth = input.Depth;
            input.GoToNextTag();
            input.ReadTag(out string typeAttribute);
            while (input.Depth >= depth && !input.Finished)
            {
                XMLTypeHandler handler = _elementTypeHandler.TypeHandler;
                if (typeAttribute != null)
                {
                    Type derivedType = XmlHelpers.GetTypeByName(typeAttribute);
                    if (derivedType == null)
                    {
                        Engine.Log.Warning($"Couldn't find derived type of name {typeAttribute} in array.", MessageSource.XML);
                        return null;
                    }

                    handler = XmlHelpers.GetTypeHandler(derivedType);
                }

                object newObj = handler.Deserialize(input);
                backingArr.Add(newObj);
                input.GoToNextTag();
                input.ReadTag(out typeAttribute);
            }

            Type listGenericType = XmlHelpers.ListType.MakeGenericType(_elementTypeHandler.TypeHandler.Type);
            var list = (IList) Activator.CreateInstance(listGenericType, backingArr.Count);
            for (var i = 0; i < backingArr.Count; i++)
            {
                list.Add(backingArr[i]);
            }

            return list;
        }
    }
}