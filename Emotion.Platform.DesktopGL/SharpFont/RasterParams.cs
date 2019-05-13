#region Using

using System;
using System.Runtime.InteropServices;
using SharpFont.Internal;

#endregion

namespace SharpFont
{
    /// <summary>
    /// A function used as a call-back by the anti-aliased renderer in order to let client applications draw themselves
    /// the gray pixel spans on each scan line.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///     This callback allows client applications to directly render the gray spans of the anti-aliased bitmap to any
    ///     kind of surfaces.
    ///     </para>
    ///     <para>
    ///     This can be used to write anti-aliased outlines directly to a given background bitmap, and even perform
    ///     translucency.
    ///     </para>
    ///     <para>
    ///     Note that the ‘count’ field cannot be greater than a fixed value defined by the ‘FT_MAX_GRAY_SPANS’
    ///     configuration macro in ‘ftoption.h’. By default, this value is set to 32, which means that if there are more
    ///     than 32 spans on a given scanline, the callback is called several times with the same ‘y’ parameter in order to
    ///     draw all callbacks.
    ///     </para>
    ///     <para>
    ///     Otherwise, the callback is only called once per scan-line, and only for those scanlines that do have ‘gray’
    ///     pixels on them.
    ///     </para>
    /// </remarks>
    /// <param name="y">The scanline's y coordinate.</param>
    /// <param name="count">The number of spans to draw on this scanline.</param>
    /// <param name="spans">A table of ‘count’ spans to draw on the scanline.</param>
    /// <param name="user">User-supplied data that is passed to the callback.</param>
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void RasterSpanFunc(int y, int count, NativeReference<Span> spans, IntPtr user);

    /// <summary>
    ///     <para>
    ///     THIS TYPE IS DEPRECATED. DO NOT USE IT.
    ///     </para>
    ///     <para>
    ///     A function used as a call-back by the monochrome scan-converter to test whether a given target pixel is already
    ///     set to the drawing ‘color’. These tests are crucial to implement drop-out control per-se the TrueType spec.
    ///     </para>
    /// </summary>
    /// <param name="y">The pixel's y coordinate.</param>
    /// <param name="x">The pixel's x coordinate.</param>
    /// <param name="user">User-supplied data that is passed to the callback.</param>
    /// <returns>1 if the pixel is ‘set’, 0 otherwise.</returns>
    [Obsolete("This type is deprecated. Do not use it. See FreeType docuementation.")]
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate int RasterBitTestFunc(int y, int x, IntPtr user);

    /// <summary>
    ///     <para>
    ///     THIS TYPE IS DEPRECATED. DO NOT USE IT.
    ///     </para>
    ///     <para>
    ///     A function used as a call-back by the monochrome scan-converter to set an individual target pixel. This is
    ///     crucial to implement drop-out control according to the TrueType specification.
    ///     </para>
    /// </summary>
    /// <param name="y">The pixel's y coordinate.</param>
    /// <param name="x">The pixel's x coordinate.</param>
    /// <param name="user">User-supplied data that is passed to the callback.</param>
    [Obsolete("This type is deprecated. Do not use it. See FreeType docuementation.")]
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void RasterBitSetFunc(int y, int x, IntPtr user);

    /// <summary>
    /// A structure to hold the arguments used by a raster's render function.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///     An anti-aliased glyph bitmap is drawn if the <see cref="RasterFlags.AntiAlias" /> bit flag is set in the ‘flags’
    ///     field, otherwise a monochrome bitmap is generated.
    ///     </para>
    ///     <para>
    ///     If the <see cref="RasterFlags.Direct" /> bit flag is set in ‘flags’, the raster will call the ‘gray_spans’
    ///     callback to draw gray pixel spans, in the case of an aa glyph bitmap, it will call ‘black_spans’, and
    ///     ‘bit_test’ and ‘bit_set’ in the case of a monochrome bitmap. This allows direct composition over a pre-existing
    ///     bitmap through user-provided callbacks to perform the span drawing/composition.
    ///     </para>
    ///     <para>
    ///     Note that the ‘bit_test’ and ‘bit_set’ callbacks are required when rendering a monochrome bitmap, as they are
    ///     crucial to implement correct drop-out control as defined in the TrueType specification.
    ///     </para>
    /// </remarks>
    public class RasterParams : NativeObject
    {
        #region Fields

        private RasterParamsRec rec;

        #endregion

        #region Constructors

        internal RasterParams(IntPtr reference) : base(reference)
        {
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the target bitmap.
        /// </summary>
        public FTBitmap Target
        {
            get => new FTBitmap(rec.target, null);
        }

        /// <summary>
        /// Gets a pointer to the source glyph image (e.g., an <see cref="Outline" />).
        /// </summary>
        public IntPtr Source
        {
            get => rec.source;
        }

        /// <summary>
        /// Gets the rendering flags.
        /// </summary>
        public RasterFlags Flags
        {
            get => rec.flags;
        }

        /// <summary>
        /// Gets the gray span drawing callback.
        /// </summary>
        public RasterSpanFunc GraySpans
        {
            get => rec.gray_spans;
        }

        /// <summary>
        /// Gets the black span drawing callback. UNIMPLEMENTED!
        /// </summary>
        [Obsolete]
        public RasterSpanFunc BlackSpans
        {
            get => rec.black_spans;
        }

        /// <summary>
        /// Gets the bit test callback. UNIMPLEMENTED!
        /// </summary>
        [Obsolete]
        public RasterBitTestFunc BitTest
        {
            get => rec.bit_test;
        }

        /// <summary>
        /// Gets the bit set callback. UNIMPLEMENTED!
        /// </summary>
        [Obsolete]
        public RasterBitSetFunc BitSet
        {
            get => rec.bit_set;
        }

        /// <summary>
        /// Gets the user-supplied data that is passed to each drawing callback.
        /// </summary>
        public IntPtr User
        {
            get => rec.user;
        }

        /// <summary>
        /// Gets an optional clipping box. It is only used in direct rendering mode. Note that coordinates here should
        /// be expressed in integer pixels (and not in 26.6 fixed-point units).
        /// </summary>
        public BBox ClipBox
        {
            get => rec.clip_box;
        }

        internal override IntPtr Reference
        {
            get => base.Reference;

            set
            {
                base.Reference = value;
                rec = PInvokeHelper.PtrToStructure<RasterParamsRec>(value);
            }
        }

        #endregion
    }
}