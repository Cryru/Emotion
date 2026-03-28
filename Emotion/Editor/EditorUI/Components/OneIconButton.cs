#nullable enable

using Emotion.Game.Systems.UI2.Editor;

namespace Emotion.Editor.EditorUI.Components;

public class OneIconButton : UIBaseButton
{
    public TextureReference Texture
    {
        get => _pic.Texture;
        set => _pic.Texture = value;
    }

    protected UIPicture _pic;
    protected ButtonState _state = ButtonState.Default;
    protected ButtonType _type = ButtonType.Default;

    private Color _defaultColor;
    private Color _hoverColor;
    private Color _disabledColor;
    private Color _iconColor;
    private Color _iconColorDisabled;

    public OneIconButton(TextureReference texture, Action<UIBaseButton>? onClicked = null, ButtonType buttonType = ButtonType.Default)
    {
        Visuals = new UIWindowVisualConfig()
        {
            RoundRadius = 4
        };
        Layout = new UIWindowLayoutConfig()
        {
            SizingX = UISizing.Fixed(28),
            SizingY = UISizing.Fixed(28),
        };

        var pic = new UIPicture()
        {
            Texture = texture,
            Smooth = true,
            Layout =
            {
                AnchorAndParentAnchor = UIAnchor.CenterCenter,
                SizingX = UISizing.Fixed(20), // todo: This shouldn't be needed - the parent is fixed so it shouldn't be expanded
                SizingY = UISizing.Fixed(20),
            }
        };
        AddChild(pic);
        _pic = pic;

        // Setup style
        _type = buttonType;
        _defaultColor = OnePalette.PRIMARY_6;
        _hoverColor = OnePalette.PRIMARY_5;
        _disabledColor = OnePalette.PRIMARY_DISABLED_3;
        _iconColor = OnePalette.PRIMARY_2;
        _iconColorDisabled = OnePalette.PRIMARY_DISABLED_1;

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
        }
        else if (_type == ButtonType.Warning)
        {
            _defaultColor = OnePalette.WARNING_3;
            _hoverColor = OnePalette.WARNING_2;
            _disabledColor = OnePalette.WARNING_DISABLED_1;

            _iconColor = OnePalette.PRIMARY_5;
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
        _pic.ImageColor = _state switch
        {
            ButtonState.Default => _iconColor,
            ButtonState.Hover => _iconColor,
            ButtonState.Disabled => _iconColorDisabled,
            _ => _iconColor
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
