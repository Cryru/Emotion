using Emotion.Game.World.Editor;
using Emotion.IO;
using Emotion.Platform.Implementation.CommonDesktop;
using Emotion.Standard.Reflector.Handlers;
using Emotion.UI;
using Emotion.WIPUpdates.One.EditorUI.Components;
using System.Reflection.Emit;

#nullable enable

namespace Emotion.WIPUpdates.One.EditorUI.ObjectPropertiesEditorHelpers;

public class BooleanEditor : ObjectPropertyEditor
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
