#nullable enable

using Emotion.Serialization.XML;
using Emotion.Standard.OptimizedStringReadWrite;
using Emotion.WIPUpdates.One.EditorUI.ObjectPropertiesEditorHelpers;
using System.Text.Json;

namespace Emotion.Standard.Reflector.Handlers.Interfaces;

public interface IGenericReflectorTypeHandler
{
    public string TypeName { get; }

    public Type Type { get; }

    public TypeEditor? GetEditor();

    public void PostInit();

    public bool IsTypeAssignableTo(Type otherType)
    {
        return Type.IsAssignableTo(otherType);
    }

    #region Serialization Read

    public T? ParseFromJSON<T>(ref Utf8JsonReader reader);

    public T? ParseFromXML<T>(ref ValueStringReader reader);

    #endregion

    #region Serialization Write

    public void WriteAsCode<OwnerT>(OwnerT? value, ref ValueStringWriter writer);

    public void WriteAsXML<OwnerT>(OwnerT? value, ref ValueStringWriter writer, bool addTypeTags, XMLConfig config, int indent = 0);

    #endregion
}
