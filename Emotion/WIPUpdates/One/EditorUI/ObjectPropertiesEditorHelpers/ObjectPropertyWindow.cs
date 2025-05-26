using Emotion.Standard.Reflector;
using Emotion.Standard.Reflector.Handlers.Base;
using Emotion.Standard.Reflector.Handlers.Interfaces;
using Emotion.UI;
using Emotion.WIPUpdates.One.EditorUI.Components;

#nullable enable

namespace Emotion.WIPUpdates.One.EditorUI.ObjectPropertiesEditorHelpers;

public class ObjectPropertyWindow : UIBaseWindow
{
    public object? ObjectBeingEdited { get; protected set; }

    private TypeEditor? _editor;

    public ObjectPropertyWindow()
    {
        LayoutMode = LayoutMode.VerticalList;
    }

    public override void AttachedToController(UIController controller)
    {
        base.AttachedToController(controller);
        SpawnEditors();
    }

    public void SetEditor(object? obj)
    {
        ObjectBeingEdited = obj;
        _pages.Clear();
        SpawnEditors();
    }

    protected void SpawnEditors()
    {
        if (Controller == null) return;

        // todo: check if value type or array without a value changed - in that case we want to display an error since
        // the object changes have nowhere to be propagated to.

        ClearChildren();

        SpawnPagingUI();

        // Editor for set object.
        object? currentObject = GetObjectValueFromStack(int.MaxValue);
        IGenericReflectorTypeHandler? typeHandler = currentObject == null ? null : ReflectorEngine.GetTypeHandler(currentObject.GetType());
        if (typeHandler == null)
        {
            EditorLabel label = new EditorLabel();
            label.Text = $"The object attempting to be edited is non-editable.";
            AddChild(label);
            return;
        }

        TypeEditor? editor = typeHandler.GetEditor();
        if (editor != null)
        {
            _editor = editor;

            editor.SetValue(currentObject);
            editor.SetCallbackOnValueChange((obj) =>
            {
                if (obj == null) return;
                SetObjectValueInStack(obj);
            });
            AddChild(editor);
        }
    }

    #region API

    /// <summary>
    /// Get the editor of the provided property name.
    /// This works only if the object currently being edited is a complex type
    /// as otherwise it doesn't have properties.
    /// </summary>
    public TypeEditor? GetEditorForProperty(string propertyName)
    {
        if (propertyName == "this")
            return _editor;

        if (_editor != null && _editor is ComplexObjectEditor complexEditor)
            return complexEditor.GetEditorForProperty(propertyName);

        return null;
    }

    /// <summary>
    /// Get the complex type member of the specified type editor.
    /// This works only if the object currently being edited is a complex type
    /// (otherwise it doesn't have members so...)
    /// </summary>
    public ComplexTypeHandlerMemberBase? GetMemberForEditor(TypeEditor typeEditor)
    {
        if (_editor != null && _editor is ComplexObjectEditor complexEditor)
            return complexEditor.GetMemberForEditor(typeEditor);

        return null;
    }

    /// <summary>
    /// Returns the parent object, of the provided object within the hierarchy of the object currently being edited.
    /// </summary>
    public object? GetParentObjectOfObject(object obj)
    {
        return GetParentObjectOfObjectOfKind<object>(obj);
    }

    /// <summary>
    /// Returns the parent object, that fits the requested type,
    /// of the provided object within the hierarchy of the object currently being edited.
    /// </summary>
    public T? GetParentObjectOfObjectOfKind<T>(object obj)
    {
        if (_pagingRoot == null)
            return default;

        T? parentCandidate = default;
        if (_pagingRoot is T rootAsT) parentCandidate = rootAsT;

        object? val = _pagingRoot;
        foreach (var entry in _pages)
        {
            val = entry.ResolveValue(val);
            if (val == null) return default;

            if (val is T valAsT)
                parentCandidate = valAsT;

            if (val == obj)
                return parentCandidate;
        }

        return default;
    }

    #endregion

    #region Paging (provides support for nested objects, and setting properties in value types)

    private object? _pagingRoot => ObjectBeingEdited;
    private List<ObjectPropertyEditorStackEntry> _pages = new List<ObjectPropertyEditorStackEntry>();

    private class ObjectPropertyEditorStackEntry
    {
        private ComplexTypeHandlerMemberBase? _member;

