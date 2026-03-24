#nullable enable

using static Emotion.Game.Systems.UI.UIBaseWindow;

namespace Emotion.Game.Systems.UI2;

public record struct UILayoutMethod
{
    public static Dictionary<UIMethodName, LayoutMethodCodeClass> LayoutClasses = new();

    static UILayoutMethod()
    {
        // todo
        LayoutClasses.Add(UIMethodName.Free, new FreeLayout());
        LayoutClasses.Add(UIMethodName.HorizontalList, new ListLayout());
        LayoutClasses.Add(UIMethodName.VerticalList, new ListLayout());
        LayoutClasses.Add(UIMethodName.HorizontalListWrap, new ListLayout());
        LayoutClasses.Add(UIMethodName.VerticalListWrap, new ListLayout());
        LayoutClasses.Add(UIMethodName.Grid, new GridLayout());
    }

    public static LayoutMethodCodeClass GetLayoutCode(in UILayoutMethod layoutMethod)
    {
        LayoutClasses.TryGetValue(layoutMethod.Mode, out LayoutMethodCodeClass? method);
        return method!;
    }

    public enum UIMethodName
    {
        Free,
        HorizontalList,
        VerticalList,
        HorizontalListWrap,
        VerticalListWrap,
        Grid,
    }

    public enum ListLayoutItemsAlign
    {
        Beginning,
        Center,
        End
    }

    public struct LayoutPropertiesGrid
    {
        public int ColumnCount;
        public bool UniformRowHeight;
        public bool UniformColumnWidth;
    }

    public UIMethodName Mode;
    public IntVector2 ListSpacing;
    public ListLayoutItemsAlign ListItemsAlign; // todo
    public LayoutPropertiesGrid GridProperties;

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

    public static UILayoutMethod Grid(int columnCount, int cellSpacingX = 0, int cellSpacingY = 0, bool uniformRowHeight = false, bool uniformColumnWidth = false)
    {
        return new UILayoutMethod()
        {
            Mode = UIMethodName.Grid,
            ListSpacing = new IntVector2(cellSpacingX, cellSpacingY),
            GridProperties = new LayoutPropertiesGrid()
            {
                ColumnCount = columnCount,
                UniformRowHeight = uniformRowHeight,
                UniformColumnWidth = uniformColumnWidth,
            }
        };
    }

    public override readonly string ToString()
    {
        if (Mode == UIMethodName.HorizontalList || Mode == UIMethodName.VerticalList)
            return $"{Mode} {ListSpacing} (Align {ListItemsAlign})";

        if (Mode == UIMethodName.Grid)
            return $"{Mode} Cols={GridProperties.ColumnCount}";

        return $"{Mode}";
    }
}
