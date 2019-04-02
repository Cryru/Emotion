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
        public const string MajorVersion = "0.0.10";

        /// <summary>
        /// The commit hash for this version.
        /// </summary>
        public const string CommitHash = "7334a02faf201c9f6d2abaeb4d446aec1c65e905";

        /// <summary>
        /// The message of the built commit.
        /// </summary>
        public const string CommitMessage = "Added Coroutines and fixed frametime during non-fixed timesteps.";

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