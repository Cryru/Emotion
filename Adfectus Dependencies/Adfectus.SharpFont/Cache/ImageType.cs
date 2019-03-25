#region Using

using System;
using SharpFont.Cache.Internal;

#endregion

namespace SharpFont.Cache
{
    /// <summary>
    /// A structure used to model the type of images in a glyph cache.
    /// </summary>
    public class ImageType
    {
        #region Fields

        private IntPtr reference;
        private ImageTypeRec rec;

        #endregion

        #region Constructors

        internal ImageType(IntPtr reference)
        {
            Reference = reference;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the face ID.
        /// </summary>
        public IntPtr FaceId
        {
            get => rec.face_id;
        }

        /// <summary>
        /// Gets the width in pixels.
        /// </summary>
        public int Width
        {
            get => rec.width;
        }

        /// <summary>
        /// Gets the height in pixels.
        /// </summary>
        public int Height
        {
            get => rec.height;
        }

        /// <summary>
        /// Gets the load flags, as in <see cref="Face.LoadGlyph" />
        /// </summary>

        public LoadFlags Flags
        {
            get => rec.flags;
        }

        internal IntPtr Reference
        {
            get => reference;

            set
            {
                reference = value;
                rec = PInvokeHelper.PtrToStructure<ImageTypeRec>(reference);
            }
        }

        #endregion
    }
}