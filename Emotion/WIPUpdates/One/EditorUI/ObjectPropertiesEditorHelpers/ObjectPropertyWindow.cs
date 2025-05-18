using Emotion.Standard.Reflector;
using Emotion.Standard.Reflector.Handlers.Interfaces;
using Emotion.UI;
using Emotion.WIPUpdates.One.EditorUI.Components;

#nullable enable

namespace Emotion.WIPUpdates.One.EditorUI.ObjectPropertiesEditorHelpers;

public class ObjectPropertyWindow : UIBaseWindow
{
    public string ObjectBeingEditedName { get; protected set; } = string.Empty;

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
        _pages.Clear();
        AddEditPage("root", obj);
    }

    protected void SpawnEditors()
    {
        if (Controller == null) return;

        if (_pages.Count > 0)
        {
            (string pageName, object obj) = _pages.Peek();
            ObjectBeingEditedName = pageName;
            ObjectBeingEdited = obj;
        }

        var type = ObjectBeingEdited?.GetType();
        if (type == null) return;

        IGenericReflectorTypeHandler? typeHandler = ReflectorEngine.GetTypeHandler(type);
        ClearChildren();

        var pagingContainer = new UIBaseWindow()
        {
            LayoutMode = LayoutMode.HorizontalListWrap,
            ListSpacing = new Vector2(5, 5)
        };
        AddChild(pagingContainer);

        int idx = _pages.Count;
        foreach ((string pageName, object obj) in _pages)
        {
            int myIdx = idx;
            EditorButton pageLabel = new EditorButton(pageName);
            pageLabel.OnClickedProxy = (_) =>
            {
                while (myIdx != _pages.Count)
                {
                    _pages.Pop();
                }
                SpawnEditors();
            };
            pageLabel.OrderInParent = idx;
            idx--;
            pagingContainer.AddChild(pageLabel);
        }

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
            editor.SetValue(ObjectBeingEditedName, ObjectBeingEdited);
            editor.SetCallbackOnValueChange((obj) =>
            {
                if (obj == null) return;
                EngineEditor.ObjectChanged(obj, ObjectChangeType.ValueChanged, editor);
            });
            AddChild(editor);
        }
    }

    public TypeEditor? GetEditorForProperty(string propertyName)
    {
        if (propertyName == "this")
            return _editor;

        if (_editor != null && _editor is ComplexObjectEditor complexEditor)
            return complexEditor.GetEditorForProperty(propertyName);

        return null;
    }

    #region Paging

    private Stack<(string pageName, object obj)> _pages = new();

    public void AddEditPage(string pageName, object? obj)
    {
        if (obj != null)
            _pages.Push((pageName, obj));

        SpawnEditors();
    }

    public void PageBack()
    {
        if (_pages.Count <= 1) return;

        _pages.Pop();
        SpawnEditors();
    }

    public object? GetParentObjectOfObject(object obj)
    {
        bool nextOne = false;
        foreach (var item in _pages)
        {
            if (nextOne)
            {
                return item.obj;
            }

            if (item.obj == obj)
                nextOne = true;
        }

        return default;
    }
    public T? GetParentObjectOfObjectOfKind<T>(object obj)
    {
        bool nextOne = false;
        foreach (var item in _pages)
        {
            if (nextOne)
            {
                if (item.obj is T objAsT)
                    return objAsT;
                continue;
            }

            if (item.obj == obj)
                nextOne = true;
        }

        return default;
    }

    #endregion
}
