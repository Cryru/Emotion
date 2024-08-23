#nullable enable

using Emotion.Standard.Reflector.Handlers;
using System.Runtime.CompilerServices;

namespace Emotion.Standard.Reflector;

public static class ReflectorEngine
{
    private static Dictionary<Type, IReflectorTypeHandler> _typeHandlers = new();

    public static void RegisterTypeHandler(IReflectorTypeHandler typeHandler)
    {
        _typeHandlers.Add(typeHandler.Type, typeHandler);
    }

    public static IReflectorTypeHandler? GetTypeHandler(Type typ)
    {
        if (_typeHandlers.TryGetValue(typ, out IReflectorTypeHandler? handler)) return handler;
        return null;
    }
}
