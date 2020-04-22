﻿#region Using

using System;
using System.Collections;
using System.Collections.Generic;

#endregion

namespace Emotion.Standard.XML.TypeHandlers
{
    public class XMLListHandler : XMLArrayTypeHandler
    {
        public XMLListHandler(Type type, Type elementType) : base(type, elementType)
        {
        }

        public override object Deserialize(XMLReader input)
        {
            var backingArr = new List<object>();

            int depth = input.Depth;
            input.GoToNextTag();
            XMLTypeHandler handler = XMLHelpers.GetDerivedTypeHandlerFromXMLTag(input, out string tag) ?? _elementTypeHandler;
            while (input.Depth >= depth && !input.Finished)
            {
                object newObj = tag.Contains("/") ? null : handler.Deserialize(input);
                backingArr.Add(newObj);
                input.GoToNextTag();
                handler = XMLHelpers.GetDerivedTypeHandlerFromXMLTag(input, out tag) ?? _elementTypeHandler;
            }

            Type listGenericType = XMLHelpers.ListType.MakeGenericType(_elementType);
            var list = (IList) Activator.CreateInstance(listGenericType, backingArr.Count);
            for (var i = 0; i < backingArr.Count; i++)
            {
                list.Add(backingArr[i]);
            }

            return list;
        }
    }
}