#nullable enable

using Emotion.Primitives.Grids;
using System.Buffers;

namespace Emotion.Game.Systems.UI;

public partial class UIBaseWindow
{
    public class GridLayout : LayoutMethodCodeClass
    {
        public override void PreLayout(UIBaseWindow self)
        {
            ref UIWindowCalculatedMetrics calc = ref self.CalculatedMetrics;
            UILayoutMethod layoutMethod = self.Layout.LayoutMethod;

            int childrenToLayout = 0;
            foreach (UIBaseWindow child in self.Children)
            {
                if (SkipWindowLayout(child)) continue;
                childrenToLayout++;
            }

            bool fixedColumns = HasFixedColumns(layoutMethod);
            bool fixedRows = HasFixedRows(layoutMethod);
            int columnCount = layoutMethod.GridProperties.ColumnCount;
            int rowCount = layoutMethod.GridProperties.RowCount;
            if (fixedColumns && fixedRows)
            {
                columnCount = Math.Max(1, columnCount);
                rowCount = Math.Max(1, rowCount);
            }
            else if (fixedRows)
            {
                columnCount = Math.Max(1, (childrenToLayout + rowCount - 1) / rowCount);
            }
            else if (fixedColumns)
            {
                rowCount = Math.Max(1, (childrenToLayout + columnCount - 1) / columnCount);
            }

            calc.GridColumnCount = columnCount;
            calc.GridRowCount = rowCount;

            List<int>? columnWidths = calc.GridColumnWidths ?? new List<int>(columnCount);
            calc.GridColumnWidths = columnWidths;
            columnWidths.Reset(columnCount);

            List<int>? rowHeights = calc.GridRowHeights ?? new List<int>(rowCount);
            calc.GridRowHeights = rowHeights;
            rowHeights.Reset(rowCount);
        }

        public override int GetMainAxis(UIBaseWindow self)
        {
            UILayoutMethod layoutMethod = self.Layout.LayoutMethod;

            // If the columns (axis 0 - X) are fixed we are actually layout out in the row direction (axis 1 - Y)
            bool fixedColumns = HasFixedColumns(layoutMethod);
            return fixedColumns ? 1 : 0;
        }

        public override int GetChildrenSize(UIBaseWindow self, int axis)
        {
            UILayoutMethod layoutMethod = self.Layout.LayoutMethod;
            ref UIWindowCalculatedMetrics calc = ref self.CalculatedMetrics;

            List<int>? listForAxis = axis == 0 ? calc.GridColumnWidths : calc.GridRowHeights;
            AssertNotNull(listForAxis);
            if (listForAxis == null) return 0;

            int columnCount = calc.GridColumnCount;

            int childrenToLayout = 0;
            foreach (UIBaseWindow child in self.Children)
            {
                if (SkipWindowLayout(child)) continue;

                GridHelpers.GetCoordinate2DFrom1D(childrenToLayout, columnCount, out int col, out int row);
                int idxForAxis = axis == 0 ? col : row;

                int childSizeInAxis = child.CalculatedMetrics.Size[axis] + child.CalculatedMetrics.MarginTotalSize[axis];

                // The column/row size is dependant on the largest child in it.
                listForAxis[idxForAxis] = Math.Max(listForAxis[idxForAxis], childSizeInAxis);

                childrenToLayout++;
            }

            // If uniform size enabled then all rows/columns should be the same size (meaning as big as the largest)
            bool directionIsUniform = axis == 0 ? layoutMethod.GridProperties.UniformColumnWidth : layoutMethod.GridProperties.UniformRowHeight;
            if (directionIsUniform)
            {
                int max = listForAxis.GetMax();
                listForAxis.SetAll(max);
            }

            int spacingAxis = ListLayout.GetListSpacing(self, axis);
            int total = listForAxis.GetSum();
            total += spacingAxis * (listForAxis.Count - 1);
            return total;
        }

