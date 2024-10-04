#nullable enable

using Emotion.Standard.OptimizedStringReadWrite;
using System.Text;

namespace Emotion.Standard.Reflector.Handlers;

public interface IGenericReflectorTypeHandler
{
    public Type Type { get; }

    /// <summary>
    /// Whether the type handler's value can be get/set as a string.
    /// </summary>
    public bool CanGetOrParseValueAsString { get; }

    public bool WriteValueAsStringGeneric<T>(ref ValueStringWriter stringWriter, T? instance)
    {
        throw new Exception("Not supported!");
    }

    public bool WriteValueAsStringGeneric<T>(StringBuilder builder, T? instance)
    {
        ValueStringWriter writer = new ValueStringWriter(builder);
        return WriteValueAsStringGeneric<T>(ref writer, instance);
    }

    public bool ParseValueFromStringGeneric<TReader>(TReader reader, out object? result) where TReader : IStringReader
    {
        throw new Exception("Not supported!");
    }

    public ComplexTypeHandlerMember[]? GetMembers()
    {
        return null;
    }
}
