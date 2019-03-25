namespace Adfectus.Common
{
    /// <summary>
    /// Meta information about the engine.
    /// </summary>
    public static class Meta
    {
        /// <summary>
        /// The version of Emotion.
        /// </summary>
        public const string MajorVersion = "1";

        /// <summary>
        /// The build version of Emotion.
        /// </summary>
        public const string BuildVersion = "000";

        /// <summary>
        /// The commit hash for this version.
        /// </summary>
        public const string CommitHash = "><";

        /// <summary>
        /// The message of the built commit.
        /// </summary>
        public const string CommitMessage = "none";

        /// <summary>
        /// The full version string.
        /// </summary>
        public static string FullVersion
        {
            get
            {
                string version = $"{MajorVersion} - {BuildVersion} : {CommitHash}({CommitMessage})";

#if DEBUG

                version += " [DEBUG]";

#endif

                return version;
            }
        }
    }
}