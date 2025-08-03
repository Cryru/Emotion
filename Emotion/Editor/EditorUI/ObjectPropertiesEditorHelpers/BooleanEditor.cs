#nullable enable

using Emotion.Editor.EditorUI.Components;

namespace Emotion.Editor.EditorUI.ObjectPropertiesEditorHelpers;

public class BooleanEditor : TypeEditor
{
    public BooleanEditor()
    {
        var checkbox = new EditorCheckboxButton()
        {
            Id = "Checkbox",
            OnValueChanged = OnInputValueChanged
        };
        AddChild(checkbox);
    }

    public override void SetValue(object? value)
    {
        var input = GetWindowById<EditorCheckboxButton>("Checkbox");
        AssertNotNull(input);
        input.Value = (bool?) value ?? false;
    }

    private void OnInputValueChanged(bool newValue)
    {
        OnValueChanged(newValue);
    }
}
