using Emotion.Game.World.Editor;
using Emotion.UI;

#nullable enable

namespace Emotion.WIPUpdates.One.EditorUI.ObjectPropertiesEditorHelpers;

public class StringEditor : TypeEditor
{
    private string? _text;

    public StringEditor()
    {
        var inputBackground = new UISolidColor
        {
            WindowColor = Color.Black * 0.5f,
            Paddings = new Primitives.Rectangle(5, 3, 5, 3)
        };
        AddChild(inputBackground);

        UITextInput2 input = new UITextInput2
        {
            Id = "TextInput",

            FontSize = EditorColorPalette.EditorButtonTextSize,
            MinSizeX = 100,
            AnchorAndParentAnchor = UIAnchor.CenterLeft,
            IgnoreParentColor = true,

            SubmitOnEnter = true,
            SubmitOnFocusLoss = true,
            OnSubmit = OnTextInputChanged
        };
        inputBackground.AddChild(input);
    }

    public override void SetValue(object? value)
    {
        UITextInput2? textInput = GetWindowById<UITextInput2>("TextInput");
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
