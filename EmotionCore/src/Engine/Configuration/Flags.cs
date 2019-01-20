// Emotion - https://github.com/Cryru/Emotion

#region Using

using System.Reflection;

#endregion

namespace Emotion.Engine.Configuration
{
    /// <summary>
    /// Functionality related engine configuration.
    /// </summary>
    public class Flags
    {
        /// <summary>
        /// Flags related to rendering.
        /// </summary>
        public RenderFlags RenderFlags { get; } = new RenderFlags();

        /// <summary>
        /// Flags related to the RichText class.
        /// </summary>
        public RichTextFlags RichTextFlags { get; } = new RichTextFlags();

        /// <summary>
        /// The root directory in which assets are located, relative to the execution directory.
        /// </summary>
        public string AssetRootDirectory = "Assets";

        /// <summary>
        /// Additional assemblies to look for embedded assets in.
        /// </summary>
        public Assembly[] AdditionalAssetAssemblies = new Assembly[0];

        /// <summary>
        /// Whether input focus is regained only after a mouse click is detected.
        /// </summary>
        public bool InputFocusRequireClick = true;

        /// <summary>
        /// Whether to call Environment.Exit when Context.Quit is called. On by default.
        /// </summary>
        public bool CloseEnvironmentOnQuit = true;

        /// <summary>
        /// Whether to expect console input.
        /// </summary>
        public bool ConsoleInput = true;

        /// <summary>
        /// How often to update the sound thread in milliseconds. Takes effect immediately.
        /// </summary>
        public int SoundThreadFrequency { get; set; } = 50;

        /// <summary>
        /// Read only. The scaled width to fit host resolution. Takes margins into account and is calculated on resize.
        /// </summary>
        public float ScaleResX = 0;

        /// <summary>
        /// Read only. The scaled height to fit host resolution. Takes margins into account and is calculated on resize.
        /// </summary>
        public float ScaleResY = 0;
    }
}