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
        Renderer,
        AssetLoader,
    }
}

#endif