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
    private UIRichText _label;
    private UICallbackButton _button;
    private UISolidColor _arrowParent;

    public DropdownChoiceEditor()
    {
        Paddings = new Primitives.Rectangle(3, 3, 0, 3);

        var button = new UICallbackButton()
        {
            OnClickedProxy = (_) => Clicked(),
            OnMouseEnterProxy = (_) =>
            {
                _arrowParent!.WindowColor = MapEditorColorPalette.ActiveButtonColor;
            },
            OnMouseLeaveProxy = (_) =>
            {
                _arrowParent!.WindowColor = MapEditorColorPalette.ButtonColor;
            }
        };
        AddChild(button);
        _button = button;

        var label = new EditorLabel()
        {
            Margins = new Primitives.Rectangle(10, 0, 5, 0)
        };
        button.AddChild(label);
        _label = label;

        var arrowSquare = new UISolidColor()
        {
            FillX = false,
            FillY = false,
            WindowColor = MapEditorColorPalette.ButtonColor,
            MinSizeY = 23,
            MinSizeX = 29,
            AnchorAndParentAnchor = UIAnchor.TopRight,
        };
        button.AddChild(arrowSquare);
        _arrowParent = arrowSquare;

        var arrowIcon = new UITexture()
        {
            Smooth = true,
            TextureFile = "Editor/LittleArrow.png",
            ImageScale = new Vector2(0.6f),
            Offset = new Vector2(0, 1),
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
        dropDown.Offset = new Vector2(0, -3);

        foreach (T item in _items)
        {
            var button = new EditorButton
            {
                Text = item?.ToString() ?? "<null>",
                FillX = true
            };
            dropDown.AddChild(button);
        }
    }

    protected override void OnItemsChanged(IEnumerable<T>? items)
    {

    }

    protected override void OnSelectionChanged(T? newSel)
    {
        _label.Text = newSel?.ToString() ?? "<null>";
    }

    protected override void OnSelectionEmpty()
    {
        _label.Text = "<empty>";
    }

    protected override bool RenderInternal(RenderComposer c)
    {
        c.RenderSprite(Bounds, MapEditorColorPalette.BarColor);
        c.RenderOutline(Bounds, _button.MouseInside ? MapEditorColorPalette.ActiveButtonColor : MapEditorColorPalette.ButtonColor, 3 * GetScale());

        if (_button.MouseInside) c.RenderSprite(_button.Bounds, Color.White * 0.3f);

        return base.RenderInternal(c);
    }
}
