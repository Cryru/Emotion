#nullable enable

using Emotion.Standard.Reflector.Handlers;
using Emotion.UI;
using Emotion.WIPUpdates.One.EditorUI.Components;
using System.Reflection.Metadata;

namespace Emotion.WIPUpdates.One.EditorUI.ObjectPropertiesEditorHelpers;

public class EditorWithLabel : UIBaseWindow
{
    private ObjectPropertyEditor _editor;
    private EditorLabel _label;

    private object _objectEditting;
    private ComplexTypeHandlerMember _handler;

    public EditorWithLabel(ObjectPropertyEditor editor, object parentObj, ComplexTypeHandlerMember memberHandler)
    {
        FillY = false;
        LayoutMode = LayoutMode.HorizontalList;

        EditorLabel label = new EditorLabel
        {
            Id = "Label",
            Margins = new Primitives.Rectangle(0, 0, 5, 0),
            Text = memberHandler.Name + ":"
        };
        AddChild(label);
        _label = label;

        AddChild(editor);
        editor.SetCallbackOnValueChange(OnInputChanged);
        _editor = editor;

        _objectEditting = parentObj;
        _handler = memberHandler;
        EngineEditor.RegisterForObjectChanges(parentObj, OnValueUpdated, this);

        OnValueUpdated();
    }

    private void OnInputChanged(object? newValue)
    {
        _handler.SetValueInComplexObject(_objectEditting, newValue);
        EngineEditor.ObjectChanged(_objectEditting, this);
    }

    private void OnValueUpdated()
    {
        if (_objectEditting == null) return;
        if (_handler == null) return;

        if (_handler.GetValueFromComplexObject(_objectEditting, out object? readValue))
        {
            _editor.SetValue(readValue);
        }
    }

    public override void DetachedFromController(UIController controller)
    {
        base.DetachedFromController(controller);
        EngineEditor.UnregisterForObjectChanges(this);
    }
}
