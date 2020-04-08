#region Using

using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using Emotion.Common;
using Emotion.Standard.Logging;

#endregion

namespace Emotion.Standard.XML
{
    public class XmlArrayFieldHandler : XmlFieldHandler
    {
        private XmlFieldHandler _elementTypeHandler;

        public XmlArrayFieldHandler(XmlReflectionHandler field, XmlFieldHandler elementTypeHandler) : base(field)
        {
            _elementTypeHandler = elementTypeHandler;
        }

        public override void Serialize(object obj, StringBuilder output, int indentation, XmlRecursionChecker recursionChecker)
        {
            if(obj == null) return;

            if (RecursionCheck)
            {
                if (recursionChecker == null) recursionChecker = new XmlRecursionChecker();
                if (recursionChecker.PushReference(obj)) return;
            }

            var arr = (IEnumerable) obj;
            output.AppendJoin(XmlFormat.IndentChar, new string[indentation + 1]);
            output.Append($"<{Name}>\n");
            foreach (object item in arr)
            {
                _elementTypeHandler.Serialize(item, output, indentation + 1, recursionChecker);
            }
            output.AppendJoin(XmlFormat.IndentChar, new string[indentation + 1]);
            output.Append($"</{Name}>\n");

            if (RecursionCheck) recursionChecker.PopReference(obj);
        }

        public override object Deserialize(XmlReader input)
        {
            var backingArr = new List<object>();

            int depth = input.Depth;
            input.GoToNextTag();
            input.ReadTag(out string typeAttribute);
            while (input.Depth >= depth && !input.Finished)
            {
                XmlFieldHandler handler = _elementTypeHandler;
                if (typeAttribute != null)
                {
                    Type derivedType = XmlHelpers.GetTypeByName(typeAttribute);
                    if (derivedType == null)
                    {
                        Engine.Log.Warning($"Couldn't find derived type of name {typeAttribute} in array.", MessageSource.XML);
                        return null;
                    }
                    handler = XmlHelpers.ResolveFieldHandler(derivedType, handler.ReflectionInfo, _elementTypeHandler.ReflectionInfo.Type);
                }

                object newObj = handler.Deserialize(input);
                backingArr.Add(newObj);
                input.GoToNextTag();
                input.ReadTag(out typeAttribute);
            }

            // Simple array
            if (ReflectionInfo.Type.IsArray)
            {
                var arr = Array.CreateInstance(_elementTypeHandler.ReflectionInfo.Type, backingArr.Count);
                for (var i = 0; i < backingArr.Count; i++)
                {
                    arr.SetValue(backingArr[i], i);
                }

                return arr;
            }

            // List
            Type listType = typeof(List<>);
            if (ReflectionInfo.Type != listType)
            {
                Type listGenericType = listType.MakeGenericType(_elementTypeHandler.ReflectionInfo.Type);
                var list = (IList) Activator.CreateInstance(listGenericType, backingArr.Count);
                for (var i = 0; i < backingArr.Count; i++)
                {
                    list.Add(backingArr[i]);
                }

                return list;
            }

            Engine.Log.Warning($"Unknown IEnumerable type {ReflectionInfo.Type}", MessageSource.XML);
            return null;
        }
    }
}