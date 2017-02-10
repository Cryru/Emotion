//////////////////////////////////////////////////////////////////////////////
// SoulEngine - A game engine based on the MonoGame Framework.              //
// Public Repository: https://github.com/Cryru/SoulEngine                   //
//////////////////////////////////////////////////////////////////////////////
/// <summary>
/// Enumerables used by the engine.
/// </summary>
namespace SoulEngine.Enums
{
    /// <summary>
    /// The screen's mode.
    /// </summary>
    public enum ScreenMode
    {
        /// <summary>
        /// In a window the size of which is specified by the settings.
        /// </summary>
        Windowed,
        /// <summary>
        /// Graphics exclusive mode, fullscreened.
        /// </summary>
        Fullscreen,
        /// <summary>
        /// The window is without a border and fullscreened.
        /// </summary>
        Borderless
    }

    /// <summary>
    /// The ticker's state.
    /// </summary>
    public enum TickerState
    {
        /// <summary>
        /// Ticking.
        /// </summary>
        Running,
        /// <summary>
        /// Done ticking.
        /// </summary>
        Done,
        /// <summary>
        /// Paused.
        /// </summary>
        Paused
    }

    /// <summary>
    /// The way to render a texture.
    /// </summary>
    public enum TextureMode
    {
        /// <summary>
        /// The texture is stretched to fit the size it is supposed to be.
        /// </summary>
        Stretch,
        /// <summary>
        /// The texture is tiled as many times as needed to fit the size it is supposed to be.
        /// </summary>
        Tile
    }

    /// <summary>
    /// The channel to draw on.
    /// </summary>
    public enum DrawChannel
    {
        /// <summary>
        /// Can be used to draw outside out of the screen space, like the black bars.
        /// </summary>
        Terminus,
        /// <summary>
        /// UI and other stuff goes on this channel.
        /// </summary>
        Screen,
        /// <summary>
        /// Game objects and such are here, this space is viewed through a camera.
        /// </summary>
        World
    }
}