        private ListEditorAdapter? _listAdapter;
        private int _listItemIndex = -1;

        public ObjectPropertyEditorStackEntry(ComplexTypeHandlerMemberBase member)
        {
            _member = member;
        }

        public ObjectPropertyEditorStackEntry(ListEditorAdapter listAdapter, int index)
        {
            _listAdapter = listAdapter;
            _listItemIndex = index;
        }

        public string GetObjectName()
        {
            if (_listAdapter != null)
                return $"{_listAdapter.GetName()}[{_listItemIndex}]";

            if (_member != null)
                return _member.Name;

            return "???";
        }

        public object? ResolveValue(object parent)
        {
            if (_listAdapter != null)
                return _listAdapter.GetItemAtIndex(_listItemIndex);

            if (_member != null)
            {
                _member.GetValueFromComplexObject(parent, out object? readVal);
                return readVal;
            }

            return null;
        }

        public object? SetValue(ref object parent, object value)
        {
            if (_listAdapter != null)
            {
                _listAdapter.SetItemAtIndex(_listItemIndex, value);
                return null; // Lists are always stack dead ends because they're always reference types.
            }

            if (_member != null)
                return _member.SetValueInComplexObjectAndReturnParent(parent, value);

            return null;
        }
    }

    private void SpawnPagingUI()
    {
        var pagingContainer = new UIBaseWindow()
        {
            LayoutMode = LayoutMode.HorizontalListWrap,
            ListSpacing = new Vector2(5, 5)
        };
        AddChild(pagingContainer);

        EditorButton rootPage = new EditorButton("root");
        rootPage.OnClickedProxy = (_) =>
        {
            _pages.Clear();
            SpawnEditors();
        };
        pagingContainer.AddChild(rootPage);

        int idx = _pages.Count - 1;
        foreach (var entry in _pages)
        {
            int myIdx = idx;
            EditorButton pageLabel = new EditorButton(entry.GetObjectName());
            pageLabel.OnClickedProxy = (_) =>
            {
                for (int i = 0; i < myIdx; i++)
                {
                    _pages.RemoveAt(_pages.Count - 1);
                }
                SpawnEditors();
            };
            idx--;
            pagingContainer.AddChild(pageLabel);
        }

        if (pagingContainer.Children != null)
        {
            UIBaseWindow lastLabel = pagingContainer.Children[^1];
            if (lastLabel is EditorButton button)
                button.SetActiveMode(true);
        }
    }

    public void AddEditPage(ComplexTypeHandlerMemberBase member)
    {
        var newItem = new ObjectPropertyEditorStackEntry(member);
        _pages.Add(newItem);
        SpawnEditors();
    }

    public void AddEditPage(ListEditorAdapter listAdapter, int index)
    {
        var newItem = new ObjectPropertyEditorStackEntry(listAdapter, index);
        _pages.Add(newItem);
        SpawnEditors();
    }


    private object? GetObjectValueFromStack(int depth)
    {
        if (_pagingRoot == null)
            return null;

        object? val = _pagingRoot;
        foreach (var entry in _pages)
        {
            depth--;
            if (depth < 0) break;

            val = entry.ResolveValue(val);
            if (val == null) return null;
        }

        return val;
    }

    private void SetObjectValueInStack(object? newValue)
    {
        object? valObj = newValue;
        if (valObj == null)
            return;

        if (valObj is ValueType)
        {
            for (int i = _pages.Count - 1; i >= 0; i--)
            {
                ObjectPropertyEditorStackEntry entry = _pages[i];
                object? parentObj = GetObjectValueFromStack(i);
                if (parentObj == null)
                    break;

                valObj = entry.SetValue(ref parentObj, valObj);
                if (valObj == null)
                    break;

                if (valObj is not ValueType)
                    break;
            }
        }
    }

    public void ThrowObjectPropertyChangedThroughStack()
    {
        if (_pagingRoot == null)
            return;

        EngineEditor.ObjectChanged(_pagingRoot, ObjectChangeType.ComplexObject_PropertyChanged, this);

        object? val = _pagingRoot;
        foreach (var entry in _pages)
        {
            val = entry.ResolveValue(val);
            if (val == null) return;

            if (val is not ValueType)
                EngineEditor.ObjectChanged(val, ObjectChangeType.ComplexObject_PropertyChanged, this);
        }
    }

    #endregion
}
