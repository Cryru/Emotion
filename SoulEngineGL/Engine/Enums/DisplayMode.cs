// SoulEngine - https://github.com/Cryru/SoulEngine

namespace Soul.Engine.Enums
{
    /// <summary>
    /// The way the window should be drawn.
    /// </summary>
    public enum DisplayMode
    {
        /// <summary>
        /// The window is a standard window with the specified size.
        /// </summary>
        Windowed,

        /// <summary>
        /// Graphics exclusive mode - fullscreened.
        /// </summary>
        Fullscreen,

        /// <summary>
        /// The window is without a border and fullscreened.
        /// </summary>
        BorderlessFullscreen
    }
}