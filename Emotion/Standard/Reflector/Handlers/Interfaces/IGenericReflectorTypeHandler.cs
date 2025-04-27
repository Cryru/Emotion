#nullable enable

using Emotion;
using Emotion.Standard.OptimizedStringReadWrite;
using Emotion.WIPUpdates.One.EditorUI.ObjectPropertiesEditorHelpers;
using System.Text;

namespace Emotion.Standard.Reflector.Handlers.Interfaces;

public interface IGenericReflectorTypeHandler
{
    public string TypeName { get; }

    public Type Type { get; }

    /// <summary>
    /// Whether the type handler's value can be get/set as a string.
    /// </summary>
    public bool CanGetOrParseValueAsString { get; }

    public TypeEditor? GetEditor();

    public void PostInit();

    public bool IsTypeAssignableTo(Type otherType)
    {
        return Type.IsAssignableTo(otherType);
    }

    #region Serialization Write

    public void WriteAsCode<OwnerT>(OwnerT? value, ref ValueStringWriter writer);

    #endregion

    public void ReadValueFromStringIntoArray(ReadOnlySpan<char> str, object array, int idx)
    {
        throw new Exception("Not supported!");
    }

    public void ReadValueFromStringIntoList(ReadOnlySpan<char> str, IList list)
    {
        throw new Exception("Not supported!");
    }

    public void ReadValueFromStringIntoObjectMember(ReadOnlySpan<char> str, object obj, ComplexTypeHandlerMember memberHandler)
    {
        throw new Exception("Not supported!");
    }

    public bool WriteValueAsStringGeneric<TParam>(ref ValueStringWriter stringWriter, TParam? instance)
    {
        throw new Exception("Not supported!");
    }

    public bool WriteValueAsStringGeneric<TParam>(StringBuilder builder, TParam? instance)
    {
        ValueStringWriter writer = new ValueStringWriter(builder);
        return WriteValueAsStringGeneric(ref writer, instance);
    }

    public bool ParseValueFromStringGeneric<TParam>(ReadOnlySpan<char> data, out TParam? result)
    {
        throw new Exception("Not supported!");
    }
}
