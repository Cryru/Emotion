using Emotion.Editor.EditorUI.Components;
using Emotion.Game.Systems.UI;

#nullable enable

namespace Emotion.Editor.EditorUI.ObjectPropertiesEditorHelpers;

public class StringEditor : TypeEditor
{
    private string? _text;

    public StringEditor()
    {
        var textInput = new OneTextInput()
        {
            Name = "TextInput"
        };
        textInput.OnSubmit = OnTextInputChanged;
        AddChild(textInput);
    }

    public override void SetValue(object? value)
    {
        OneTextInput? textInput = GetWindowById<OneTextInput>("TextInput");
        AssertNotNull(textInput);
        if (value == null)
            _text = "<null>";
        else
            _text = (string)value;

        textInput.Text = _text;
    }

    private void OnTextInputChanged(string txt)
    {
        if (_text == txt) return;
        _text = txt;
        OnValueChanged(txt);
    }
}
