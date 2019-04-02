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
        public const string MajorVersion = "0.0.11";

        /// <summary>
        /// The commit hash for this version.
        /// </summary>
        public const string CommitHash = "0af3db8fc00a6c8cead87c433e1f6dd4054cf6e5";

        /// <summary>
        /// The message of the built commit.
        /// </summary>
        public const string CommitMessage = "Fixed the render size changing the host size. Scripting improvements.";

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