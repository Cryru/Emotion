#nullable enable

using Emotion.Standard.Reflector;
using Emotion.Standard.Reflector.Handlers;
using Emotion.UI;
using Emotion.WIPUpdates.One.EditorUI.Components;

namespace Emotion.WIPUpdates.One.EditorUI.ObjectPropertiesEditorHelpers;

// generic contstraint is only for ease of use, pass object if you dont care
public class ListEditor<T> : TypeEditor
{
    private static IList<T?> EMPTY_LIST = new List<T?>();

    private Type ListItemType;
    private IGenericReflectorComplexTypeHandler? _complexTypeHandler;

    private IList<T?> _items = EMPTY_LIST;

    private UIBaseWindow _itemList;

    public ListEditor(Type typ)
    {
        ListItemType = typ;
        _complexTypeHandler = ReflectorEngine.GetComplexTypeHandler(typ);

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
            EngineEditor.RegisterForObjectChanges(value, (_) => RespawnItemsUI(_items), this);
        }

        SetSelection(-1); // Reset selection
        RespawnItemsUI(_items);
    }

    public void SetSelection(int idx)
    {
        if (_items == null || _items.Count == 0)
            idx = -1;
        else if (idx >= _items.Count)
            idx = _items.Count - 1;
        else if (idx == -1)
            idx = 0;

        _currentIndex = idx;
        ApplyItemsUISelection();

        T? selectedItem = _currentIndex == -1 || _items == null ? default : _items[_currentIndex];
        OnItemSelected?.Invoke(selectedItem);
    }

    protected void RespawnItemsUI(IList<T?>? newItems)
    {
        _itemList.ClearChildren();

        if (newItems != null)
        {
            for (int i = 0; i < newItems.Count; i++)
            {
                T? item = newItems[i];
                var editorListItem = new EditorListItem<T>(i, item, ItemsUIOnClickSelect);
                _itemList.AddChild(editorListItem);
            }
        }

        ApplyItemsUISelection();
    }

    #region Editting

    public event Action<int, T?>? OnItemRemoved;

    protected SquareEditorButton _addButton;
    protected SquareEditorButton _deleteButton;
    protected SquareEditorButton _editButton;
    protected SquareEditorButton _moveUpButton = null!;
    protected SquareEditorButton _moveDownButton = null!;

    private void SpawnEditButtons()
    {
        UIBaseWindow buttonsContainer = new UIBaseWindow()
        {
            LayoutMode = LayoutMode.HorizontalList,
            ListSpacing = new Vector2(5, 0),
            GrowY = false,
            OrderInParent = -1,
            Margins = new Primitives.Rectangle(0, 0, 0, 5)
        };
        AddChild(buttonsContainer);

        var addNew = new SquareEditorButton("+")
        {
            OnClickedProxy = (_) =>
            {
                Assert(_items != EMPTY_LIST);

                object? newObj = CreateNewItem();
                T? newObjAsT = (T?)newObj;
                if (newObjAsT == null) return;

                _items.Add(newObjAsT);
                EngineEditor.ObjectChanged(_items, ObjectChangeType.List_NewObj, this);

                RespawnItemsUI(_items);
                SetSelection(_items.IndexOf(newObjAsT)); // Select the new item, we get index as event might have sorted
            },
        };
        buttonsContainer.AddChild(addNew);
        _addButton = addNew;

        var deleteButton = new SquareEditorButtonWithTexture("Editor/TrashCan.png")
        {
            OnClickedProxy = (_) =>
            {
                Assert(_items != EMPTY_LIST);

                T? obj = _items[_currentIndex];
                OnItemRemoved?.Invoke(_currentIndex, obj);

                _items.RemoveAt(_currentIndex);
                if (_currentIndex > _items.Count - 1)
                    _currentIndex = _items.Count - 1;
                RespawnItemsUI(_items);
                EngineEditor.ObjectChanged(_items, ObjectChangeType.List_ObjectRemoved, this);
            }
        };
        buttonsContainer.AddChild(deleteButton);
        _deleteButton = deleteButton;

        var editButton = new EditorEditObjectButton(() =>
        {
            Assert(_items != EMPTY_LIST);
            return _items[_currentIndex];
        })
        {
            DontTakeSpaceWhenHidden = true
        };
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
                EngineEditor.ObjectChanged(_items, ObjectChangeType.List_Reodered, this);
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
                EngineEditor.ObjectChanged(_items, ObjectChangeType.List_Reodered, this);
            }
        };
        buttonsContainer.AddChild(moveDownButton);
        _moveDownButton = moveDownButton;

        UpdateButtonStates();
    }

    protected void UpdateButtonStates()
    {
        _addButton.Enabled = _items != EMPTY_LIST && CanCreateItems();
        _deleteButton.Enabled = _items != EMPTY_LIST && _currentIndex != -1;
        _editButton.Enabled = _items != EMPTY_LIST && _currentIndex != -1 && _items[_currentIndex] != null;
        _moveUpButton.Enabled = _items != EMPTY_LIST && _currentIndex != -1 && _currentIndex != 0;
        _moveDownButton.Enabled = _items != EMPTY_LIST && _currentIndex != -1 && _currentIndex != _items.Count - 1;
    }

    #endregion

    #region Selection

    public Action<T?>? OnItemSelected;

    protected int _currentIndex = -1;

    protected void ItemsUIOnClickSelect(int idx, T? item)
    {
        SetSelection(idx);

        if (_currentIndex != -1)
        {
            AssertNotNull(_items);
            T? newSelectedItem = _items[_currentIndex];
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

    #region Protected API

    protected virtual bool CanCreateItems()
    {
        return _complexTypeHandler?.CanCreateNew() == true;
    }

    protected virtual object? CreateNewItem()
    {
        return _complexTypeHandler?.CreateNew();
    }

    #endregion
}
