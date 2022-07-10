#region Using

using System;
using System.Numerics;
using Emotion.Primitives;

#endregion

#nullable enable

namespace Emotion.Standard.OpenType
{
    public enum GlyphDrawCommandType : byte
    {
        Move,
        Line,
        Curve,
        Close
    }

    public struct GlyphDrawCommand
    {
        public GlyphDrawCommandType Type;
        public Vector2 P0;
        public Vector2 P1;

        // for debugging
        public override string ToString()
        {
            return $"{Type} {P0} {P1}";
        }
    }

    public class FontGlyphComponent
    {
        public int GlyphIdx;
        public float[] Matrix;

        public FontGlyphComponent(int glyphIdx, float[] mat)
        {
            GlyphIdx = glyphIdx;
            Matrix = mat;
        }
    }

    /// <summary>
    /// Represents a single font glyph. One glyph can represent multiple characters in a font.
    /// Contains all the meta information and command to draw the glyph contour.
    /// </summary>
    public class FontGlyph
    {
        public int MapIndex;
        public string? Name; // optional

        public bool Composite;
        public FontGlyphComponent[]? Components;

        public float AdvanceWidth;
        public float LeftSideBearing;

        public Vector2 Min;
        public Vector2 Max;

        public GlyphDrawCommand[]? Commands;

        /// <summary>
        /// The bounding box for the glyph.
        /// </summary>
        /// <param name="scale">The scale to get the bounding box at.</param>
        public Rectangle GetBBox(float scale)
        {
            return new Rectangle(
                MathF.Floor(Min.X * scale),
                MathF.Floor(-Max.Y * scale),
                MathF.Ceiling(Max.X * scale),
                MathF.Ceiling(-Min.Y * scale)
            );
        }


        /// <summary>
        /// The bounding box for the glyph.
        /// </summary>
        /// <param name="scale">The scale to get the bounding box at.</param>
        public Rectangle GetBBoxFloat(float scale)
        {
            return new Rectangle(
                Min.X * scale,
                -Max.Y * scale,
                Max.X * scale,
                -Min.Y * scale
            );
        }
    }
}