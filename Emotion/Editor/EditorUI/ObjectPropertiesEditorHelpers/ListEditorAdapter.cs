#nullable enable

using Emotion.Standard.Reflector.Handlers.Base;

namespace Emotion.Editor.EditorUI.ObjectPropertiesEditorHelpers;

[DontSerialize]
public abstract class ListEditorAdapter
{
    public abstract string GetName();

    public abstract object? GetItemAtIndex(int idx);

    public abstract void SetItemAtIndex(int idx, object val);
}

[DontSerialize]
public class ListEditorAdapter<TItem> : ListEditorAdapter
{
    private ComplexTypeHandlerMemberBase? _member;
    private IList<TItem?>? _itemsList;
    private TItem?[]? _itemsArray;

    public ListEditorAdapter(ComplexTypeHandlerMemberBase? member, IList<TItem?> list)
    {
        _member = member;
        _itemsList = list;
    }

    public ListEditorAdapter(ComplexTypeHandlerMemberBase? member, TItem?[] array)
    {
        _member = member;
        _itemsArray = array;
    }

    public override string GetName()
    {
        if (_member != null)
            return _member.Name;

        return "Unknown List";
    }

    public override object? GetItemAtIndex(int idx)
    {
        if (_itemsList != null)
            return _itemsList[idx];
        if (_itemsArray != null)
            return _itemsArray[idx];
        return default;
    }

    public override void SetItemAtIndex(int idx, object val)
    {
        if (val is not TItem valAsT)
            return;

        if (_itemsList != null)
            _itemsList[idx] = valAsT;
        if (_itemsArray != null)
            _itemsArray[idx] = valAsT;
    }
}
