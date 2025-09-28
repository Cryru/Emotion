#nullable enable

using Emotion.Game.Systems.UI2;

namespace Emotion.Editor.EditorUI;

public class EditorDropDown : UIDropDown
{
    public bool ClampToSpawningWindowWidth = false;

    private UIBaseWindow? _contentParent;

    public EditorDropDown() : base()
    {
        Visuals.BackgroundColor = EditorColorPalette.BarColor;
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

    protected override void InternalCustomLayout()
    {
        if (ClampToSpawningWindowWidth && AttachedTo != null)
            Layout.SizingX = UISizing.Fixed(AttachedTo.CalculatedMetrics.Size.X);
        else
            Layout.SizingX = UISizing.Fit();

        base.InternalCustomLayout();
    }

    public static EditorDropDown OpenListDropdown(UIBaseWindow spawningWindow)
    {
        EditorDropDown dropDown = new()
        {
            Layout =
            {
                Padding = new UISpacing(5, 5, 5, 5),
                Anchor = UIAnchor.TopLeft,
                ParentAnchor = UIAnchor.BottomLeft,
            },
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