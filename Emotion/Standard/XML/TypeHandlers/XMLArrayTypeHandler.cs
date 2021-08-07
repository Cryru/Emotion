#region Using

using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

#endregion

namespace Emotion.Standard.XML.TypeHandlers
{
    public class XMLArrayTypeHandler : XMLTypeHandler
    {
        protected XMLTypeHandler _elementTypeHandler;
        protected Type _elementType;
        protected bool _nonOpaque;

        public XMLArrayTypeHandler(Type type, Type nonOpaqueElementType, XMLTypeHandler elementTypeHandler) : base(type)
        {
            _elementTypeHandler = elementTypeHandler;
            _elementType = nonOpaqueElementType;
        }

        public override bool Serialize(object obj, StringBuilder output, int indentation = 1, XMLRecursionChecker recursionChecker = null, string fieldName = null)
        {
            if (obj == null) return false;

            fieldName ??= TypeName;
            output.AppendJoin(XMLFormat.IndentChar, new string[indentation]);
            output.Append($"<{fieldName}>\n");
            var iterable = (IEnumerable) obj;
            foreach (object item in iterable)
            {
                if (item == null)
                {
                    output.AppendJoin(XMLFormat.IndentChar, new string[indentation + 1]);
                    output.Append($"<{_elementTypeHandler.TypeName} xsi:nil=\"true\" />\n");
                    continue;
                }

                bool serialized = _elementTypeHandler.Serialize(item, output, indentation + 1, recursionChecker);

                // Force serialize in arrays, to preserve length.
                if (serialized) continue;
                output.AppendJoin(XMLFormat.IndentChar, new string[indentation + 1]);
                output.Append($"<{_elementTypeHandler.TypeName}></{_elementTypeHandler.TypeName}>\n");
            }

            output.AppendJoin(XMLFormat.IndentChar, new string[indentation]);
            output.Append($"</{fieldName}>\n");
            return true;
        }

        public override object Deserialize(XMLReader input)
        {
            var backingList = new List<object>();

            int depth = input.Depth;
            input.GoToNextTag();
            XMLTypeHandler handler = XMLHelpers.GetInheritedTypeHandlerFromXMLTag(input, out string tag) ?? _elementTypeHandler;
            while (input.Depth >= depth && !input.Finished)
            {
                object newObj = tag[^1] == '/' ? null : handler.Deserialize(input);
                backingList.Add(newObj);
                input.GoToNextTag();
                handler = XMLHelpers.GetInheritedTypeHandlerFromXMLTag(input, out tag) ?? _elementTypeHandler;
            }

            var arr = Array.CreateInstance(_elementType, backingList.Count);
            for (var i = 0; i < backingList.Count; i++)
            {
                arr.SetValue(backingList[i], i);
            }

            return arr;
        }
    }
}