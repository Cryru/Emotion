using Emotion.UI;
using Emotion.Utility;
using Emotion.WIPUpdates.One.EditorUI.Base;

#nullable enable

namespace Emotion.WIPUpdates.One.EditorUI.Components;

public class EditorSelectableList<T> : ArrayEditorBase<T>
{
    private UIScrollArea _scrollArea;
    private UIBaseWindow _itemList;

    public EditorSelectableList()
    {
        _scrollArea = new EditorScrollArea();
        AddChild(_scrollArea);

        _itemList = new UIBaseWindow()
        {
            LayoutMode = LayoutMode.VerticalList,
            ListSpacing = new Vector2(0, 3)
        };
        _scrollArea.AddChildInside(_itemList);
    }

    protected override void OnItemsChanged(IEnumerable<T>? items)
    {
        _itemList.ClearChildren();

        if (items == null) return;

        foreach (T item in items)
        {
            _itemList.AddChild(new EditorListItem<T>(item, ChangeSelectedItem));
        }
    }

    protected override void OnSelectionChanged(T? newSel)
    {
        foreach (EditorListItem<T> listItem in _itemList.WindowChildren())
        {
            listItem.Selected = Helpers.AreObjectsEqual(listItem.Item, newSel);
        }
    }

    protected override void OnSelectionEmpty()
    {
        foreach (EditorListItem<T> listItem in _itemList.WindowChildren())
        {
            listItem.Selected = false;
        }
    }
}
