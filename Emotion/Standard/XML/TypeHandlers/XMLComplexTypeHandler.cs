#region Using

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using Emotion.Common;
using Emotion.Common.Serialization;
using Emotion.Standard.Logging;

#endregion

namespace Emotion.Standard.XML.TypeHandlers
{
    public class XMLComplexTypeHandler : XMLComplexBaseTypeHandler
    {
        /// <summary>
        /// Whether this type contains fields which could reference it.
        /// </summary>
        public bool RecursiveType
        {
            get
            {
                if (_recursiveType != null) return _recursiveType.Value;
                _recursiveType = (_baseClass?.RecursiveType ?? false) || IsRecursiveWith(Type);
                return _recursiveType.Value;
            }
            protected set => _recursiveType = value;
        }

        private bool? _recursiveType;

        /// <summary>
        /// The handler for this type's base class (if any).
        /// </summary>
        protected XMLComplexTypeHandler _baseClass;

        /// <summary>
        /// The default value of the complex type when constructed.
        /// </summary>
        protected object _defaultConstruct;

        public XMLComplexTypeHandler(Type type) : base(type)
        {
            // Check if the type is excluding any fields.
            ExcludeMembersAttribute exclusionAttribute = null;
            object[] exclusions = type.GetCustomAttributes(typeof(ExcludeMembersAttribute), true);
            if (exclusions.Length > 0) exclusionAttribute = exclusions[0] as ExcludeMembersAttribute;

            // Check if inheriting anything.
            if (Type.BaseType != null && Type.BaseType != typeof(object))
            {
                _baseClass = (XMLComplexTypeHandler) XMLHelpers.GetTypeHandler(Type.BaseType);
                if (_baseClass != null && exclusionAttribute != null) _baseClass = (XMLComplexTypeHandler) _baseClass.DeriveWithExclusions(exclusionAttribute);
            }

            _defaultConstruct = Activator.CreateInstance(type, true);
        }

        protected override Dictionary<string, XMLFieldHandler> IndexFields()
        {
            Dictionary<string, XMLFieldHandler> fields = base.IndexFields();
            foreach (KeyValuePair<string, XMLFieldHandler> handler in fields)
            {
                handler.Value.SetDefaultValue(_defaultConstruct);
            }

            return fields;
        }

