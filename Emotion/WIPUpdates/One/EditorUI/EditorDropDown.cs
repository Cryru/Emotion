using Emotion.Game.World.Editor;
using Emotion.UI;

#nullable enable

namespace Emotion.WIPUpdates.One.Tools;

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
            pos.X -= diff;
            size.X += diff;
        }

        base.Layout(pos, size);
    }

    protected override bool RenderInternal(RenderComposer c)
    {
        c.RenderSprite(Position, Size, MapEditorColorPalette.BarColor);
        c.RenderOutline(Position, Size, MapEditorColorPalette.ActiveButtonColor, 3 * GetScale(), false);

        return base.RenderInternal(c);
    }

    public static EditorDropDown OpenListDropdown(UIBaseWindow spawningWindow)
    {
        Emotion.WIPUpdates.One.Tools.EditorDropDown dropDown = new(spawningWindow)
        {
            Paddings = new Primitives.Rectangle(5, 5, 5, 5),
            Anchor = UIAnchor.TopLeft,
            ParentAnchor = UIAnchor.BottomLeft,
        };
        spawningWindow.Controller?.AddChild(dropDown);

        UIScrollArea scrollArea = new UIScrollArea()
        {
            AutoHideScrollY = true
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