using Emotion.Editor.EditorUI.Base;
using Emotion.Editor.EditorUI.Components;
using Emotion.Game.Systems.UI;

#nullable enable

namespace Emotion.Editor.EditorUI.ObjectPropertiesEditorHelpers;

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
            Layout =
            {
                Offset = new IntVector2(0, 4)
            },

            Smooth = true,
            TextureFile = "Editor/LittleArrow.png",
            ImageScale = new Vector2(0.6f),
            AnchorAndParentAnchor = UIAnchor.CenterCenter
        };
        arrowSquare.AddChild(arrowIcon);
    }

    protected void Clicked()
    {
        if (_items == null) return;

        EditorDropDown dropDown = EditorDropDown.OpenListDropdown(this);
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

    protected override bool RenderInternal(Renderer c)
    {
        return base.RenderInternal(c);
    }
}
