namespace Adfectus.Common.Configuration
{
    /// <summary>
    /// Functionality related engine configuration.
    /// </summary>
    public class Flags
    {
        /// <summary>
        /// Whether to crash when an error is encountered. If a debugger is attached it will break at the specified location. On by
        /// default. Takes effect immediately.
        /// </summary>
        public bool CrashOnError { get; set; } = true;

        /// <summary>
        /// Whether to pause loop execution when the host loses focus. On by default. Takes effect immediately.
        /// </summary>
        public bool PauseOnFocusLoss { get; set; } = true;

        /// <summary>
        /// Flags related to rendering.
        /// </summary>
        public RenderFlags RenderFlags { get; } = new RenderFlags();

        /// <summary>
        /// Flags related to the RichText class.
        /// </summary>
        public RichTextFlags RichTextFlags { get; } = new RichTextFlags();

        /// <summary>
        /// How often to update the sound thread in milliseconds. Takes effect immediately. 50 by default.
        /// </summary>
        public int SoundThreadFrequency { get; set; } = 50;

        /// <summary>
        /// Whether to crash on script error.
        /// </summary>
        public bool StrictScripts { get; set; } = false;
    }
}