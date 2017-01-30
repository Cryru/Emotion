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
        Windowed, //In a window the size of which is specified by the settings.
        Fullscreen, //Graphics exclusive mode, fullscreened.
        Borderless //The window is without a border and fullscreened.
    }

    /// <summary>
    /// The ticker's state.
    /// </summary>
    public enum TickerState
    {
        Running, //Ticking.
        Done, //Done ticking.
        Paused //Paused.
    }

    /// <summary>
    /// 
    /// </summary>
    public enum TextureMode
    {
        Stretch, //xxxx
        Tile //xxxx
    }
}
