#region Using

using System;
using SharpFont.Internal;

#endregion

namespace SharpFont
{
    /// <summary>
    /// This structure models the metrics of a bitmap strike (i.e., a set of
    /// glyphs for a given point size and resolution) in a bitmap font. It is
    /// used for the <see cref="Face.AvailableSizes" /> field of
    /// <see cref="Face" />.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///     Windows FNT: The nominal size given in a FNT font is not reliable. Thus
    ///     when the driver finds it incorrect, it sets ‘size’ to some calculated
    ///     values and sets ‘x_ppem’ and ‘y_ppem’ to the pixel width and height
    ///     given in the font, respectively.
    ///     </para>
    ///     <para>
    ///     TrueType embedded bitmaps: ‘size’, ‘width’, and ‘height’ values are not
    ///     contained in the bitmap strike itself. They are computed from the
    ///     global font parameters.
    ///     </para>
    /// </remarks>
    public sealed class BitmapSize
    {
        #region Fields

        private IntPtr reference;
        private BitmapSizeRec rec;

        #endregion

        #region Constructors

        internal BitmapSize(IntPtr reference)
        {
            Reference = reference;
        }

        internal BitmapSize(BitmapSizeRec bmpSizeInt)
        {
            rec = bmpSizeInt;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the vertical distance, in pixels, between two consecutive
        /// baselines. It is always positive.
        /// </summary>
        public short Height
        {
            get => rec.height;
        }

        /// <summary>
        /// Gets the average width, in pixels, of all glyphs in the strike.
        /// </summary>
        public short Width
        {
            get => rec.width;
        }

        /// <summary>
        /// Gets the nominal size of the strike in 26.6 fractional points. This
        /// field is not very useful.
        /// </summary>
        public Fixed26Dot6 Size
        {
            get => Fixed26Dot6.FromRawValue((int) rec.size);
        }

        /// <summary>
        /// Gets the horizontal ppem (nominal width) in 26.6 fractional pixels.
        /// </summary>
        public Fixed26Dot6 NominalWidth
        {
            get => Fixed26Dot6.FromRawValue((int) rec.x_ppem);
        }

        /// <summary>
        /// Gets the vertical ppem (nominal height) in 26.6 fractional pixels.
        /// </summary>
        public Fixed26Dot6 NominalHeight
        {
            get => Fixed26Dot6.FromRawValue((int) rec.y_ppem);
        }

        internal IntPtr Reference
        {
            get => reference;

            set
            {
                reference = value;
                rec = PInvokeHelper.PtrToStructure<BitmapSizeRec>(reference);
            }
        }

        #endregion
    }
}