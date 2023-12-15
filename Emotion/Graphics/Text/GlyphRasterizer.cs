namespace Emotion.Graphics.Text
{
    public enum GlyphRasterizer
    {
        /// <summary>
        /// A custom GPU-based rasterizer built for the engine. Still not mature enough to be the default.
        /// Slow and ugliest.
        /// </summary>
        Emotion,

        /// <summary>
        /// A custom GPU-based rasterizer that requires a custom shader when drawing glyphs.
        /// First a very high resolution atlas is rendered using the Emotion rasterizer and
        /// a signed distance field generating algorithm is ran over it. This is cached to a file (if an asset store is loaded).
        /// Then the atlas is used for all font sizes and read from using the SDF shader to provide crisp text at any size.
        /// Good looking, but very slow to render. Writes caches images to reduce subsequent loading but loading the image isn't
        /// very fast either.
        /// Best looking mode at huge font sizes, however might not work on weaker GPUs (without a precached file) as it can
        /// generate huge textures.
        /// Can reuse atlas images for various font sizes, reducing memory usage overall.
        /// </summary>
        EmotionSDFVer3,

        /// <summary>
        /// A custom GPU-based rasterizer that requires a custom shader when drawing glyphs.
        /// An SDF texture is created on the GPU using a reference font size and cached to a file.
        /// Generation of this texture doesn't take long and can even be done at runtime. Looks good at
        /// most font sizes, especially small ones.
        /// Can reuse atlas images for various font sizes, reducing memory usage overall.
        /// </summary>
        EmotionSDFVer4,

        /// <summary>
        /// Mature software rasterizer.
        /// Default.
        /// </summary>
        StbTrueType,
    }
}