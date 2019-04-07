namespace Adfectus.Common
{
    /// <summary>
    /// Meta information about the engine.
    /// </summary>
    public static class Meta
    {
        /// <summary>
        /// The version of Adfectus.
        /// </summary>
        public const string MajorVersion = "0.0.13";

        /// <summary>
        /// The commit hash for this version.
        /// </summary>
        public const string CommitHash = "a428203fd2699ffe0e68bbfd3d53bfdb5647ce65";

        /// <summary>
        /// The message of the built commit.
        /// </summary>
        public const string CommitMessage = "Improvements and Fixes";

        /// <summary>
        /// The full version string.
        /// </summary>
        public static string FullVersion
        {
            get
            {
                string version = $"{MajorVersion} - {CommitHash}({CommitMessage})";

#if DEBUG

                version += " [DEBUG]";

#endif

                return version;
            }
        }
    }
}