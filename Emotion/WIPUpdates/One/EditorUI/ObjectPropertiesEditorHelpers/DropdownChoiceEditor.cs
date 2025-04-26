using Emotion.Editor.EditorHelpers;
using Emotion.Game.World.Editor;
using Emotion.UI;
using Emotion.Utility;
using Emotion.WIPUpdates.One.EditorUI.Base;
using Emotion.WIPUpdates.One.EditorUI.Components;

#nullable enable

namespace Emotion.WIPUpdates.One.EditorUI.ObjectPropertiesEditorHelpers;

public class DropdownChoiceEditor<T> : ArrayEditorBase<T>
{
    public string? Text
    {
        get => _button.Text;
        set => _button.Text = value;
    }

    private EditorButton _button;

    public DropdownChoiceEditor()
    {
        var button = new EditorButton()
        {
            GrowX = true,
            OnClickedProxy = (_) => Clicked()
        };
        AddChild(button);
        _button = button;

        var arrowSquare = new UIBaseWindow()
        {
            GrowX = false,
            GrowY = false,
            MinSizeY = 23,
            MinSizeX = 29,
            AnchorAndParentAnchor = UIAnchor.TopRight,
        };
        AddChild(arrowSquare);

        var arrowIcon = new UITexture()
        {
            Smooth = true,
            TextureFile = "Editor/LittleArrow.png",
            ImageScale = new Vector2(0.6f),
            Offset = new Vector2(0, 4),
            AnchorAndParentAnchor = UIAnchor.CenterCenter
        };
        arrowSquare.AddChild(arrowIcon);
    }

    protected void Clicked()
    {
        if (_items == null) return;

        Emotion.WIPUpdates.One.Tools.EditorDropDown dropDown = Emotion.WIPUpdates.One.Tools.EditorDropDown.OpenListDropdown(this);
        //dropDown.ClampToSpawningWindowWidth = true;
        dropDown.MaxSizeY = 300;
        dropDown.ParentAnchor = UIAnchor.BottomRight;
        dropDown.Anchor = UIAnchor.TopRight;

        int idx = 0;
        foreach (T item in _items)
        {
            var button = new EditorListItem<T>(-1, item, (_, item) => dropDown.Close());
            button.Selected = _currentIndex == idx;
            dropDown.AddChild(button);

            idx++;
        }
    }

    protected override void OnItemsChanged(IEnumerable<T>? items)
    {

    }

    protected override void OnSelectionChanged(T? newSel)
    {
        _button.Text = newSel?.ToString() ?? "<null>";
    }

    protected override void OnSelectionEmpty()
    {
        _button.Text = "<empty>";
    }

    protected override bool RenderInternal(RenderComposer c)
    {
        return base.RenderInternal(c);
    }
}
