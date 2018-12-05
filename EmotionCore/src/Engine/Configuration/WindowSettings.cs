// Emotion - https://github.com/Cryru/Emotion

#region Using

using System.Drawing;
using Emotion.Engine.Hosting.Desktop;

#endregion

namespace Emotion.Engine.Configuration
{
    /// <summary>
    /// Settings pertaining to the host.
    /// </summary>
    public class HostSettings
    {
        /// <summary>
        /// The host's title. "Untitled" by default.
        /// </summary>
        public string Title = "Untitled";

        /// <summary>
        /// The width size of the host. By default this is 960.
        /// </summary>
        public int Width = 960;

        /// <summary>
        /// The height size of the host. By default this is 540.
        /// </summary>
        public int Height = 540;

        /// <summary>
        /// The window mode for window hosts. Windowed by default.
        /// </summary>
        public WindowMode WindowMode = WindowMode.Windowed;

        /// <summary>
        /// The host icon if any. Requires System.Drawing to use.
        /// </summary>
        public Icon WindowIcon;
    }
}