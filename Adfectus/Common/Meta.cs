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
        public const string MajorVersion = "0.0.9";

        /// <summary>
        /// The commit hash for this version.
        /// </summary>
        public const string CommitHash = "7befafe95102cbe4f5948214d46409aa1f08612d";

        /// <summary>
        /// The message of the built commit.
        /// </summary>
        public const string CommitMessage = "Fixed AL error causing crashes (as some as expected) and frame time being wrong.";

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