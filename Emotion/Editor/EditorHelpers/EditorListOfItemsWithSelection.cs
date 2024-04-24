#nullable enable

#region Using

using Emotion;
using Emotion.UI;
using System.Linq;

#endregion

namespace Emotion.Editor.EditorHelpers;

public class EditorListOfItemsWithSelection<T> : UIBaseWindow
{
    // index of item | item reference | whether it is now selected or not
    public Action<int, T, bool>? OnSelectionChanged;
    public Func<int, T, string>? ResolveLabelCallback;

    public bool AllowMultiSelect = false;

    protected List<T> _items = new();
    protected List<int> _selectedItems = new();

    public EditorListOfItemsWithSelection()
    {
        UseNewLayoutSystem = true;
    }

    public void SetItems(List<T> items, List<int>? initialSelection = null)
    {
        _selectedItems.Clear();
        _items = items ?? new();

        ClearChildren();

        var list = new UICallbackListNavigator
        {
            LayoutMode = LayoutMode.VerticalList,
            ListSpacing = new Vector2(0, 0),
            HideScrollBarWhenNothingToScroll = true,
        };

        var scrollBar = new EditorScrollBar
        {
            Dock = UIDockDirection.Right,
            DontTakeSpaceWhenHidden = true,
            Margins = new Rectangle(2, 0, 0, 0)
        };
        list.SetScrollbar(scrollBar);
        AddChild(scrollBar);
        AddChild(list);

        for (int i = 0; i < _items.Count; i++)
        {
            T item = _items[i];

            string? label = null;
            if (ResolveLabelCallback != null) label = ResolveLabelCallback(i, item);
            label ??= item?.ToString() ?? "<null>";

            var button = new EditorListItemButton
            {
                Text = label,
                FillX = true,
                UserData = i,
                OnClickedProxy = ButtonClicked,
                Selected = _selectedItems.Contains(i),
                Id = $"ItemButton{i}"
            };
            list.AddChild(button);
        }

        if (initialSelection != null)
        {
            SetSelectedMulti(initialSelection);
        }
        else if (_items.Count > 0)
        {
            SetSelected(0);
        }
    }

    public void ButtonClicked(UICallbackButton button)
    {
        EditorListItemButton? buttonListItem = button as EditorListItemButton;

        AssertNotNull(buttonListItem);
        AssertNotNull(_items);
        AssertNotNull(buttonListItem.UserData);

        int myIdx = (int)buttonListItem.UserData;
        var item = _items[myIdx];

        var isCtrlHeld = Engine.Host.IsCtrlModifierHeld() && AllowMultiSelect;
        var isShiftHeld = Engine.Host.IsShiftModifierHeld() && AllowMultiSelect;

        if (isShiftHeld)
        {
            // todo: select all between current and last selected
        }
        else if (isCtrlHeld)
        {
            var newSel = new List<int>();
            newSel.AddRange(_selectedItems);
            newSel.Add(myIdx);
            SetSelectedMulti(newSel);
        }
        else
        {
            SetSelected(myIdx);
        }
    }

    public void SetSelected(int idx)
    {
        SetSelectedMulti(new List<int>() { idx });
    }

    public void SetSelectedMulti(List<int> selection)
    {
        if (selection.Count > 1 && !AllowMultiSelect) return;

        // Deselect those that shouldn't be selected.
        for (int i = _selectedItems.Count - 1; i >= 0; i--)
        {
            var oldSel = _selectedItems[i];
            if (!selection.Contains(oldSel))
            {
                var item = _items[oldSel];
                OnSelectionChanged?.Invoke(oldSel, item, false);
                _selectedItems.RemoveAt(i);

                EditorListItemButton? ui = GetWindowById($"ItemButton{oldSel}") as EditorListItemButton;
                if (ui != null) ui.Selected = false;
            }
        }

        // Select those that should be.
        for (int i = 0; i < selection.Count; i++)
        {
            var newSel = selection[i];
            if (!_selectedItems.Contains(newSel) && newSel >= 0 && newSel < _items.Count)
            {
                var item = _items[newSel];
                OnSelectionChanged?.Invoke(newSel, item, true);
                _selectedItems.Add(newSel);

                EditorListItemButton? ui = GetWindowById($"ItemButton{newSel}") as EditorListItemButton;
                if (ui != null) ui.Selected = true;
            }
        }
    }
}