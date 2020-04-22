#region Using

using System;
using System.Text;
using Emotion.Common;
using Emotion.Standard.Logging;

#endregion

namespace Emotion.Standard.XML.TypeHandlers
{
    public class XMLComplexValueTypeHandler : XMLComplexBaseTypeHandler
    {
        /// <summary>
        /// The default value of this complex type, if a value type.
        /// </summary>
        private object _defaultValue;

        public XMLComplexValueTypeHandler(Type type, bool opaque) : base(type)
        {
            _defaultValue = opaque ? Activator.CreateInstance(type, true) : null;
        }

        public override bool Serialize(object obj, StringBuilder output, int indentation = 1, XMLRecursionChecker recursionChecker = null, string fieldName = null)
        {
            if (_defaultValue != null && obj.Equals(_defaultValue) || obj == null) return false;

            fieldName ??= TypeName;
            output.AppendJoin(XMLFormat.IndentChar, new string[indentation]);
            output.Append($"<{fieldName}>\n");
            SerializeFields(obj, output, indentation + 1, recursionChecker);
            output.AppendJoin(XMLFormat.IndentChar, new string[indentation]);
            output.Append($"</{fieldName}>\n");

            return true;
        }

        public override object Deserialize(XMLReader input)
        {
            int depth = input.Depth;
            object newObj = Activator.CreateInstance(Type, true);

            input.GoToNextTag();
            while (input.Depth >= depth && !input.Finished)
            {
                string currentTag = input.ReadTagWithoutAttribute();
                if (!GetFieldHandler(currentTag, out XMLFieldHandler field))
                {
                    Engine.Log.Warning($"Couldn't find handler for field - {currentTag}", MessageSource.XML);
                    return newObj;
                }

                object val = field.TypeHandler.Deserialize(input);
                field.ReflectionInfo.SetValue(newObj, val);
                input.GoToNextTag();
            }

            return newObj;
        }

        /// <summary>
        /// Get the handler for a field, recursively searches within the base class.
        /// </summary>
        /// <param name="tag">The field name.</param>
        /// <param name="handler">The handler for that field.</param>
        /// <returns>Whether a handler was found for this field.</returns>
        protected bool GetFieldHandler(string tag, out XMLFieldHandler handler)
        {
            return _fieldHandlers.Value.TryGetValue(tag, out handler);
        }
    }
}