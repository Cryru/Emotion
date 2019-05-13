#region Using

using System;
using SharpFont.Internal;

#endregion

namespace SharpFont
{
    /// <summary>
    /// A structure used to model a single span of gray (or black) pixels when rendering a monochrome or anti-aliased
    /// bitmap.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///     This structure is used by the span drawing callback type named <see cref="RasterSpanFunc" /> which takes the y
    ///     coordinate of the span as a a parameter.
    ///     </para>
    ///     <para>
    ///     The coverage value is always between 0 and 255. If you want less gray values, the callback function has to
    ///     reduce them.
    ///     </para>
    /// </remarks>
    public class Span : NativeObject
    {
        #region Fields

        private SpanRec rec;

        #endregion

        #region Constructors

        internal Span(IntPtr reference) : base(reference)
        {
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the span's horizontal start position.
        /// </summary>
        public short X
        {
            get => rec.x;
        }

        /// <summary>
        /// Gets the span's length in pixels.
        /// </summary>

        public ushort Length
        {
            get => rec.len;
        }

        /// <summary>
        /// Gets the span color/coverage, ranging from 0 (background) to 255 (foreground). Only used for anti-aliased
        /// rendering.
        /// </summary>
        public byte Coverage
        {
            get => rec.coverage;
        }

        internal override IntPtr Reference
        {
            get => base.Reference;

            set
            {
                base.Reference = value;
                rec = PInvokeHelper.PtrToStructure<SpanRec>(value);
            }
        }

        #endregion
    }
}