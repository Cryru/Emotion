using Emotion.Game.World.Editor;
using Emotion.IO;
using Emotion.Platform.Implementation.CommonDesktop;
using Emotion.Standard.Reflector.Handlers;
using Emotion.UI;
using Emotion.WIPUpdates.One.EditorUI.Components;
using System.Reflection.Emit;

#nullable enable

namespace Emotion.WIPUpdates.One.EditorUI.ObjectPropertiesEditorHelpers;

public class StringEditor : UIBaseWindow
{
    private object? _objectEditting;
    private string? _value;

    private ComplexTypeHandlerMember? _handler;

    public StringEditor()
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

        var inputBackground = new UISolidColor
        {
            WindowColor = Color.Black * 0.5f,
            Paddings = new Primitives.Rectangle(5, 3, 5, 3)
        };
        container.AddChild(inputBackground);

        UITextInput2 input = new UITextInput2
        {
            Id = "TextInput",

            FontSize = MapEditorColorPalette.EditorButtonTextSize,
            MinSizeX = 100,
            AnchorAndParentAnchor = UIAnchor.CenterLeft,
            IgnoreParentColor = true,

            SubmitOnEnter = true,
            SubmitOnFocusLoss = true,
            OnSubmit = OnTextInputChanged
        };
        inputBackground.AddChild(input);
    }

    public override void DetachedFromController(UIController controller)
    {
        base.DetachedFromController(controller);
        EngineEditor.UnregisterForObjectChanges(this);
    }

    private void OnTextInputChanged(string txt)
    {
        if (_objectEditting == null) return;

        _handler?.SetValueInComplexObject(_objectEditting, txt);
        EngineEditor.ObjectChanged(_objectEditting, this);
    }

    private void OnValueUpdated()
    {
        if (_objectEditting == null) return;

        UITextInput2? textInput = GetWindowById<UITextInput2>("TextInput");
        AssertNotNull(textInput);
        textInput.Text = _value ?? "";
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
            Assert(readValue is string);
            _value = (string?) readValue;
            OnValueUpdated();
        }
    }
}
