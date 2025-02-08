#nullable enable

using Emotion.UI;
using Emotion.WIPUpdates.One.EditorUI.Components;

namespace Emotion.WIPUpdates.One.EditorUI.ObjectPropertiesEditorHelpers;

public class ListEditor<T> : TypeEditor where T : new() // temp generic constraint - use reflector to initialize
{
    private SquareEditorButton _addButton;
    private IList<T?>? _items;

    private UIBaseWindow _itemList;

    // Configuration
    public bool CanEdit
    {
        get => _canEdit;
        set
        {
            if (value == _canEdit) return;
            _canEdit = value;

            // Respawn items to apply settings
            RespawnItemsUI(_items);
        }
    }
    private bool _canEdit;

    public bool CanSelect = false;

    public ListEditor()
    {
        LayoutMode = LayoutMode.VerticalList;

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
                Assert(CanEdit);
                AssertNotNull(_items);
                _items.Add(new T());
                RespawnItemsUI(_items);
            },
            Enabled = false
        };
        buttonsContainer.AddChild(addNew);
        _addButton = addNew;

        var scrollArea = new EditorScrollArea();
        AddChild(scrollArea);

        _itemList = new UIBaseWindow()
        {
            LayoutMode = LayoutMode.VerticalList,
            ListSpacing = new Vector2(0, 3)
        };
        scrollArea.AddChildInside(_itemList);
    }

    public override void SetValue(object? value)
    {
        _items = (IList<T?>?)value;

        if (_items == null || !CanSelect || _items.Count == 0)
        {
            _currentIndex = -1;
        }
        else
        {
            // Select first by default
            _currentIndex = 0;
        }

        RespawnItemsUI(_items);
    }

    public void SetSelection(int idx)
    {
        if (!CanSelect) idx = -1;
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

                if (CanEdit)
                {
                    var editButton = new EditorEditObjectButton();
                    editButton.SetEditor(item);
                    editorListItem.AttachButton(editButton);
                }

                _itemList.AddChild(editorListItem);
            }
        }

        ApplySettings();
        ApplyItemsUISelection();
    }

    #region Settings

    protected void ApplySettings()
    {
        _addButton.Enabled = CanEdit && _items != null;
    }

    #endregion

    #region Editting



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
        bool canSelect = CanSelect;

        int selectedIndex = _currentIndex;
        int idx = 0;
        foreach (EditorListItem<T> listItem in _itemList.WindowChildren())
        {
            listItem.Selected = !canSelect || selectedIndex == idx;
            idx++;
        }
    }

    #endregion
}
