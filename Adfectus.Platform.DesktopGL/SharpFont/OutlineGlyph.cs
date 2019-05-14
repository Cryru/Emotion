#region Using

using System;
using SharpFont.Internal;

#endregion

namespace SharpFont
{
    /// <summary>
    /// A structure used for outline (vectorial) glyph images. This really is a ‘sub-class’ of <see cref="Glyph" />.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///     You can typecast an <see cref="Glyph" /> to <see cref="OutlineGlyph" /> if you have ‘<see cref="Glyph.Format" />
    ///     == <see cref="GlyphFormat.Outline" />’. This lets you access the outline's content easily.
    ///     </para>
    ///     <para>
    ///     As the outline is extracted from a glyph slot, its coordinates are expressed normally in 26.6 pixels, unless
    ///     the flag <see cref="LoadFlags.NoScale" /> was used in <see cref="Face.LoadGlyph" /> or
    ///     <see cref="Face.LoadChar" />.
    ///     </para>
    ///     <para>
    ///     The outline's tables are always owned by the object and are destroyed with it.
    ///     </para>
    /// </remarks>
    public class OutlineGlyph : IDisposable
    {
        #region Fields

        private Glyph original;
        private OutlineGlyphRec rec;

        #endregion

        #region Constructors

        internal OutlineGlyph(Glyph original)
        {
            this.original = original;
            Reference = original.Reference; //sets the rec
        }

        /// <summary>
        /// Finalizes an instance of the <see cref="OutlineGlyph" /> class.
        /// </summary>
        ~OutlineGlyph()
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
                    throw new ObjectDisposedException("Bitmap", "Cannot access a disposed object.");

                return original;
            }
        }

        /// <summary>
        /// Gets a descriptor for the outline.
        /// </summary>
        public Outline Outline
        {
            get
            {
                if (IsDisposed)
                    throw new ObjectDisposedException("Bitmap", "Cannot access a disposed object.");

                return new Outline(PInvokeHelper.AbsoluteOffsetOf<OutlineGlyphRec>(Reference, "outline"), rec.outline);
            }
        }

        internal IntPtr Reference
        {
            get
            {
                if (IsDisposed)
                    throw new ObjectDisposedException("Bitmap", "Cannot access a disposed object.");

                return original.Reference;
            }

            set
            {
                if (IsDisposed)
                    throw new ObjectDisposedException("Bitmap", "Cannot access a disposed object.");

                rec = PInvokeHelper.PtrToStructure<OutlineGlyphRec>(original.Reference);
            }
        }

        #endregion

        #region Operators

        /// <summary>
        /// Casts a <see cref="OutlineGlyph" /> back up to a <see cref="Glyph" />. The eqivalent of
        /// <see cref="OutlineGlyph.Root" />.
        /// </summary>
        /// <param name="g">A <see cref="OutlineGlyph" />.</param>
        /// <returns>A <see cref="Glyph" />.</returns>
        public static implicit operator Glyph(OutlineGlyph g)
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
        /// Disposes an instance of the <see cref="OutlineGlyph" /> class.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if (disposing)
                original.Dispose();
        }

        #endregion
    }
}