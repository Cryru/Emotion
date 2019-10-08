#region Using

using System;
using System.Runtime.InteropServices;
using Emotion.Common;
using Emotion.Platform.Config;
using Emotion.Platform.Implementation;
using Emotion.Platform.Implementation.Null;
using Emotion.Platform.Implementation.Win32;

#endregion

namespace Emotion.Platform
{
    public class Loader
    {
        /// <summary>
        /// The configuration the loader was initialized with.
        /// </summary>
        internal static InitConfig InitConfig;

        /// <summary>
        /// Setup a native platform.
        /// </summary>
        /// <param name="initConfig">The initialization configuration.</param>
        /// <param name="conf">Configuration for the platform's graphic device, window, and other things.</param>
        /// <returns>The native platform.</returns>
        public static PlatformBase Setup(InitConfig initConfig = null, PlatformConfig conf = null)
        {
            initConfig ??= new InitConfig();
            InitConfig = initConfig;

            PlatformBase platform = null;

            // Detect platform.
            if (InitConfig.NullPlatform)
            {
                platform = new NullPlatform();
            }
            else if (InitConfig.CustomPlatformBase != null)
            {
                platform = InitConfig.CustomPlatformBase;
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                // Win32
                platform = new Win32Platform();
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                // Cocoa
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                // Check for Wayland.
                if (Environment.GetEnvironmentVariable("WAYLAND_DISPLAY") != null)
                {
                }
            }

            if (platform == null) throw new Exception("Unknown platform.");

            Engine.Log.Info($"Platform is: {platform}", "Emotion.Platform.Loader");
            platform.Setup(conf);

            return platform;
        }
    }
}