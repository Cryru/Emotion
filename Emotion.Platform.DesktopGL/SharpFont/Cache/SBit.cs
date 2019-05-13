#region Using

using System;
using SharpFont.Cache.Internal;

#endregion

namespace SharpFont.Cache
{
    /// <summary>
    /// A handle to a small bitmap cache. These are special cache objects used to store small glyph bitmaps (and
    /// anti-aliased pixmaps) in a much more efficient way than the traditional glyph image cache implemented by
    /// <see cref="ImageCache" />.
    /// </summary>
    public class SBit
    {
        #region Fields

        private IntPtr reference;
        private SBitRec rec;

        #endregion

        #region Constructors

        internal SBit(IntPtr reference)
        {
            Reference = reference;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the bitmap width in pixels.
        /// </summary>
        public byte Width
        {
            get => rec.width;
        }

        /// <summary>
        /// Gets the bitmap height in pixels.
        /// </summary>
        public byte Height
        {
            get => rec.height;
        }

        /// <summary>
        /// Gets the horizontal distance from the pen position to the left bitmap border (a.k.a. ‘left side bearing’,
        /// or ‘lsb’).
        /// </summary>
        public byte Left
        {
            get => rec.left;
        }

        /// <summary>
        /// Gets the vertical distance from the pen position (on the baseline) to the upper bitmap border (a.k.a. ‘top
        /// side bearing’). The distance is positive for upwards y coordinates.
        /// </summary>
        public byte Top
        {
            get => rec.top;
        }

        /// <summary>
        /// Gets the format of the glyph bitmap (monochrome or gray).
        /// </summary>
        public byte Format
        {
            get => rec.format;
        }

        /// <summary>
        /// Gets the maximum gray level value (in the range 1 to 255).
        /// </summary>
        public byte MaxGrays
        {
            get => rec.max_grays;
        }

        /// <summary>
        /// Gets the number of bytes per bitmap line. May be positive or negative.
        /// </summary>
        public short Pitch
        {
            get => rec.pitch;
        }

        /// <summary>
        /// Gets the horizontal advance width in pixels.
        /// </summary>
        public byte AdvanceX
        {
            get => rec.xadvance;
        }

        /// <summary>
        /// Gets the vertical advance height in pixels.
        /// </summary>
        public byte AdvanceY
        {
            get => rec.yadvance;
        }

        /// <summary>
        /// Gets a pointer to the bitmap pixels.
        /// </summary>
        public IntPtr Buffer
        {
            get => rec.buffer;
        }

        internal IntPtr Reference
        {
            get => reference;

            set
            {
                reference = value;
                rec = PInvokeHelper.PtrToStructure<SBitRec>(reference);
            }
        }

        #endregion
    }
}