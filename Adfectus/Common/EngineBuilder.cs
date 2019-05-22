#region Using

using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Adfectus.Common.Configuration;
using Adfectus.IO;
using Adfectus.Logging;

#endregion

namespace Adfectus.Common
{
    /// <summary>
    /// A class used for customizing and configuring the engine prior to running it.
    /// </summary>
    public sealed class EngineBuilder
    {
        #region Build Properties

        /// <summary>
        /// The type of logger.
        /// </summary>
        public Type Logger { get; private set; }

        /// <summary>
        /// Whether the engine is in debug mode.
        /// </summary>
        public bool DebugMode { get; private set; }

        /// <summary>
        /// The title label of the host.
        /// </summary>
        public string HostTitle { get; private set; } = "Untitled";

        /// <summary>
        /// The initial window mode of the host.
        /// </summary>
        public WindowMode HostWindowMode { get; private set; } = WindowMode.Windowed;

        /// <summary>
        /// The initial size of the window host.
        /// </summary>
        public Vector2 HostSize { get; private set; } = new Vector2(640 * 2, 360 * 2);

        /// <summary>
        /// Whether the host is resizable by the user, should the platform allow.
        /// </summary>
        public bool HostResizable { get; private set; } = true;

        /// <summary>
        /// The default assets folder.
        /// </summary>
        public string AssetFolder { get; private set; } = "Assets";

        /// <summary>
        /// Additional sources for the default asset loader.
        /// </summary>
        public IEnumerable<AssetSource> AdditionalAssetSources { get; private set; }

        /// <summary>
        /// List of plugins which will be loaded.
        /// </summary>
        public List<Plugin> Plugins { get; private set; } = new List<Plugin>();

        /// <summary>
        /// Whether to perform platform specific native library loading. True by default, is required to run the engine.
        /// </summary>
        public bool LoadNativeLibraries { get; private set; } = true;

        /// <summary>
        /// The resolution to render at.
        /// </summary>
        public Vector2 RenderSize { get; private set; } = new Vector2(640, 360);

        /// <summary>
        /// Whether to automatically rescale the render size on 16:10 screens.
        /// </summary>
        public bool RescaleAutomatic { get; private set; } = true;

        /// <summary>
        /// The target ticks per second.
        /// </summary>
        public int TargetTPS { get; private set; } = 60;

        /// <summary>
        /// The time a script has to be executing for, before being stopped.
        /// </summary>
        public TimeSpan ScriptTimeout { get; private set; } = new TimeSpan(0, 0, 0, 0, 500);

        /// <summary>
        /// Whether sound is initially enabled or disabled.
        /// </summary>
        public bool InitialSound { get; set; } = true;

        /// <summary>
        /// The initial volume of the sound manager.
        /// </summary>
        public float InitialVolume { get; set; } = 100;

        #endregion

        /// <summary>
        /// Setup the logger.
        /// </summary>
        /// <typeparam name="T">The logger type.</typeparam>
        /// <returns>This builder, for chaining.</returns>
        public EngineBuilder SetLogger<T>() where T : LoggingProvider
        {
            Logger = typeof(T);

            return this;
        }

        /// <summary>
        /// Setup the host parameters.
        /// </summary>
        /// <param name="title">The title label of the host.</param>
        /// <param name="winMode">The initial window mode of the host.</param>
        /// <param name="winSize">The initial window size of the host.</param>
        /// <param name="resizable">Whether the host is resizable.</param>
        /// <returns>This builder, for chaining</returns>
        public EngineBuilder SetupHost(string title = "Untitled", WindowMode winMode = WindowMode.Windowed, Vector2? winSize = null, bool resizable = true)
        {
            HostTitle = title;
            HostWindowMode = winMode;
            if (winSize != null) HostSize = (Vector2) winSize;
            HostResizable = resizable;

            return this;
        }

        /// <summary>
        /// Setup AssetLoader concerning properties.
        /// </summary>
        /// <param name="defaultFolder">The default assets folder.</param>
        /// <param name="additionalAssetSources">Additional sources for the default AssetLoader.</param>
        /// <returns>This builder, for chaining</returns>
        public EngineBuilder SetupAssets(string defaultFolder = "Assets", IEnumerable<AssetSource> additionalAssetSources = null)
        {
            AssetFolder = defaultFolder;
            AdditionalAssetSources = additionalAssetSources;

            return this;
        }

        /// <summary>
        /// Setup configuration flags concern the initial build.
        /// </summary>
        /// <param name="renderSize">The resolution to render at.</param>
        /// <param name="rescaleAutomatic">Whether to automatically rescale the render size on 16:10 screens.</param>
        /// <param name="performBootstrap">Whether to perform loading of native libraries.</param>
        /// <param name="targetTPS">The target ticks per second.</param>
        /// <param name="debugMode">Whether the engine is in debug mode.</param>
        /// <returns>This builder, for chaining</returns>
        public EngineBuilder SetupFlags(Vector2? renderSize = null, bool rescaleAutomatic = true, bool performBootstrap = true, int targetTPS = 60, bool debugMode = false)
        {
            if(renderSize != null) RenderSize = (Vector2) renderSize;
            RescaleAutomatic = rescaleAutomatic;
            LoadNativeLibraries = performBootstrap;
            TargetTPS = targetTPS;
            DebugMode = debugMode;

            return this;
        }

        /// <summary>
        /// Setup scripting engine configuration.
        /// </summary>
        /// <param name="timeout">The time a script has to be executing for, before being stopped.</param>
        /// <returns>This builder, for chaining</returns>
        public EngineBuilder SetupScripting(TimeSpan timeout)
        {
            ScriptTimeout = timeout;

            return this;
        }

        /// <summary>
        /// Setup initial sound configuration.
        /// </summary>
        /// <param name="initialSound">Whether the sound is initially enabled.</param>
        /// <param name="initialVolume">Initial volume settings.</param>
        /// <returns>This builder, for chaining</returns>
        public EngineBuilder SetupSound(bool initialSound = true, float initialVolume = 100)
        {
            InitialSound = initialSound;
            InitialVolume = initialVolume;

            return this;
        }

        /// <summary>
        /// Add a generic plugin.
        /// </summary>
        /// <param name="plugin">The plugin to add.</param>
        public EngineBuilder AddGenericPlugin(Plugin plugin)
        {
            // Check if a plugin of this type already exists.
            if (Plugins.Any(x => x.GetType() == plugin.GetType())) return this;

            // Add the plugin.
            Plugins.Add(plugin);

            return this;
        }
    }
}