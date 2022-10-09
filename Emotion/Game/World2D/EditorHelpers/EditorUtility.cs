#region Using

using System;
using System.Collections.Generic;
using System.Reflection;
using Emotion.Utility;

#endregion

namespace Emotion.Game.World2D.EditorHelpers
{
    public static class EditorUtility
    {
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
    }
}