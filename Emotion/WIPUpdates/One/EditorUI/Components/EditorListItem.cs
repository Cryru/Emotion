#nullable enable

using Emotion.Game.World.Editor;
using Emotion.UI;

namespace Emotion.WIPUpdates.One.EditorUI.Components;

public class EditorListItem<T> : EditorButton
{
    public Color SelectedColor = MapEditorColorPalette.ButtonColor;

    public T? Item { get; protected set; }

    public bool Selected
    {
        get => _selected;
        set
        {
            _selected = value;
            RecalculateButtonColor();
        }
    }

    protected bool _selected;
    protected int _itemIdx;

    private Action<int, T?>? _onClick;
    private UIBaseWindow? _buttonList;

    public EditorListItem(int itemIdx, T? item, Action<int, T?>? onClick)
    {
        NormalColor = MapEditorColorPalette.ButtonColor.CloneWithAlpha(150);

        _itemIdx = itemIdx;
        Item = item;
        FillX = true;
        _onClick = onClick;

        if (item != null)
            EngineEditor.RegisterForObjectChanges(item, UpdateLabel, this);
        UpdateLabel();
    }

    public void AttachButton(SquareEditorButton button)
    {
        Assert(button.Parent == null, "Button shouldn't have a UI parent");

        if (_buttonList == null)
        {
            _buttonList = new UIBaseWindow()
            {
                LayoutMode = LayoutMode.HorizontalList,
                ListSpacing = new Vector2(3, 0)
            };
            AddChild(_buttonList);

            _label.Margins = new Primitives.Rectangle(0, 0, 35, 0);
        }
       
        button.ParentAnchor = UI.UIAnchor.CenterRight;
        button.Anchor = UI.UIAnchor.CenterRight;
        _buttonList.AddChild(button);
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
        _onClick?.Invoke(_itemIdx, Item);
    }

    protected override void RecalculateButtonColor()
    {
        base.RecalculateButtonColor();
        if (Selected)
            WindowColor = SelectedColor;

        _label.IgnoreParentColor = true;
    }
}
