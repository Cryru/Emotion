#nullable enable

using System.Buffers;
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
            int listMask = GetListMask(layoutMethod);
            int inverseListMask = GetListInverseMask(layoutMethod);

            int listChildrenCount = 0;
            IntVector2 pen = IntVector2.Zero;
            foreach (UIBaseWindow child in self.Children)
            {
                if (SkipWindowLayout(child)) continue;

                child.Layout_Step1_Measure();
                IntVector2 childSize = child.CalculatedMetrics.Size;
                pen[listMask] += childSize[listMask] + child.CalculatedMetrics.MarginTotalSize[listMask];
                pen[inverseListMask] = Math.Max(pen[inverseListMask], childSize[inverseListMask] + child.CalculatedMetrics.MarginTotalSize[inverseListMask]);
                listChildrenCount++;
            }

            int totalSpacing = GetListSpacing(self, listMask) * (listChildrenCount - 1);

            //bool fitAlongList = listMask == 0 ? Layout.SizingX.Mode == UISizing.UISizingMode.Fit : Layout.SizingY.Mode == UISizing.UISizingMode.Fit;
            childrenSize[listMask] = pen[listMask] + totalSpacing;
            childrenSize[inverseListMask] = pen[inverseListMask];

            //bool fitAcrossList = inverseListMask == 0 ? Layout.SizingX.Mode == UISizing.UISizingMode.Fit : Layout.SizingY.Mode == UISizing.UISizingMode.Fit;
            //if (fitAcrossList)
            //    mySize[inverseListMask] += pen[inverseListMask];
        }

        public override void Step2_Grow(UIBaseWindow self)
        {
            UILayoutMethod layoutMethod = self.Layout.LayoutMethod;
            int listMask = GetListMask(layoutMethod);
            int inverseListMask = GetListInverseMask(layoutMethod);

            IntVector2 myMeasuredSize = self.CalculatedMetrics.GetContentSize();

            // Grow across list
            foreach (UIBaseWindow child in self.Children)
            {
                if (SkipWindowLayout(child)) continue;

                bool growingAcross = GrowingAcrossList(layoutMethod, child.Layout);
                if (!growingAcross) continue;

                child.CalculatedMetrics.Size[inverseListMask] = myMeasuredSize[inverseListMask] - child.CalculatedMetrics.MarginTotalSize[inverseListMask];
            }

            GrowAlongList(self, myMeasuredSize, listMask);

            // Now the children can grow their children
            foreach (UIBaseWindow child in self.Children)
            {
                if (SkipWindowLayout(child)) continue;
                child.Layout_Step2_Grow();
            }
        }

        private static void GrowAlongList(UIBaseWindow self, IntVector2 myMeasuredSize, int listMask)
        {
            UILayoutMethod layoutMethod = self.Layout.LayoutMethod;

            // Fixed/non-growing children
            int listChildrenCount = 0;
            float listRemainingSize = myMeasuredSize[listMask];
            foreach (UIBaseWindow child in self.Children)
            {
                if (SkipWindowLayout(child)) continue;
                listRemainingSize -= child.CalculatedMetrics.Size[listMask] + child.CalculatedMetrics.MarginTotalSize[listMask];
                listChildrenCount++;
            }
            listRemainingSize -= GetListSpacing(self, listMask) * (listChildrenCount - 1);
            if (listRemainingSize <= 0) return; // No space left for the growing ones

            // Collect window limits
            var growingChildren = new List<UIBaseWindow>(); // todo: pool
            foreach (UIBaseWindow child in self.Children)
            {
                if (SkipWindowLayout(child)) continue;
                if (!GrowingAlongList(layoutMethod, child.Layout)) continue;
                growingChildren.Add(child);
            }

            // Distribute remaining size among growers
            bool[] frozen = ArrayPool<bool>.Shared.Rent(growingChildren.Count);
            Array.Clear(frozen, 0, growingChildren.Count);

            int unfrozenCount = growingChildren.Count;
            while (unfrozenCount > 0 && listRemainingSize > 1f)
            {
                float share = listRemainingSize / unfrozenCount;
                float newRemaining = 0f;

                // Distribute the share to growing children
                for (int i = 0; i < growingChildren.Count; i++)
                {
                    if (frozen[i]) continue;

                    UIBaseWindow child = growingChildren[i];
                    int baseSize = child.CalculatedMetrics.Size[listMask] + child.CalculatedMetrics.MarginTotalSize[listMask];
                    int desired = (int)Math.Floor(baseSize + share);
                    int clamped = Math.Clamp(desired, child.CalculatedMetrics.MinSize[listMask], child.CalculatedMetrics.MaxSize[listMask]);

                    child.CalculatedMetrics.Size[listMask] = clamped;

                    // Hit the max for this window, freeze it
                    if (clamped < desired)
                    {
                        frozen[i] = true;
                        unfrozenCount--;
                        newRemaining += desired - clamped;
                    }
                }

                // Nothing was frozen this pass we distribute the leftover pixels one pixel at a time
                if (newRemaining == 0f)
                {
                    int distributed = 0;
                    for (int i = 0; i < growingChildren.Count; i++)
                    {
                        if (frozen[i]) continue;
                        distributed += (int)Math.Floor(share);
                    }

                    int remainderPixels = (int)listRemainingSize - distributed;
                    for (int i = 0; i < growingChildren.Count && remainderPixels > 0; i++)
                    {
                        if (frozen[i]) continue;
                        growingChildren[i].CalculatedMetrics.Size[listMask]++;
                        remainderPixels--;
                    }
                    break;
                }

                listRemainingSize = newRemaining;
            }
            ArrayPool<bool>.Shared.Return(frozen);
        }

        public override void Step3_Position(UIBaseWindow self)
        {
            IntRectangle contentRect = self.CalculatedMetrics.GetContentRect();
            IntRectangle boundsRect = self.CalculatedMetrics.Bounds;

            UILayoutMethod layoutMethod = self.Layout.LayoutMethod;
            int listMask = GetListMask(layoutMethod);
            int inverseListMask = GetListInverseMask(layoutMethod);

            int listSpacing = GetListSpacing(self, listMask);
            IntVector2 pen = contentRect.Position;
            foreach (UIBaseWindow child in self.Children)
            {
                if (SkipWindowLayout(child)) continue;
                if (!child.CalculatedMetrics.InsideParent)
                {
                    // Parents outisde the parent list are free layout
                    FreeLayout.FreeLayoutChild(child, contentRect, boundsRect);
                    continue;
                }

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

        public static int GetListSpacing(UIBaseWindow self, int listMask)
        {
            return (int)Maths.RoundAwayFromZero(self.Layout.LayoutMethod.ListSpacing[listMask] * self.CalculatedMetrics.Scale[listMask]);
        }

        public static int GetListMask(in UILayoutMethod layout)
        {
            return layout.Mode == UIMethodName.HorizontalList ? 0 : 1;
        }

        public static int GetListInverseMask(in UILayoutMethod layout)
        {
            int mask = GetListMask(layout);
            return mask == 0 ? 1 : 0;
        }

        public static bool GrowingAlongList(in UILayoutMethod parentLayoutMethod, in UIWindowLayoutConfig layout)
        {
            int mask = GetListMask(parentLayoutMethod);
            return mask == 0 ? layout.SizingX.Mode == UISizing.UISizingMode.Grow : layout.SizingY.Mode == UISizing.UISizingMode.Grow;
        }

        public static bool GrowingAcrossList(in UILayoutMethod parentLayoutMethod, in UIWindowLayoutConfig layout)
        {
            int inverseMask = GetListInverseMask(parentLayoutMethod);
            return inverseMask == 0 ? layout.SizingX.Mode == UISizing.UISizingMode.Grow : layout.SizingY.Mode == UISizing.UISizingMode.Grow;
        }
    }
}
