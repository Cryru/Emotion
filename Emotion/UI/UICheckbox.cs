using Emotion.Platform.Input;

namespace Emotion.UI;

public class UICheckbox : UIBaseWindow
{
    #region Theme

    public Color NormalColor = Color.PrettyYellow;
    public Color RolloverColor = new Color("#6da832");
    public Color DisabledColor = new Color("#888888");
    public Color CheckmarkColor = Color.Black;

    #endregion

    public Action<UICheckbox, bool>? OnValueChanged;

    public bool Value
    {
        get => _value;
        set
        {
            if (_value == value) return;
            _value = value;
            OnValueChanged?.Invoke(this, value);

            UIBaseWindow? checkMark = GetWindowById("Checkmark");
            if (checkMark != null)
                checkMark.Visible = value;
        }
    }

    private bool _value;

    public UICheckbox(bool initialValue)
    {
        StretchX = true;
        StretchY = true;

        FillX = false;
        FillY = false;
        HandleInput = true;

        Value = initialValue;
    }

    public override void AttachedToController(UIController controller)
    {
        base.AttachedToController(controller);

        var inputBg = new UISolidColor
        {
            WindowColor = NormalColor,
            Anchor = UIAnchor.CenterLeft,
            ParentAnchor = UIAnchor.CenterLeft,
            Id = "Background",
            MinSize = new Vector2(12, 12)
        };
        AddChild(inputBg);

        var checkMark = new UITexture
        {
            TextureFile = "Editor/Checkmark.png",
            RenderSize = new Vector2(10, 10),
            Smooth = true,
            Visible = _value,
            WindowColor = CheckmarkColor,
            Id = "Checkmark",
            AnchorAndParentAnchor = UIAnchor.CenterCenter
        };
        inputBg.AddChild(checkMark);
    }

    public override void OnMouseEnter(Vector2 _)
    {
        base.OnMouseEnter(_);

        UIBaseWindow? bg = GetWindowById("Background");
        if (bg != null)
            bg.WindowColor = RolloverColor;
    }

    public override void OnMouseLeft(Vector2 mousePos)
    {
        base.OnMouseLeft(mousePos);

        UIBaseWindow? bg = GetWindowById("Background");
        if (bg != null)
            bg.WindowColor = NormalColor;
    }

    public override bool OnKey(Key key, KeyState status, Vector2 mousePos)
    {
        if (key == Key.MouseKeyLeft && status == KeyState.Down)
        {
            Value = !Value;
            return false;
        }

        return base.OnKey(key, status, mousePos);
    }
}
