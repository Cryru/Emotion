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
        public const string MajorVersion = "0.0.16";

        /// <summary>
        /// The commit hash for this version.
        /// </summary>
        public const string CommitHash = "NONE";

        /// <summary>
        /// The message of the built commit.
        /// </summary>
        public const string CommitMessage = "Introduced Platforms";

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