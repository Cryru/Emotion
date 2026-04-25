#nullable enable

namespace Emotion.Game.Systems.UI;

public partial class UIBaseWindow
{
    public class FreeLayout : LayoutMethodCodeClass
    {
        public override int GetMainAxis(UIBaseWindow self)
        {
            return 0;
        }

        public override int GetChildrenSize(UIBaseWindow self, int axis)
        {
            int childrenSize = 0;

            foreach (UIBaseWindow child in self.Children)
            {
                if (SkipWindowLayout(child)) continue;
                if (!child.CalculatedMetrics.InsideParent) continue;

                int childSize = child.CalculatedMetrics.Size[axis] + child.CalculatedMetrics.MarginTotalSize[axis];
                childrenSize = Math.Max(childrenSize, childSize);
            }

            return childrenSize;
        }

        public override void GrowShrinkAxis(UIBaseWindow self, int axis)
        {
            IntVector2 myMeasuredSize = self.CalculatedMetrics.GetContentSize();
            int availableSize = myMeasuredSize[axis];
            foreach (UIBaseWindow child in self.Children)
            {
                if (SkipWindowLayout(child)) continue;

                int childSize = child.CalculatedMetrics.Size[axis];
                int sizeInto = availableSize - child.CalculatedMetrics.MarginTotalSize[axis];

                UISizing sizing = GetSizingInDirection(child, axis);
                if (sizing.CanGrow() && childSize < sizeInto)
                    child.CalculatedMetrics.Size[axis] = sizeInto;
                if (sizing.CanShrink() && childSize > sizeInto)
                    child.CalculatedMetrics.Size[axis] = sizeInto;
            }
        }

        public override void PositionChildren(UIBaseWindow self)
        {
            IntRectangle contentRect = self.CalculatedMetrics.GetContentRect();
            IntRectangle boundsRect = self.CalculatedMetrics.Bounds;

            foreach (UIBaseWindow child in self.Children)
            {
                if (SkipWindowLayout(child)) continue;
                FreeLayoutPosition(child, contentRect, boundsRect);
            }
        }

        public static void FreeLayoutPosition(UIBaseWindow child, in IntRectangle contentRect, in IntRectangle boundsRect)
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
    }
}
