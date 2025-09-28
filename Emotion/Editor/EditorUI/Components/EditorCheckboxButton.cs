#nullable enable

namespace Emotion.Editor.EditorUI.Components;

public class EditorCheckboxButton : SquareEditorButtonWithTexture
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
            Texture.Visuals.Visible = value;
            _valueSet = true;
        }
    }

    private bool _value;
    private bool _valueSet = false;

    public EditorCheckboxButton() : base("Editor/Checkmark.png", 24, true)
    {
    }

    protected override void OnClicked()
    {
        base.OnClicked();

        Value = !Value;
        OnValueChanged?.Invoke(Value);
    }
}
