#nullable enable

using Emotion.WIPUpdates.One.EditorUI.Components;

namespace Emotion.WIPUpdates.One.EditorUI.ObjectPropertiesEditorHelpers;

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

    public override void SetValue(string memberName, object? value)
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
