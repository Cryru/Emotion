#nullable enable

using Emotion.Standard.Reflector.Handlers;
using System.Runtime.InteropServices;

namespace Emotion.Standard.Reflector;

public static class ReflectorEngine
{
    private static Dictionary<Type, IGenericReflectorTypeHandler> _typeHandlers = new();
    private static Dictionary<Type, Type[]> _typeRelations = new();
    private static Dictionary<int, Type> _typeNameToType = new();

    internal static void PostInit()
    {
        BuildRelations();
        Engine.Log.Info($"Loaded {_typeHandlers.Count} type handlers!", "Reflector");
    }

    public static void RegisterTypeHandler(IGenericReflectorTypeHandler typeHandler)
    {
        Type type = typeHandler.Type;
        if (_typeHandlers.ContainsKey(type)) return;
        _typeHandlers.Add(type, typeHandler);

        int hash = typeHandler.TypeName.GetStableHashCode();
        if (_typeNameToType.ContainsKey(hash)) return;
        _typeNameToType.Add(hash, type);
    }

    public static IGenericReflectorTypeHandler? GetTypeHandler(Type typ)
    {
        if (_typeHandlers.TryGetValue(typ, out IGenericReflectorTypeHandler? handler)) return handler;
        return null;
    }

    public static ReflectorTypeHandlerBase<T>? GetTypeHandler<T>()
    {
        Type typ = typeof(T);
        if (_typeHandlers.TryGetValue(typ, out IGenericReflectorTypeHandler? handler)) return (ReflectorTypeHandlerBase<T>)handler;
        return null;
    }

    public static IGenericReflectorComplexTypeHandler? GetComplexTypeHandler<T>()
    {
        Type typ = typeof(T);
        if (_typeHandlers.TryGetValue(typ, out IGenericReflectorTypeHandler? handler))
            return (IGenericReflectorComplexTypeHandler)handler;
        return null;
    }

    public static IGenericReflectorTypeHandler? GetTypeHandlerByName(string name)
    {
        return GetTypeHandlerByName(name.AsSpan());
    }

    public static IGenericReflectorTypeHandler? GetTypeHandlerByName(ReadOnlySpan<char> name)
    {
        ReadOnlySpan<byte> asBytes = MemoryMarshal.Cast<char, byte>(name);
        int hash = asBytes.GetStableHashCode();
        if (!_typeNameToType.TryGetValue(hash, out Type? typ)) return null;

        return GetTypeHandler(typ);
    }

    #region Relations

    private static void BuildRelations()
    {
        _typeRelations.Clear();

        foreach ((Type typ, IGenericReflectorTypeHandler handler) in _typeHandlers)
        {
            if (handler is not IGenericReflectorComplexTypeHandler) continue;

            Type[]? descendants = null;
            foreach ((Type candidate, IGenericReflectorTypeHandler _) in _typeHandlers)
            {
                if (handler is not IGenericReflectorComplexTypeHandler) continue;

                Type? baseType = candidate.BaseType;
                if (baseType == null) continue;

                bool isDescended = baseType == typ;
                if (!isDescended)
                {
                    // Deep search
                    while (baseType != null)
                    {
                        baseType = baseType.BaseType;
                        if (baseType == typ)
                        {
                            isDescended = true;
                            break;
                        }
                    }
                }

                if (!isDescended) continue;

                if (descendants == null)
                    descendants = new Type[1];
                else
                    Array.Resize(ref descendants, descendants.Length + 1);

                descendants[^1] = candidate;
            }

            if (descendants != null) _typeRelations.Add(typ, descendants);
        }
    }

    public static Type[] GetTypesDescendedFrom(Type typ)
    {
        if (_typeRelations.ContainsKey(typ)) return _typeRelations[typ];
        return Array.Empty<Type>();
    }

    public static Type? GetBaseType(Type typ)
    {
        return typ.BaseType;
    }

    #endregion
}
