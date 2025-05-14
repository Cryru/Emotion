using Emotion.Standard.Reflector;
using Emotion.Standard.Reflector.Handlers.Interfaces;
using Emotion.UI;
using Emotion.WIPUpdates.One.EditorUI.Components;

#nullable enable

namespace Emotion.WIPUpdates.One.EditorUI.ObjectPropertiesEditorHelpers;

public class ObjectPropertyWindow : UIBaseWindow
{
    public object? ObjectBeingEdited { get; protected set; }
    protected Type? _type;
    private TypeEditor? _editor;

    public void SetEditor(object? obj)
    {
        ObjectBeingEdited = obj;
        _type = obj?.GetType();
        SpawnEditors();
    }

    protected void SpawnEditors()
    {
        if (_type == null) return;

        IGenericReflectorTypeHandler? typeHandler = ReflectorEngine.GetTypeHandler(_type);
        ClearChildren();

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
}
