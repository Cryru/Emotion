#region Using

using System;
using System.Collections.Generic;
using System.Reflection;
using Emotion.Standard.XML;
using Emotion.Standard.XML.TypeHandlers;
using Emotion.Utility;

#endregion

namespace Emotion.Game.World2D.EditorHelpers
{
    public static class EditorUtility
    {
        public class TypeAndFieldHandlers
        {
            public Type DeclaringType;
            public List<XMLFieldHandler> Fields = new();

            public TypeAndFieldHandlers(Type t)
            {
                DeclaringType = t;
            }
        }

        public static List<Type> GetTypesWhichInherit<T>()
        {
            List<Type> inheritors = new();
            Type type = typeof(T);
            foreach (Assembly assembly in Helpers.AssociatedAssemblies)
            {
                Type[] types = assembly.GetTypes();
                foreach (Type assemblyType in types)
                {
                    if (!type.IsAssignableFrom(assemblyType)) continue;

                    bool invalid = assemblyType.GetConstructor(Type.EmptyTypes) == null;
                    if (invalid) continue;

                    inheritors.Add(assemblyType);
                }
            }

            return inheritors;
        }

        public static List<TypeAndFieldHandlers> GetTypeFields<T>(T obj)
        {
            var typeHandler = (XMLComplexBaseTypeHandler) XMLHelpers.GetTypeHandler(obj.GetType());
            List<TypeAndFieldHandlers> currentWindowHandlers = new ();
            currentWindowHandlers.Clear();

            if (typeHandler == null) return currentWindowHandlers;

            // Collect type handlers sorted by declared type.
            IEnumerator<XMLFieldHandler> fields = typeHandler.EnumFields();
            while (fields.MoveNext())
            {
                XMLFieldHandler field = fields.Current;
                if (field == null || field.Name == "Children") continue;

                TypeAndFieldHandlers handlerMatch = null;
                for (var i = 0; i < currentWindowHandlers.Count; i++)
                {
                    TypeAndFieldHandlers handler = currentWindowHandlers[i];
                    if (handler.DeclaringType == field.ReflectionInfo.DeclaredIn)
                    {
                        handlerMatch = handler;
                        break;
                    }
                }

                if (handlerMatch == null)
                {
                    handlerMatch = new TypeAndFieldHandlers(field.ReflectionInfo.DeclaredIn);
                    currentWindowHandlers.Add(handlerMatch);
                }

                handlerMatch.Fields.Add(field);
            }

            // Sort by inheritance.
            var indices = new int[currentWindowHandlers.Count];
            var idx = 0;
            Type t = typeHandler.Type;
            while (t != typeof(object))
            {
                for (var i = 0; i < currentWindowHandlers.Count; i++)
                {
                    TypeAndFieldHandlers handler = currentWindowHandlers[i];
                    if (handler.DeclaringType != t) continue;
                    indices[i] = idx;
                    idx++;
                    break;
                }

                t = t!.BaseType;
            }

            List<TypeAndFieldHandlers> originalIndices = new ();
            originalIndices.AddRange(currentWindowHandlers);

            currentWindowHandlers.Sort((x, y) =>
            {
                int idxX = originalIndices.IndexOf(x);
                int idxY = originalIndices.IndexOf(y);
                return indices[idxY] -  indices[idxX];
            });

            return currentWindowHandlers;
        }

        public static void ReplaceMapButKeepReference(Map2D source, Map2D destination)
        {
	        var typeHandler = (XMLComplexBaseTypeHandler) XMLHelpers.GetTypeHandler(source.GetType());
	        if (typeHandler == null) return;

	        IEnumerator<XMLFieldHandler> enumerator = typeHandler.EnumFields();
	        while (enumerator.MoveNext())
	        {
		        XMLFieldHandler handler = enumerator.Current;
		        object val = handler.ReflectionInfo.GetValue(source);
		        handler.ReflectionInfo.SetValue(destination, val);
	        }
        }
    }
}