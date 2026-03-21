#nullable enable

namespace Emotion.Game.Systems.UI;

public partial class UIBaseWindow
{
    public class FreeLayout : LayoutMethodCodeClass
    {
        public override void Step1_Measure(UIBaseWindow self, out IntVector2 childrenSize)
        {
            childrenSize = IntVector2.Zero;

            foreach (UIBaseWindow child in self.Children)
            {
                if (SkipWindowLayout(child)) continue;

                child.Layout_Step1_Measure();

                if (!child.CalculatedMetrics.InsideParent) continue;

                IntVector2 childSize = child.CalculatedMetrics.Size;
                childrenSize = IntVector2.Max(childrenSize, childSize + child.CalculatedMetrics.MarginTotalSize);
            }
        }

        public override void Step2_Grow(UIBaseWindow self)
        {
            IntVector2 myMeasuredSize = self.CalculatedMetrics.GetContentSize();
            foreach (UIBaseWindow child in self.Children)
            {
                if (SkipWindowLayout(child)) continue;

                if (child.Layout.SizingX.Mode == UISizing.UISizingMode.Grow)
                    child.CalculatedMetrics.Size.X = Math.Max(child.CalculatedMetrics.Size.X, myMeasuredSize.X - child.CalculatedMetrics.MarginTotalSize.X);

                if (child.Layout.SizingY.Mode == UISizing.UISizingMode.Grow)
                    child.CalculatedMetrics.Size.Y = Math.Max(child.CalculatedMetrics.Size.Y, myMeasuredSize.Y - child.CalculatedMetrics.MarginTotalSize.Y);

                child.Layout_Step2_Grow();
            }
        }

        public override void Step3_Position(UIBaseWindow self)
        {
            IntRectangle contentRect = self.CalculatedMetrics.GetContentRect();
            IntRectangle boundsRect = self.CalculatedMetrics.Bounds;

            foreach (UIBaseWindow child in self.Children)
            {
                if (SkipWindowLayout(child)) continue;
                FreeLayoutChild(child, contentRect, boundsRect);
            }
        }

        public static void FreeLayoutChild(UIBaseWindow child, in IntRectangle contentRect, in IntRectangle boundsRect)
        {
            // Shortcut for most common
            if (child.Layout.Anchor == UIAnchor.TopLeft && child.Layout.ParentAnchor == UIAnchor.TopLeft)
            {
                child.Layout_Step3_Position(contentRect.Position + child.CalculatedMetrics.MarginLeftTop + child.CalculatedMetrics.Offsets);
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
            child.Layout_Step3_Position(anchorPos + child.CalculatedMetrics.Offsets);
        }
    }
}
