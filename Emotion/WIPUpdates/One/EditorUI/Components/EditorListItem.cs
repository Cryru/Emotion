#nullable enable

using Emotion.Game.World.Editor;

namespace Emotion.WIPUpdates.One.EditorUI.Components;

public class EditorListItem<T> : EditorButton
{
    public Color SelectedColor = MapEditorColorPalette.ButtonColor;
    public T Item;

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

    private Action<T>? _onClick;

    public EditorListItem(T item, Action<T>? onClick)
    {
        NormalColor = MapEditorColorPalette.ButtonColor.CloneWithAlpha(150);

        Item = item;
        FillX = true;
        Text = item?.ToString() ?? "<null>";
        _onClick = onClick;
    }

    protected override bool RenderInternal(RenderComposer c)
    {
        c.RenderSprite(Position, Size, _calculatedColor);
        return true;
    }

    protected override void OnClicked()
    {
        _onClick?.Invoke(Item);
    }

    protected override void RecalculateButtonColor()
    {
        base.RecalculateButtonColor();
        if (Selected)
            WindowColor = SelectedColor;

        _label.IgnoreParentColor = true;
    }
}
