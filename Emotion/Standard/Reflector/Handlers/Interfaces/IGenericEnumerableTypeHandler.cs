#nullable enable

namespace Emotion.Standard.Reflector.Handlers.Interfaces;

public interface IGenericEnumerableTypeHandler
{
    public string ItemTypeName { get; }

    public Type ItemType { get; }
}
