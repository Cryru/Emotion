#region Using

using System;
using SharpFont.Internal;

#endregion

namespace SharpFont
{
    /// <summary>
    /// A structure used for bitmap glyph images. This really is a ‘sub-class’ of <see cref="Glyph" />.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///     You can typecast an <see cref="Glyph" /> to <see cref="BitmapGlyph" /> if you have ‘<see cref="Glyph.Format" /> ==
    ///     <see cref="GlyphFormat.Bitmap" />’. This lets you access the bitmap's contents easily.
    ///     </para>
    ///     <para>
    ///     The corresponding pixel buffer is always owned by <see cref="BitmapGlyph" /> and is thus created and destroyed
    ///     with it.
    ///     </para>
    /// </remarks>
    public sealed class BitmapGlyph : IDisposable
    {
        #region Fields

        private Glyph original;
        private BitmapGlyphRec rec;

        #endregion

        #region Constructors

        internal BitmapGlyph(Glyph original)
        {
            this.original = original;
            Reference = original.Reference; //generates the rec.
        }

        /// <summary>
        /// Finalizes an instance of the <see cref="BitmapGlyph" /> class.
        /// </summary>
        ~BitmapGlyph()
        {
            Dispose(false);
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets a value indicating whether the object has been disposed.
        /// </summary>
        public bool IsDisposed
        {
            get => original.IsDisposed;
        }

        /// <summary>
        /// Gets the root <see cref="Glyph" /> fields.
        /// </summary>
        public Glyph Root
        {
            get
            {
                if (IsDisposed)
                    throw new ObjectDisposedException("Root", "Cannot access a disposed object.");

                return original;
            }
        }

        /// <summary>
        /// Gets the left-side bearing, i.e., the horizontal distance from the current pen position to the left border
        /// of the glyph bitmap.
        /// </summary>
        public int Left
        {
            get
            {
                if (IsDisposed)
                    throw new ObjectDisposedException("Left", "Cannot access a disposed object.");

                return rec.left;
            }
        }

        /// <summary>
        /// Gets the top-side bearing, i.e., the vertical distance from the current pen position to the top border of
        /// the glyph bitmap. This distance is positive for upwards y!
        /// </summary>
        public int Top
        {
            get
            {
                if (IsDisposed)
                    throw new ObjectDisposedException("Top", "Cannot access a disposed object.");

                return rec.top;
            }
        }

        /// <summary>
        /// Gets a descriptor for the bitmap.
        /// </summary>
        public FTBitmap Bitmap
        {
            get
            {
                if (IsDisposed)
                    throw new ObjectDisposedException("Bitmap", "Cannot access a disposed object.");

                return new FTBitmap(PInvokeHelper.AbsoluteOffsetOf<BitmapGlyphRec>(Reference, "bitmap"), rec.bitmap, null);
            }
        }

        internal IntPtr Reference
        {
            get
            {
                if (IsDisposed)
                    throw new ObjectDisposedException("Reference", "Cannot access a disposed object.");

                return original.Reference;
            }

            set
            {
                if (IsDisposed)
                    throw new ObjectDisposedException("Reference", "Cannot modify a disposed object.");

                rec = PInvokeHelper.PtrToStructure<BitmapGlyphRec>(original.Reference);
            }
        }

        #endregion

        #region Operators

        /// <summary>
        /// Casts a <see cref="BitmapGlyph" /> back up to a <see cref="Glyph" />. The eqivalent of
        /// <see cref="BitmapGlyph.Root" />.
        /// </summary>
        /// <param name="g">A <see cref="BitmapGlyph" />.</param>
        /// <returns>A <see cref="Glyph" />.</returns>
        public static implicit operator Glyph(BitmapGlyph g)
        {
            return g.original;
        }

        #endregion

        #region Methods

        /// <summary>
        /// A CLS-compliant version of the implicit cast to <see cref="Glyph" />.
        /// </summary>
        /// <returns>A <see cref="Glyph" />.</returns>
        public Glyph ToGlyph()
        {
            return this;
        }

        /// <summary>
        /// Disposes an instance of the <see cref="BitmapGlyph" /> class.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if (disposing)
            {
                original.Dispose();
                original = null;
            }
        }

        #endregion
    }
}