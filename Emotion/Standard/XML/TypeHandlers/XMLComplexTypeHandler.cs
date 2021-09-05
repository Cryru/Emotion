#region Using

using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using Emotion.Common;
using Emotion.Standard.Logging;

#endregion

namespace Emotion.Standard.XML.TypeHandlers
{
    public class XMLComplexTypeHandler : XMLComplexBaseTypeHandler
    {
        /// <summary>
        /// The default value of the complex type when constructed.
        /// </summary>
        protected object _defaultConstruct;

        public XMLComplexTypeHandler(Type type) : base(type)
        {
            // Check if inheriting anything. If so copy its excluded members as well.
            if (Type.BaseType != null && Type.BaseType != typeof(object))
            {
                var baseClass = (XMLComplexTypeHandler)XMLHelpers.GetTypeHandler(Type.BaseType);
                HashSet<string> baseTypeExcludedMembers = baseClass?._excludedMembers;
                if (baseTypeExcludedMembers != null)
                {
                    if (_excludedMembers == null)
                    {
                        _excludedMembers = baseTypeExcludedMembers;
                    }
                    else
                    {
                        // Copy hashset as not to modify reference of attribute.
                        var newHashSet = new HashSet<string>();
                        foreach (string excludedMember in _excludedMembers)
                        {
                            newHashSet.Add(excludedMember);
                        }

                        // Add values from base type.
                        foreach (string excludedMember in baseTypeExcludedMembers)
                        {
                            newHashSet.Add(excludedMember);
                        }

                        _excludedMembers = newHashSet;
                    }
                }
            }

            // Create default value reference.
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

            // Pop a reference to check if this type holds a recursive reference.
            recursionChecker ??= new XMLRecursionChecker();
            if (recursionChecker.PushReference(obj))
            {
                Engine.Log.Warning($"Recursive reference in field {fieldName}.", MessageSource.XML);
                recursionChecker.PopReference(obj);
                return true;
            }

            // Handle field value being of inherited type.
            XMLComplexTypeHandler typeHandler = GetInheritedTypeHandler(obj, out string inheritedType) ?? this;

            output.AppendJoin(XMLFormat.IndentChar, new string[indentation]);
            fieldName ??= TypeName;
            output.Append(inheritedType == null ? $"<{fieldName}>\n" : $"<{fieldName} type=\"{inheritedType}\">\n");
            typeHandler.SerializeFields(obj, output, indentation + 1, recursionChecker);
            output.AppendJoin(XMLFormat.IndentChar, new string[indentation]);
            output.Append($"</{fieldName}>\n");

            recursionChecker.PopReference(obj);
            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected void SerializeFields(object obj, StringBuilder output, int indentation, XMLRecursionChecker recursionChecker)
        {
            Dictionary<string, XMLFieldHandler> fieldHandlers = _fieldHandlers.Value;
            foreach ((string _, XMLFieldHandler field) in fieldHandlers)
            {
                if (field.Skip) continue;

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
                XMLTypeHandler inheritedHandler = XMLHelpers.GetInheritedTypeHandlerFromXMLTag(input, out string currentTag);
                var nullValue = false;
                if (currentTag[^1] == '/')
                {
                    currentTag = currentTag[..^1];
                    nullValue = true;
                }

                if (!_fieldHandlers.Value.TryGetValue(currentTag, out XMLFieldHandler field))
                {
                    Engine.Log.Warning($"Couldn't find handler for field - {currentTag}", MessageSource.XML);
                    return newObj;
                }

                XMLTypeHandler typeHandler = inheritedHandler ?? field.TypeHandler;
                if (typeHandler != null)
                {
                    object val = nullValue ? null : typeHandler.Deserialize(input);
                    if (!field.Skip) field.ReflectionInfo.SetValue(newObj, val);
                }
                else
                {
                    // Find closing tag of current tag.
                    var c = "";
                    while (c != $"/{currentTag}")
                    {
                        c = input.ReadTagWithoutAttribute();
                        if (input.Finished) break;
                    }
                }

                input.GoToNextTag();
            }

            return newObj;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected XMLComplexTypeHandler GetInheritedTypeHandler(object obj, out string inheritedType)
        {
            inheritedType = null;
            Type objType = obj.GetType();
            if (objType == Type) return null;

            // Encountering a type which inherits from this type.
            if (Type.IsAssignableFrom(objType))
            {
                inheritedType = XMLHelpers.GetTypeName(objType, true);
                return (XMLComplexTypeHandler)XMLHelpers.GetTypeHandler(objType);
            }

            // wtf?
            Engine.Log.Warning($"Unknown object of type {objType.Name} was passed to handler of type {TypeName}", MessageSource.XML);
            return null;
        }
    }
}