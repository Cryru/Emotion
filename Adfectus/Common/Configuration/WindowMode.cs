namespace Adfectus.Common.Configuration
{
    /// <summary>
    /// The display mode states of a window.
    /// </summary>
    public enum WindowMode
    {
        /// <summary>
        /// The window is an ordinary window.
        /// </summary>
        Windowed,

        /// <summary>
        /// The window is fullscreen by stretching it over the primary display and removing its borders.
        /// Is applied only at start.
        /// </summary>
        Borderless,

        /// <summary>
        /// The window is in exclusive fullscreen mode.
        /// </summary>
        Fullscreen
    }
}