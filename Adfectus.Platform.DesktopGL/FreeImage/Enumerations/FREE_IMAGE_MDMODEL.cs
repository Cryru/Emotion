namespace FreeImageAPI
{
    /// <summary>
    /// Metadata models supported by FreeImage.
    /// </summary>
    public enum FREE_IMAGE_MDMODEL
    {
        /// <summary>
        /// No data
        /// </summary>
        FIMD_NODATA = -1,

        /// <summary>
        /// single comment or keywords
        /// </summary>
        FIMD_COMMENTS = 0,

        /// <summary>
        /// Exif-TIFF metadata
        /// </summary>
        FIMD_EXIF_MAIN = 1,

        /// <summary>
        /// Exif-specific metadata
        /// </summary>
        FIMD_EXIF_EXIF = 2,

        /// <summary>
        /// Exif GPS metadata
        /// </summary>
        FIMD_EXIF_GPS = 3,

        /// <summary>
        /// Exif maker note metadata
        /// </summary>
        FIMD_EXIF_MAKERNOTE = 4,

        /// <summary>
        /// Exif interoperability metadata
        /// </summary>
        FIMD_EXIF_INTEROP = 5,

        /// <summary>
        /// IPTC/NAA metadata
        /// </summary>
        FIMD_IPTC = 6,

        /// <summary>
        /// Abobe XMP metadata
        /// </summary>
        FIMD_XMP = 7,

        /// <summary>
        /// GeoTIFF metadata
        /// </summary>
        FIMD_GEOTIFF = 8,

        /// <summary>
        /// Animation metadata
        /// </summary>
        FIMD_ANIMATION = 9,

        /// <summary>
        /// Used to attach other metadata types to a dib
        /// </summary>
        FIMD_CUSTOM = 10
    }
}