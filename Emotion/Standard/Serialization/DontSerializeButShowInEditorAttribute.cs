#nullable enable

namespace Emotion.Standard.Serialization;

/// <summary>
/// Marker for a field/property which should not be serialized.
/// Even if the field exists in the document it wont be deserialized.
/// </summary>
public class DontSerializeButShowInEditorAttribute : Attribute
{
}