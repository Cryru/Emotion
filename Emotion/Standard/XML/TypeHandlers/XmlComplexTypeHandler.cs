#region Using

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using Emotion.Common;
using Emotion.Standard.Logging;

#endregion

namespace Emotion.Standard.XML.TypeHandlers
{
    public class XmlComplexTypeHandler : XMLTypeHandler
    {
        public override bool CanBeInherited
        {
            get => true;
        }

        public override bool RecursiveType
        {
            get
            {
                if (_recursiveType != null) return _recursiveType.Value;
                _recursiveType = _fieldHandlers.Value.Any(x => x.Value.TypeHandler.IsRecursiveWith(Type));
                return _recursiveType.Value;
            }
            protected set => _recursiveType = value;
        }

        private bool? _recursiveType;

        /// <summary>
        /// Handlers for all fields of this type.
        /// These have to be lazily initialized as they may contain referenced to this type handler.
        /// </summary>
        protected Lazy<Dictionary<string, XmlFieldHandler>> _fieldHandlers;

        /// <summary>
        /// The handler for this type's base class (if any).
        /// </summary>
        private XmlComplexTypeHandler _baseClass;

        private object _defaultValue;

        public XmlComplexTypeHandler(Type type) : base(type)
        {
            // Value types are complex and have custom defaults.
            if (Type.IsValueType) _defaultValue = Activator.CreateInstance(Type, true);

            // Check if inheriting anything.
            if (!Type.IsValueType && Type.BaseType != typeof(object)) _baseClass = (XmlComplexTypeHandler) XmlHelpers.GetTypeHandler(Type.BaseType);

            _fieldHandlers = new Lazy<Dictionary<string, XmlFieldHandler>>(IndexFields);
        }

        private Dictionary<string, XmlFieldHandler> IndexFields()
        {
            // Gather fields and create field handlers for them.
            PropertyInfo[] properties = Type.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);
            FieldInfo[] fields = Type.GetFields(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);
            var fieldHandlers = new Dictionary<string, XmlFieldHandler>(properties.Length + fields.Length);

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
                fieldHandlers.TryAdd(property.Name, handler);
            }

            for (var i = 0; i < fields.Length; i++)
            {
                FieldInfo field = fields[i];

                // Exclude fields marked as "DontSerialize"
                if (field.CustomAttributes.Any(x => x.AttributeType == XmlHelpers.DontSerializeAttributeType)) continue;

                XmlFieldHandler handler = ResolveFieldHandler(field.FieldType, new XmlReflectionHandler(field));
                if (handler == null) continue;
                fieldHandlers.TryAdd(field.Name, handler);
            }

            return fieldHandlers;
        }

        public override void Serialize(object obj, StringBuilder output, int indentation, XmlRecursionChecker recursionChecker)
        {
            Debug.Assert(Type.IsInstanceOfType(obj));
            output.Append("\n");
            SerializeFields(obj, output, indentation, recursionChecker);
            output.AppendJoin(XmlFormat.IndentChar, new string[indentation]);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SerializeFields(object obj, StringBuilder output, int indentation, XmlRecursionChecker recursionChecker)
        {
            _baseClass?.SerializeFields(obj, output, indentation, recursionChecker);
            foreach ((string _, XmlFieldHandler lazyHandler) in _fieldHandlers.Value)
            {
                XmlFieldHandler handler = lazyHandler;
                handler?.Serialize(handler.ReflectionInfo.GetValue(obj), output, indentation, recursionChecker);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override bool ShouldSerialize(object obj)
        {
            return obj != null && !obj.Equals(_defaultValue);
        }

        public override object Deserialize(XmlReader input)
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
            if (_fieldHandlers.Value.TryGetValue(tag, out handler)) return true;
            return _baseClass != null && _baseClass.GetFieldHandler(tag, out handler);
        }

        protected XmlFieldHandler ResolveFieldHandler(Type type, XmlReflectionHandler property)
        {
            // Recursive
            if (Type != type) return XmlHelpers.ResolveFieldHandler(type, property);
            RecursiveType = true;
            return new XmlFieldHandler(property, this, true);
        }
    }
}