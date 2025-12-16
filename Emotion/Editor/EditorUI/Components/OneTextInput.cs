#nullable enable

using Emotion.Graphics.Text.TextUpdate;

namespace Emotion.Editor.EditorUI.Components;

public class OneTextInput : UIBaseWindow
{
    public OneTextInput(string placeholder = "Type something", bool multiLine = true)
    {
        Visuals = new UIWindowVisualConfig()
        {
            RoundRadius = 4,
            BackgroundColor = OnePalette.PRIMARY_6
        };
        Layout = new UIWindowLayoutConfig()
        {
            SizingX = UISizing.Fit(),
            MinSizeX = 200
        };

        var textContainer = new UIBaseWindow()
        {
            Layout =
            {
                Padding = new UISpacing(12, 6, 12, 6),
            }
        };
        AddChild(textContainer);

        var input = new UITextInput()
        {
            TextColor = OnePalette.PRIMARY_2,
            FontSize = OnePalette.FONT_SIZE,
            Font = OnePalette.FONT,
            Text = placeholder,
            MultiLine = multiLine
        };
        textContainer.AddChild(input);

        var line = new UIBaseWindow()
        {
            Layout =
            {
                AnchorAndParentAnchor = UIAnchor.BottomLeft,
                Offset = new IntVector2(0, -1),
                SizingX = UISizing.Grow(),
                SizingY = UISizing.Fixed(1)
            },
            Visuals =
            {
                BackgroundColor = OnePalette.PRIMARY_2
            }
        };
        AddChild(line);
    }
}
