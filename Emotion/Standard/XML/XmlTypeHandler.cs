#region Using

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using Emotion.Common;
using Emotion.Standard.Logging;

#endregion

namespace Emotion.Standard.XML
{
    /// <summary>
    /// Object which knows how to handle the serialization and deserialization of complex types (non primitives).
    /// </summary>
    public class XMLTypeHandler
    {
        /// <summary>
        /// The type this handler relates to.
        /// </summary>
        public Type Type { get; }

        /// <summary>
        /// Whether this type a reference to itself or its base type/s.
        /// </summary>
        public bool PossibleRecursion { get; set; }

        /// <summary>
        /// Handlers for all fields of this type.
        /// </summary>
        private Dictionary<string, XmlFieldHandler> _fieldHandlers;

        /// <summary>
        /// The handler for this type's base class (if any).
        /// </summary>
        private XMLTypeHandler _baseClass;

        public XMLTypeHandler(Type type)
        {
            Type = type;
        }

        public void Init()
        {
            // Check if inheriting anything.
            if (!Type.IsValueType && Type.BaseType != typeof(object)) _baseClass = XmlHelpers.GetTypeHandler(Type.BaseType);

            // Gather fields and create field handlers for them.
            PropertyInfo[] properties = Type.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);
            _fieldHandlers = new Dictionary<string, XmlFieldHandler>(properties.Length);

            for (var i = 0; i < properties.Length; i++)
            {
                PropertyInfo property = properties[i];

                // Only serialize properties with public get; set; and who aren't marked as "DontSerialize"
                MethodInfo readMethod = property.GetMethod;
                MethodInfo writeMethod = property.SetMethod;
                if (!property.CanRead || !property.CanWrite || readMethod == null || writeMethod == null || !readMethod.IsPublic || !writeMethod.IsPublic ||
                    property.CustomAttributes.Any(x => x.AttributeType == XmlHelpers.DontSerializeAttributeType)) continue;

                XmlFieldHandler handler = ResolveFieldHandler(property.PropertyType, new XmlReflectionHandler(property));
                if (handler == null) continue;

                if (handler.RecursionCheck) PossibleRecursion = true;
                _fieldHandlers.Add(handler.Name, handler);
            }

            FieldInfo[] fields = Type.GetFields(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);
            for (var i = 0; i < fields.Length; i++)
            {
                FieldInfo field = fields[i];

                // Exclude fields marked as "DontSerialize"
                if (field.CustomAttributes.Any(x => x.AttributeType == XmlHelpers.DontSerializeAttributeType)) continue;

                XmlFieldHandler handler = ResolveFieldHandler(field.FieldType, new XmlReflectionHandler(field));
                if (handler == null) continue;

                if (handler.RecursionCheck) PossibleRecursion = true;
                _fieldHandlers.Add(handler.Name, handler);
            }
        }

        public void Serialize(object obj, StringBuilder output, int indentation, XmlRecursionChecker recursionChecker)
        {
            if (obj == null) return;

            Debug.Assert(Type.IsInstanceOfType(obj));
            _baseClass?.Serialize(obj, output, indentation, recursionChecker);
            foreach ((string _, XmlFieldHandler value) in _fieldHandlers)
            {
                value.Serialize(value.ReflectionInfo.GetValue(obj), output, indentation, recursionChecker);
            }
        }

        public object Deserialize(XmlReader input)
        {
            int depth = input.Depth;
            object newObj = Activator.CreateInstance(Type, true);

            input.GoToNextTag();
            while (input.Depth >= depth && !input.Finished)
            {
                string currentTag = input.ReadTag(out string typeAttribute);
                if (!GetFieldHandler(currentTag, out XmlFieldHandler handler))
                {
                    Engine.Log.Warning($"Couldn't find handler for field - {currentTag}", MessageSource.XML);
                    return newObj;
                }

                // Derived type.
                if (typeAttribute != null)
                {
                    Type derivedType = XmlHelpers.GetTypeByName(typeAttribute);
                    if (derivedType == null)
                        Engine.Log.Warning($"Couldn't find derived type of name {typeAttribute}.", MessageSource.XML);
                    else
                        handler = ResolveFieldHandler(derivedType, handler.ReflectionInfo);
                }

                object val = handler.Deserialize(input);
                handler.ReflectionInfo.SetValue(newObj, val);
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
        public bool GetFieldHandler(string tag, out XmlFieldHandler handler)
        {
            if (_fieldHandlers.TryGetValue(tag, out handler)) return true;
            return _baseClass != null && _baseClass.GetFieldHandler(tag, out handler);
        }

        private XmlFieldHandler ResolveFieldHandler(Type type, XmlReflectionHandler property)
        {
            // Recursive
            if (Type != type) return XmlHelpers.ResolveFieldHandler(type, property, Type);
            PossibleRecursion = true;
            return new XmlComplexFieldHandler(property, this);
        }
    }
}