        public override bool Serialize(object obj, StringBuilder output, int indentation = 1, XMLRecursionChecker recursionChecker = null, string fieldName = null)
        {
            if (obj == null) return false;

            if (RecursiveType)
            {
                recursionChecker ??= new XMLRecursionChecker();
                if (recursionChecker.PushReference(obj))
                {
                    Engine.Log.Warning($"Recursive reference in field {fieldName}.", MessageSource.XML);
                    recursionChecker.PopReference(obj);
                    return true;
                }
            }

            // Handle derived type.
            XMLComplexTypeHandler typeHandler = GetDerivedTypeHandler(obj, out string derivedType) ?? this;

            fieldName ??= TypeName;
            output.AppendJoin(XMLFormat.IndentChar, new string[indentation]);
            output.Append(derivedType == null ? $"<{fieldName}>\n" : $"<{fieldName} type=\"{derivedType}\">\n");
            typeHandler.SerializeFields(obj, output, indentation + 1, recursionChecker);
            output.AppendJoin(XMLFormat.IndentChar, new string[indentation]);
            output.Append($"</{fieldName}>\n");

            if (RecursiveType) recursionChecker!.PopReference(obj);
            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected void SerializeFields(object obj, StringBuilder output, int indentation, XMLRecursionChecker recursionChecker)
        {
            _baseClass?.SerializeFields(obj, output, indentation, recursionChecker);
            Dictionary<string, XMLFieldHandler> fieldHandlers = _fieldHandlers.Value;
            foreach ((string _, XMLFieldHandler field) in fieldHandlers)
            {
                if (IsFieldExcluded(field)) continue;

                object propertyVal = field.ReflectionInfo.GetValue(obj);
                string fieldName = field.Name;
                object defaultValue = field.DefaultValue;

                // Serialize null as self closing tag.
                if (propertyVal == null)
                {
                    if (defaultValue == null) continue;
                    output.AppendJoin(XMLFormat.IndentChar, new string[indentation]);
                    output.Append($"<{fieldName}/>\n");
                    continue;
                }

                // If the property value is the same as the default value don't serialize it.
                if (propertyVal.Equals(defaultValue)) continue;

                bool serialized = field.TypeHandler.Serialize(propertyVal, output, indentation, recursionChecker, fieldName);

                // If not serialized that means the value passed is the default one of the type.
                // However we want to serialize it in this case, since it isn't the default of the field.
                // We do so by creating a field tag without contents, which will result in a default for the field-type value.
                if (serialized) continue;
                output.AppendJoin(XMLFormat.IndentChar, new string[indentation]);
                output.Append($"<{fieldName}></{fieldName}>\n");
            }
        }

        public override object Deserialize(XMLReader input)
        {
            int depth = input.Depth;
            object newObj = Activator.CreateInstance(Type, true);

            input.GoToNextTag();
            while (input.Depth >= depth && !input.Finished)
            {
                XMLTypeHandler derivedHandler = XMLHelpers.GetDerivedTypeHandlerFromXMLTag(input, out string currentTag);
                var nullValue = false;
                if (currentTag[^1] == '/')
                {
                    currentTag = currentTag[..^1];
                    nullValue = true;
                }

                if (!GetFieldHandler(currentTag, out XMLFieldHandler field))
                {
                    Engine.Log.Warning($"Couldn't find handler for field - {currentTag}", MessageSource.XML);
                    return newObj;
                }

                // Derived type.
                if (derivedHandler != null) field = new XMLFieldHandler(field.ReflectionInfo, derivedHandler);

                object val = nullValue ? null : field.TypeHandler.Deserialize(input);
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
            if (_fieldHandlers.Value.TryGetValue(tag, out handler)) return true;
            return _baseClass != null && _baseClass.GetFieldHandler(tag, out handler);
        }

        
        /// <summary>
        /// Derive with exclusions recursively down the base type.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override XMLComplexBaseTypeHandler DeriveWithExclusions(ExcludeMembersAttribute exclusions)
        {
            var clone = (XMLComplexTypeHandler) MemberwiseClone();
            clone._exclusions = exclusions.Members;
            if (clone._baseClass != null) clone._baseClass = (XMLComplexTypeHandler) clone._baseClass.DeriveWithExclusions(exclusions);

            return clone;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected XMLComplexTypeHandler GetDerivedTypeHandler(object obj, out string derivedType)
        {
            derivedType = null;
            Type objType = obj.GetType();
            if (objType == Type) return null;

            // Encountering a type which inherits from this type.
            if (Type.IsAssignableFrom(objType))
            {
                derivedType = XMLHelpers.GetTypeName(objType, true);
                return (XMLComplexTypeHandler) XMLHelpers.GetTypeHandler(objType);
            }

            // wtf?
            Engine.Log.Warning($"Unknown object of type {objType.Name} was passed to handler of type {TypeName}", MessageSource.XML);
            return null;
        }

        public override IEnumerator<XMLFieldHandler> EnumFields()
        {
            Dictionary<string, XMLFieldHandler> handlers = _fieldHandlers.Value;
            foreach (KeyValuePair<string, XMLFieldHandler> field in handlers)
            {
                XMLFieldHandler fieldHandler = field.Value;
                if (IsFieldExcluded(fieldHandler)) continue;
                yield return fieldHandler;
            }

            XMLComplexTypeHandler baseClass = _baseClass;
            while (baseClass != null)
            {
                handlers = baseClass._fieldHandlers.Value;
                foreach (KeyValuePair<string, XMLFieldHandler> field in handlers)
                {
                    XMLFieldHandler fieldHandler = field.Value;
                    if (baseClass.IsFieldExcluded(fieldHandler)) continue;
                    yield return fieldHandler;
                }

                baseClass = baseClass._baseClass;
            }
        }

        public override bool IsRecursiveWith(Type type)
        {
            return type.IsAssignableFrom(Type) || _fieldHandlers.Value.Any(x => x.Value.TypeHandler.IsRecursiveWith(type));
        }
    }
}