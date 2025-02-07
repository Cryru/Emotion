using Emotion.Game.World.Editor;
using Emotion.IO;
using Emotion.Platform.Implementation.CommonDesktop;
using Emotion.Standard.Reflector.Handlers;
using Emotion.UI;
using Emotion.WIPUpdates.One.EditorUI.Components;
using System.Globalization;
using System.Reflection.Emit;

#nullable enable

namespace Emotion.WIPUpdates.One.EditorUI.ObjectPropertiesEditorHelpers;

public class StringEditor : ObjectPropertyEditor
{
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

    public override void SetValue(object? value)
    {
        UITextInput2? textInput = GetWindowById<UITextInput2>("TextInput");
        AssertNotNull(textInput);
        if (value == null)
            textInput.Text = "<null>";
        else
            textInput.Text = (string)value;
    }

    private void OnTextInputChanged(string txt)
    {
        OnValueChanged(txt);
    }
}
