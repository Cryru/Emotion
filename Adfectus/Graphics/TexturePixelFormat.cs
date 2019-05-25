namespace Adfectus.Graphics
{
    /// <summary>
    /// The pixel format of a texture.
    /// </summary>
    public enum TexturePixelFormat
    {
        Rgba,
        Red,
        Green,
        Blue,
        Alpha,
        Rgb,

        /// <summary>
        /// BGRA is not compatible with OpenGL ES. Instead a swizzle mask is used.
        /// </summary>
        Bgra
    }
}