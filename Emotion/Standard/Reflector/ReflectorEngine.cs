#nullable enable

using Emotion.Standard.Reflector.Handlers;
using Emotion.Standard.Reflector.Handlers.Base;
using Emotion.Standard.Reflector.Handlers.Interfaces;
using System.Dynamic;
using System.Runtime.InteropServices;

namespace Emotion.Standard.Reflector;

public static class ReflectorEngine
{
    private static Dictionary<Type, IGenericReflectorTypeHandler> _typeHandlers = new();
    private static Dictionary<Type, Type[]> _typeRelations = new();
    private static Dictionary<Type, Type[]> _typeRelationsDirect = new();
    private static Dictionary<int, Type> _typeNameToType = new();
    private static bool _postInitCalled = false;

    internal static void Init()
    {
        // Setup built in types
        ReflectorEngine.RegisterTypeHandler(new PrimitiveNumericTypeHandler<byte>());
        ReflectorEngine.RegisterTypeHandler(new ArrayTypeHandler<byte[], byte>());
        ReflectorEngine.RegisterTypeHandler(new ListTypeHandler<List<byte>, byte>());

        ReflectorEngine.RegisterTypeHandler(new PrimitiveNumericTypeHandler<ushort>());
        ReflectorEngine.RegisterTypeHandler(new ArrayTypeHandler<ushort[], ushort>());
        ReflectorEngine.RegisterTypeHandler(new ListTypeHandler<List<ushort>, ushort>());

        ReflectorEngine.RegisterTypeHandler(new PrimitiveNumericTypeHandler<uint>());
        ReflectorEngine.RegisterTypeHandler(new ArrayTypeHandler<uint[], uint>());
        ReflectorEngine.RegisterTypeHandler(new ListTypeHandler<List<uint>, uint>());

        ReflectorEngine.RegisterTypeHandler(new PrimitiveNumericTypeHandler<ulong>());
        ReflectorEngine.RegisterTypeHandler(new ArrayTypeHandler<ulong[], ulong>());
        ReflectorEngine.RegisterTypeHandler(new ListTypeHandler<List<ulong>, ulong>());

        ReflectorEngine.RegisterTypeHandler(new PrimitiveNumericTypeHandler<sbyte>());
        ReflectorEngine.RegisterTypeHandler(new ArrayTypeHandler<sbyte[], sbyte>());
        ReflectorEngine.RegisterTypeHandler(new ListTypeHandler<List<sbyte>, sbyte>());

        ReflectorEngine.RegisterTypeHandler(new PrimitiveNumericTypeHandler<short>());
        ReflectorEngine.RegisterTypeHandler(new ArrayTypeHandler<short[], short>());
        ReflectorEngine.RegisterTypeHandler(new ListTypeHandler<List<short>, short>());

        ReflectorEngine.RegisterTypeHandler(new PrimitiveNumericTypeHandler<int>());
        ReflectorEngine.RegisterTypeHandler(new ArrayTypeHandler<int[], int>());
        ReflectorEngine.RegisterTypeHandler(new ListTypeHandler<List<int>, int>());

        ReflectorEngine.RegisterTypeHandler(new PrimitiveNumericTypeHandler<long>());
        ReflectorEngine.RegisterTypeHandler(new ArrayTypeHandler<long[], long>());
        ReflectorEngine.RegisterTypeHandler(new ListTypeHandler<List<long>, long>());

        ReflectorEngine.RegisterTypeHandler(new PrimitiveNumericTypeHandler<char>());
        ReflectorEngine.RegisterTypeHandler(new ArrayTypeHandler<char[], char>());
        ReflectorEngine.RegisterTypeHandler(new ListTypeHandler<List<char>, char>());

        ReflectorEngine.RegisterTypeHandler(new PrimitiveNumericTypeHandler<float>());
        ReflectorEngine.RegisterTypeHandler(new ArrayTypeHandler<float[], float>());
        ReflectorEngine.RegisterTypeHandler(new ListTypeHandler<List<float>, float>());

        ReflectorEngine.RegisterTypeHandler(new PrimitiveNumericTypeHandler<double>());
        ReflectorEngine.RegisterTypeHandler(new ArrayTypeHandler<double[], double>());
        ReflectorEngine.RegisterTypeHandler(new ListTypeHandler<List<double>, double>());

        ReflectorEngine.RegisterTypeHandler(new PrimitiveNumericTypeHandler<decimal>());
        ReflectorEngine.RegisterTypeHandler(new ArrayTypeHandler<decimal[], decimal>());
        ReflectorEngine.RegisterTypeHandler(new ListTypeHandler<List<decimal>, decimal>());

        ReflectorEngine.RegisterTypeHandler(new StringTypeHandler());
        ReflectorEngine.RegisterTypeHandler(new ArrayTypeHandler<string[], string>());
        ReflectorEngine.RegisterTypeHandler(new ListTypeHandler<List<string?>, string>());

        ReflectorEngine.RegisterTypeHandler(new BooleanTypeHandler());
        ReflectorEngine.RegisterTypeHandler(new ArrayTypeHandler<bool[], bool>());
        ReflectorEngine.RegisterTypeHandler(new ListTypeHandler<List<bool>, bool>());


        BuildRelations();
        CallHandlersPostInit();
        Engine.Log.Info($"Loaded {_typeHandlers.Count} type handlers!", "Reflector");
    }

