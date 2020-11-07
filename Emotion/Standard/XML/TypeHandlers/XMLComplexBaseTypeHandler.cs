#region Using

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
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

                var excludeProp = property.GetCustomAttribute<ExcludeMembersAttribute>();
                XMLFieldHandler handler = XMLHelpers.ResolveFieldHandlerWithExclusions(property.PropertyType, new ReflectedMemberHandler(property), excludeProp);
                if (handler == null) continue;
                fieldHandlers.TryAdd(property.Name, handler);
            }

            for (var i = 0; i < fields.Length; i++)
            {
                FieldInfo field = fields[i];

                // Exclude fields marked as "DontSerialize"
                if (field.CustomAttributes.Any(x => x.AttributeType == XMLHelpers.DontSerializeAttributeType)) continue;

                var excludeProp = field.GetCustomAttribute<ExcludeMembersAttribute>();
                XMLFieldHandler handler = XMLHelpers.ResolveFieldHandlerWithExclusions(field.FieldType, new ReflectedMemberHandler(field), excludeProp);
                if (handler == null) continue;
                fieldHandlers.TryAdd(field.Name, handler);
            }

            return fieldHandlers;
        }

        #region Exclusion Support

        /// <summary>
        /// Any applied exclusions.
        /// </summary>
        protected HashSet<string> _exclusions;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsFieldExcluded(XMLFieldHandler fieldHandler)
        {
            return _exclusions != null && _exclusions.Contains(fieldHandler.Name);
        }

        public XMLComplexBaseTypeHandler DeriveWithExclusions(ExcludeMembersAttribute exclusions)
        {
            var clone = (XMLComplexBaseTypeHandler) MemberwiseClone();
            clone._exclusions = exclusions.Members;
            return clone;
        }

        #endregion
    }
}