#nullable enable

using Emotion.Standard.Reflector.Handlers;
using System.Runtime.InteropServices;

namespace Emotion.Standard.Reflector;

public static class ReflectorEngine
{
    private static Dictionary<Type, IGenericReflectorTypeHandler> _typeHandlers = new();
    private static Dictionary<Type, Type[]> _typeRelations = new();
    private static Dictionary<Type, Type[]> _typeRelationsDirect = new();
    private static Dictionary<int, Type> _typeNameToType = new();

    internal static void Init()
    {
        // Setup built in types
        ReflectorEngine.RegisterTypeHandler(new PrimitiveNumericTypeHandler<byte>());
        ReflectorEngine.RegisterTypeHandler(new ArrayTypeHandler<byte[], byte>());

        ReflectorEngine.RegisterTypeHandler(new PrimitiveNumericTypeHandler<ushort>());
        ReflectorEngine.RegisterTypeHandler(new ArrayTypeHandler<ushort[], ushort>());

        ReflectorEngine.RegisterTypeHandler(new PrimitiveNumericTypeHandler<uint>());
        ReflectorEngine.RegisterTypeHandler(new ArrayTypeHandler<uint[], uint>());

        ReflectorEngine.RegisterTypeHandler(new PrimitiveNumericTypeHandler<ulong>());
        ReflectorEngine.RegisterTypeHandler(new ArrayTypeHandler<ulong[], ulong>());

        ReflectorEngine.RegisterTypeHandler(new PrimitiveNumericTypeHandler<sbyte>());
        ReflectorEngine.RegisterTypeHandler(new ArrayTypeHandler<sbyte[], sbyte>());

        ReflectorEngine.RegisterTypeHandler(new PrimitiveNumericTypeHandler<short>());
        ReflectorEngine.RegisterTypeHandler(new ArrayTypeHandler<short[], short>());

        ReflectorEngine.RegisterTypeHandler(new PrimitiveNumericTypeHandler<int>());
        ReflectorEngine.RegisterTypeHandler(new ArrayTypeHandler<int[], int>());

        ReflectorEngine.RegisterTypeHandler(new PrimitiveNumericTypeHandler<long>());
        ReflectorEngine.RegisterTypeHandler(new ArrayTypeHandler<long[], long>());

        ReflectorEngine.RegisterTypeHandler(new PrimitiveNumericTypeHandler<char>());
        ReflectorEngine.RegisterTypeHandler(new ArrayTypeHandler<char[], char>());

        ReflectorEngine.RegisterTypeHandler(new PrimitiveNumericTypeHandler<float>());
        ReflectorEngine.RegisterTypeHandler(new ArrayTypeHandler<float[], float>());

        ReflectorEngine.RegisterTypeHandler(new PrimitiveNumericTypeHandler<double>());
        ReflectorEngine.RegisterTypeHandler(new ArrayTypeHandler<float[], float>());

        ReflectorEngine.RegisterTypeHandler(new PrimitiveNumericTypeHandler<decimal>());
        ReflectorEngine.RegisterTypeHandler(new ArrayTypeHandler<decimal[], decimal>());

        ReflectorEngine.RegisterTypeHandler(new StringTypeHandler());
        ReflectorEngine.RegisterTypeHandler(new ArrayTypeHandler<string[], string>());

        ReflectorEngine.RegisterTypeHandler(new BooleanTypeHandler());
        ReflectorEngine.RegisterTypeHandler(new ArrayTypeHandler<bool[], bool>());


        BuildRelations();
        CallHandlersPostInit();
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
        if (_typeHandlers.TryGetValue(typ, out IGenericReflectorTypeHandler? handler))
            return handler;
        return null;
    }

    public static ReflectorTypeHandlerBase<T>? GetTypeHandler<T>()
    {
        Type typ = typeof(T);
        if (_typeHandlers.TryGetValue(typ, out IGenericReflectorTypeHandler? handler))
            return (ReflectorTypeHandlerBase<T>)handler;
        return null;
    }

    public static IGenericReflectorComplexTypeHandler? GetComplexTypeHandler<T>()
    {
        Type typ = typeof(T);
        if (_typeHandlers.TryGetValue(typ, out IGenericReflectorTypeHandler? handler))
            return (IGenericReflectorComplexTypeHandler)handler;
        return null;
    }

    public static IGenericReflectorComplexTypeHandler? GetComplexTypeHandler(Type typ)
    {
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
            Type[]? descendantsDirect = null;
            foreach ((Type candidate, IGenericReflectorTypeHandler _) in _typeHandlers)
            {
                if (handler is not IGenericReflectorComplexTypeHandler) continue;
                if (candidate == typ) continue;

                Type? baseType = candidate.BaseType;
                if (baseType == null) continue;

                bool isDescended = baseType == typ;
                if (isDescended) // Direct descendant
                {
                    if (descendantsDirect == null)
                        descendantsDirect = new Type[1];
                    else
                        Array.Resize(ref descendantsDirect, descendantsDirect.Length + 1);

                    descendantsDirect[^1] = candidate;
                }
                else // If not direct descendant do a deep search up.
                {
                    while (baseType != null)
                    {
                        baseType = baseType.BaseType;
                        if (baseType == typ)
                        {
                            isDescended = true;
                            break;
                        }
                    }

                    // Deep search found nothing
                    if (!isDescended) continue;
                }

                if (descendants == null)
                    descendants = new Type[1];
                else
                    Array.Resize(ref descendants, descendants.Length + 1);

                descendants[^1] = candidate;
            }

            if (descendants != null) _typeRelations.Add(typ, descendants);
            if (descendantsDirect != null) _typeRelationsDirect.Add(typ, descendantsDirect);
        }
    }

    public static Type[] GetTypesDescendedFrom(Type typ, bool directOnly = false)
    {
        Dictionary<Type, Type[]> dict = directOnly ? _typeRelationsDirect : _typeRelations;
        if (dict.TryGetValue(typ, out Type[]? value)) return value;
        return Array.Empty<Type>();
    }

    public static Type? GetBaseType(Type typ)
    {
        return typ.BaseType;
    }

    #endregion

    private static void CallHandlersPostInit()
    {
        foreach ((Type typ, IGenericReflectorTypeHandler handler) in _typeHandlers)
        {
            handler.PostInit();
        }
    }
}