    internal static void OnHotReload(Type[] updatedTypes)
    {
        for (int i = 0; i < updatedTypes.Length; i++)
        {
            Type type = updatedTypes[i];
#pragma warning disable IL2065 // The method has a DynamicallyAccessedMembersAttribute (which applies to the implicit 'this' parameter), but the value used for the 'this' parameter can not be statically analyzed.
            System.Reflection.MethodInfo? methodInfo = type.GetMethod("LoadReflector", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public);
#pragma warning restore IL2065 // The method has a DynamicallyAccessedMembersAttribute (which applies to the implicit 'this' parameter), but the value used for the 'this' parameter can not be statically analyzed.
            methodInfo?.Invoke(null, null);
        }
    }

    public static void RegisterTypeHandler(IGenericReflectorTypeHandler typeHandler)
    {
        Type type = typeHandler.Type;
        _typeHandlers[type] = typeHandler;

        int hash = typeHandler.TypeName.GetStableHashCode();
        _typeNameToType[hash] = type;

        // Late post init
        if (_postInitCalled)
            typeHandler.PostInit();
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

    public static ComplexTypeHandler<T>? GetComplexTypeHandler<T>()
    {
        Type typ = typeof(T);
        if (_typeHandlers.TryGetValue(typ, out IGenericReflectorTypeHandler? handler))
            return (ComplexTypeHandler<T>) handler;
        return null;
    }

    public static IGenericReflectorComplexTypeHandler? GetComplexTypeHandler(Type typ)
    {
        if (_typeHandlers.TryGetValue(typ, out IGenericReflectorTypeHandler? handler))
            return (IGenericReflectorComplexTypeHandler)handler;
        return null;
    }

    public static IGenericReflectorComplexTypeHandler? GetComplexTypeHandlerByName(string name)
    {
        ReadOnlySpan<byte> asBytes = MemoryMarshal.Cast<char, byte>(name);
        int hash = asBytes.GetStableHashCode();
        if (!_typeNameToType.TryGetValue(hash, out Type? typ)) return null;

        return GetComplexTypeHandler(typ);
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

    public static T? CreateCopyOf<T>(T obj)
    {
        if (obj == null) return obj;

        // todo: generate this for each complex handler
        IGenericReflectorComplexTypeHandler? handler = GetComplexTypeHandler(obj.GetType());
        if (handler == null || !handler.CanCreateNew()) return default;

        T? newObj = (T?) handler.CreateNew();
        if (newObj == null) return newObj;

        IEnumerable<ComplexTypeHandlerMemberBase> members = handler.GetMembersDeep();
        foreach (var member in members)
        {
            if (member.GetValueFromComplexObject(obj, out object? val))
                member.SetValueInComplexObject(newObj, val);
        }

        return newObj;
    }

    // Generic is to ensure that both are the same type.
    public static bool CopyProperties<T>(T from, T to)
    {
        if (from == null || to == null) return false;

        // todo: generate this for each complex handler
        IGenericReflectorComplexTypeHandler? handler = GetComplexTypeHandler(from.GetType());
        if (handler == null) return false;

        IEnumerable<ComplexTypeHandlerMemberBase> members = handler.GetMembersDeep();
        foreach (ComplexTypeHandlerMemberBase member in members)
        {
            if (member.GetValueFromComplexObject(from, out object? val))
                member.SetValueInComplexObject(to, val);
        }
        return true;
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
        _postInitCalled = true;
    }

    public static string GetTypeName(Type typ)
    {
        IGenericReflectorTypeHandler? handler = GetTypeHandler(typ);
        if (handler == null) return typ.Name;
        return handler.TypeName;
    }
}
