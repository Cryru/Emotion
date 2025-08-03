using Emotion.Game.Systems.UI;

#nullable enable

namespace Emotion.Editor.EditorUI;

public class EditorDropDown : UIDropDown
{
    public bool ClampToSpawningWindowWidth = false;

    private UIBaseWindow? _contentParent;

    public EditorDropDown(UIBaseWindow spawningWindow) : base(spawningWindow)
    {

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

    protected override void Layout(Vector2 pos, Vector2 size)
    {
        if (ClampToSpawningWindowWidth && size.X > SpawningWindow.Width)
            size.X = SpawningWindow.Width;

        if (size.X < SpawningWindow.Width)
        {
            float diff = SpawningWindow.Width - Size.X;
            if (Anchor == UIAnchor.TopRight) pos.X -= diff;
            size.X += diff;
        }

        base.Layout(pos, size);
    }

    protected override bool RenderInternal(Renderer c)
    {
        c.RenderSprite(Position, Size, EditorColorPalette.BarColor);
        c.RenderRectOutline(Position, Size, EditorColorPalette.ActiveButtonColor, 3 * GetScale());

        return base.RenderInternal(c);
    }

    public static EditorDropDown OpenListDropdown(UIBaseWindow spawningWindow)
    {
        EditorDropDown dropDown = new(spawningWindow)
        {
            Paddings = new Rectangle(5, 5, 5, 5),
            Anchor = UIAnchor.TopLeft,
            ParentAnchor = UIAnchor.BottomLeft,
        };
        spawningWindow.Controller?.AddChild(dropDown);

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
            LayoutMode = LayoutMode.VerticalList,
            ListSpacing = new Vector2(0, 3),
        };
        scrollArea.AddChildInside(list);

        dropDown._contentParent = list;

        return dropDown;
    }
}