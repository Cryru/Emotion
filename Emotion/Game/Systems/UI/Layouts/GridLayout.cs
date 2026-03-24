#nullable enable

using Emotion.Primitives.Grids;
using System.Buffers;
using static Emotion.Game.Systems.UI2.UILayoutMethod;

namespace Emotion.Game.Systems.UI;

public partial class UIBaseWindow
{
    public class GridLayout : LayoutMethodCodeClass
    {
        public override void Step1_Measure(UIBaseWindow self, out IntVector2 childrenSize)
        {
            childrenSize = IntVector2.Zero;

            UILayoutMethod layoutMethod = self.Layout.LayoutMethod;
            int columnCount = Math.Max(1, layoutMethod.GridProperties.ColumnCount);

            // Measure children
            foreach (UIBaseWindow child in self.Children)
            {
                if (SkipWindowLayout(child)) continue;
                child.Layout_Step1_Measure();
            }

            ref UIWindowCalculatedMetrics calc = ref self.CalculatedMetrics;

            // Config says column count - rows are determined by child count
            int childrenToLayout = 0;
            foreach (UIBaseWindow child in self.Children)
            {
                if (SkipWindowLayout(child)) continue;
                childrenToLayout++;
            }

            int rowCount = (childrenToLayout + columnCount - 1) / columnCount;
            calc.GridRowCount = rowCount;

            List<int>? columnWidths = calc.GridColumnWidths ?? new List<int>(columnCount);
            calc.GridColumnWidths = columnWidths;
            columnWidths.Reset(columnCount);

            List<int>? rowHeights = calc.GridRowHeights ?? new List<int>(rowCount);
            calc.GridRowHeights = rowHeights;
            rowHeights.Reset(rowCount);

            childrenToLayout = 0;
            for (int i = 0; i < self.Children.Count; i++)
            {
                UIBaseWindow child = self.Children[i];
                if (SkipWindowLayout(child)) continue;

                GridHelpers.GetCoordinate2DFrom1D(childrenToLayout, columnCount, out int col, out int row);

                IntVector2 childSize = child.CalculatedMetrics.Size + child.CalculatedMetrics.MarginTotalSize;

                // The row/column size is dependant on the largest child in it
                columnWidths[col] = Math.Max(columnWidths[col], childSize.X);
                rowHeights[row] = Math.Max(rowHeights[row], childSize.Y);

                childrenToLayout++;
            }

            RecalculateUniformGridConfig(self, columnWidths, rowHeights);

            childrenSize.X = calc.GridTotalWidth;
            childrenSize.Y = calc.GridTotalHeight;
        }

        public override void Step2_Grow(UIBaseWindow self)
        {
            UILayoutMethod layoutMethod = self.Layout.LayoutMethod;
            int columnCount = Math.Max(1, layoutMethod.GridProperties.ColumnCount);

            IntVector2 myMeasuredSize = self.CalculatedMetrics.GetContentSize();

            List<int>? columnWidths = self.CalculatedMetrics.GridColumnWidths;
            List<int>? rowHeights = self.CalculatedMetrics.GridRowHeights;
            AssertNotNull(columnWidths);
            AssertNotNull(rowHeights);
            if (columnWidths == null || rowHeights == null) return;

            int totalWidth = self.CalculatedMetrics.GridTotalWidth;
            int totalHeight = self.CalculatedMetrics.GridTotalHeight;
            int rowCount = self.CalculatedMetrics.GridRowCount;

            if (totalWidth <= myMeasuredSize.X || totalHeight <= myMeasuredSize.Y)
            {
                // Calculate growing count per directions
                bool[] growingColumns = ArrayPool<bool>.Shared.Rent(columnCount); // Renting can be deffered to first growing found
                Array.Clear(growingColumns, 0, columnCount);

                bool[] growingRows = ArrayPool<bool>.Shared.Rent(rowCount);
                Array.Clear(growingRows, 0, rowCount);

                int childrenToLayout = 0;
                foreach (UIBaseWindow child in self.Children)
                {
                    if (SkipWindowLayout(child)) continue;

                    GridHelpers.GetCoordinate2DFrom1D(childrenToLayout, columnCount, out int col, out int row);
                    if (child.Layout.SizingX.Mode == UISizing.UISizingMode.Grow)
                        growingColumns[col] = true;
                    if (child.Layout.SizingY.Mode == UISizing.UISizingMode.Grow)
                        growingRows[row] = true;

                    childrenToLayout++;
                }

                DistributeInDirection(growingColumns, columnCount, myMeasuredSize, 0, totalWidth, columnWidths);
                DistributeInDirection(growingRows, rowCount, myMeasuredSize, 1, totalHeight, rowHeights);
                RecalculateUniformGridConfig(self, columnWidths, rowHeights);

                ArrayPool<bool>.Shared.Return(growingColumns);
                ArrayPool<bool>.Shared.Return(growingRows);

                // Apply the growth to the children
                childrenToLayout = 0;
                foreach (UIBaseWindow child in self.Children)
                {
                    if (SkipWindowLayout(child)) continue;

                    GridHelpers.GetCoordinate2DFrom1D(childrenToLayout, columnCount, out int col, out int row);
                    if (child.Layout.SizingX.Mode == UISizing.UISizingMode.Grow)
                        child.CalculatedMetrics.Size.X = Math.Max(child.CalculatedMetrics.Size.X, columnWidths[col] - child.CalculatedMetrics.MarginTotalSize.X);

                    if (child.Layout.SizingY.Mode == UISizing.UISizingMode.Grow)
                        child.CalculatedMetrics.Size.Y = Math.Max(child.CalculatedMetrics.Size.Y, rowHeights[row] - child.CalculatedMetrics.MarginTotalSize.Y);

                    childrenToLayout++;
                }
            }

            // Now the children can grow their children
            foreach (UIBaseWindow child in self.Children)
            {
                if (SkipWindowLayout(child)) continue;
                child.Layout_Step2_Grow();
            }
        }

