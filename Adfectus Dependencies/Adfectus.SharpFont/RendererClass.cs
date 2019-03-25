#region Using

using System;
using SharpFont.Internal;

#endregion

namespace SharpFont
{
    /// <summary>
    /// The renderer module class descriptor.
    /// </summary>
    public class RendererClass
    {
        #region Fields

        private IntPtr reference;
        private RendererClassRec rec;

        #endregion

        #region Constructors

        internal RendererClass(IntPtr reference)
        {
            Reference = reference;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the root <see cref="ModuleClass" /> fields.
        /// </summary>
        public ModuleClass Root
        {
            get => new ModuleClass(reference);
        }

        /// <summary>
        /// Gets the glyph image format this renderer handles.
        /// </summary>

        public GlyphFormat Format
        {
            get => rec.glyph_format;
        }

        /// <summary>
        /// Gets a method used to render the image that is in a given glyph slot into a bitmap.
        /// </summary>
        public IntPtr RenderGlyph
        {
            get => rec.render_glyph;
        }

        /// <summary>
        /// Gets a method used to transform the image that is in a given glyph slot.
        /// </summary>
        public IntPtr TransformGlyph
        {
            get => rec.transform_glyph;
        }

        /// <summary>
        /// Gets a method used to access the glyph's cbox.
        /// </summary>
        public IntPtr GetGlyphCBox
        {
            get => rec.get_glyph_cbox;
        }

        /// <summary>
        /// Gets a method used to pass additional parameters.
        /// </summary>
        public IntPtr SetMode
        {
            get => rec.set_mode;
        }

        /// <summary>
        /// Gets a pointer to its raster's class.
        /// </summary>
        /// <remarks>For <see cref="GlyphFormat.Outline" /> renderers only.</remarks>
        public RasterFuncs RasterClass
        {
            get => new RasterFuncs(PInvokeHelper.AbsoluteOffsetOf<RendererClassRec>(Reference, "raster_class"));
        }

        internal IntPtr Reference
        {
            get => reference;

            set
            {
                reference = value;
                rec = PInvokeHelper.PtrToStructure<RendererClassRec>(reference);
            }
        }

        #endregion
    }
}