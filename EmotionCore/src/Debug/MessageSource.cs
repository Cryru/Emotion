// Emotion - https://github.com/Cryru/Emotion

namespace Emotion.Debug
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
        AssetLoader,
        Input,
        Renderer,
        SoundManager,
        GL,

        // User
        Game,
        UIController
    }
}