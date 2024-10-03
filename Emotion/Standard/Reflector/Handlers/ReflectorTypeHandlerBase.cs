#nullable enable

using Emotion.Standard.ByteReadWrite;
using System.Text;

namespace Emotion.Standard.Reflector.Handlers;

public abstract class ReflectorTypeHandlerBase<T> : IGenericReflectorTypeHandler
{
    public abstract Type Type { get; }

    public abstract bool CanGetOrParseValueAsString { get; }

    public bool WriteValueAsStringGeneric<TParam>(StringBuilder builder, TParam? instance)
    {
        if (instance is not T instanceAsT) return false;
        return WriteValueAsString(builder, instanceAsT);
    }

    public bool ParseValueFromStringGeneric<TReader>(TReader reader, out object? result) where TReader : IStringReader
    {
        throw new Exception("Not supported!");
    }

    public abstract bool WriteValueAsString(StringBuilder builder, T? instance);

    public abstract bool ParseValueAsString<TReader>(TReader reader, out T? result) where TReader : IStringReader;
}