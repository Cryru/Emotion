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
    Pressed,
    Disabled
}

public class UIBaseButton : UIBaseWindow
{
    [DontSerialize]
    public Action<UIBaseButton>? OnClicked;

    public bool Enabled
    {
        get => _enabled;
        set
        {
            if (_enabled == value) return;
            _enabled = value;
            OnEnabledChanged();
        }
    }

    private bool _enabled = true;

    protected bool _clickIsOnUp;

    public UIBaseButton()
    {
        HandleInput = true;
    }

    public override bool OnKey(Key key, KeyState status, Vector2 mousePos)
    {
        if (key == Key.MouseKeyLeft && Enabled)
        {
            if (status == KeyState.Down && !_clickIsOnUp)
            {
                InternalOnClicked();
                return false;
            }

            if (status == KeyState.Up && _clickIsOnUp)
            {
                InternalOnClicked();
                return false;
            }
        }

        return base.OnKey(key, status, mousePos);
    }

    protected virtual void InternalOnClicked()
    {
        OnClicked?.Invoke(this);
    }

    protected virtual void OnEnabledChanged()
    {

    }
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

        if (buttonType == ButtonType.Destructive)
            _clickIsOnUp = true;

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

    private void RecalculateState()
    {
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
