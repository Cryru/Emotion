#nullable enable

using Emotion.Standard.Reflector.Handlers;
using Emotion.UI;
using Emotion.WIPUpdates.One.EditorUI.Components;
using System.Reflection.Metadata;

namespace Emotion.WIPUpdates.One.EditorUI.ObjectPropertiesEditorHelpers;

public class EditorWithLabel : UIBaseWindow
{
    public EditorLabel Label { get; private set; }

    private TypeEditor _editor;
    private object _objectEditting;
    private ComplexTypeHandlerMember _handler;

    public EditorWithLabel(TypeEditor editor, object parentObj, ComplexTypeHandlerMember memberHandler)
    {
        Assert(editor.Parent == null, "TypeEditor shouldn't have a UI parent");

        FillY = false;
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
        EngineEditor.RegisterForObjectChanges(parentObj, OnValueUpdated, this);

        OnValueUpdated();
    }

    public EditorWithLabel(string labelText, TypeEditor editor, object? startingValue, Action<object?> onValueChanged)
    {
        FillY = false;
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
