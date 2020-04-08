#region Using

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using Emotion.Common;
using Emotion.Standard.Logging;
using Emotion.Utility;

#endregion

namespace Emotion.Standard.XML
{
    public static class XmlHelpers
    {
        public static readonly Type DontSerializeAttributeType = typeof(DontSerializeAttribute);
        private static readonly Type EnumerableType = typeof(IEnumerable<>);

        /// <summary>
        /// Every complex type is analyzed using reflection to determine how to serialize it.
        /// </summary>
        private static readonly Dictionary<Type, XMLTypeHandler> Handlers = new Dictionary<Type, XMLTypeHandler>();

        public static XMLTypeHandler GetTypeHandler(Type type)
        {
            // Check if type is excluded.
            if(type.CustomAttributes.Any(x => x.AttributeType == DontSerializeAttributeType)) return null;

            if (Handlers.TryGetValue(type, out XMLTypeHandler newHandler)) return newHandler;

            newHandler = new XMLTypeHandler(type);
            Handlers.Add(type, newHandler);
            newHandler.Init();
            return newHandler;
        }

        /// <summary>
        /// Type names which are resolved are added here.
        /// </summary>
        private static readonly Dictionary<string, Type> ResolvedTypes = new Dictionary<string, Type>();

        /// <summary>
        /// Get the type of the specified name.
        /// </summary>
        /// <param name="typeName">The name of the type.</param>
        /// <returns>The type under that name.</returns>
        public static Type GetTypeByName(string typeName)
        {
            if (ResolvedTypes.TryGetValue(typeName, out Type type)) return type;

            for (var i = 0; i < Helpers.AssociatedAssemblies.Length; i++)
            {
                type = Helpers.AssociatedAssemblies[i].GetType(typeName, false, true);
                if (type == null) continue;
                ResolvedTypes.Add(typeName, type);
                return type;
            }

            Engine.Log.Warning($"Couldn't find type - {typeName}", MessageSource.XML);
            return null;
        }

        /// <summary>
        /// Get the XML name of a type. The format is decided by the .Net xml serializer.
        /// </summary>
        /// <param name="type">The type's name to get.</param>
        /// <param name="full">Whether you want the full name - assembly and type.</param>
        /// <returns>The type name.</returns>
        public static string GetTypeName(Type type, bool full = false)
        {
            if (!type.IsArray && !type.IsGenericType) return full ? type.FullName : type.Name;

            var typeName = new StringBuilder();
            if (type.IsArray)
            {
                typeName.Append("ArrayOf");
                typeName.Append(GetTypeName(type.GetElementType()));
                return typeName.ToString();
            }

            Type[] generics = type.GetGenericArguments();
            string name = full ? type.FullName : type.Name;
            if (name == null) return null;

            int genericSeperator = name.IndexOf("`", StringComparison.Ordinal);
            typeName.Append(genericSeperator == -1 ? $"{name}Of" : $"{name.Substring(0, genericSeperator)}Of");

            for (var i = 0; i < generics.Length; i++)
            {
                typeName.Append($"{GetTypeName(generics[i], full)}");
            }
            return typeName.ToString();
        }

        /// <summary>
        /// Some types are transparent, meaning you don't see them.
        /// This function returns the type you want to see.
        /// </summary>
        /// <param name="type">The type to check.</param>
        /// <param name="opaque">Whether the passed in type is opaque itself.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Type GetOpaqueType(Type type, out bool opaque)
        {
            // Nullable types are transparent in XML.
            Type underNullable = Nullable.GetUnderlyingType(type);
            if (underNullable != null)
            {
                opaque = false;
                return underNullable;
            }

            opaque = true;
            return type;
        }

        /// <summary>
        /// Resolve the handler for a specified field.
        /// </summary>
        /// <param name="type">The field type.</param>
        /// <param name="property">The reflection handler for the field.</param>
        /// <param name="referencingType">The type containing this field. Used for recursion checks.</param>
        /// <returns>A handler for the specified field.</returns>
        public static XmlFieldHandler ResolveFieldHandler(Type type, XmlReflectionHandler property, Type referencingType = null)
        {
            type = GetOpaqueType(type, out bool _);

            string typeName = GetTypeName(type);
            if(!ResolvedTypes.ContainsKey(typeName))
            {
                ResolvedTypes.Add(typeName, type);
            }

            // Trivial types.
            if (XmlTrivialFieldHandler.TypeIsTrivial(type)) return new XmlTrivialFieldHandler(property);

            // Arrays and array-like
            if (type.IsArray || type.GetInterface("IEnumerable") != null)
            {
                Type elementType = type.GetElementType() ?? type.GetGenericArguments().FirstOrDefault();
                if (elementType == null) return null;
                XmlFieldHandler elementTypeHandler = ResolveFieldHandler(elementType, new XmlReflectionHandler(elementType), referencingType);
                if(elementTypeHandler == null) return null;

                var arrayHandler = new XmlArrayFieldHandler(property, elementTypeHandler);

                // Recursion with parent.
                if (referencingType != null && referencingType.IsAssignableFrom(elementType)) arrayHandler.RecursionCheck = true;

                // Recursion with self.
                if (type.IsAssignableFrom(elementType)) arrayHandler.RecursionCheck = true;
                return arrayHandler;
            }

            // Complex type
            XMLTypeHandler typeHandler = GetTypeHandler(type);
            if(typeHandler == null) return null; // Excluded or some other error.
            var complexHandler = new XmlComplexFieldHandler(property, typeHandler);
            if (referencingType != null && type.IsSubclassOf(referencingType)) complexHandler.RecursionCheck = true;
            return complexHandler;
        }
    }
}