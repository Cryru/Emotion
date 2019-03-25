namespace Adfectus.Logging
{
    public enum MessageSource
    {
        /// <summary>
        /// Internal engine logs.
        /// </summary>
        Engine,

        /// <summary>
        /// Logs by the host.
        /// </summary>
        Host,

        /// <summary>
        /// Logs by the cross platform bootstrapper.
        /// </summary>
        Bootstrap,

        /// <summary>
        /// Logs by the graphics manager and other GL related functionality.
        /// </summary>
        GL,

        /// <summary>
        /// Logs by the ScriptingEngine module.
        /// </summary>
        ScriptingEngine,

        /// <summary>
        /// Logs by the SceneManager module.
        /// </summary>
        SceneManager,

        /// <summary>
        /// Logs by the AssetLoader module.
        /// </summary>
        AssetLoader,

        /// <summary>
        /// Logs by the InputManager module.
        /// </summary>
        Input,

        /// <summary>
        /// Logs by the renderer.
        /// </summary>
        Renderer,

        /// <summary>
        /// Logs by the sound manager.
        /// </summary>
        SoundManager,

        /// <summary>
        /// Other unidentified logs.
        /// </summary>
        Other,

        /// <summary>
        /// Submitted exceptions. Logged by the ErrorHandler.
        /// </summary>
        StdErr,

        // Emotion Legacy
        Debugger,
        FileManager,

        // User
        Game,
        UIController,

        // Other
        StdOut
    }
}