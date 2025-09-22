using Emotion.Game.Systems.UI;
using Emotion.Game.Systems.UI2;

#nullable enable

namespace Emotion.Editor.EditorUI;

public class EditorDropDown : UIDropDown
{
    public bool ClampToSpawningWindowWidth = false;

    private UIBaseWindow? _contentParent;

    public EditorDropDown() : base()
    {
        Visuals.Color = EditorColorPalette.BarColor;
        Visuals.Border = 3;
        Visuals.BorderColor = EditorColorPalette.ActiveButtonColor;
    }

    public override void AddChild(UIBaseWindow? child)
    {
        if (_contentParent != null)
        {
            _contentParent.AddChild(child);
            return;
        }
        base.AddChild(child);
    }

    protected override void OLDLayout(Vector2 pos, Vector2 size)
    {
        //if (ClampToSpawningWindowWidth && size.X > SpawningWindow.Width)
        //    size.X = SpawningWindow.Width;

        //if (size.X < SpawningWindow.Width)
        //{
        //    float diff = SpawningWindow.Width - Size.X;
        //    if (Anchor == UIAnchor.TopRight) pos.X -= diff;
        //    size.X += diff;
        //}

        base.OLDLayout(pos, size);
    }

    public static EditorDropDown OpenListDropdown(UIBaseWindow spawningWindow)
    {
        EditorDropDown dropDown = new()
        {
            Layout =
            {
                Padding = new UISpacing(5, 5, 5, 5)
            },
            Anchor = UIAnchor.TopLeft,
            ParentAnchor = UIAnchor.BottomLeft,
        };

        var scrollArea = new UIScrollArea()
        {
            AutoHideScrollY = true,
            ExpandX = true,
            ExpandY = true,
            // todo: there should probably be a max height here
        };
        dropDown.AddChild(scrollArea);

        var list = new UIBaseWindow()
        {
            Layout =
            {
                LayoutMethod = UILayoutMethod.VerticalList(3)
            }
        };
        scrollArea.AddChildInside(list);

        dropDown._contentParent = list;
        Engine.UI.OpenDropdown(spawningWindow, dropDown);

        return dropDown;
    }
}