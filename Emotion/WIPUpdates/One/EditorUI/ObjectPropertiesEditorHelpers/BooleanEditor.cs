using Emotion.Game.World.Editor;
using Emotion.IO;
using Emotion.Platform.Implementation.CommonDesktop;
using Emotion.Standard.Reflector.Handlers;
using Emotion.UI;
using Emotion.WIPUpdates.One.EditorUI.Components;
using System.Reflection.Emit;

#nullable enable

namespace Emotion.WIPUpdates.One.EditorUI.ObjectPropertiesEditorHelpers;

public class BooleanEditor : UIBaseWindow, IObjectPropertyEditor
{
    private object? _objectEditting;
    private bool? _value;

    private ComplexTypeHandlerMember? _handler;

    public BooleanEditor()
    {
        FillY = false;

        UIBaseWindow container = new UIBaseWindow
        {
            LayoutMode = LayoutMode.HorizontalList
        };
        AddChild(container);

        EditorLabel label = new EditorLabel
        {
            Id = "Label",
            Margins = new Primitives.Rectangle(0, 0, 5, 0)
        };
        container.AddChild(label);

        var checkbox = new EditorCheckboxButton()
        {
            Id = "Checkbox",
            OnValueChanged = OnInputValueChanged
        };
        container.AddChild(checkbox);
    }

    public override void DetachedFromController(UIController controller)
    {
        base.DetachedFromController(controller);
        EngineEditor.UnregisterForObjectChanges(this);
    }

    private void OnInputValueChanged(bool newValue)
    {
        if (_objectEditting == null) return;

        _handler?.SetValueInComplexObject(_objectEditting, newValue);
        EngineEditor.ObjectChanged(_objectEditting, this);
    }

    private void OnValueUpdated()
    {
        if (_objectEditting == null) return;

        var textInput = GetWindowById<EditorCheckboxButton>("Checkbox");
        AssertNotNull(textInput);
        textInput.Value = _value ?? false;
    }

    public void SetEditor(object parentObj, ComplexTypeHandlerMember memberHandler)
    {
        _objectEditting = parentObj;
        _handler = memberHandler;

        EditorLabel? label = GetWindowById<EditorLabel>("Label");
        AssertNotNull(label);
        label.Text = memberHandler.Name + ":";

        EngineEditor.UnregisterForObjectChanges(this);
        EngineEditor.RegisterForObjectChanges(_objectEditting, OnValueUpdated, this);

        if (memberHandler.GetValueFromComplexObject(parentObj, out object? readValue))
        {
            Assert(readValue is bool);
            _value = (bool?) readValue;
            OnValueUpdated();
        }
    }
}
