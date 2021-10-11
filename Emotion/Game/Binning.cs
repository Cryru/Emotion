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

            protected PackingSpace()
            {

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
        /// <param name="maintainOrder">
        /// Whether the rectangles should be placed in the same order as they are in the array. This usually results
        /// in suboptimal results if the rectangles aren't relatively the same height. False by default.
        /// </param>
        /// <param name="fillResumeState">If a resumable state instance is provided it will be filled with the data needed to
        /// resume binning from the end state of this operation.</param>
        /// <returns></returns>
        public static Vector2 FitRectangles(Memory<Rectangle> rectMemory, bool maintainOrder = false, BinningResumableState fillResumeState = null)
        {
            if (rectMemory.IsEmpty) return Vector2.Zero;
            if (rectMemory.Length == 1) return rectMemory.Span[0].Size;

            // Sorting is done with a separate key table, as not to rearrange rectMemory.
            int[] keys = Enumerable.Range(0, rectMemory.Length).ToArray();
            if (!maintainOrder) // Sort by height if we can change placement order.
                Array.Sort(keys, (x, y) =>
                {
                    Rectangle rectX = rectMemory.Span[x];
                    Rectangle rectY = rectMemory.Span[y];
                    return Math.Sign(rectY.Height - rectX.Height);
                });

            Span<Rectangle> rects = rectMemory.Span;
            ref Rectangle tallestRect = ref rects[0];
            var canvasSize = new Vector2(Maths.ClosestPowerOfTwoGreaterThan((int) tallestRect.Width), Maths.ClosestPowerOfTwoGreaterThan((int) tallestRect.Height));
            var packingSpaces = new List<PackingSpace>();

            restart:
            Vector2 canvasPos = Vector2.Zero;
            packingSpaces.Clear();
            for (var i = 0; i < rects.Length; i++)
            {
                ref Rectangle curRect = ref rects[keys[i]];
                var foundPlace = false;

                // Empty or invalid.
                if (curRect.Width == 0 || curRect.Height == 0) continue;

                // Check if any packing space can contain this rect.
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

                // Going into the canvas packing space.
                curRect.Position = canvasPos;
                canvasPos.Y = curRect.Bottom;

                // Check if it needs extending on the height.
                if (canvasPos.Y > canvasSize.Y) canvasSize.Y = Maths.ClosestPowerOfTwoGreaterThan((int) canvasPos.Y);

                // Check if the height is bigger than the width now.
                float scaleDiff = MathF.Log2(canvasSize.Y) - MathF.Log2(canvasSize.X);
                if (scaleDiff > 0.0f)
                {
                    // Set the width to the current height, and restart.
                    canvasSize.X = canvasSize.Y;
                    packingSpaces.Clear();
                    goto restart;
                }

                var packingSpaceRightOfInMaster = new Rectangle(curRect.Right, curRect.Y, PackingSpace.ExtendToWidth, curRect.Height);
                packingSpaces.Add(new PackingSpace(packingSpaceRightOfInMaster));
            }

            // Check if height can be reduced.
            float bottomMostRect = 0;
            for (var i = 0; i < rects.Length; i++)
            {
                bottomMostRect = MathF.Max(rects[i].Bottom, bottomMostRect);
            }

            float bottomMostRectP2 = Maths.ClosestPowerOfTwoGreaterThan((int) bottomMostRect);
            if (bottomMostRectP2 < canvasSize.Y) canvasSize.Y = bottomMostRectP2;

            // Fill resumeable state.
            if (fillResumeState != null)
            {
                fillResumeState.Size = canvasSize;
                fillResumeState.PackingSpaces = packingSpaces;
                fillResumeState.CanvasPos = canvasPos;
            }

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

        public class BinningResumableState
        {
            public Vector2 CanvasPos;
            public Vector2 Size;
            public List<PackingSpace> PackingSpaces = new List<PackingSpace>();

            public BinningResumableState(Vector2 canvasDimensions)
            {
                Size = canvasDimensions;
            }

            // Serialization constructor.
            protected BinningResumableState()
            {

            }
        }

        /// <summary>
        /// Fit a rectangle inside a canvas filled with rectangles.
        /// </summary>
        /// <param name="rectangleSize">The size of the new rectangle.</param>
        /// <param name="oldState">
        /// The canvas state so far. If just starting construct a new instance.
        /// </param>
        /// <returns>The position of the rectangle within the canvas, or null if it couldn't fit.</returns>
        public static Vector2? FitRectanglesResumable(Vector2 rectangleSize, BinningResumableState oldState)
        {
            Vector2 canvasSize = oldState.Size;
            List<PackingSpace> packingSpaces = oldState.PackingSpaces;
            ref Vector2 canvasPos = ref oldState.CanvasPos;
            var curRect = new Rectangle(0, 0, rectangleSize);

            // Empty or invalid.
            if (curRect.Width == 0 || curRect.Height == 0) return null;

            // Check if any packing space can contain this rect.
            var foundPlace = false;
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

            if (foundPlace) return curRect.Position;

            // Going into the canvas packing space.
            curRect.Position = canvasPos;
            canvasPos.Y = curRect.Bottom;

            // Check if it needs extending on the height.
            if (canvasPos.Y > canvasSize.Y) return null;

            var packingSpaceRightOfInMaster = new Rectangle(curRect.Right, curRect.Y, PackingSpace.ExtendToWidth, curRect.Height);
            packingSpaces.Add(new PackingSpace(packingSpaceRightOfInMaster));

            return curRect.Position;
        }
    }
}