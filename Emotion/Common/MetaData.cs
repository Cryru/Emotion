namespace Emotion.Common
{
    /// <summary>
    /// Data about the engine.
    /// </summary>
    public static class MetaData
    {
        /// <summary>
        /// The engine version.
        /// </summary>
        public static string Version = "0.0.0";

        /// <summary>
        /// The hash of the git commit, if built from one.
        /// </summary>
        public static string GitHash = "None";

        /// <summary>
        /// Which build configuration this is.
        /// </summary>
#if DEBUG
        public static string BuildConfig = "DEBUG";
#elif RELEASE
        public static string BuildConfig = "RELEASE";
#endif
    }
}