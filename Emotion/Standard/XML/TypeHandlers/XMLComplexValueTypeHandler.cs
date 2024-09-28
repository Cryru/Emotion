#region Using

using System.Text;

#endregion

#nullable enable

namespace Emotion.Standard.XML.TypeHandlers
{
    public class XMLComplexValueTypeHandler : XMLComplexBaseTypeHandler
    {
        public XMLComplexValueTypeHandler(Type type, bool nonNullable) : base(type)
        {
            // Create default value reference.
            _defaultValue = nonNullable ? Activator.CreateInstance(type, true) : null;
        }

        public override void SerializeValue(object obj, StringBuilder output, int indentation = 1, XMLRecursionChecker? recursionChecker = null)
        {
            output.Append("\n");

            Dictionary<string, XMLFieldHandler> fieldHandlers = _fieldHandlers.Value;
            foreach ((string _, XMLFieldHandler field) in fieldHandlers)
            {
                if (field.Skip) continue;

                object? propertyVal = field.ReflectionInfo.GetValue(obj);

                // If the value is the default of the type don't serialize it.
                // Complex value types will serialize field defaults though, unlike complex reference types.
                if (field.TypeHandler.IsTypeDefault(propertyVal)) continue;

                field.TypeHandler.Serialize(propertyVal, output, indentation + 1, recursionChecker, field.Name);
            }

            output.AppendJoin(XMLFormat.IndentChar, new string[indentation]);
        }

        public override object? Deserialize(XMLReader input)
        {
            int depth = input.Depth;
            object? newObj = Activator.CreateInstance(Type, true);
            if (newObj == null) return null;

            input.GoToNextTag();
            while (input.Depth >= depth && !input.Finished)
            {
                XMLTypeHandler? inheritedHandler = XMLHelpers.GetInheritedTypeHandlerFromXMLTag(input, out string currentTag);
                if (!_fieldHandlers.Value.TryGetValue(currentTag, out XMLFieldHandler? field))
                {
                    Engine.Log.Warning($"Couldn't find handler for field - {currentTag}", MessageSource.XML);
                    return newObj;
                }

                XMLTypeHandler? typeHandler = inheritedHandler ?? field?.TypeHandler;
                if (field != null && typeHandler != null)
                {
                    object? val = typeHandler.Deserialize(input);
                    if (!field.Skip) field.ReflectionInfo.SetValue(newObj, val);
                }

                input.GoToNextTag();
            }

            return newObj;
        }
    }
}