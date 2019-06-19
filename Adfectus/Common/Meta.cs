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
        public const string MajorVersion = "0.0.21";

        /// <summary>
        /// The commit hash for this version.
        /// </summary>
        public const string CommitHash = "4f7a286ddb127138ac742850af009753928f4a84";

        /// <summary>
        /// The message of the built commit.
        /// </summary>
        public const string CommitMessage = "Fixed a bug with scissoring on Nvidia GPUs";

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