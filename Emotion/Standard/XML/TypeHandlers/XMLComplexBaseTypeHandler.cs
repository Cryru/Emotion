#region Using

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;

#endregion

namespace Emotion.Standard.XML.TypeHandlers
{
    public abstract class XMLComplexBaseTypeHandler : XMLTypeHandler
    {
        /// <summary>
        /// Handlers for all fields of this type.
        /// These have to be lazily initialized as they may contain referenced to this type handler.
        /// </summary>
        protected Lazy<Dictionary<string, XMLFieldHandler>> _fieldHandlers;

        protected XMLComplexBaseTypeHandler(Type type) : base(type)
        {
            _fieldHandlers = new Lazy<Dictionary<string, XMLFieldHandler>>(IndexFields);
        }

        protected virtual Dictionary<string, XMLFieldHandler> IndexFields()
        {
            // Gather fields and create field handlers for them.
            PropertyInfo[] properties = Type.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);
            FieldInfo[] fields = Type.GetFields(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);
            var fieldHandlers = new Dictionary<string, XMLFieldHandler>(properties.Length + fields.Length);

            for (var i = 0; i < properties.Length; i++)
            {
                PropertyInfo property = properties[i];

                // Only serialize properties with public get; set; and who aren't marked as "DontSerialize"
                MethodInfo readMethod = property.GetMethod;
                MethodInfo writeMethod = property.SetMethod;
                if (!property.CanRead || !property.CanWrite || readMethod == null || writeMethod == null || !readMethod.IsPublic || !writeMethod.IsPublic ||
                    property.CustomAttributes.Any(x => x.AttributeType == XMLHelpers.DontSerializeAttributeType)) continue;

                XMLFieldHandler handler = XMLHelpers.ResolveFieldHandler(property.PropertyType, new XMLReflectionHandler(property));
                if (handler == null) continue;
                fieldHandlers.TryAdd(property.Name, handler);
            }

            for (var i = 0; i < fields.Length; i++)
            {
                FieldInfo field = fields[i];

                // Exclude fields marked as "DontSerialize"
                if (field.CustomAttributes.Any(x => x.AttributeType == XMLHelpers.DontSerializeAttributeType)) continue;

                XMLFieldHandler handler = XMLHelpers.ResolveFieldHandler(field.FieldType, new XMLReflectionHandler(field));
                if (handler == null) continue;
                fieldHandlers.TryAdd(field.Name, handler);
            }

            return fieldHandlers;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected virtual void SerializeFields(object obj, StringBuilder output, int indentation, XMLRecursionChecker recursionChecker)
        {
            Dictionary<string, XMLFieldHandler> fieldHandlers = _fieldHandlers.Value;
            foreach ((string _, XMLFieldHandler field) in fieldHandlers)
            {
                object propertyVal = field.ReflectionInfo.GetValue(obj);
                string fieldName = field.Name;
                field.TypeHandler.Serialize(propertyVal, output, indentation, recursionChecker, fieldName);
            }
        }
    }
}