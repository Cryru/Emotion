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

        public override int GetChildrenSize(UIBaseWindow self, List<UIBaseWindow> children, int axis)
        {
            int childrenSize = 0;

            foreach (UIBaseWindow child in children)
            {
                int childSize = child.CalculatedMetrics.Size[axis] + child.CalculatedMetrics.MarginTotalSize[axis];
                childrenSize = Math.Max(childrenSize, childSize);
            }

            return childrenSize;
        }

        public override void GrowShrinkAxis(UIBaseWindow self, List<UIBaseWindow> children, int axis)
        {
            IntVector2 myMeasuredSize = GetLayoutRect(self).Size;
            int availableSize = myMeasuredSize[axis];
            foreach (UIBaseWindow child in children)
            {
                int childSize = child.CalculatedMetrics.Size[axis];
                int sizeInto = availableSize - child.CalculatedMetrics.MarginTotalSize[axis];

                UISizing sizing = GetSizingInDirection(child, axis);
                if (sizing.CanGrow() && childSize < sizeInto)
                    child.CalculatedMetrics.Size[axis] = sizeInto;
                if (sizing.CanShrink() && childSize > sizeInto)
                    child.CalculatedMetrics.Size[axis] = sizeInto;
            }
        }

        public override void PositionChildren(UIBaseWindow self, List<UIBaseWindow> children)
        {
            IntRectangle contentRect = GetLayoutRect(self);
            IntRectangle boundsRect = self.CalculatedMetrics.Bounds;

            foreach (UIBaseWindow child in children)
            {
                SetAnchorPosition(child, contentRect, boundsRect);
            }
        }

        protected virtual IntRectangle GetLayoutRect(UIBaseWindow self)
        {
            return self.CalculatedMetrics.GetViewportRect();
        }
    }
}
