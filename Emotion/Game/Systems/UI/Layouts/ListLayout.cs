#nullable enable

using static Emotion.Game.Systems.UI2.UILayoutMethod;

namespace Emotion.Game.Systems.UI;

public partial class UIBaseWindow
{
    public class ListLayout : LayoutMethodCodeClass
    {
        public override void Step1_Measure(UIBaseWindow self, out IntVector2 childrenSize)
        {
            childrenSize = IntVector2.Zero;

            UILayoutMethod layoutMethod = self.Layout.LayoutMethod;
            int listMask = layoutMethod.GetListMask();
            int inverseListMask = layoutMethod.GetListInverseMask();

            int listChildrenCount = 0;
            IntVector2 pen = IntVector2.Zero;
            foreach (UIBaseWindow child in self.Children)
            {
                if (SkipWindowLayout(child)) continue;

                IntVector2 childSize;
                if (child.IsLoading())
                {
                    childSize = IntVector2.Zero;
                }
                else
                {
                    child.Layout_Step1_Measure();
                    childSize = child.CalculatedMetrics.Size;
                }
                pen[listMask] += childSize[listMask] + child.CalculatedMetrics.MarginTotalSize[listMask];
                pen[inverseListMask] = Math.Max(pen[inverseListMask], childSize[inverseListMask] + child.CalculatedMetrics.MarginTotalSize[inverseListMask]);
                listChildrenCount++;
            }

            int totalSpacing = GetListSpacing(self, listMask) * (listChildrenCount - 1);

            //bool fitAlongList = listMask == 0 ? Layout.SizingX.Mode == UISizing.UISizingMode.Fit : Layout.SizingY.Mode == UISizing.UISizingMode.Fit;
            childrenSize[listMask] += pen[listMask] + totalSpacing;
            childrenSize[inverseListMask] += pen[inverseListMask];

            //bool fitAcrossList = inverseListMask == 0 ? Layout.SizingX.Mode == UISizing.UISizingMode.Fit : Layout.SizingY.Mode == UISizing.UISizingMode.Fit;
            //if (fitAcrossList)
            //    mySize[inverseListMask] += pen[inverseListMask];
        }

        public override void Step2_Grow(UIBaseWindow self)
        {
            UILayoutMethod layoutMethod = self.Layout.LayoutMethod;
            int listMask = layoutMethod.GetListMask();
            int inverseListMask = layoutMethod.GetListInverseMask();

            IntVector2 myMeasuredSize = self.CalculatedMetrics.GetContentSize();

            // Grow across list
            foreach (UIBaseWindow child in self.Children)
            {
                if (SkipWindowLayout(child)) continue;

                bool growingAcross = layoutMethod.GrowingAcrossList(child.Layout);
                if (!growingAcross) continue;

                child.CalculatedMetrics.Size[inverseListMask] = myMeasuredSize[inverseListMask] - child.CalculatedMetrics.MarginTotalSize[inverseListMask];
            }

            // Grow along list
            int listChildrenCount = 0;
            float listRemainingSize = myMeasuredSize[listMask];
            foreach (UIBaseWindow child in self.Children)
            {
                if (SkipWindowLayout(child)) continue;

                float childSize = child.CalculatedMetrics.Size[listMask] + child.CalculatedMetrics.MarginTotalSize[listMask];
                listRemainingSize -= childSize;
                listChildrenCount++;
            }
            listRemainingSize -= GetListSpacing(self, listMask) * (listChildrenCount - 1);

            int infinitePrevention = 0;
            while (listRemainingSize > self.Children.Count - 1) // Until list remaining size cannot be split
            {
                infinitePrevention++;
                if (infinitePrevention > 50)
                {
                    Assert(false, "Infinite loop in GrowWindow() :(");
                    break;
                }

                int smallest = int.MaxValue;
                int secondSmallest = int.MaxValue;
                int growingCount = 0;
                foreach (UIBaseWindow child in self.Children)
                {
                    if (SkipWindowLayout(child)) continue;

                    bool growAlongList = layoutMethod.GrowingAlongList(child.Layout);
                    if (!growAlongList) continue;

                    growingCount++;

                    int sizeListDirection = child.CalculatedMetrics.Size[listMask];
                    // Initialize smallest
                    if (smallest == int.MaxValue)
                    {
                        smallest = sizeListDirection;
                        continue;
                    }
                    // Smaller than smallest
                    else if (sizeListDirection < smallest)
                    {
                        secondSmallest = smallest;
                        smallest = sizeListDirection;
                    }
                    // Bigger than smallest but smaller than second smallest
                    else if (sizeListDirection > smallest && sizeListDirection < secondSmallest)
                    {
                        secondSmallest = sizeListDirection;
                    }
                }

                // Nothing to do.
                if (growingCount == 0)
                    break;

                int widthToAdd = Math.Min(secondSmallest - smallest, (int)Math.Round(listRemainingSize / growingCount));
                foreach (UIBaseWindow child in self.Children)
                {
                    if (SkipWindowLayout(child)) continue;

                    bool growAlongList = layoutMethod.GrowingAlongList(child.Layout);
                    if (!growAlongList) continue;

                    int sizeListDirection = child.CalculatedMetrics.Size[listMask];
                    if (sizeListDirection == smallest)
                    {
                        child.CalculatedMetrics.Size[listMask] += widthToAdd;
                        listRemainingSize -= widthToAdd;
                    }
                }
            }

            // Now the children can grow their children
            foreach (UIBaseWindow child in self.Children)
            {
                if (SkipWindowLayout(child)) continue;
                child.Layout_Step2_Grow();
            }
        }

