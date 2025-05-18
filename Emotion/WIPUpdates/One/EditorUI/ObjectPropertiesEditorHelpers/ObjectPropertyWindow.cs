using Emotion.Standard.Reflector;
using Emotion.Standard.Reflector.Handlers.Interfaces;
using Emotion.UI;
using Emotion.WIPUpdates.One.EditorUI.Components;

#nullable enable

namespace Emotion.WIPUpdates.One.EditorUI.ObjectPropertiesEditorHelpers;

public class ObjectPropertyWindow : UIBaseWindow
{
    public object? ObjectBeingEdited { get; protected set; }
    public object? ParentObject { get; protected set; }

    private TypeEditor? _editor;

    public ObjectPropertyWindow()
    {
        LayoutMode = LayoutMode.VerticalList;
    }

    public void SetEditor(object? obj, object? parentObj = null)
    {
        _pages.Clear();
        AddEditPage("root", obj);
    }

    protected void SpawnEditors()
    {
        if (_pages.Count > 0)
        {
            (string pageName, object obj) top = _pages.Peek();
            ObjectBeingEdited = top.obj;
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
            editor.SetParentObject(ParentObject);
            editor.SetValue(ObjectBeingEdited);
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

    #endregion
}
