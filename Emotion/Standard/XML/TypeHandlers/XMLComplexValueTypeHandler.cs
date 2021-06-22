#region Using

using System;
using System.Collections.Generic;
using System.Text;
using Emotion.Common;
using Emotion.Common.Serialization;
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
            // Check if the type is excluding any fields.
            DontSerializeMembers exclusion = null;
            object[] exclusions = type.GetCustomAttributes(typeof(DontSerializeMembers), true);
            if (exclusions.Length > 0) exclusion = exclusions[0] as DontSerializeMembers;
            if (exclusion != null) AddExclusions(exclusion);

            // Create default value reference.
            _defaultValue = opaque ? Activator.CreateInstance(type, true) : null;
        }

        public override bool Serialize(object obj, StringBuilder output, int indentation = 1, XMLRecursionChecker recursionChecker = null, string fieldName = null)
        {
            if (_defaultValue != null && obj.Equals(_defaultValue) || obj == null) return false;

            fieldName ??= TypeName;
            output.AppendJoin(XMLFormat.IndentChar, new string[indentation]);
            output.Append($"<{fieldName}>\n");

            Dictionary<string, XMLFieldHandler> fieldHandlers = _fieldHandlers.Value;
            foreach ((string _, XMLFieldHandler field) in fieldHandlers)
            {
                if (IsFieldExcluded(field)) continue;
                object propertyVal = field.ReflectionInfo.GetValue(obj);
                field.TypeHandler.Serialize(propertyVal, output, indentation + 1, recursionChecker, field.Name);
            }

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
                if (!_fieldHandlers.Value.TryGetValue(currentTag, out XMLFieldHandler field))
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
    }
}