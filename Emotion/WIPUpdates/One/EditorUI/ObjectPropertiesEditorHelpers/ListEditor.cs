#nullable enable

using Emotion.UI;
using Emotion.WIPUpdates.One.EditorUI.Components;

namespace Emotion.WIPUpdates.One.EditorUI.ObjectPropertiesEditorHelpers;

public class ListEditor<T> : TypeEditor where T : new() // temp generic constraint - use reflector to initialize
{
    private static IList<T?> EMPTY_LIST = new List<T?>();

    private IList<T?> _items = EMPTY_LIST;

    private UIBaseWindow _itemList;

    public ListEditor()
    {
        LayoutMode = LayoutMode.VerticalList;

        SpawnEditButtons();
        AssertNotNull(_addButton);
        AssertNotNull(_deleteButton);
        AssertNotNull(_editButton);

        var scrollArea = new EditorScrollArea();
        AddChild(scrollArea);

        _itemList = new UIBaseWindow()
        {
            LayoutMode = LayoutMode.VerticalList,
            ListSpacing = new Vector2(0, 3)
        };
        scrollArea.AddChildInside(_itemList);
    }

    public override void DetachedFromController(UIController controller)
    {
        base.DetachedFromController(controller);
        EngineEditor.UnregisterForObjectChanges(this);
    }

    public override void SetValue(object? value)
    {
        EngineEditor.UnregisterForObjectChanges(this);

        if (value == null)
        {
            _items = EMPTY_LIST;
        }
        else
        {
            _items = (IList<T?>)value;
            EngineEditor.RegisterForObjectChanges(value, () => RespawnItemsUI(_items), this);
        }

        // Select first by default
        _currentIndex = _items.Count == 0 ? -1 : 0;
        RespawnItemsUI(_items);
    }

    public void SetSelection(int idx)
    {
        if (_items == null) idx = -1;

        _currentIndex = idx;
        ApplyItemsUISelection();
    }

    protected void RespawnItemsUI(IList<T?>? newItems)
    {
        _itemList.ClearChildren();

        if (newItems != null)
        {
            for (int i = 0; i < newItems.Count; i++)
            {
                T? item = newItems[i];
                var editorListItem = new EditorListItem<T>(i, item, ItemsUISelected);
                _itemList.AddChild(editorListItem);
            }
        }

        ApplyItemsUISelection();
    }

    #region Editting

    private SquareEditorButton _addButton;
    private SquareEditorButton _deleteButton;
    private SquareEditorButton _editButton;
    private SquareEditorButton _moveUpButton;
    private SquareEditorButton _moveDownButton;

    public void SpawnEditButtons()
    {
        UIBaseWindow buttonsContainer = new UIBaseWindow()
        {
            LayoutMode = LayoutMode.HorizontalList,
            ListSpacing = new Vector2(5, 0),
            FillY = false,
            OrderInParent = -1,
            Margins = new Primitives.Rectangle(0, 0, 0, 5)
        };
        AddChild(buttonsContainer);

        var addNew = new SquareEditorButton("+")
        {
            OnClickedProxy = (_) =>
            {
                Assert(_items != EMPTY_LIST);

                _items.Add(new T());
                RespawnItemsUI(_items);
                EngineEditor.ObjectChanged(_items, this);
            },
            Enabled = false
        };
        buttonsContainer.AddChild(addNew);
        _addButton = addNew;

        var deleteButton = new SquareEditorButtonWithTexture("Editor/TrashCan.png")
        {
            OnClickedProxy = (_) =>
            {
                Assert(_items != EMPTY_LIST);

                T? selectedItem = _items[_currentIndex];
                _items.Remove(selectedItem);
                if (_currentIndex > _items.Count - 1)
                    _currentIndex = _items.Count - 1;
                RespawnItemsUI(_items);
                EngineEditor.ObjectChanged(_items, this);
            }
        };
        buttonsContainer.AddChild(deleteButton);
        _deleteButton = deleteButton;

        var editButton = new EditorEditObjectButton(() =>
        {
            Assert(_items != EMPTY_LIST);
            return _items[_currentIndex];
        });
        buttonsContainer.AddChild(editButton);
        _editButton = editButton;

        var moveUpButton = new SquareEditorButtonWithTexture("Editor/LittleArrow.png")
        {
            OnClickedProxy = (_) =>
            {
                var idxPrev = _currentIndex - 1;
                Assert(idxPrev >= 0);

                (_items[_currentIndex], _items[idxPrev]) = (_items[idxPrev], _items[_currentIndex]);
                _currentIndex = idxPrev;
                RespawnItemsUI(_items);
                EngineEditor.ObjectChanged(_items, this);
            }
        };
        moveUpButton.Texture.SetRotation(180);
        buttonsContainer.AddChild(moveUpButton);
        _moveUpButton = moveUpButton;

        var moveDownButton = new SquareEditorButtonWithTexture("Editor/LittleArrow.png")
        {
            OnClickedProxy = (_) =>
            {
                var idxNext = _currentIndex + 1;
                Assert(idxNext <= _items.Count - 1);

                (_items[_currentIndex], _items[idxNext]) = (_items[idxNext], _items[_currentIndex]);
                _currentIndex = idxNext;
                RespawnItemsUI(_items);
                EngineEditor.ObjectChanged(_items, this);
            }
        };
        buttonsContainer.AddChild(moveDownButton);
        _moveDownButton = moveDownButton;
    }

    protected void UpdateButtonStates()
    {
        _addButton.Enabled = _items != EMPTY_LIST;
        _deleteButton.Enabled = _items != EMPTY_LIST && _currentIndex != -1;
        _editButton.Enabled = _items != EMPTY_LIST && _currentIndex != -1 && _items[_currentIndex] != null;
        _moveUpButton.Enabled = _items != EMPTY_LIST && _currentIndex != -1 && _currentIndex != 0;
        _moveDownButton.Enabled = _items != EMPTY_LIST && _currentIndex != -1 && _currentIndex != _items.Count - 1;
    }

    #endregion

    #region Selection

    public Action<T?>? OnItemSelected;

    protected int _currentIndex = -1;

    protected void ItemsUISelected(int idx, T? item)
    {
        SetSelection(idx);

        if (_currentIndex != -1)
        {
            AssertNotNull(_items);
            T? newSelectedItem = _items[_currentIndex];
            OnItemSelected?.Invoke(newSelectedItem);
        }
    }

    protected void ApplyItemsUISelection()
    {
        int selectedIndex = _currentIndex;
        int idx = 0;
        foreach (EditorListItem<T> listItem in _itemList.WindowChildren())
        {
            listItem.Selected = selectedIndex == idx;
            idx++;
        }

        UpdateButtonStates();
    }

    #endregion
}