        public override void Step3_Position(UIBaseWindow self, IntRectangle contentRect)
        {
            UILayoutMethod layoutMethod = self.Layout.LayoutMethod;
            int listMask = layoutMethod.GetListMask();
            int inverseListMask = layoutMethod.GetListInverseMask();

            int listSpacing = GetListSpacing(self, listMask);
            IntVector2 pen = contentRect.Position;
            foreach (UIBaseWindow child in self.Children)
            {
                if (SkipWindowLayout(child)) continue;

                child.CalculatedMetrics.InsideParent = true;

                IntVector2 childPosition = pen;
                ListLayoutItemsAlign alignAcrossList = GetItemsAlignAcrossFromList(layoutMethod.Mode, child.Layout.Anchor);
                switch (alignAcrossList)
                {
                    case ListLayoutItemsAlign.Center:
                        childPosition[inverseListMask] += contentRect.Size[inverseListMask] / 2 - child.CalculatedMetrics.Size[inverseListMask] / 2;
                        break;
                    case ListLayoutItemsAlign.End:
                        childPosition[inverseListMask] += contentRect.Size[inverseListMask] - child.CalculatedMetrics.Size[inverseListMask];
                        break;
                }
                child.AddWarning(child.Layout.Anchor != UIAnchor.TopLeft && alignAcrossList == ListLayoutItemsAlign.Beginning, UILayoutWarning.AnchorInListDoesntDoAnything);

                // Add margin (todo: this needs to be the right margin when items are aligned to end, none when centered (for the two outside ones) etc)
                IntVector2 childTopLeftMargin = child.CalculatedMetrics.MarginLeftTop;
                childPosition[listMask] += childTopLeftMargin[listMask];

                child.Layout_Step3_Position(childPosition + child.CalculatedMetrics.Offsets);
                pen[listMask] += child.CalculatedMetrics.Size[listMask] + child.CalculatedMetrics.MarginTotalSize[listMask] + listSpacing;
            }
        }

        private int GetListSpacing(UIBaseWindow self, int listMask)
        {
            return (int)Maths.RoundAwayFromZero(self.Layout.LayoutMethod.ListSpacing[listMask] * self.CalculatedMetrics.Scale[listMask]);
        }
    }
}
