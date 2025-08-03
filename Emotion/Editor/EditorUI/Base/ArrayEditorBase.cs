#nullable enable

using Emotion.Game.Systems.UI;

namespace Emotion.Editor.EditorUI.Base;

 public abstract class ArrayEditorBase<T> : UIBaseWindow
{
    protected IEnumerable<T>? _items;
    protected T? _current;
    protected int _currentIndex = -1;

    private Action<T?>? _onSelect;
    
    protected void ChangeSelectedItem(T? item)
    {
        _current = item;

        int idx = GetIndexOfItem(item);
        _currentIndex = idx;

        if (idx != -1)
            OnSelectionChanged(_current);
        else
            OnSelectionEmpty();

        _onSelect?.Invoke(_current);
    }

    public void SetEditor(IEnumerable<T>? values, int currentValueIdx, Action<T?>? onSelect)
    {
        _onSelect = onSelect;

        _items = values;

        OnItemsChanged(_items);
        if (values == null)
        {
            _currentIndex = -1;
            OnSelectionEmpty();
            return;
        }

        _currentIndex = currentValueIdx;

        int i = 0;
        bool found = false;
        foreach (T? item in values)
        {
            if (i == _currentIndex)
            {
                _current = item;
                found = true;
                break;
            }
            i++;
        }

        if (found)
            OnSelectionChanged(_current);
        else
            OnSelectionEmpty();
    }

    public void SetEditor(IEnumerable<T>? values, T? currentValue, Action<T?>? onSelect)
    {
        _items = values;
        SetEditor(values, GetIndexOfItem(currentValue), onSelect);
    }

    #region Helpers

    private int GetIndexOfItem(T? value)
    {
        if (_items == null) return -1;

        int i = 0;
        int idxFound = -1;
        foreach (var item in _items)
        {
            if (Helpers.AreObjectsEqual(item, value))
            {
                idxFound = i;
                break;
            }
            i++;
        }
        return idxFound;
    }

    #endregion

    #region Interface

    protected abstract void OnSelectionChanged(T? newSel);

    protected abstract void OnSelectionEmpty();

    protected abstract void OnItemsChanged(IEnumerable<T>? items);

    #endregion
}
