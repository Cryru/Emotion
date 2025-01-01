#nullable enable

using Emotion.Standard.Reflector.Handlers;
using System.Runtime.CompilerServices;

namespace Emotion.Standard.Reflector;

public static class ReflectorEngineInit
{
    public static event Action? OnInit;

    public static void Init()
    {
        ReflectorEngine.RegisterTypeHandler(new PrimitiveNumericTypeHandler<byte>());
        ReflectorEngine.RegisterTypeHandler(new PrimitiveNumericTypeHandler<ushort>());
        ReflectorEngine.RegisterTypeHandler(new PrimitiveNumericTypeHandler<uint>());
        ReflectorEngine.RegisterTypeHandler(new PrimitiveNumericTypeHandler<ulong>());

        ReflectorEngine.RegisterTypeHandler(new PrimitiveNumericTypeHandler<sbyte>());
        ReflectorEngine.RegisterTypeHandler(new PrimitiveNumericTypeHandler<short>());
        ReflectorEngine.RegisterTypeHandler(new PrimitiveNumericTypeHandler<int>());
        ReflectorEngine.RegisterTypeHandler(new PrimitiveNumericTypeHandler<long>());

        ReflectorEngine.RegisterTypeHandler(new PrimitiveNumericTypeHandler<float>());
        ReflectorEngine.RegisterTypeHandler(new PrimitiveNumericTypeHandler<double>());

        ReflectorEngine.RegisterTypeHandler(new PrimitiveNumericTypeHandler<decimal>());

        ReflectorEngine.RegisterTypeHandler(new StringTypeHandler());

        OnInit?.Invoke();
    }
}

public static class ReflectorEngine
{
    private static Dictionary<Type, IGenericReflectorTypeHandler> _typeHandlers = new();

    public static void RegisterTypeHandler(IGenericReflectorTypeHandler typeHandler)
    {
        Type type = typeHandler.Type;
        if (_typeHandlers.ContainsKey(type)) return;
        _typeHandlers.Add(type, typeHandler);
    }

    public static IGenericReflectorTypeHandler? GetTypeHandler(Type typ)
    {
        if (_typeHandlers.TryGetValue(typ, out IGenericReflectorTypeHandler? handler)) return handler;
        return null;
    }

    public static ReflectorTypeHandlerBase<T>? GetTypeHandler<T>()
    {
        var typ = typeof(T);
        if (_typeHandlers.TryGetValue(typ, out IGenericReflectorTypeHandler? handler)) return (ReflectorTypeHandlerBase<T>) handler;
        return null;
    }
}
