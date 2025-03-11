#nullable enable

using Emotion.Standard.Reflector.Handlers;

namespace Emotion.Standard.Reflector;

public static class ReflectorEngineInit
{
    public static event Action? OnInit;

    public static void Init()
    {
        Engine.Log.Info("Initializing Reflector...", "Reflector");

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

        // All code generated handlers are attached to this event.
        OnInit?.Invoke();

        // Build relations and other meta.
        ReflectorEngine.PostInit();
    }
}
