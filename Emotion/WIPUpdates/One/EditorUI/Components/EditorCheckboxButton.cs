#nullable enable

using Emotion.Common.Serialization;
using Emotion.UI;

namespace Emotion.WIPUpdates.One.EditorUI.Components;

public class EditorCheckboxButton : SquareEditorButton
{
    [DontSerialize]
    public Action<bool>? OnValueChanged;

    public bool Value
    {
        get => _value;
        set
        {
            if (_value == value && _valueSet) return;
            _value = value;
            _checkIcon.Visible = value;
            _valueSet = true;
        }
    }

    private bool _value;
    private bool _valueSet = false;

    private UITexture _checkIcon;

    public EditorCheckboxButton() : base()
    {
        UITexture checkIcon = new UITexture()
        {
            TextureFile = "Editor/Checkmark.png",
            ImageScale = new Vector2(0.75f),
            Smooth = true,
            AnchorAndParentAnchor = UIAnchor.CenterCenter,
            IgnoreParentColor = true
        };
        AddChild(checkIcon);
        _checkIcon = checkIcon;
    }

    protected override void OnClicked()
    {
        base.OnClicked();

        Value = !Value;
        OnValueChanged?.Invoke(Value);
    }
}
