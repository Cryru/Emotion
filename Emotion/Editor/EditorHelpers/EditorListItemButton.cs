#nullable enable

#region Using

using Emotion.Game.World.Editor;
using Emotion.Graphics;
using Emotion.Platform.Input;
using Emotion.UI;

#endregion

namespace Emotion.Editor.EditorHelpers;

// Used in EditorListOfItemsWithSelection
public class EditorListItemButton : UICallbackButton
{
    public string Text
    {
        get => _label.Text;
        set => _label.Text = value;
    }

    public bool Enabled
    {
        get => _enabled;
        set
        {
            _enabled = value;
            RecalculateButtonColor();
        }
    }

    private bool _enabled = true;

    public bool Selected
    {
        get => _selected;
        set
        {
            _selected = value;
            RecalculateButtonColor();
        }
    }

    private bool _selected;

    public object? UserData;

    private UIText _label;

    public EditorListItemButton(string label) : this()
    {
        Text = label;
    }

    public EditorListItemButton()
    {
        WindowColor = MapEditorColorPalette.ButtonColor;
        ScaleMode = UIScaleMode.FloatScale;

        var txt = new UIText();
        txt.ParentAnchor = UIAnchor.CenterLeft;
        txt.Anchor = UIAnchor.CenterLeft;
        txt.ScaleMode = UIScaleMode.FloatScale;
        txt.WindowColor = MapEditorColorPalette.TextColor;
        txt.Id = "buttonText";
        txt.FontSize = MapEditorColorPalette.EditorButtonTextSize;
        txt.IgnoreParentColor = true;
        _label = txt;
        AddChild(txt);

        FillX = false;
        FillY = false;

        StretchX = true;
        StretchY = true;
        Paddings = new Rectangle(2, 1, 2, 1);
    }

    protected override bool RenderInternal(RenderComposer c)
    {
        if (MouseInside || Selected) c.RenderSprite(Bounds, _calculatedColor);
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

    private void RecalculateButtonColor()
    {
        if (!Enabled)
        {
            WindowColor = MapEditorColorPalette.ButtonColorDisabled.SetAlpha(150);
            _label.IgnoreParentColor = false;
            return;
        }

        if (Selected)
        {
            WindowColor = MapEditorColorPalette.ButtonColor;
        }
        else if(MouseInside)
        {
            WindowColor = MapEditorColorPalette.ButtonColor * 0.5f;
        }

        _label.IgnoreParentColor = true;
    }
}