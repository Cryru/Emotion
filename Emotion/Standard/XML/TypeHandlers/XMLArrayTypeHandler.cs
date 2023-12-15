#region Using

using System.Collections;
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

        public override void SerializeValue(object obj, StringBuilder output, int indentation = 1, XMLRecursionChecker recursionChecker = null)
        {
            output.Append("\n");

            var iterable = (IEnumerable) obj;
            foreach (object item in iterable)
            {
                // We serialize nulls in a special way (.Net spec compliant) in order to
                // differentiate between default values and nulls, and to retain array structure.
                if (item == null)
                {
                    output.AppendJoin(XMLFormat.IndentChar, new string[indentation + 1]);
                    output.Append($"<{_elementTypeHandler.TypeName} xsi:nil=\"true\" />\n");
                    continue;
                }

                bool isDefaultOfType = _elementTypeHandler.IsTypeDefault(item);

                // Check if item is a recursive reference.
                recursionChecker ??= new XMLRecursionChecker();
                if (recursionChecker.PushReference(item, "array")) isDefaultOfType = true;

                if (isDefaultOfType) // Force serialize in arrays, to preserve length and indices.
                {
                    output.AppendJoin(XMLFormat.IndentChar, new string[indentation + 1]);
                    output.Append($"<{_elementTypeHandler.TypeName}></{_elementTypeHandler.TypeName}>\n");
                }
                else
                {
                    _elementTypeHandler.Serialize(item, output, indentation + 1, recursionChecker);
                }

                recursionChecker.PopReference(item);
            }

            output.AppendJoin(XMLFormat.IndentChar, new string[indentation]);
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