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
        public const string MajorVersion = "0.0.15";

        /// <summary>
        /// The commit hash for this version.
        /// </summary>
        public const string CommitHash = "a5badba8e676055502526da4133a6a7a7a67df85";

        /// <summary>
        /// The message of the built commit.
        /// </summary>
        public const string CommitMessage = "Removed win32 support. Improved Steam Plugin";

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