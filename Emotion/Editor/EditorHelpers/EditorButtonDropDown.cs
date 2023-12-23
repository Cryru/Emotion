#region Using

using Emotion.Common.Serialization;
using Emotion.Game.World.Editor;
using Emotion.Game.World2D.EditorHelpers;
using Emotion.UI;
using System.Linq;

#endregion

#nullable enable

namespace Emotion.Editor.EditorHelpers;

/// <summary>
/// A button which opens a dropdown when clicked.
/// </summary>
public class EditorButtonDropDown : UIBaseWindow
{
    private EditorDropDownItem[] _dropDownNoItems =
    {
        new()
        {
            Name = "Empty"
        }
    };

    public string? Text
    {
        get => _text;
        set
        {
            _text = value;
            var label = (MapEditorLabel?)GetWindowById("Label");
            if (label != null) label.Text = _text;
        }
    }

    private string? _text;

    protected int _currentOption;
    protected EditorDropDownItem[] _items;

    [DontSerialize]
    public Action<int, EditorDropDownItem>? OnSelectionChanged;

    public EditorButtonDropDown()
    {
        StretchX = true;
        StretchY = true;
        LayoutMode = LayoutMode.HorizontalList;

        _items = _dropDownNoItems;
        _currentOption = 0;
    }

    public void SetItems(EditorDropDownItem[]? items, int current = 0)
    {
        _items = items == null ? _dropDownNoItems : items;

        var setSelection = 0;
        if (_items.Length > current) setSelection = current;
        SetSelectedItem(setSelection);
    }

    public void SetItems<T>(List<T>? items, int current = 0)
    {
        if (items == null || items.Count == 0)
        {
            SetItems(null, 0);
            return;
        }

        EditorDropDownItem[] dropDownItems = new EditorDropDownItem[items.Count];
        if (items != null)
            for (int i = 0; i < items.Count; i++)
            {
                var item = items[i];
                var itemDescr = new EditorDropDownItem
                {
                    Name = item?.ToString() ?? "<null>",
                    UserData = item
                };
                dropDownItems[i] = itemDescr;
            }

        SetItems(dropDownItems, current);
    }

    public void SetSelectedItem(int idx)
    {
        _currentOption = idx;
        UpdateCurrentOptionText();

        if (_items.Length > idx)
            OnSelectionChanged?.Invoke(idx, _items[idx]);
    }

    protected virtual void UpdateCurrentOptionText()
    {
        var button = (EditorButton?)GetWindowById("Button");
        if (button == null) return;

        var currentOptionItem = _items[_currentOption];
        button.Text = currentOptionItem?.Name ?? "<null>";
        button.Enabled = _items != null && _items.Length > 1;
    }

    public override void AttachedToController(UIController controller)
    {
        base.AttachedToController(controller);

        // -> UseNewLayoutSystem container
        UIBaseWindow tempContainer = new UIBaseWindow();
        tempContainer.UseNewLayoutSystem = true;
        tempContainer.LayoutMode = LayoutMode.HorizontalList;
        tempContainer.StretchX = true;
        tempContainer.StretchY = true;
        tempContainer.ListSpacing = new Vector2(1, 0);
        LayoutMode = LayoutMode.Free;
        AddChild(tempContainer);

        var label = new MapEditorLabel(_text);
        label.Id = "Label";
        tempContainer.AddChild(label);

        var button = new EditorButton();
        button.AlignAnchor = UIAnchor.CenterLeft;
        button.MinSize = new Vector2(20, 0);
        button.Id = "Button";
        button.OnClickedProxy = click =>
        {
            var dropDown = new EditorDropDown(true);
            dropDown.Offset = button.RenderBounds.BottomLeft / button.GetScale();
            dropDown.SetItems(_items, (i, selected) => SetSelectedItem(i));
            Controller!.AddChild(dropDown);
        };
        tempContainer.AddChild(button);

        var arrowImage = new UITexture();
        arrowImage.TextureFile = "Editor/LittleArrow.png";
        arrowImage.ImageScale = new Vector2(0.2f);
        arrowImage.Margins = new Rectangle(3, 0, 0, 0);
        arrowImage.AlignAnchor = UIAnchor.CenterRight;
        arrowImage.Dock = UIDockDirection.Right;
        arrowImage.ZOffset = -1;
        button.AddChild(arrowImage);

        UpdateCurrentOptionText();
    }
}