using Emotion.UI;
using Emotion.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

#nullable enable

namespace Emotion.WIPUpdates.One.EditorUI.Base;

 public abstract class ArrayEditorBase<T> : UIBaseWindow
{
    protected IEnumerable<T>? _items;
    protected T? _current;
    protected int _currentIndex = -1;

    public void SetEditor(IEnumerable<T>? values, int currentValueIdx)
    {
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
        foreach (var item in values)
        {
            if (i == _currentIndex)
            {
                OnSelectionChanged(item);
                break;
            }
            i++;
        }

        OnSelectionEmpty();
    }

    public void SetEditor(IEnumerable<T>? values, T? currentValue)
    {
        if (values == null)
        {
            SetEditor(values, -1);
            return;
        }

        int i = 0;
        int idxFound = -1;
        foreach (var item in values)
        {
            if (Helpers.AreObjectsEqual(item, currentValue))
            {
                idxFound = i;
                break;
            }
            i++;
        }

        SetEditor(values, idxFound);
    }

    protected abstract void OnSelectionChanged(T? newSel);

    protected abstract void OnSelectionEmpty();

    protected abstract void OnItemsChanged(IEnumerable<T>? items);
}
