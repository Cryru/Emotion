namespace Emotion.Standard.OpenType
{
    /// <summary>
    /// The type of operation this glyph vertex represents.
    /// </summary>
    public enum VertexTypeFlag : byte
    {
        /// <summary>
        /// Moving the pen.
        /// </summary>
        Move = 1,

        /// <summary>
        /// Drawing a line.
        /// </summary>
        Line = 2,

        /// <summary>
        /// Drawing a quadratic curve.
        /// </summary>
        Curve = 3,

        /// <summary>
        /// Drawing a cubic curve.
        /// </summary>
        Cubic = 4
    }

    /// <summary>
    /// A glyph vertex command.
    /// </summary>
    public struct GlyphVertex
    {
        /// <summary>
        /// X Coordinate of the point.
        /// </summary>
        public short X;

        /// <summary>
        /// X Coordinate of the point.
        /// </summary>
        public short Y;

        /// <summary>
        /// X Coordinate of the curve.
        /// </summary>
        public short Cx;

        /// <summary>
        /// Y Coordinate of the curve.
        /// </summary>
        public short Cy;

        /// <summary>
        /// X Coordinate of the cubic curve control point.
        /// </summary>
        public short Cx1;

        /// <summary>
        /// Y Coordinate of the cubic curve control point.
        /// </summary>
        public short Cy1;

        /// <summary>
        /// During reading this is filled with the glyf table's point flag.
        /// After parsing this is a value from "VertexTypeFlag" and points to
        /// where the point is.
        /// </summary>
        public byte Flags;

        /// <summary>
        /// Flags property as VertexTypeFlag enum
        /// </summary>
        public VertexTypeFlag TypeFlag
        {
            get => (VertexTypeFlag) Flags;
            set => Flags = (byte) value;
        }
    }
}