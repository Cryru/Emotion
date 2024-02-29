#nullable enable

#region Using

using Emotion.Game.World.Editor;
using Emotion.Graphics;
using Emotion.Platform.Input;
using Emotion.UI;

#endregion

namespace Emotion.Editor.EditorHelpers;

public class EditorButton : UICallbackButton
{
    public string? Text
    {
        get => _text;
        set
        {
            _text = value;
            if (_label != null) _label.Text = _text;
        }
    }

    private string? _text;

    public bool Enabled
    {
        get => _enabled;
        set
        {
            _enabled = value;

            if (Controller == null) return;
            RecalculateButtonColor();
        }
    }

    private bool _enabled = true;

    #region Theme

    public Color NormalColor = MapEditorColorPalette.ButtonColor;
    public Color RolloverColor = MapEditorColorPalette.ActiveButtonColor;
    public Color DisabledColor = MapEditorColorPalette.ButtonColorDisabled.SetAlpha(150);

    #endregion

    public object? UserData;

    private bool _activeMode;
    private UIText _label = null!;

    public EditorButton(string label) : this()
    {
        Text = label;
    }

    public EditorButton()
    {
        ScaleMode = UIScaleMode.FloatScale;
        FillX = false;
        FillY = false;

        StretchX = true;
        StretchY = true;
        Paddings = new Rectangle(2, 1, 2, 1);
    }

    public override void AttachedToController(UIController controller)
    {
        base.AttachedToController(controller);

        WindowColor = NormalColor;

        var txt = new UIText();
        txt.ParentAnchor = UIAnchor.CenterLeft;
        txt.Anchor = UIAnchor.CenterLeft;
        txt.ScaleMode = UIScaleMode.FloatScale;
        txt.WindowColor = MapEditorColorPalette.TextColor;
        txt.Id = "buttonText";
        txt.FontSize = MapEditorColorPalette.EditorButtonTextSize;
        txt.IgnoreParentColor = true;
        txt.Text = _text;
        _label = txt;
        AddChild(txt);

        RecalculateButtonColor();
    }

    protected override bool RenderInternal(RenderComposer c)
    {
        c.RenderSprite(Bounds, _calculatedColor);
        return base.RenderInternal(c);
    }

    public override bool OnKey(Key key, KeyStatus status, Vector2 mousePos)
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

    private void RecalculateButtonColor()
    {
        _label.IgnoreParentColor = Enabled;
        if (!Enabled)
        {
            WindowColor = DisabledColor;
            return;
        }

        if (_activeMode)
        {
            WindowColor = RolloverColor;
            return;
        }

        WindowColor = MouseInside ? RolloverColor : NormalColor;
    }
}