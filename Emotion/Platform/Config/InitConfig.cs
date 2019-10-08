#region Using

using Emotion.Platform.Implementation;

#endregion

namespace Emotion.Platform.Config
{
    /// <summary>
    /// The initialization configuration.
    /// </summary>
    public class InitConfig
    {
        /// <summary>
        /// Whether to use the noop platform.
        /// </summary>
        public bool NullPlatform = false;

        /// <summary>
        /// A custom platform to use instead of detecting one.
        /// </summary>
        public PlatformBase CustomPlatformBase = null;

        public bool HatButtons = true;
        public bool MacMenuBar = true;
        public bool MacBundleChDir = true;
    }
}