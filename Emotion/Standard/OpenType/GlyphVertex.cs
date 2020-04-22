namespace Emotion.Standard.OpenType
{
    public enum VertexTypeFlag : byte
    {
        Move = 1,
        Line = 2,
        Curve = 3,
        Cubic = 4
    }

    public struct GlyphVertex
    {
        /// <summary>
        /// Coordinate of the point.
        /// </summary>
        public short X;

        public short Y;

        /// <summary>
        /// Coordinate of the curve.
        /// </summary>
        public short Cx;

        public short Cy;

        /// <summary>
        /// Used for cubic curves.
        /// </summary>
        public short Cx1;

        public short Cy1;

        /// <summary>
        /// During reading this is filled with the glyf table's point flag.
        /// After parsing this is a value from "VertexTypeFlag" and points to
        /// where the point is.
        /// </summary>
        public byte Flags;

        public VertexTypeFlag TypeFlag
        {
            get => (VertexTypeFlag) Flags;
            set => Flags = (byte) value;
        }
    }
}