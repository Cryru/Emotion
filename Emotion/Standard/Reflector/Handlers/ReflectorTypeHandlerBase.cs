#nullable enable

using Emotion.Standard.OptimizedStringReadWrite;
using Emotion.WIPUpdates.One.EditorUI.ObjectPropertiesEditorHelpers;
using System.Text;

namespace Emotion.Standard.Reflector.Handlers;

public abstract class ReflectorTypeHandlerBase<T> : IGenericReflectorTypeHandler
{
    public abstract Type Type { get; }

    public abstract bool CanGetOrParseValueAsString { get; }

    public virtual IObjectPropertyEditor? GetEditor()
    {
        return null;
    }

    public bool WriteValueAsStringGeneric<TParam>(ref ValueStringWriter stringWriter, TParam? instance)
    {
        if (instance is not T instanceAsT) return false;
        return WriteValueAsString(ref stringWriter, instanceAsT);
    }

    public bool WriteValueAsStringGeneric<TParam>(StringBuilder builder, TParam? instance)
    {
        if (instance is not T instanceAsT) return false;
        return WriteValueAsString(builder, instanceAsT);
    }

    public bool WriteValueAsString(StringBuilder builder, T? instance)
    {
        ValueStringWriter writer = new ValueStringWriter(builder);
        return WriteValueAsString(ref writer, instance);
    }

    public abstract bool WriteValueAsString(ref ValueStringWriter stringWriter, T? instance);

    public bool ParseValueFromStringGeneric<TReader>(TReader reader, out object? result) where TReader : IStringReader
    {
        throw new Exception("Not supported!");
    }

    public abstract bool ParseValueAsString<TReader>(TReader reader, out T? result) where TReader : IStringReader;
}