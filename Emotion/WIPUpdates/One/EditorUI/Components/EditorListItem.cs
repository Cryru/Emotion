#nullable enable

using Emotion.Game.World.Editor;
using Emotion.UI;

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

    public EditorListItem(T item, Action<T>? onClick, bool canEdit = false)
    {
        AssertNotNull(item);

        NormalColor = MapEditorColorPalette.ButtonColor.CloneWithAlpha(150);

        Item = item;
        FillX = true;
        _onClick = onClick;

        if (canEdit)
        {
            _label.Margins = new Primitives.Rectangle(0, 0, 35, 0);

            var editButton = new EditorEditObjectButton()
            {
                ParentAnchor = UI.UIAnchor.CenterRight,
                Anchor = UI.UIAnchor.CenterRight
            };
            editButton.SetEditor(item);
            AddChild(editButton);
        }

        EngineEditor.RegisterForObjectChanges(item, UpdateLabel, this);
        UpdateLabel();
    }

    private void UpdateLabel()
    {
        Text = Item?.ToString() ?? "<null>";
    }

    public override void DetachedFromController(UIController controller)
    {
        base.DetachedFromController(controller);
        EngineEditor.UnregisterForObjectChanges(this);
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
