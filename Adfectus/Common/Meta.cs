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
        public const string MajorVersion = "0.0.14";

        /// <summary>
        /// The commit hash for this version.
        /// </summary>
        public const string CommitHash = "ac9087708210cd8901e5c1fb71f93d6badbd745e";

        /// <summary>
        /// The message of the built commit.
        /// </summary>
        public const string CommitMessage = "Fixed textures being mirrored, and win32 libraries being loaded from …";

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