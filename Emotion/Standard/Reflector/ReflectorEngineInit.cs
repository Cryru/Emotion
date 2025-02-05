#nullable enable

using Emotion.Standard.Reflector.Handlers;

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
        ReflectorEngine.RegisterTypeHandler(new PrimitiveNumericTypeHandler<char>());

        ReflectorEngine.RegisterTypeHandler(new PrimitiveNumericTypeHandler<float>());
        ReflectorEngine.RegisterTypeHandler(new PrimitiveNumericTypeHandler<double>());

        ReflectorEngine.RegisterTypeHandler(new PrimitiveNumericTypeHandler<decimal>());

        ReflectorEngine.RegisterTypeHandler(new StringTypeHandler());

        OnInit?.Invoke();
        ReflectorEngine.BuildRelations();
    }
}
