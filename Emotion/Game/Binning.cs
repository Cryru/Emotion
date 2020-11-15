#region Using

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using Emotion.Primitives;
using Emotion.Utility;

#endregion

namespace Emotion.Game
{
    public static class Binning
    {
        public class PackingSpace
        {
            public static readonly float ExtendToWidth = -1;
            public static readonly float ExtendToHeight = -2;

            public Rectangle Area;

            public PackingSpace(Rectangle area)
            {
                Area = area;
            }

            public Rectangle GetAbsoluteArea(Vector2 outsideSize)
            {
                float x = Area.X;
                float y = Area.Y;
                float width = Area.Width;
                float height = Area.Height;
                if (width == ExtendToWidth) width = outsideSize.X - x;
                if (height == ExtendToHeight) height = outsideSize.Y - y;
                return new Rectangle(x, y, width, height);
            }
        }

        /// <summary>
        /// Fit the array of rectangles within one automatically resizing, power of two sized, rectangle.
        /// The positions of the rectangles within the memory will be modified to the new ones.
        /// </summary>
        /// <param name="rectMemory">The rectangles to fit.</param>
        /// <param name="maxPasses">The number of passes to perform. Usually at least 2 are needed for optimal results.</param>
        /// <param name="maintainOrder">
        /// Whether the rectangles should maintain their array order when placed. This usually results
        /// in suboptimal results if the rectangles aren't relatively the same height. False by default.
        /// </param>
        /// <returns></returns>
        public static Vector2 FitRectangles(Memory<Rectangle> rectMemory, int maxPasses = int.MaxValue, bool maintainOrder = false)
        {
            if (rectMemory.IsEmpty) return Vector2.Zero;
            if (rectMemory.Length == 1) return rectMemory.Span[0].Size;

            // Sorting is down with a separate key table, to ensure the order is the same.
            int[] keys = Enumerable.Range(0, rectMemory.Length).ToArray();
            if (!maintainOrder) Array.Sort(keys, (x, y) =>
            {
                Rectangle rectX = rectMemory.Span[x];
                Rectangle rectY = rectMemory.Span[y];
                return Math.Sign(rectY.Height - rectX.Height);
            });

            Span<Rectangle> rects = rectMemory.Span;
            ref Rectangle tallestRect = ref rects[0];
            var canvasSize = new Vector2(Maths.ClosestPowerOfTwoGreaterThan((int) tallestRect.Width), Maths.ClosestPowerOfTwoGreaterThan((int) tallestRect.Height));
            var packingSpaces = new List<PackingSpace>();
            var currentPass = 0;
            while (currentPass < maxPasses)
            {
                restart:
                currentPass++;
                Vector2 canvasPos = Vector2.Zero;
                packingSpaces.Clear();
                for (var i = 0; i < rects.Length; i++)
                {
                    ref Rectangle curRect = ref rects[keys[i]];
                    var foundPlace = false;

                    // Empty or invalid.
                    if (curRect.Width == 0 || curRect.Height == 0) continue;

                    for (var pp = 0; pp < packingSpaces.Count; pp++)
                    {
                        PackingSpace space = packingSpaces[pp];
                        Rectangle currentSpace = space.GetAbsoluteArea(canvasSize);
                        if (!(currentSpace.Width >= curRect.Width) || !(currentSpace.Height >= curRect.Height)) continue;

                        curRect.Position = currentSpace.Position;

                        // Remove this space.
                        packingSpaces.RemoveAt(pp);

                        // Split it into the space on the right, and the space on top.
                        float rightSide;
                        if (space.Area.Width == PackingSpace.ExtendToWidth)
                            rightSide = PackingSpace.ExtendToWidth;
                        else
                            rightSide = space.Area.Width - curRect.Width;

                        var packingSpaceRightOf = new Rectangle(curRect.Right, currentSpace.Y, rightSide, currentSpace.Height);
                        packingSpaces.Add(new PackingSpace(packingSpaceRightOf));

                        var packingSpaceBottomOf = new Rectangle(curRect.X, curRect.Bottom, curRect.Width, currentSpace.Height - curRect.Height);
                        packingSpaces.Add(new PackingSpace(packingSpaceBottomOf));

                        foundPlace = true;
                        break;
                    }

                    if (foundPlace) continue;

                    // Going into the master packing space.
                    curRect.Position = canvasPos;
                    canvasPos.Y = curRect.Bottom;

                    // Check if it needs extending on the height.
                    if (canvasPos.Y > canvasSize.Y) canvasSize.Y = Maths.ClosestPowerOfTwoGreaterThan((int) canvasPos.Y);

                    // Check if the height is much bigger than the width now.
                    float scaleDiff = MathF.Log2(canvasSize.Y) - MathF.Log2(canvasSize.X);
                    if (scaleDiff > 0.0f)
                    {
                        // Set the width to the current height, and start a new pass.
                        canvasSize.X = canvasSize.Y;
                        packingSpaces.Clear();
                        goto restart;
                    }

                    var packingSpaceRightOfInMaster = new Rectangle(curRect.Right, curRect.Y, PackingSpace.ExtendToWidth, curRect.Height);
                    packingSpaces.Add(new PackingSpace(packingSpaceRightOfInMaster));
                }

                break;
            }

            // Check if height can be reduced.
            float bottomMostRect = 0;
            for (var i = 0; i < rects.Length; i++)
            {
                bottomMostRect = MathF.Max(rects[i].Bottom, bottomMostRect);
            }

            float bottomMostRectP2 = Maths.ClosestPowerOfTwoGreaterThan((int) bottomMostRect);
            if (bottomMostRectP2 < canvasSize.Y) canvasSize.Y = bottomMostRectP2;

#if DEBUG
            // Verify, no overlap.
            for (var i = 0; i < rects.Length; i++)
            {
                Rectangle thisRect = rects[i];
                for (var j = 0; j < rects.Length; j++)
                {
                    if (i == j) continue;
                    Rectangle thatRect = rects[j];
                    Debug.Assert(!thisRect.Intersects(ref thatRect));
                }
            }
#endif

            return canvasSize;
        }
    }
}