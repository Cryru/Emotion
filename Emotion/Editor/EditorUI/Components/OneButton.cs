#nullable enable

using Emotion.Graphics.Text;

namespace Emotion.Editor.EditorUI.Components;

public enum ButtonType
{
    Default,
    Outlined,
    Important,
    Destructive,
    Warning
}

public enum ButtonState
{
    Default,
    Hover,
    Pressed,
    Disabled
}

public class OneButton : UIBaseWindow
{
    public string Text
    {
        get => _label.Text;
        set => _label.Text = value;
    }

    protected UIText _label;
    protected ButtonState _state = ButtonState.Default;

    public OneButton(string labelText, ButtonType buttonType = ButtonType.Default)
    {
        Visuals = new UIWindowVisualConfig()
        {
            RoundRadius = 4
        };
        Layout = new UIWindowLayoutConfig()
        {
            Padding = new UISpacing(16, 0, 16, 0),
            SizingX = UISizing.Fit(),
            SizingY = UISizing.Fixed(28),
        };

        if (buttonType == ButtonType.Outlined)
        {
            Visuals.Border = 1;
            Visuals.BorderColor = OnePalette.PRIMARY_2;
        }

        var label = new UIText()
        {
            TextColor = OnePalette.PRIMARY_2,
            FontSize = OnePalette.FONT_SIZE,
            Font = OnePalette.FONT,
            Text = labelText,
            Layout =
            {
                AnchorAndParentAnchor = UIAnchor.CenterCenter
            }
        };
        AddChild(label);
        _label = label;

        _state = ButtonState.Default;
        ApplyButtonState();
    }

    protected void ApplyButtonState()
    {
        switch (_state)
        {
            case ButtonState.Default:
                {
                    Visuals.BackgroundColor = OnePalette.PRIMARY_6;
                    break;
                }
            case ButtonState.Hover:
                {
                    Visuals.BackgroundColor = OnePalette.PRIMARY_5;
                    break;
                }
        }
    }

    public void SetState(ButtonState state)
    {
        _state = state;
        ApplyButtonState();
    }
}
