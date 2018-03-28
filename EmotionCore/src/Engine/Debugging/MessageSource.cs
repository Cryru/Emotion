// Emotion - https://github.com/Cryru/Emotion

#if DEBUG

namespace Emotion.Engine.Debugging
{
    public enum MessageSource
    {
        Engine,
        Debugger,
        FileManager,
        ScriptingEngine,

        // Platform related.
        PlatformCore,
        Platform,
        Renderer,
        AssetLoader,

        // User
        Game
    }
}

#endif