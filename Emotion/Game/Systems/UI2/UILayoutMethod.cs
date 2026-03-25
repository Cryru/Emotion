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
        // 0 in the column or row count means auto calculate

        public int ColumnCount;
        public int RowCount;

        public bool UniformRowHeight;
        public bool UniformColumnWidth;
    }

    public UIMethodName Mode;
    public IntVector2 ListSpacing;
    public ListLayoutItemsAlign ListItemsAlign; // todo
    public LayoutPropertiesGrid GridProperties;

    public static UILayoutMethod Free()
    {
        return new UILayoutMethod()
        {
            Mode = UIMethodName.Free,
        };
    }

    public static UILayoutMethod HorizontalList(int spacing, ListLayoutItemsAlign alignItems = ListLayoutItemsAlign.Beginning)
    {
        //return new UILayoutMethod()
        //{
        //    Mode = UIMethodName.HorizontalList,
        //    ListSpacing = new IntVector2(spacing, 0),
        //    ListItemsAlign = alignItems
        //};

        return new UILayoutMethod()
        {
            Mode = UIMethodName.Grid,
            ListSpacing = new IntVector2(spacing, 0),
            ListItemsAlign = alignItems,
            GridProperties = new LayoutPropertiesGrid()
            {
                ColumnCount = 0,
                RowCount = 1,
                UniformRowHeight = false,
                UniformColumnWidth = false,
            }
        };
    }

    public static UILayoutMethod VerticalList(int spacing, ListLayoutItemsAlign alignItems = ListLayoutItemsAlign.Beginning)
    {
        //return new UILayoutMethod()
        //{
        //    Mode = UIMethodName.VerticalList,
        //    ListSpacing = new IntVector2(0, spacing),
        //    ListItemsAlign = alignItems
        //};

        return new UILayoutMethod()
        {
            Mode = UIMethodName.Grid,
            ListSpacing = new IntVector2(0, spacing),
            ListItemsAlign = alignItems,
            GridProperties = new LayoutPropertiesGrid()
            {
                ColumnCount = 1,
                RowCount = 0,
                UniformRowHeight = false,
                UniformColumnWidth = false,
            }
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

    public static UILayoutMethod Grid_FixedColumns(int columnCount, int cellSpacingX = 0, int cellSpacingY = 0, bool uniformRowHeight = false, bool uniformColumnWidth = false)
    {
        return new UILayoutMethod()
        {
            Mode = UIMethodName.Grid,
            ListSpacing = new IntVector2(cellSpacingX, cellSpacingY),
            GridProperties = new LayoutPropertiesGrid()
            {
                ColumnCount = columnCount,
                RowCount = 0,
                UniformRowHeight = uniformRowHeight,
                UniformColumnWidth = uniformColumnWidth,
            }
        };
    }

    public static UILayoutMethod Grid_FixedRows(int rowCount, int cellSpacingX = 0, int cellSpacingY = 0, bool uniformRowHeight = false, bool uniformColumnWidth = false)
    {
        return new UILayoutMethod()
        {
            Mode = UIMethodName.Grid,
            ListSpacing = new IntVector2(cellSpacingX, cellSpacingY),
            GridProperties = new LayoutPropertiesGrid()
            {
                ColumnCount = 0,
                RowCount = rowCount,
                UniformRowHeight = uniformRowHeight,
                UniformColumnWidth = uniformColumnWidth,
            }
        };
    }

    public static UILayoutMethod Grid_Auto(int cellSpacingX = 0, int cellSpacingY = 0, bool uniformRowHeight = false, bool uniformColumnWidth = false)
    {
        return new UILayoutMethod()
        {
            Mode = UIMethodName.Grid,
            ListSpacing = new IntVector2(cellSpacingX, cellSpacingY),
            GridProperties = new LayoutPropertiesGrid()
            {
                ColumnCount = 0,
                RowCount = 0,
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
