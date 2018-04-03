// Emotion - https://github.com/Cryru/Emotion

#if DEBUG

namespace Emotion.Engine.Debugging
{
    public enum MessageSource
    {
        Other,

        // Emotion
        Engine,
        Debugger,
        FileManager,
        ScriptingEngine,
        LayerManager,

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