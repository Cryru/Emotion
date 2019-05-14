#region Using

using System;
using SharpFont.Cache.Internal;

#endregion

namespace SharpFont.Cache
{
    /// <summary>
    /// A structure used to describe a given character size in either pixels or points to the cache manager.
    /// </summary>
    /// <remarks>
    /// This type is mainly used to retrieve <see cref="FTSize" /> objects through the cache manager.
    /// </remarks>
    /// <see cref="Manager.LookupSize" />
    public class Scaler
    {
        #region Fields

        private IntPtr reference;
        private ScalerRec rec;

        #endregion

        #region Constructors

        internal Scaler(IntPtr reference)
        {
            Reference = reference;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the source face ID.
        /// </summary>
        public IntPtr FaceId
        {
            get => rec.face_id;
        }

        /// <summary>
        /// Gets the character width.
        /// </summary>

        public uint Width
        {
            get => rec.width;
        }

        /// <summary>
        /// Gets the character height.
        /// </summary>

        public uint Height
        {
            get => rec.height;
        }

        /// <summary>
        /// Gets a value indicating whether the ‘width’ and ‘height’ fields are interpreted as integer pixel character
        /// sizes. Otherwise, they are expressed as 1/64th of points.
        /// </summary>
        public bool Pixel
        {
            get => rec.pixel == 1;
        }

        /// <summary>
        /// Gets the horizontal resolution in dpi; only used when ‘pixel’ is value 0.
        /// </summary>

        public uint ResolutionX
        {
            get => rec.x_res;
        }

        /// <summary>
        /// Gets the vertical resolution in dpi; only used when ‘pixel’ is value 0.
        /// </summary>

        public uint ResolutionY
        {
            get => rec.y_res;
        }

        internal IntPtr Reference
        {
            get => reference;

            set
            {
                reference = value;
                rec = PInvokeHelper.PtrToStructure<ScalerRec>(reference);
            }
        }

        #endregion
    }
}