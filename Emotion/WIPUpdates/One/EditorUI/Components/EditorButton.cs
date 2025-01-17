#nullable enable

#region Using

using Emotion;
using Emotion.Game.World.Editor;
using Emotion.Platform.Input;
using Emotion.UI;

#endregion

namespace Emotion.WIPUpdates.One.EditorUI.Components;

public class EditorButton : UICallbackButton
{
    public string? Text
    {
        get => _text;
        set
        {
            _text = value;

            AssertNotNull(_label);
            if (_label != null)
            {
                _label.Text = _text;
                _label.Visible = _text != null;
            }
        }
    }

    private string? _text;

    #region Theme

    public Color NormalColor = MapEditorColorPalette.ButtonColor;
    public Color RolloverColor = MapEditorColorPalette.ActiveButtonColor;
    public Color ActiveColor = MapEditorColorPalette.ActiveButtonColor;
    public Color DisabledColor = MapEditorColorPalette.ButtonColorDisabled.SetAlpha(150);

    #endregion

    public object? UserData;

    protected bool _activeMode;
    protected EditorLabel _label;

    public EditorButton(string label) : this()
    {
        Text = label;
    }

    public EditorButton()
    {
        FillX = false;
        FillY = false;
        Paddings = new Rectangle(6, 3, 6, 3);

        _label = new EditorLabel
        {
            IgnoreParentColor = true,
            Id = "buttonText",
            Text = _text,
            Visible = _text != null,
            DontTakeSpaceWhenHidden = true
        };
        AddChild(_label);
    }

    public override void AttachedToController(UIController controller)
    {
        base.AttachedToController(controller);
        RecalculateButtonColor();
    }

    protected override bool RenderInternal(RenderComposer c)
    {
        c.RenderSprite(Position, Size, _calculatedColor);
        return base.RenderInternal(c);
    }

    protected override bool UpdateInternal()
    {
        UpdateDropdownState();
        return base.UpdateInternal();
    }

    public override bool OnKey(Key key, KeyState status, Vector2 mousePos)
    {
        if (!Enabled) return false;
        return base.OnKey(key, status, mousePos);
    }

    public override void OnMouseEnter(Vector2 _)
    {
        if (!Enabled) return;
        base.OnMouseEnter(_);
        RecalculateButtonColor();
    }

    public override void OnMouseLeft(Vector2 _)
    {
        if (!Enabled) return;
        base.OnMouseLeft(_);

        CheckIfDropdownSpawned();
        RecalculateButtonColor();
    }

    public void SetActiveMode(bool activeLock)
    {
        _activeMode = activeLock;
        RecalculateButtonColor();
    }

    protected override void OnEnabledChanged()
    {
        base.OnEnabledChanged();
        RecalculateButtonColor();
    }

    protected virtual void RecalculateButtonColor()
    {
        if (_label != null)
            _label.IgnoreParentColor = Enabled;

        if (!Enabled)
        {
            WindowColor = DisabledColor;
            return;
        }

        if (_activeMode || _dropDownSpawned)
        {
            WindowColor = ActiveColor;
            return;
        }

        WindowColor = MouseInside ? RolloverColor : NormalColor;
    }

    #region DropDown Support

    private bool _dropDownSpawned;

    private void CheckIfDropdownSpawned()
    {
        if (HasDropdown()) _dropDownSpawned = true;
    }

    private bool HasDropdown()
    {
        return Controller != null && Controller.DropDown != null && Controller.DropDown.SpawningWindow == this;
    }

    private void UpdateDropdownState()
    {
        if (_dropDownSpawned)
        {
            bool hasDropdown = HasDropdown();
            if (!hasDropdown)
            {
                _dropDownSpawned = false;
                RecalculateButtonColor();
            }
        }
    }

    #endregion
}