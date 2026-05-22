#nullable enable

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
    Disabled
}

public class OneButton : UIBaseButton
{
    public string Text
    {
        get => _label.Text;
        set => _label.Text = value;
    }

    protected UIText _label;
    protected ButtonState _state = ButtonState.Default;
    protected ButtonType _type = ButtonType.Default;

    private Color _defaultColor;
    private Color _hoverColor;
    private Color _disabledColor;
    private Color _textColor;
    private Color _textColorDisabled;

    public OneButton(string labelText, Action<UIBaseButton>? onClicked = null, ButtonType buttonType = ButtonType.Default)
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

        var label = new UIText()
        {
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

        // Setup style
        _type = buttonType;
        _defaultColor = OnePalette.PRIMARY_6;
        _hoverColor = OnePalette.PRIMARY_5;
        _disabledColor = OnePalette.PRIMARY_DISABLED_3;
        _textColor = OnePalette.PRIMARY_2;
        _textColorDisabled = OnePalette.PRIMARY_DISABLED_1;

        if (_type == ButtonType.Outlined)
        {
            Visuals.Border = 1;
            Visuals.BorderColor = OnePalette.PRIMARY_2;
        }
        else if (_type == ButtonType.Destructive)
        {
            _clickIsOnUp = true;
            _defaultColor = OnePalette.ERROR_2;
            _hoverColor = OnePalette.ERROR_1;
            _disabledColor = OnePalette.ERROR_DISABLED_1;
        }
        else if (_type == ButtonType.Important)
        {
            _defaultColor = OnePalette.PRIMARY_4;
            _hoverColor = OnePalette.PRIMARY_7;
        }
        else if (_type == ButtonType.Warning)
        {
            _defaultColor = OnePalette.WARNING_3;
            _hoverColor = OnePalette.WARNING_2;
            _disabledColor = OnePalette.WARNING_DISABLED_1;
            _textColor = OnePalette.PRIMARY_5;
        }

        // Initialize
        _state = ButtonState.Default;
        ApplyButtonState();

        OnClicked = onClicked;
    }

    protected void ApplyButtonState()
    {
        Visuals.BackgroundColor = _state switch
        {
            ButtonState.Default => _defaultColor,
            ButtonState.Hover => _hoverColor,
            ButtonState.Disabled => _disabledColor,
            _ => _defaultColor
        };
        _label.TextColor = _state switch
        {
            ButtonState.Default => _textColor,
            ButtonState.Hover => _textColor,
            ButtonState.Disabled => _textColorDisabled,
            _ => _textColor
        };
    }

    public void SetState(ButtonState state, bool freezeState = false)
    {
        _state = state;
        ApplyButtonState();
        _freezeState = freezeState;
    }

    private bool _freezeState = false;

    private void RecalculateState()
    {
        if (_freezeState) return;

        if (!Enabled)
            SetState(ButtonState.Disabled);
        else if (MouseInside)
            SetState(ButtonState.Hover);
        else
            SetState(ButtonState.Default);
    }

    public override void OnMouseEnter(Vector2 mousePos)
    {
        base.OnMouseEnter(mousePos);
        RecalculateState();
    }

    public override void OnMouseLeft(Vector2 mousePos)
    {
        base.OnMouseLeft(mousePos);
        RecalculateState();
    }
}
