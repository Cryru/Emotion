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

    /// <summary>
    /// The text style to use when rendering.
    /// </summary>
    public enum TextStyle
    {
        /// <summary>
        /// Default. The text is aligned to the left of the box.
        /// </summary>
        Left,
        /// <summary>
        /// Each line is centered.
        /// </summary>
        Center,
        /// <summary>
        /// The text is aligned to the right of the box.
        /// </summary>
        Right,
        /// <summary>
        /// Each line of text is stretched to be somewhat the same width creating a box effect.
        /// </summary>
        Justified,
        /// <summary>
        /// Each line of text is stretched to be somewhat the same width creating a box effect in addition to being centered.
        /// </summary>
        JustifiedCenter
    }

    /// <summary>
    /// The layer to draw the object on.
    /// </summary>
    public enum ObjectLayer
    {
        /// <summary>
        /// The object belongs to the UI layer.
        /// </summary>
        UI,
        /// <summary>
        /// The object belongs to the world layer.
        /// </summary>
        World,
    }
}
