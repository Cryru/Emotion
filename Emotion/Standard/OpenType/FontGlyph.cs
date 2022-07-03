#region Using

using System.Numerics;

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

    public class FontGlyph
    {
        public int MapIndex;
        public string? Name; // optional

        public bool Composite;
        public FontGlyphComponent[]? Components;

        public float Advance;
        public float LeftSideBearing;

        public Vector2 Min;
        public Vector2 Max;

        public GlyphDrawCommand[]? Commands;
    }
}