        public override void Step3_Position(UIBaseWindow self)
        {
            IntRectangle contentRect = self.CalculatedMetrics.GetContentRect();
            IntRectangle boundsRect = self.CalculatedMetrics.Bounds;

            UILayoutMethod layoutMethod = self.Layout.LayoutMethod;
            int columnCount = Math.Max(1, layoutMethod.GridProperties.ColumnCount);

            int spacingX = ListLayout.GetListSpacing(self, 0);
            int spacingY = ListLayout.GetListSpacing(self, 1);

            List<int>? columnWidths = self.CalculatedMetrics.GridColumnWidths;
            List<int>? rowHeights = self.CalculatedMetrics.GridRowHeights;
            AssertNotNull(columnWidths);
            AssertNotNull(rowHeights);
            if (columnWidths == null || rowHeights == null) return;

            RecalculateUniformGridConfig(self, columnWidths, rowHeights);

            int rowCount = self.CalculatedMetrics.GridRowCount;

            int childrenToLayout = 0;
            int currentRow = 0;
            int penY = contentRect.Position.Y;
            foreach (UIBaseWindow child in self.Children)
            {
                if (SkipWindowLayout(child)) continue;

                GridHelpers.GetCoordinate2DFrom1D(childrenToLayout, columnCount, out int col, out int row);

                // Check if pen going on a new row
                if (currentRow != row)
                {
                    penY += rowHeights[currentRow] + spacingY;
                    currentRow = row;
                }

                // Sum columns up to this
                int penX = contentRect.Position.X;
                for (int c = 0; c < col; c++)
                {
                    penX += columnWidths[c] + spacingX;
                }

                // Position within cell
                IntVector2 cellPosition = new IntVector2(penX, penY);
                IntRectangle cellRect = new IntRectangle(cellPosition, new IntVector2(columnWidths[col], rowHeights[row]));

                ListLayoutItemsAlign alignContent = GetItemsAlignAcrossFromList(UIMethodName.HorizontalList, child.Layout.Anchor);
                IntVector2 childPosition = cellPosition;

                switch (alignContent)
                {
                    case ListLayoutItemsAlign.Center:
                        childPosition.X += cellRect.Width / 2 - child.CalculatedMetrics.Size.X / 2;
                        break;
                    case ListLayoutItemsAlign.End:
                        childPosition.X += cellRect.Width - child.CalculatedMetrics.Size.X;
                        break;
                }

                // Apply margins
                IntVector2 childTopLeftMargin = child.CalculatedMetrics.MarginLeftTop;
                childPosition += childTopLeftMargin;

                child.Layout_Step3_Position(childPosition + child.CalculatedMetrics.Offsets);

                childrenToLayout++;
            }
        }

        private static void RecalculateUniformGridConfig(UIBaseWindow self, List<int> columnWidths, List<int> rowHeights)
        {
            UILayoutMethod layoutMethod = self.Layout.LayoutMethod;
            ref UIWindowCalculatedMetrics calc = ref self.CalculatedMetrics;

            int columnCount = Math.Max(1, layoutMethod.GridProperties.ColumnCount);
            int rowCount = calc.GridRowCount;

            // If uniform size enabled then all rows/columns are of the size of the largest one.
            if (layoutMethod.GridProperties.UniformColumnWidth)
            {
                int maxWidth = columnWidths.GetMax();
                columnWidths.SetAll(maxWidth);
            }

            if (layoutMethod.GridProperties.UniformRowHeight)
            {
                int maxHeight = rowHeights.GetMax();
                rowHeights.SetAll(maxHeight);
            }

            int spacingX = ListLayout.GetListSpacing(self, 0);
            int spacingY = ListLayout.GetListSpacing(self, 1);

            // Calculate total size
            int totalWidth = columnWidths.GetSum();
            totalWidth += spacingX * (columnCount - 1);
            calc.GridTotalWidth = totalWidth;

            int totalHeight = rowHeights.GetSum();
            totalHeight += spacingY * (rowCount - 1);
            calc.GridTotalHeight = totalHeight;
        }

        private static void DistributeInDirection(bool[] growingMask, int arrLength, IntVector2 myMeasuredSize, int mask, int totalMasked, List<int> sizesInDirection)
        {
            int growingCount = growingMask.CountTrue(arrLength);
            if (growingCount <= 0 || totalMasked >= myMeasuredSize[mask]) return;

            int remaining = myMeasuredSize[mask] - totalMasked;
            int sharePer = remaining / growingCount;
            int remainingPixels = remaining % growingCount;

            for (int i = 0; i < arrLength; i++)
            {
                if (!growingMask[i]) continue;

                sizesInDirection[i] += sharePer;
                if (remainingPixels <= 0) continue;

                sizesInDirection[i]++;
                remainingPixels--;
            }
        }
    }
}
