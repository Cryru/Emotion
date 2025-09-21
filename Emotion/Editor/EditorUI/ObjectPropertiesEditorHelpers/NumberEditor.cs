using Emotion.Game.Systems.UI;
using Emotion.Game.Systems.UI.Text.TextUpdate;
using System.Globalization;

#nullable enable

namespace Emotion.Editor.EditorUI.ObjectPropertiesEditorHelpers;

public class NumberEditor<TNumber> : TypeEditor where TNumber : INumber<TNumber>
{
    public NumberEditor()
    {
        var inputBackground = new UISolidColor
        {
            WindowColor = Color.Black * 0.5f,
            Paddings = new Rectangle(5, 3, 5, 3)
        };
        AddChild(inputBackground);

        UITextInput2 input = new UITextInput2
        {
            Name = "TextInput",

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
            textInput.Text = "<null>";
        else
            textInput.Text = ((TNumber)value).ToString();
    }

    private void OnTextInputChanged(string txt)
    {
        if (TNumber.TryParse(txt, CultureInfo.InvariantCulture, out TNumber? valParsed))
            OnValueChanged(valParsed);
    }
}