        public override void GrowShrinkAxis(UIBaseWindow self, int axis)
        {
            UILayoutMethod layoutMethod = self.Layout.LayoutMethod;
            ref UIWindowCalculatedMetrics calc = ref self.CalculatedMetrics;

            List<int>? listForAxis = axis == 0 ? calc.GridColumnWidths : calc.GridRowHeights;
            AssertNotNull(listForAxis);
            if (listForAxis == null) return;

            int columnCount = calc.GridColumnCount;

            IntVector2 myMeasuredSize = self.CalculatedMetrics.GetContentSize();
            int availableSize = myMeasuredSize[axis];

            int spacingAxis = ListLayout.GetListSpacing(self, axis);
            int sizeTaken = listForAxis.GetSum();
            sizeTaken += spacingAxis * (listForAxis.Count - 1);

            availableSize -= sizeTaken;

            if (availableSize != 0)
            {
                bool[] growingAxis = ArrayPool<bool>.Shared.Rent(listForAxis.Count); // Renting can be deffered to first growing found
                Array.Clear(growingAxis, 0, listForAxis.Count);

                bool[] shrinkingAxis = ArrayPool<bool>.Shared.Rent(listForAxis.Count); // Renting can be deffered to first growing found
                Array.Clear(shrinkingAxis, 0, listForAxis.Count);

                int childrenToLayout = 0;
                foreach (UIBaseWindow child in self.Children)
                {
                    if (SkipWindowLayout(child)) continue;

                    GridHelpers.GetCoordinate2DFrom1D(childrenToLayout, columnCount, out int col, out int row);
                    int idxForAxis = axis == 0 ? col : row;

                    UISizing sizing = GetSizingInDirection(child, axis);
                    growingAxis[idxForAxis] = sizing.CanGrow();
                    shrinkingAxis[idxForAxis] = sizing.CanShrink();

                    childrenToLayout++;
                }

                GrowAxis(growingAxis, listForAxis.Count, ref availableSize, listForAxis);
                ShrinkAxis(shrinkingAxis, listForAxis.Count, ref availableSize, listForAxis);
                ArrayPool<bool>.Shared.Return(growingAxis);
                ArrayPool<bool>.Shared.Return(shrinkingAxis);

                // If uniform size enabled then all rows/columns should be the same size (meaning as big as the largest)
                bool directionIsUniform = axis == 0 ? layoutMethod.GridProperties.UniformColumnWidth : layoutMethod.GridProperties.UniformRowHeight;
                if (directionIsUniform)
                {
                    int max = listForAxis.GetMax();
                    listForAxis.SetAll(max);
                }
            }

            // Apply the growth to the children
            int childIdx = 0;
            foreach (UIBaseWindow child in self.Children)
            {
                if (SkipWindowLayout(child)) continue;

                UISizing sizing = GetSizingInDirection(child, axis);
                if (sizing.CanGrow())
                {
                    GridHelpers.GetCoordinate2DFrom1D(childIdx, columnCount, out int col, out int row);
                    int idxForAxis = axis == 0 ? col : row;
                    child.CalculatedMetrics.Size[axis] = listForAxis[idxForAxis] - child.CalculatedMetrics.MarginTotalSize[axis];
                }

                childIdx++;
            }
        }

