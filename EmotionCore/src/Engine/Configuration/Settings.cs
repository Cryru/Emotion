// Emotion - https://github.com/Cryru/Emotion

#region Using

#endregion

namespace Emotion.Engine.Configuration
{
    /// <summary>
    /// Engine configuration.
    /// </summary>
    public sealed class Settings
    {
        /// <summary>
        /// Settings pertaining to the host.
        /// </summary>
        public HostSettings HostSettings { get; } = new HostSettings();

        /// <summary>
        /// Settings pertaining to rendering.
        /// </summary>
        public RenderSettings RenderSettings { get; } = new RenderSettings();

        /// <summary>
        /// Settings pertaining to the scripting engine module.
        /// </summary>
        public ScriptingSettings ScriptingSettings { get; } = new ScriptingSettings();

        /// <summary>
        /// Settings pertaining to audio functionality.
        /// </summary>
        public SoundSettings SoundSettings { get; } = new SoundSettings();
    }
}