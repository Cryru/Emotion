#nullable enable

namespace Emotion.Standard.Reflector.Handlers;

public interface IGenericEnumerableTypeHandler
{
    public string ItemTypeName { get; }

    public Type ItemType { get; }
}