        public override void PositionChildren(UIBaseWindow self)
        {
            ref UIWindowCalculatedMetrics calc = ref self.CalculatedMetrics;
            UILayoutMethod layoutMethod = self.Layout.LayoutMethod;

            IntRectangle contentRect = calc.GetContentRect();
            IntRectangle boundsRect = calc.Bounds;

            int spacingX = ListLayout.GetListSpacing(self, 0);
            int spacingY = ListLayout.GetListSpacing(self, 1);

            List<int>? columnWidths = calc.GridColumnWidths;
            List<int>? rowHeights = calc.GridRowHeights;
            AssertNotNull(columnWidths);
            AssertNotNull(rowHeights);
            if (columnWidths == null || rowHeights == null) return;

            int columnCount = calc.GridColumnCount;

            int childrenToLayout = 0;
            int currentRow = 0;
            int penY = contentRect.Position.Y;
            foreach (UIBaseWindow child in self.Children)
            {
                if (SkipWindowLayout(child)) continue;
                if (!child.CalculatedMetrics.InsideParent)
                {
                    // Parents outisde the parent list are free layout
                    FreeLayout.FreeLayoutPosition(child, contentRect, boundsRect);
                    continue;
                }

                GridHelpers.GetCoordinate2DFrom1D(childrenToLayout, columnCount, out int col, out int row);

                // Check if pen going on a new row
                if (currentRow != row)
                {
                    penY += rowHeights[currentRow] + spacingY;
                    currentRow = row;
                }

                // Sum columns up to this
                int penX = contentRect.Position.X;
                penX += spacingX * col;
                penX += columnWidths.GetSum(col);

                // Free layout within the cell
                IntVector2 cellPosition = new IntVector2(penX, penY);
                IntRectangle cellRect = new IntRectangle(cellPosition, new IntVector2(columnWidths[col], rowHeights[row]));
                FreeLayout.FreeLayoutPosition(child, cellRect, cellRect);
                childrenToLayout++;
            }
        }

        private static void GrowAxis(bool[] mask, int arrLength, ref int remaining, List<int> sizes)
        {
            while (remaining > float.Epsilon)
            {
                int smallest = int.MaxValue;
                int secondSmallest = int.MaxValue;
                int growable = 0;
                for (int i = 0; i < arrLength; i++)
                {
                    bool growing = mask[i];
                    if (!growing) continue;
                    growable++;

                    int size = sizes[i];

                    if (smallest == int.MaxValue)
                    {
                        smallest = size;
                        secondSmallest = size;
                        continue;
                    }
                    else if (size < smallest)
                    {
                        secondSmallest = smallest;
                        smallest = size;
                    }
                    else if (size > smallest && size < secondSmallest)
                    {
                        secondSmallest = size;
                    }
                }
                if (growable == 0) break;

                int diff = secondSmallest - smallest;
                diff = Math.Max(diff, remaining / growable);
                if (diff == 0)
                {
                    // Distribute last pixels sequentially so we don't have leftover space at the end.
                    if (remaining > 0)
                    {
                        for (int i = 0; i < arrLength; i++)
                        {
                            if (!mask[i]) continue;

                            sizes[i]++;
                            remaining--;
                            if (remaining == 0) break;
                        }
                    }
                    break;
                }

                for (int i = 0; i < arrLength; i++)
                {
                    bool growing = mask[i];
                    if (!growing) continue;

                    int size = sizes[i];
                    if (size == smallest)
                    {
                        sizes[i] += diff;
                        remaining -= diff;
                    }
                }
            }
        }

        private static void ShrinkAxis(bool[] mask, int arrLength, ref int remaining, List<int> sizes)
        {
            while (remaining < -float.Epsilon)
            {
                int largest = 0;
                int secondLargest = 0;
                int shrinkable = 0;
                for (int i = 0; i < arrLength; i++)
                {
                    bool growing = mask[i];
                    if (!growing) continue;
                    shrinkable++;

                    int size = sizes[i];
                    if (size > largest)
                    {
                        secondLargest = largest;
                        largest = size;
                    }
                    if (size < largest && size > secondLargest)
                    {
                        secondLargest = size;
                    }
                }
                if (shrinkable == 0) break;

                int diff = secondLargest - largest;
                diff = Math.Max(diff, remaining / shrinkable);
                if (diff == 0) break;

                for (int i = 0; i < arrLength; i++)
                {
                    bool growing = mask[i];
                    if (!growing) continue;

                    int size = sizes[i];
                    if (size == largest)
                    {
                        sizes[i] += diff;
                        remaining -= diff;
                    }
                }
            }
        }

        private static bool HasFixedColumns(in UILayoutMethod layoutMethod)
        {
            return layoutMethod.GridProperties.ColumnCount > 0;
        }

        private static bool HasFixedRows(in UILayoutMethod layoutMethod)
        {
            return layoutMethod.GridProperties.RowCount > 0;
        }
    }
}
