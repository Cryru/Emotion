﻿#region Using

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using Emotion.Common;
using Emotion.Standard.Logging;
using Emotion.Standard.XML.TypeHandlers;
using Emotion.Utility;

#endregion

namespace Emotion.Standard.XML
{
    public static class XMLHelpers
    {
        public static readonly Type DontSerializeAttributeType = typeof(DontSerializeAttribute);
        public static Type ListType = typeof(List<>);
        public static readonly Type KeyValuePairType = typeof(KeyValuePair<,>);
        public static readonly Type StringType = typeof(string);
        public static readonly Type NullableType = typeof(Nullable<>);

        /// <summary>
        /// Every complex type is analyzed using reflection to determine how to serialize it.
        /// </summary>
        private static readonly LazyConcurrentDictionary<Type, XMLTypeHandler> Handlers = new LazyConcurrentDictionary<Type, XMLTypeHandler>();

        /// <summary>
        /// Type names which are resolved are added here.
        /// </summary>
        private static readonly LazyConcurrentDictionary<string, Type> ResolvedTypes = new LazyConcurrentDictionary<string, Type>();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static XMLTypeHandler GetTypeHandler(Type type)
        {
            // Check if type is excluded.
            if (type.CustomAttributes.Any(x => x.AttributeType == DontSerializeAttributeType)) return null;
            return Handlers.GetOrAddValue(type, TypeHandlerFactory);
        }

        private static XMLTypeHandler TypeHandlerFactory(Type type)
        {
            type = GetOpaqueType(type, out bool opaque);

            // Index name.
            string typeName = GetTypeName(type);
            ResolvedTypes.TryAdd(typeName, new Lazy<Type>(type));

            XMLTypeHandler newHandler = null;

            // Trivial types.
            if (type.IsPrimitive) newHandler = new XMLPrimitiveTypeHandler(type, opaque);
            if (type.IsEnum) newHandler = new XMLEnumTypeHandler(type, opaque);
            if (type == StringType) newHandler = new XMLStringTypeHandler(type);

            // IEnumerable
            if (type.IsArray || type.GetInterface("IEnumerable") != null)
            {
                Type elementType;
                if (type.IsArray)
                {
                    elementType = type.GetElementType();
                    newHandler = new XMLArrayTypeHandler(type, elementType);
                }
                else if (type.GetInterface("IList") != null)
                {
                    elementType = type.GetGenericArguments().FirstOrDefault();
                    newHandler = new XMLListHandler(type, elementType);
                }
                else if (type.GetInterface("IDictionary") != null)
                {
                    // The dictionary is basically an array of key value types. Synthesize this here.
                    Type[] generics = type.GetGenericArguments();
                    Type keyType = generics[0];
                    Type valueType = generics[1];
                    elementType = KeyValuePairType.MakeGenericType(keyType, valueType);
                    newHandler = new XMLDictionaryTypeHandler(type, elementType);
                }
            }

            // KeyValue
            if (type.IsGenericType && type.GetGenericTypeDefinition() == KeyValuePairType) newHandler = new XMLKeyValueTypeHandler(type);

            // Some other type, for sure a complex one.
            if (newHandler == null && type.IsValueType) return new XMLComplexValueTypeHandler(type, opaque);

            return newHandler ?? new XMLComplexTypeHandler(type);
        }

        /// <summary>
        /// Get the type of the specified name.
        /// </summary>
        /// <param name="typeName">The name of the type.</param>
        /// <returns>The type under that name.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Type GetTypeByName(string typeName)
        {
            return ResolvedTypes.GetOrAddValue(typeName, GetTypeByNameFactory);
        }

        private static Type GetTypeByNameFactory(string typeName)
        {
            for (var i = 0; i < Helpers.AssociatedAssemblies.Length; i++)
            {
                Type type = Helpers.AssociatedAssemblies[i].GetType(typeName, false, true);
                if (type == null) continue;
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
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
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
        /// <returns>A handler for the specified field.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static XMLFieldHandler ResolveFieldHandler(Type type, XMLReflectionHandler property)
        {
            XMLTypeHandler typeHandler = GetTypeHandler(type);
            return typeHandler == null ? null : new XMLFieldHandler(property, typeHandler); // TypeHandler is null if an excluded type.
        }

        /// <summary>
        /// Read an XML tag and return the type handler for it if it's a derived type, otherwise return null.
        /// </summary>
        /// <param name="reader">The XML reader primed just before the tag.</param>
        /// <param name="tag">The read tag.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static XMLTypeHandler GetDerivedTypeHandlerFromXMLTag(XMLReader reader, out string tag)
        {
            tag = reader.SerializationReadTagAndTypeAttribute(out string typeAttribute);
            if (typeAttribute == null) return null;
            Type derivedType = GetTypeByName(typeAttribute);
            if (derivedType != null) return GetTypeHandler(derivedType);
            Engine.Log.Warning($"Couldn't find derived type of name {typeAttribute} in array.", MessageSource.XML);
            return null;
        }
    }
}