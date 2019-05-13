namespace FreeImageAPI
{
    /// <summary>
    /// Lossless JPEG transformations constants used in FreeImage_JPEGTransform.
    /// </summary>
    public enum FREE_IMAGE_JPEG_OPERATION
    {
        /// <summary>
        /// no transformation
        /// </summary>
        FIJPEG_OP_NONE = 0,

        /// <summary>
        /// horizontal flip
        /// </summary>
        FIJPEG_OP_FLIP_H = 1,

        /// <summary>
        /// vertical flip
        /// </summary>
        FIJPEG_OP_FLIP_V = 2,

        /// <summary>
        /// transpose across UL-to-LR axis
        /// </summary>
        FIJPEG_OP_TRANSPOSE = 3,

        /// <summary>
        /// transpose across UR-to-LL axis
        /// </summary>
        FIJPEG_OP_TRANSVERSE = 4,

        /// <summary>
        /// 90-degree clockwise rotation
        /// </summary>
        FIJPEG_OP_ROTATE_90 = 5,

        /// <summary>
        /// 180-degree rotation
        /// </summary>
        FIJPEG_OP_ROTATE_180 = 6,

        /// <summary>
        /// 270-degree clockwise (or 90 ccw)
        /// </summary>
        FIJPEG_OP_ROTATE_270 = 7
    }
}