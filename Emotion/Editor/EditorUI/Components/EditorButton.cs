#nullable enable

#region Using

using Emotion.Core.Systems.Input;
using Emotion.Game.Systems.UI;
using Emotion.Game.Systems.UI2;

#endregion

namespace Emotion.Editor.EditorUI.Components;

public class EditorButton : UICallbackButton
{
    public string Text
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

    private string _text = string.Empty;

    #region Theme

    public Color NormalColor = EditorColorPalette.ButtonColor;
    public Color RolloverColor = EditorColorPalette.ActiveButtonColor;
    public Color ActiveColor = EditorColorPalette.ActiveButtonColor;
    public Color DisabledColor = EditorColorPalette.ButtonColorDisabled.SetAlpha(150);

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
        Layout.SizingX = UISizing.Fit();
        Layout.SizingY = UISizing.Fit();
        Layout.Padding = new UISpacing(6, 3, 6, 3);

        _label = new EditorLabel
        {
            IgnoreParentColor = true,
            Name = "buttonText",
            Text = _text,
            Visible = _text != null,
            DontTakeSpaceWhenHidden = true
        };
        AddChild(_label);
        RecalculateButtonColor();
    }

    protected override void OnOpen()
    {
        base.OnOpen();
        RecalculateButtonColor();
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
            Visuals.BackgroundColor = DisabledColor;
            return;
        }

        if (_activeMode || Engine.UI.HasDropdown(this))
        {
            Visuals.BackgroundColor = ActiveColor;
            return;
        }

        Visuals.BackgroundColor = MouseInside ? RolloverColor : NormalColor;
    }

    public override void OnDropdownStateChanged(bool opened)
    {
        RecalculateButtonColor();
    }
}