#nullable enable

using Emotion.Standard.Reflector.Handlers.Base;
using Emotion.UI;
using Emotion.WIPUpdates.One.EditorUI.Components;

namespace Emotion.WIPUpdates.One.EditorUI.ObjectPropertiesEditorHelpers;

public class ObjectPropertyEditor : UIBaseWindow
{
    public EditorLabel Label { get; private set; }

    private TypeEditor _editor;
    private object _objectEditting;
    private ComplexTypeHandlerMemberBase _handler;

    public ObjectPropertyEditor(TypeEditor editor, object parentObj, ComplexTypeHandlerMemberBase memberHandler) // via handler
    {
        Assert(editor.Parent == null, "TypeEditor shouldn't have a UI parent");

        GrowY = false;
        LayoutMode = LayoutMode.HorizontalList;

        EditorLabel label = new EditorLabel
        {
            Id = "Label",
            Margins = new Primitives.Rectangle(0, 0, 10, 0),
            Text = memberHandler.Name + ":"
        };
        AddChild(label);
        Label = label;

        AddChild(editor);
        editor.SetCallbackOnValueChange(OnInputChanged);
        _editor = editor;

        _objectEditting = parentObj;
        _handler = memberHandler;
        EngineEditor.RegisterForObjectChanges(parentObj, ObjectChangedEvent, this);

        OnValueUpdated();

        if (editor is IListEditor)
            editor.MinSizeY = 200;

        if (editor is NestedComplexObjectEditor || editor is IListEditor)
            SetVertical();
    }

    public override void DetachedFromController(UIController controller)
    {
        base.DetachedFromController(controller);
        EngineEditor.UnregisterForObjectChanges(this);
    }

    private void ObjectChangedEvent(ObjectChangeType _)
    {
        OnValueUpdated();
    }

    public ObjectPropertyEditor(string labelText, TypeEditor editor, object? startingValue, Action<object?> onValueChanged) // custom edit of a property
    {
        GrowY = false;
        LayoutMode = LayoutMode.HorizontalList;

        EditorLabel label = new EditorLabel
        {
            Id = "Label",
            Margins = new Primitives.Rectangle(0, 0, 10, 0),
            Text = labelText,
        };
        AddChild(label);
        Label = label;

        editor.SetValue(startingValue);
        editor.SetCallbackOnValueChange(onValueChanged);
        AddChild(editor);

        _editor = null!;
        _objectEditting = null!;
        _handler = null!;
    }

    public void SetVertical()
    {
        LayoutMode = LayoutMode.VerticalList;
        Label.Margins = new Primitives.Rectangle(0, 0, 0, 5);
    }

    private void OnInputChanged(object? newValue)
    {
        _handler.SetValueInComplexObject(_objectEditting, newValue);
        EngineEditor.ObjectChanged(_objectEditting, ObjectChangeType.ValueChanged, this);
    }

    private void OnValueUpdated()
    {
        if (_objectEditting == null) return;
        if (_handler == null) return;

        if (_handler.GetValueFromComplexObject(_objectEditting, out object? readValue))
            _editor.SetValue(readValue);
    }
}
