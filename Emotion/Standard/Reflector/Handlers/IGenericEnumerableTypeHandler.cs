#nullable enable

namespace Emotion.Standard.Reflector.Handlers;

public interface IGenericEnumerableTypeHandler
{
    public string ItemTypeName { get; }

    public Type ItemType { get; }

    public object? CreateNew();

    public object? CreateNewFromList(IList list);

    public IList CreateList();
}
