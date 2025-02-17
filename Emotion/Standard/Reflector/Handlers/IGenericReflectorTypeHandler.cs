#nullable enable

using Emotion.Standard.OptimizedStringReadWrite;
using Emotion.WIPUpdates.One.EditorUI.ObjectPropertiesEditorHelpers;
using System.Text;

namespace Emotion.Standard.Reflector.Handlers;

public interface IGenericReflectorTypeHandler
{
    public Type Type { get; }

    /// <summary>
    /// Whether the type handler's value can be get/set as a string.
    /// </summary>
    public bool CanGetOrParseValueAsString { get; }

    public virtual TypeEditor? GetEditor()
    {
        return null;
    }

    public bool WriteValueAsStringGeneric<TParam>(ref ValueStringWriter stringWriter, TParam? instance)
    {
        throw new Exception("Not supported!");
    }

    public bool WriteValueAsStringGeneric<TParam>(StringBuilder builder, TParam? instance)
    {
        ValueStringWriter writer = new ValueStringWriter(builder);
        return WriteValueAsStringGeneric<TParam>(ref writer, instance);
    }

    public bool ParseValueFromStringGeneric<TParam>(ReadOnlySpan<char> data, out TParam? result)
    {
        throw new Exception("Not supported!");
    }
}
