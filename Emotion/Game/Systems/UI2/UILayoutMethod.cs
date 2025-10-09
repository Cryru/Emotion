#nullable enable

namespace Emotion.Game.Systems.UI2;

public record struct UILayoutMethod
{
    public enum UIMethodName
    {
        Free,
        HorizontalList,
        VerticalList,
        HorizontalListWrap,
        VerticalListWrap,
    }

    public enum ListLayoutItemsAlign
    {
        Beginning,
        Center,
        End
    }

    public UIMethodName Mode;
    public IntVector2 ListSpacing;
    public ListLayoutItemsAlign ListItemsAlign; // todo

    public readonly int GetListMask()
    {
        return Mode == UIMethodName.HorizontalList ? 0 : 1;
    }

    public int GetListInverseMask()
    {
        int mask = GetListMask();
        return mask == 0 ? 1 : 0;
    }

    public bool GrowingAlongList(UIWindowLayoutConfig layout)
    {
        int mask = GetListMask();
        return mask == 0 ? layout.SizingX.Mode == UISizing.UISizingMode.Grow : layout.SizingY.Mode == UISizing.UISizingMode.Grow;
    }

    public bool GrowingAcrossList(UIWindowLayoutConfig layout)
    {
        int inverseMask = GetListInverseMask();
        return inverseMask == 0 ? layout.SizingX.Mode == UISizing.UISizingMode.Grow : layout.SizingY.Mode == UISizing.UISizingMode.Grow;
    }

    public static UILayoutMethod Free()
    {
        return new UILayoutMethod()
        {
            Mode = UIMethodName.Free,
        };
    }

    public static UILayoutMethod HorizontalList(int spacing, ListLayoutItemsAlign alignItems = ListLayoutItemsAlign.Beginning)
    {
        return new UILayoutMethod()
        {
            Mode = UIMethodName.HorizontalList,
            ListSpacing = new IntVector2(spacing, 0),
            ListItemsAlign = alignItems
        };
    }

    public static UILayoutMethod VerticalList(int spacing, ListLayoutItemsAlign alignItems = ListLayoutItemsAlign.Beginning)
    {
        return new UILayoutMethod()
        {
            Mode = UIMethodName.VerticalList,
            ListSpacing = new IntVector2(0, spacing),
            ListItemsAlign = alignItems
        };
    }

    public static UILayoutMethod HorizontalListWrap(int spacingX, int spacingY, ListLayoutItemsAlign alignItems = ListLayoutItemsAlign.Beginning)
    {
        return new UILayoutMethod()
        {
            Mode = UIMethodName.HorizontalListWrap,
            ListSpacing = new IntVector2(spacingX, spacingY),
            ListItemsAlign = alignItems
        };
    }

    public static UILayoutMethod VerticalListWrap(int spacingX, int spacingY, ListLayoutItemsAlign alignItems = ListLayoutItemsAlign.Beginning)
    {
        return new UILayoutMethod()
        {
            Mode = UIMethodName.VerticalListWrap,
            ListSpacing = new IntVector2(spacingX, spacingY),
            ListItemsAlign = alignItems
        };
    }

    public override readonly string ToString()
    {
        if (Mode == UIMethodName.HorizontalList || Mode == UIMethodName.VerticalList)
            return $"{Mode} {ListSpacing} (Align {ListItemsAlign})";

        return $"{Mode}";
    }
}
