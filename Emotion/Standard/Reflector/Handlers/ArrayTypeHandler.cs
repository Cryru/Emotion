#nullable enable

using Emotion.Standard.OptimizedStringReadWrite;

namespace Emotion.Standard.Reflector.Handlers;

public class ArrayTypeHandler<T, TItem> : ReflectorTypeHandlerBase<T>, IGenericEnumerableTypeHandler
{
    public override string TypeName => $"ArrayOf{ItemType.Name}";

    public override Type Type => typeof(T);

    public override bool CanGetOrParseValueAsString => false;

    public string ItemTypeName => ItemType.Name;

    public Type ItemType => typeof(TItem);

    public object? CreateNew()
    {
        return Array.Empty<TItem>();
    }

    public object? CreateNewFromList(IList list)
    {
        if (list is not List<TItem> listAsT) return Array.Empty<TItem>();

        TItem[] values = new TItem[listAsT.Count];
        for (int i = 0; i < listAsT.Count; i++)
        {
            values[i] = listAsT[i];
        }
        return values;
    }

    public IList CreateList()
    {
        return new List<TItem>();
    }

    public override bool ParseValueAsString(ReadOnlySpan<char> data, out T? result)
    {
        result = default;
        return false;
    }

    public override bool WriteValueAsString(ref ValueStringWriter stringWriter, T? instance)
    {
        return false;
    }
}
