﻿#nullable enable

using Emotion.Standard.Reflector;
using Emotion.Standard.Reflector.Handlers.Base;
using Emotion.Standard.Reflector.Handlers.Interfaces;
using Emotion.UI;
using Emotion.WIPUpdates.One.EditorUI.Components;

namespace Emotion.WIPUpdates.One.EditorUI.ObjectPropertiesEditorHelpers;

public abstract class ListEditor : TypeEditor
{
}

// generic constraint is the item type is only for ease of use, pass object if you dont care
public class ListEditor<TItem> : ListEditor
{
    private static IList<TItem?> EMPTY_LIST = new List<TItem?>();

    private Type ListItemType;
    private IGenericReflectorComplexTypeHandler? _complexTypeHandler;

    private IList<TItem?> _items = EMPTY_LIST;

    private UIBaseWindow _itemList;

    // Communication with the object property editor
    private ObjectPropertyWindow? _objEdit;
    private ComplexTypeHandlerMemberBase? _member;

    public ListEditor(Type typ)
    {
        ListItemType = typ;
        _complexTypeHandler = ReflectorEngine.GetComplexTypeHandler(typ);

        LayoutMode = LayoutMode.VerticalList;
        SpawnEditButtons();
        AssertNotNull(_addButton);
        AssertNotNull(_deleteButton);

        var scrollArea = new EditorScrollArea();
        AddChild(scrollArea);

        _itemList = new UIBaseWindow()
        {
            LayoutMode = LayoutMode.VerticalList,
            ListSpacing = new Vector2(0, 3)
        };
        scrollArea.AddChildInside(_itemList);
    }

    public override void AttachedToController(UIController controller)
    {
        base.AttachedToController(controller);

        _objEdit = GetParentOfKind<ObjectPropertyWindow>();
        _member = _objEdit?.GetMemberForEditor(this);
        RespawnItemsUI(_items);
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
            _items = (IList<TItem?>)value;
            EngineEditor.RegisterForObjectChanges(_items, (_) => RespawnItemsUI(_items), this);

            // Listen to individual object changes
            EngineEditor.RegisterForObjectChangesList(_items, (_) => RespawnItemsUI(_items), this);
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

        TItem? selectedItem = _currentIndex == -1 || _items == null ? default : _items[_currentIndex];
        OnItemSelected?.Invoke(selectedItem);
    }

    protected void RespawnItemsUI(IList<TItem?>? newItems)
    {
        if (Controller == null) return;
        _itemList.ClearChildren();

        if (newItems != null)
        {
            for (int i = 0; i < newItems.Count; i++)
            {
                TItem? item = newItems[i];
                var editorListItem = new EditorListItem<TItem>(i, item, ItemsUIOnClickSelect);

                if (item != null)
                {
                    var editItemButton = new SquareEditorButtonWithTexture("Editor/Edit.png");
                    if (_objEdit != null)
                    {
                        int myIdx = i;
                        editItemButton.OnClickedProxy = (_) => _objEdit.AddEditPage(new ListEditorAdapter<TItem>(_member, newItems), myIdx);
                    }
                    else
                    {
                        // todo: value type-like stack?
                        // todo: array support?
                        editItemButton.OnClickedProxy = (_) =>
                        {
                            var editorWindow = new ObjectPropertyEditorWindow(item);
                            EngineEditor.EditorRoot.AddChild(editorWindow);
                        };
                    }
                    editorListItem.AttachButton(editItemButton);
                }

                _itemList.AddChild(editorListItem);
            }
        }

        ApplyItemsUISelection();
    }

    #region Editting

    public event Action<int, TItem?>? OnItemRemoved;

    protected SquareEditorButton _addButton;
    protected SquareEditorButton _deleteButton;
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
                TItem? newObjAsT = (TItem?)newObj;
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

                TItem? obj = _items[_currentIndex];
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
        _moveUpButton.Enabled = _items != EMPTY_LIST && _currentIndex != -1 && _currentIndex != 0;
        _moveDownButton.Enabled = _items != EMPTY_LIST && _currentIndex != -1 && _currentIndex != _items.Count - 1;
    }

    #endregion

    #region Selection

    public Action<TItem?>? OnItemSelected;

    protected int _currentIndex = -1;

    protected void ItemsUIOnClickSelect(int idx, TItem? item)
    {
        SetSelection(idx);

        if (_currentIndex != -1)
        {
            AssertNotNull(_items);
            TItem? newSelectedItem = _items[_currentIndex];
        }
    }

    protected void ApplyItemsUISelection()
    {
        int selectedIndex = _currentIndex;
        int idx = 0;
        foreach (EditorListItem<TItem> listItem in _itemList.WindowChildren())
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

    #region API

    public TItem? GetSelected()
    {
        if (_currentIndex == -1) return default;
        return _items[_currentIndex];
    }

    #endregion
}
