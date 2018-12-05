// Emotion - https://github.com/Cryru/Emotion

namespace Emotion.Engine.Configuration
{
    /// <summary>
    /// Functionality related engine configuration.
    /// </summary>
    public class Flags
    {
        /// <summary>
        /// Flags related to rendering.
        /// </summary>
        public RenderFlags RenderFlags { get; } = new RenderFlags();

        /// <summary>
        /// Flags related to the RichText class.
        /// </summary>
        public RichTextFlags RichTextFlags { get; } = new RichTextFlags();

        /// <summary>
        /// The root directory in which assets are located, relative to the execution directory.
        /// </summary>
        public string AssetRootDirectory = "Assets";
    }
}