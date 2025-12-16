using Emotion.Game.Systems.UI;

#nullable enable

namespace Emotion.Editor.EditorUI.ObjectPropertiesEditorHelpers;

public class StringEditor : TypeEditor
{
    private string? _text;

    public StringEditor()
    {
        var inputBackground = new UISolidColor
        {
            WindowColor = Color.Black * 0.5f,
            Paddings = new Rectangle(5, 3, 5, 3)
        };
        AddChild(inputBackground);

        UITextInput input = new UITextInput
        {
            Name = "TextInput",

            FontSize = EditorColorPalette.EditorButtonTextSize,
            Layout =
            {
                MinSizeX = 100,
                AnchorAndParentAnchor = UIAnchor.CenterLeft
            },
            IgnoreParentColor = true,

            SubmitOnEnter = true,
            SubmitOnFocusLoss = true,
            OnSubmit = OnTextInputChanged
        };
        inputBackground.AddChild(input);
    }

    public override void SetValue(object? value)
    {
        UITextInput? textInput = GetWindowById<UITextInput>("TextInput");
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
