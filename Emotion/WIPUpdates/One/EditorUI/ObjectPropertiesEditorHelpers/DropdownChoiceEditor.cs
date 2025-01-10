using Emotion.Editor.EditorHelpers;
using Emotion.Game.World.Editor;
using Emotion.UI;
using Emotion.Utility;
using Emotion.WIPUpdates.One.EditorUI.Components;

#nullable enable

namespace Emotion.WIPUpdates.One.EditorUI.ObjectPropertiesEditorHelpers;

public class DropdownChoiceEditor<T> : UIBaseWindow
{
    private UIRichText _label;

    private IEnumerable<T>? _items;
    private T? _current;
    private int _currentIndex = -1;

    public DropdownChoiceEditor()
    {
        Paddings = new Primitives.Rectangle(2, 2, 2, 2);
        
        LayoutMode = LayoutMode.HorizontalList;
        ListSpacing = new Vector2(5, 0);

        var label = new EditorLabel()
        {
            Margins = new Primitives.Rectangle(10, 0, 5, 0)
        };
        AddChild(label);
        _label = label;

        var button = new EditorButton()
        {
            MinSizeY = 25,
            MinSizeX = 25,
            Paddings = Rectangle.Empty,
            AnchorAndParentAnchor = UIAnchor.TopRight,
            OnClickedProxy = (me) =>
            {
                if (_items == null) return;

                Emotion.WIPUpdates.One.Tools.EditorDropDown dropDown = Emotion.WIPUpdates.One.Tools.EditorDropDown.OpenListDropdown(this);
                //dropDown.ClampToSpawningWindowWidth = true;
                dropDown.MaxSizeY = 300;
                dropDown.ParentAnchor = UIAnchor.BottomRight;
                dropDown.Anchor = UIAnchor.TopRight;

                foreach (T item in _items)
                {
                    {
                        var button = new EditorButton
                        {
                            Text = item?.ToString() ?? "<null>",
                            FillX = true
                        };
                        dropDown.AddChild(button);
                    }
                    {
                        var button = new EditorButton
                        {
                            Text = item?.ToString() ?? "<null>",
                            FillX = true
                        };
                        dropDown.AddChild(button);
                    }
                    {
                        var button = new EditorButton
                        {
                            Text = item?.ToString() ?? "<null>",
                            FillX = true
                        };
                        dropDown.AddChild(button);
                    }
                    {
                        var button = new EditorButton
                        {
                            Text = item?.ToString() ?? "<null>",
                            FillX = true
                        };
                        dropDown.AddChild(button);
                    }
                    {
                        var button = new EditorButton
                        {
                            Text = item?.ToString() ?? "<null>",
                            FillX = true
                        };
                        dropDown.AddChild(button);
                    }
                    {
                        var button = new EditorButton
                        {
                            Text = item?.ToString() ?? "<null>",
                            FillX = true
                        };
                        dropDown.AddChild(button);
                    }
                    {
                        var button = new EditorButton
                        {
                            Text = item?.ToString() ?? "<null>",
                            FillX = true
                        };
                        dropDown.AddChild(button);
                    }
                    {
                        var button = new EditorButton
                        {
                            Text = item?.ToString() ?? "<null>",
                            FillX = true
                        };
                        dropDown.AddChild(button);
                    }

                }
            }
        };
        // enabled = _items != null
        AddChild(button);

        var arrowIcon = new UITexture()
        {
            Smooth = true,
            TextureFile = "Editor/LittleArrow.png",
            ImageScale = new Vector2(0.6f),
            Offset = new Vector2(0, 1),
            AnchorAndParentAnchor = UIAnchor.CenterCenter
  
        };
        button.AddChild(arrowIcon);
    }

    public void SetEditor(IEnumerable<T> values, int currentValueIdx)
    {
        _items = values;
        _currentIndex = currentValueIdx;

        int i = 0;
        foreach (var item in values)
        {
            if (i == _currentIndex)
            {
                _label.Text = item?.ToString() ?? "<null>";
                break;
            }

            i++;
        }

        _label.Text = "<empty>";
    }

    public void SetEditor(IEnumerable<T> values, T? currentValue)
    {
        int i = 0;
        int idxFound = -1;
        foreach (var item in values)
        {
            if (Helpers.AreObjectsEqual(item, currentValue))
            {
                idxFound = i;
                break;
            }
            i++;
        }

        SetEditor(values, idxFound);
    }

    protected override bool RenderInternal(RenderComposer c)
    {
        c.RenderSprite(Bounds, MapEditorColorPalette.BarColor);
        c.RenderOutline(Bounds, MapEditorColorPalette.ActiveButtonColor, 2 * GetScale());

        return base.RenderInternal(c);
    }
}
