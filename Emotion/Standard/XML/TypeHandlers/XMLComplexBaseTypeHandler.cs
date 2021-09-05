#region Using

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using Emotion.Common.Serialization;

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

        /// <summary>
        /// Fields not to be serialized by the type, or member field.
        /// </summary>
        protected HashSet<string> _excludedMembers;

        protected XMLComplexBaseTypeHandler(Type type) : base(type)
        {
            // Check if the type is excluding any fields.
            DontSerializeMembersAttribute exclusion = null;
            object[] exclusions = type.GetCustomAttributes(typeof(DontSerializeMembersAttribute), false);
            if (exclusions.Length > 0) exclusion = exclusions[0] as DontSerializeMembersAttribute;
            _excludedMembers = exclusion?.Members;

            _fieldHandlers = new Lazy<Dictionary<string, XMLFieldHandler>>(IndexFields);
        }

        protected virtual Dictionary<string, XMLFieldHandler> IndexFields()
        {
            // Gather fields and create field handlers for them.
            PropertyInfo[] properties = Type.GetProperties(BindingFlags.Public | BindingFlags.Instance);
            FieldInfo[] fields = Type.GetFields(BindingFlags.Public | BindingFlags.Instance);
            var fieldHandlers = new Dictionary<string, XMLFieldHandler>(properties.Length + fields.Length);

            for (var i = 0; i < properties.Length; i++)
            {
                PropertyInfo property = properties[i];

                // Only serialize properties with get and set methods;
                MethodInfo readMethod = property.GetMethod;
                MethodInfo writeMethod = property.SetMethod;
                if (!property.CanRead || !property.CanWrite || readMethod == null || writeMethod == null) continue;

                bool nonPublicAllowed = property.GetCustomAttribute<SerializeNonPublicGetSetAttribute>() != null;
                bool valid = nonPublicAllowed || readMethod.IsPublic && writeMethod.IsPublic;
                if (!valid) continue;

                var excludeProp = property.GetCustomAttribute<DontSerializeMembersAttribute>();

                // Mark members marked as "DontSerialize" or "DontSerializeMembers" as skip.
                bool skip = property.GetCustomAttribute<DontSerializeAttribute>() != null;
                string propertyName = property.Name;
                if (!skip && _excludedMembers != null && _excludedMembers.Contains(propertyName)) skip = true;

                var reflectionHandler = new ReflectedMemberHandler(property);
                XMLFieldHandler handler = skip ? new XMLSkippedMember(reflectionHandler) : XMLHelpers.ResolveFieldHandlerWithExclusions(property.PropertyType, reflectionHandler, excludeProp);
                if (handler == null) continue;

                fieldHandlers.TryAdd(propertyName, handler);
            }

            for (var i = 0; i < fields.Length; i++)
            {
                FieldInfo field = fields[i];

                var excludeProp = field.GetCustomAttribute<DontSerializeMembersAttribute>();

                bool skip = field.GetCustomAttribute<DontSerializeAttribute>() != null;
                string fieldName = field.Name;
                if (!skip && _excludedMembers != null && _excludedMembers.Contains(fieldName)) skip = true;

                var reflectionHandler = new ReflectedMemberHandler(field);
                XMLFieldHandler handler = skip ? new XMLSkippedMember(reflectionHandler) : XMLHelpers.ResolveFieldHandlerWithExclusions(field.FieldType, reflectionHandler, excludeProp);
                if (handler == null) continue;

                fieldHandlers.TryAdd(fieldName, handler);
            }

            return fieldHandlers;
        }

        // For debugging purposes.
        protected XMLComplexBaseTypeHandler _clonedFrom;

        public XMLComplexBaseTypeHandler CloneWithExclusions(DontSerializeMembersAttribute exclusions)
        {
            Debug.Assert(exclusions != null);

            var clone = (XMLComplexBaseTypeHandler)MemberwiseClone();
            clone._clonedFrom = this;

            // Copy all field handlers to the clone, except the ones that are excluded.
            Dictionary<string, XMLFieldHandler> fieldHandlers = _fieldHandlers.Value;
            var cloneHandlers = new Dictionary<string, XMLFieldHandler>();
            HashSet<string> excludedFields = exclusions.Members;
            foreach (KeyValuePair<string, XMLFieldHandler> fields in fieldHandlers)
            {
                XMLFieldHandler fieldHandler = fields.Value;
                if (excludedFields.Contains(fields.Key)) fieldHandler = new XMLFieldHandler(fieldHandler.ReflectionInfo, fieldHandler.TypeHandler) { Skip = true };
                cloneHandlers.Add(fields.Key, fieldHandler);
            }

            clone._fieldHandlers = new Lazy<Dictionary<string, XMLFieldHandler>>(cloneHandlers);

            return clone;
        }

        public int FieldCount()
        {
            Dictionary<string, XMLFieldHandler> handlers = _fieldHandlers.Value;
            return handlers.Count;
        }

        public IEnumerator<XMLFieldHandler> EnumFields()
        {
            Dictionary<string, XMLFieldHandler> handlers = _fieldHandlers.Value;
            foreach (KeyValuePair<string, XMLFieldHandler> field in handlers)
            {
                XMLFieldHandler fieldHandler = field.Value;
                if (fieldHandler.Skip) continue;
                yield return fieldHandler;
            }
        }
    }
}