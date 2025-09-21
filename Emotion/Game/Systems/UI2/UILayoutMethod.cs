using Emotion.Game.Systems.UI;
using System.Threading.Tasks;

namespace Emotion.Game.Systems.UI2;

public struct UILayoutMethod
{
    public enum UIMethodName
    {
        Free,
        HorizontalList,
        VerticalList
    }

    public UIMethodName Mode;
    public UIAnchor Anchor;
    public UIAnchor ParentAnchor;
    public Vector2 ListSpacing;

    public int GetListMask()
    {
        return Mode == UIMethodName.HorizontalList ? 0 : 1;
    }

    public int GetListInverseMask()
    {
        int mask = GetListMask();
        return mask == 0 ? 1 : 0;
    }

    public bool GrowingAlongList(O_UIWindowLayoutMetrics layout)
    {
        int mask = GetListMask();
        return mask == 0 ? layout.SizingX.Mode == UISizing.UISizingMode.Grow : layout.SizingY.Mode == UISizing.UISizingMode.Grow;
    }

    public bool GrowingAcrossList(O_UIWindowLayoutMetrics layout)
    {
        int inverseMask = GetListInverseMask();
        return inverseMask == 0 ? layout.SizingX.Mode == UISizing.UISizingMode.Grow : layout.SizingY.Mode == UISizing.UISizingMode.Grow;
    }

    public static UILayoutMethod Free(UIAnchor anchor, UIAnchor parentAnchor)
    {
        return new UILayoutMethod()
        {
            Mode = UIMethodName.Free,
            Anchor = anchor,
            ParentAnchor = parentAnchor
        };
    }

    public static UILayoutMethod HorizontalList(float spacing)
    {
        return new UILayoutMethod()
        {
            Mode = UIMethodName.HorizontalList,
            ListSpacing = new Vector2(spacing, 0)
        };
    }

    public static UILayoutMethod VerticalList(float spacing)
    {
        return new UILayoutMethod()
        {
            Mode = UIMethodName.VerticalList,
            ListSpacing = new Vector2(0, spacing)
        };
    }

    public override string ToString()
    {
        if (Mode == UIMethodName.HorizontalList || Mode == UIMethodName.VerticalList)
            return $"{Mode} {ListSpacing}";

        if (Mode == UIMethodName.Free)
            return $"{Mode} {Anchor} {ParentAnchor}";

        return $"{Mode}";
    }
}
