#region Using

using System;
using System.Collections.Generic;
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
                if (!property.CanRead || !property.CanWrite || readMethod == null || writeMethod == null ||
                    !readMethod.IsPublic || !writeMethod.IsPublic || property.GetCustomAttribute<DontSerializeAttribute>() != null) continue;

                var excludeProp = property.GetCustomAttribute<DontSerializeMembers>();
                XMLFieldHandler handler = XMLHelpers.ResolveFieldHandlerWithExclusions(property.PropertyType, new ReflectedMemberHandler(property), excludeProp);
                if (handler == null) continue;
                fieldHandlers.TryAdd(property.Name, handler);
            }

            for (var i = 0; i < fields.Length; i++)
            {
                FieldInfo field = fields[i];

                // Exclude fields marked as "DontSerialize"
                if (field.GetCustomAttribute<DontSerializeAttribute>() != null) continue;

                var excludeProp = field.GetCustomAttribute<DontSerializeMembers>();
                XMLFieldHandler handler = XMLHelpers.ResolveFieldHandlerWithExclusions(field.FieldType, new ReflectedMemberHandler(field), excludeProp);
                if (handler == null) continue;
                fieldHandlers.TryAdd(field.Name, handler);
            }

            return fieldHandlers;
        }

        #region Exclusion Support

        /// <summary>
        /// Exclusions applied from fields/properties of other classes.
        /// </summary>
        protected HashSet<string> _exclusions;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsFieldExcluded(XMLFieldHandler fieldHandler)
        {
            return _exclusions != null && _exclusions.Contains(fieldHandler.Name);
        }

        protected void AddExclusions(DontSerializeMembers exclusions)
        {
            _exclusions ??= new HashSet<string>();
            foreach (string exclusion in exclusions.Members)
            {
                _exclusions.Add(exclusion);
            }
        }

        protected XMLComplexBaseTypeHandler Clone()
        {
            var clone = (XMLComplexBaseTypeHandler) MemberwiseClone();
            if (clone._exclusions != null)
            {
                clone._exclusions = new HashSet<string>();
                foreach (string exclusion in _exclusions)
                {
                    clone._exclusions.Add(exclusion);
                }
            }
            return clone;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public virtual XMLComplexBaseTypeHandler CloneWithExclusions(DontSerializeMembers exclusions)
        {
            XMLComplexBaseTypeHandler clone = Clone();
            clone.AddExclusions(exclusions);
            return clone;
        }

        #endregion

        public virtual IEnumerator<XMLFieldHandler> EnumFields()
        {
            Dictionary<string, XMLFieldHandler> handlers = _fieldHandlers.Value;
            foreach (KeyValuePair<string, XMLFieldHandler> field in handlers)
            {
                yield return field.Value;
            }
        }
    }
}