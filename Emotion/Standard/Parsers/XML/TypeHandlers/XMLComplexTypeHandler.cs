#nullable enable

#region Using

using Emotion.Core.Systems.Logging;
using System.Runtime.CompilerServices;
using System.Text;

#endregion

namespace Emotion.Standard.Parsers.XML.TypeHandlers;

public class XMLComplexTypeHandler : XMLComplexBaseTypeHandler
{
    public XMLComplexTypeHandler(Type type) : base(type)
    {
        // Check if inheriting anything. If so copy its excluded members as well.
        if (Type.BaseType != null && Type.BaseType != typeof(object))
        {
            var baseClass = (XMLComplexTypeHandler?) XMLHelpers.GetTypeHandler(Type.BaseType);
            HashSet<string>? baseTypeExcludedMembers = baseClass?._excludedMembers;
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
        _defaultValue = type.IsInterface || type.IsAbstract ? null : Activator.CreateInstance(type, true);
    }

    public override void Serialize(object? obj, StringBuilder output, int indentation = 1, XMLRecursionChecker? recursionChecker = null, string? fieldName = null)
    {
        if (obj == null) return;

        fieldName ??= TypeName;

        // Handle field value being of inherited type.
        XMLTypeHandler typeHandler = GetInheritedTypeHandler(obj, out string? inheritedType) ?? this;
        output.AppendJoin(XMLFormat.IndentChar, new string[indentation]);
        output.Append(inheritedType == null ? $"<{fieldName}>" : $"<{fieldName} type=\"{inheritedType}\">");
        typeHandler.SerializeValue(obj, output, indentation, recursionChecker);
        output.Append($"</{fieldName}>");
        if (indentation != 1) output.Append("\n");
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override void SerializeValue(object obj, StringBuilder output, int indentation = 1, XMLRecursionChecker? recursionChecker = null)
    {
        output.Append("\n");

        Dictionary<string, XMLFieldHandler> fieldHandlers = _fieldHandlers.Value;
        foreach ((string _, XMLFieldHandler field) in fieldHandlers)
        {
            if (field.Skip) continue;

            object? propertyVal = field.ReflectionInfo.GetValue(obj);
            string fieldName = field.Name;
            object defaultValue = field.DefaultValue;

            // Serialize null as self closing tag.
            if (propertyVal == null)
            {
                if (defaultValue == null) continue;
                output.AppendJoin(XMLFormat.IndentChar, new string[indentation + 1]);
                output.Append($"<{fieldName}/>\n");
                continue;
            }

            // If the property value is the same as the default value don't serialize it.
            if (propertyVal.Equals(defaultValue)) continue;

            // Check for recursive reference.
            recursionChecker ??= new XMLRecursionChecker();
            if (recursionChecker.PushReference(propertyVal, fieldName)) continue;

            bool isDefaultOfType = field.TypeHandler.IsTypeDefault(propertyVal);
            if (isDefaultOfType)
            {
                // Default one of the type but not default of the field.
                // By creating a field tag without contents, the result during deserialization is a default for the field-type value.
                output.AppendJoin(XMLFormat.IndentChar, new string[indentation + 1]);
                output.Append($"<{fieldName}></{fieldName}>\n");
            }
            else
            {
                field.TypeHandler.Serialize(propertyVal, output, indentation + 1, recursionChecker, fieldName);
            }

            recursionChecker.PopReference(propertyVal);
        }

        output.AppendJoin(XMLFormat.IndentChar, new string[indentation]);
    }

    public override object? Deserialize(XMLReader input)
    {
        int depth = input.Depth;
        object? newObj = Activator.CreateInstance(Type, true);

        input.GoToNextTag();
        while (input.Depth >= depth && !input.Finished)
        {
            XMLTypeHandler? inheritedHandler = XMLHelpers.GetInheritedTypeHandlerFromXMLTag(input, out string currentTag);
            var nullValue = false;
            if (currentTag[^1] == '/')
            {
                currentTag = currentTag[..^1];
                nullValue = true;
            }

            // Field can be null when a type field has been deleted from the definition
            // but the serialized document contains it
            if (!_fieldHandlers.Value.TryGetValue(currentTag, out XMLFieldHandler? field))
            {
                Engine.Log.Warning($"Couldn't find handler for field - {currentTag}", MessageSource.XML);
            }

            XMLTypeHandler? typeHandler = inheritedHandler ?? field?.TypeHandler;
            if (field != null && typeHandler != null)
            {
                Assert(newObj != null);
                object? val = nullValue ? null : typeHandler.Deserialize(input);
                if (!field.Skip) field.ReflectionInfo.SetValue(newObj!, val);
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
    protected XMLTypeHandler? GetInheritedTypeHandler(object obj, out string? inheritedType)
    {
        inheritedType = null;
        Type objType = obj.GetType();
        if (objType == Type) return null;

        // Encountering a type which inherits from this type.
        if (Type.IsAssignableFrom(objType))
        {
            // We want the full name - not user friendly name used in tags since it will be passed
            // into Assembly.GetTypeFromName. In the future if we code generate that we can
            // use the user friendly names here.
            // todo: ^
            inheritedType = objType.FullName; // XMLHelpers.GetTypeName(objType, true);
            return XMLHelpers.GetTypeHandler(objType);
        }

        // wtf?
        Engine.Log.Warning($"Unknown object of type {objType.Name} was passed to handler of type {TypeName}", MessageSource.XML);
        return null;
    }
}