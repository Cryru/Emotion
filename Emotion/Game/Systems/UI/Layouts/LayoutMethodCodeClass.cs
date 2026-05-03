#nullable enable

using static Emotion.Game.Systems.UI2.UILayoutMethod;

namespace Emotion.Game.Systems.UI;

public partial class UIBaseWindow
{
    public abstract class LayoutMethodCodeClass
    {
        public virtual void PreLayout(UIBaseWindow self, List<UIBaseWindow> children)
        {

        }
        public abstract int GetMainAxis(UIBaseWindow self);
        public abstract int GetChildrenSize(UIBaseWindow self, List<UIBaseWindow> children, int axis);
        public abstract void GrowShrinkAxis(UIBaseWindow self, List<UIBaseWindow> children, int axis);
        public abstract void PositionChildren(UIBaseWindow self, List<UIBaseWindow> children);

        #region Layout Helpers

        public static int GetListSpacing(UIBaseWindow self, int listMask)
        {
            return (int)Maths.RoundAwayFromZero(self.Layout.LayoutMethod.ListSpacing[listMask] * self.CalculatedMetrics.Scale[listMask]);
        }

        public static int GetListMask(in UILayoutMethod layout)
        {
            return layout.Mode == UIMethodName.HorizontalList ? 0 : 1;
        }

        public static UISizing GetSizingInDirection(UIBaseWindow window, int axis)
        {
            return axis == 0 ? window.Layout.SizingX : window.Layout.SizingY;
        }

        public static bool SkipWindowLayout(UIBaseWindow window)
        {
            if (!window.Visuals.Visible && window.Visuals.DontTakeSpaceWhenHidden) return true;
            if (window.IsLoading()) return true;
            if (window._useCustomLayout) return true;
            return false;
        }

        public static bool IsNotAffectedByScroll(UIBaseWindow window)
        {
            return window is UIScrollbar || !window.CalculatedMetrics.InsideParent;
        }

        public static ListLayoutItemsAlign GetItemsAlignAcrossFromList(UIMethodName listType, UIAnchor anchor)
        {
            if (anchor == UIAnchor.TopLeft)
                return ListLayoutItemsAlign.Beginning;

            if (listType == UIMethodName.HorizontalList)
            {
                if (anchor == UIAnchor.CenterLeft || anchor == UIAnchor.CenterRight || anchor == UIAnchor.CenterCenter)
                    return ListLayoutItemsAlign.Center;
                if (anchor == UIAnchor.BottomLeft || anchor == UIAnchor.BottomRight || anchor == UIAnchor.BottomCenter)
                    return ListLayoutItemsAlign.End;
            }
            else if (listType == UIMethodName.VerticalList)
            {
                if (anchor == UIAnchor.TopCenter || anchor == UIAnchor.BottomCenter || anchor == UIAnchor.CenterCenter)
                    return ListLayoutItemsAlign.Center;
                if (anchor == UIAnchor.TopRight || anchor == UIAnchor.BottomRight || anchor == UIAnchor.CenterRight)
                    return ListLayoutItemsAlign.End;
            }

            return ListLayoutItemsAlign.Beginning;
        }

        public static void SetAnchorPosition(UIBaseWindow child, in IntRectangle contentRect, in IntRectangle boundsRect)
        {
            // Shortcut for most common
            if (child.Layout.Anchor == UIAnchor.TopLeft && child.Layout.ParentAnchor == UIAnchor.TopLeft)
            {
                child.Layout_Position(contentRect.Position + child.CalculatedMetrics.MarginLeftTop + child.CalculatedMetrics.Offsets);
                return;
            }

            // This will prevent left margins affecting us when the anchor is right
            IntRectangle contentRectForThisChild = child.CalculatedMetrics.InsideParent ? contentRect : boundsRect;
            contentRectForThisChild.Position += child.CalculatedMetrics.MarginLeftTop;
            contentRectForThisChild.Size -= child.CalculatedMetrics.MarginTotalSize;

            IntVector2 anchorPos = GetAnchorPosition(
                child.Layout.ParentAnchor, contentRectForThisChild,
                child.Layout.Anchor, child.CalculatedMetrics.Size
            );
            child.Layout_Position(anchorPos + child.CalculatedMetrics.Offsets);
        }

        #endregion
    }
}
