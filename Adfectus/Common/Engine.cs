#region Using

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Reflection;
using System.Runtime.InteropServices;
using Adfectus.Common.Configuration;
using Adfectus.Common.Hosting;
using Adfectus.Graphics;
using Adfectus.Implementation.GLFW;
using Adfectus.Input;
using Adfectus.IO;
using Adfectus.Logging;
using Adfectus.Native;
using Adfectus.Scenography;
using Adfectus.Sound;

#endregion

namespace Adfectus.Common
{
    public static class Engine
    {
        #region Modules

        /// <summary>
        /// The logger instance. Is initiated first when Setup is called.
        /// </summary>
        public static LoggingProvider Log { get; private set; }

        /// <summary>
        /// Handles loading assets and storing assets.
        /// </summary>
        public static AssetLoader AssetLoader { get; private set; }

        /// <summary>
        /// Handles rendering.
        /// </summary>
        public static Renderer Renderer { get; private set; }

        /// <summary>
        /// The host which will be running the engine.
        /// Provides the GraphicsManager, InputManager, and window.
        /// </summary>
        public static IHost Host { get; private set; }

        /// <summary>
        /// The graphics manager of the engine. Is created by the host.
        /// </summary>
        public static GraphicsManager GraphicsManager { get; private set; }

        /// <summary>
        /// Handles input from the mouse, keyboard, and other devices. Is created and managed by the host.
        /// </summary>
        public static IInputManager InputManager { get; private set; }

        /// <summary>
        /// Module which manages loading and unloading of scenes.
        /// </summary>
        public static SceneManager SceneManager { get; private set; }

        /// <summary>
        /// A javascript engine.
        /// </summary>
        public static ScriptingEngine ScriptingEngine { get; private set; }

        /// <summary>
        /// Manages loading and playing sound files, using OpenAL.
        /// </summary>
        public static SoundManager SoundManager { get; private set; }

        #endregion

        #region Trackers

        /// <summary>
        /// Whether the Engine is setup.
        /// </summary>
        public static bool IsSetup { get; private set; }

        /// <summary>
        /// Whether the loop is running.
        /// </summary>
        public static bool IsRunning { get; private set; }

        /// <summary>
        /// Whether the host is unfocused.
        /// </summary>
        public static bool IsUnfocused
        {
            get => _forceUnfocused || Flags.PauseOnFocusLoss && Host != null && !Host.Focused;
        }

        /// <summary>
        /// The time it took to render the last frame, in milliseconds.
        /// </summary>
        public static float FrameTime { get; private set; }

        /// <summary>
        /// The raw frame time unhindered by the semi-fixed step, in milliseconds. Used for FPS meters and such.
        /// </summary>
        public static double RawFrameTime { get; private set; }

        /// <summary>
        /// The time which has passed since start. Used for tracking time in shaders and such.
        /// </summary>
        public static float TotalTime { get; private set; }

        #endregion

        #region Private Logic and Flags

        /// <summary>
        /// Whether to force the window as unfocused. Used for testing and debugging.
        /// </summary>
        private static bool _forceUnfocused;

        /// <summary>
        /// Whether quit was called.
        /// </summary>
        private static bool _quit;

        /// <summary>
        /// The target ticks per second.
        /// </summary>
        private static int _targetTPS;

        /// <summary>
        /// Running plugins.
        /// </summary>
        private static IEnumerable<Plugin> _plugins;

#if DEBUG

        /// <summary>
        /// The draw hook of the Rationale debugger.
        /// </summary>
        private static Action _rationaleDrawHook;

#endif

        #endregion

        /// <summary>
        /// Configuration flags for engine functionality.
        /// </summary>
        public static Flags Flags { get; } = new Flags();

        static Engine()
        {
            // Correct startup directory.
            Directory.SetCurrentDirectory(AppDomain.CurrentDomain.BaseDirectory);
        }

        /// <summary>
        /// Perform bootstrap and setup. Starts modules.
        /// </summary>
        /// <param name="builder">The engine builder to use. If none provided uses the default one.</param>
        public static void Setup(EngineBuilder builder = null)
        {
            // Check if already built and/or running.
            if (IsRunning)
            {
                Log.Warning("Adfectus Engine has already been built, and is running.", MessageSource.Engine);
                return;
            }

            // Check if was quit.
            if (_quit)
            {
                Log.Warning("Adfectus Engine was stopped - it cannot be setup again.", MessageSource.Engine);
                return;
            }

            // Check if a builder was provided, and if it wasn't get the default one.
            if (builder == null) builder = new EngineBuilder();
            _targetTPS = builder.TargetTPS;

            // Setup logger first so stuff can get traced in the logs.
            Log = (LoggingProvider) Activator.CreateInstance(builder.Logger ?? typeof(DefaultLogger));

            // Setup error handler because anything from this point onward can error.
            ErrorHandler.Setup();

            // Perform platform bootstrap if needed.
            if (builder.LoadNativeLibraries) NativeLoader.Setup();

            // ReSharper disable once RedundantAssignment
            bool wasCompiledDebug = false;

#if DEBUG
            wasCompiledDebug = true;

            // Check for Rationale debugger.
            if (File.Exists("Rationale.dll"))
            {
                // Invoke the debugger's entry point.
                Assembly rationaleAssembly = Assembly.LoadFrom("Rationale.dll");
                MethodInfo entryPoint = rationaleAssembly.GetType("Rationale.Boot").GetMethod("Main");

                // Pass the builder and entry assembly to the debugger. It can add plugins and do assembly reflection stuff.
                _rationaleDrawHook = (Action) entryPoint.Invoke(null, new object[] {builder, Assembly.GetEntryAssembly()});
            }
#endif

            Log.Info($"Starting Adfectus v{Meta.FullVersion}", MessageSource.Engine);
            Log.Info("-----------", MessageSource.Engine);
            Log.Info("Info Dump", MessageSource.Engine);
            Log.Info("-----------", MessageSource.Engine);
            // ReSharper disable once ConditionIsAlwaysTrueOrFalse
            Log.Info($"Debug Mode / Debugger Attached: {wasCompiledDebug} / {Debugger.IsAttached}", MessageSource.Engine);
            Log.Info($"Framework: {RuntimeInformation.FrameworkDescription}", MessageSource.Engine);
            Log.Info($"SIMD Vectors: {Vector.IsHardwareAccelerated}", MessageSource.Engine);
            Log.Info("-----------", MessageSource.Engine);

            // Ensure log dispose is called when app domain closes.
            AppDomain.CurrentDomain.ProcessExit += (e, a) => { Quit(); };

            // From this point onward the environment is ready and engine setup begins.
            // ------------------------------------------------------------------------------

            Log.Info("Starting module setup.", MessageSource.Engine);

            // Setup non-dependent modules first. This is done before the host to provide it with a working environment.
            ScriptingEngine = new ScriptingEngine(builder.ScriptTimeout);
            Log.Info("Created module - ScriptingEngine.", MessageSource.Engine);

            SceneManager = new SceneManager();
            Log.Info("Created module - SceneManager.", MessageSource.Engine);

            AssetLoader = BuildDefaultAssetLoader(builder.AssetFolder, builder.AdditionalAssetSources);
            Log.Info($"Created module - AssetLoader (Folder: {builder.AssetFolder}).", MessageSource.Engine);

            SoundManager = new SoundManager();
            Log.Info("Created module - SoundManager.", MessageSource.Engine);

            // Create the host.
            Type hostType = builder.HostType ?? typeof(GlfwHost);
            Host = (IHost) Activator.CreateInstance(hostType, builder);

            // Check if the host was created.
            if (Host == null)
            {
                ErrorHandler.SubmitError(new Exception("Could not setup host."));
                Quit();
                return;
            }

            // Calculate the render size.
            Vector2 renderSize = CalculateRenderSize(builder);

            // Apply host settings.
            Log.Info($"Created host of type {hostType}.", MessageSource.Engine);

            // Wait for host to focus.
            while (!Host.Open || !Host.Focused) Host.Update();

            // Check if host setup error-ed.
            if (Host.GraphicsManager == null)
            {
                ErrorHandler.SubmitError(new Exception("Could not setup GraphicsManager."));
                InternalQuit();
                return;
            }

            // Get implementation modules from the host.
            InputManager = Host.InputManager;
            Log.Info("Created module - InputManager.", MessageSource.Engine);
            GraphicsManager = Host.GraphicsManager;
            GraphicsManager.Setup(renderSize);
            Log.Info("Created module - GraphicsManager.", MessageSource.Engine);

            // Create the renderer. It depends on the graphics manager.
            Renderer = new Renderer();
            Log.Info("Created module - Renderer.", MessageSource.Engine);
            Renderer.HostResized();

            // Load plugins.
            Log.Info("Loading plugins...", MessageSource.Engine);
            _plugins = builder.Plugins;
            foreach (Plugin plugin in _plugins)
            {
                plugin.Initialize();
                Log.Trace($"Loading plugin {plugin}.", MessageSource.Engine);
            }

            Log.Info($"{builder.Plugins.Count} plugins loaded.", MessageSource.Engine);

            IsSetup = true;
        }

        /// <summary>
        /// Start the main loop and running the engine.
        /// This function is blocking.
        /// </summary>
        /// <param name="userInit">Is run once after the engine starts running.</param>
        public static void Run(Action userInit = null)
        {
            // Check if setup was called.
            if (!IsSetup) Setup();

            Log.Info("Running engine...", MessageSource.Engine);

            // Set engine as running.
            IsRunning = true;

            // Start the sound manager.
            SoundManager.Run();

            // Execute user init - if any.
            Log.Trace("Executing user init (if any)...", MessageSource.Engine);
            userInit?.Invoke();
            Log.Trace("Executed user init!", MessageSource.Engine);

            // Initiate timing mechanism.
            bool fixedStep = _targetTPS > 0;
            double targetTime = fixedStep ? 1f / _targetTPS : 0;
            Stopwatch timer = Stopwatch.StartNew();
            double accumulator = 0f;
            double lastTick = 0f;

            // Start loop.
            Log.Info("Starting loop.", MessageSource.Engine);
            while (IsRunning && !_quit)
            {
                // Always update the host first.
                Host?.Update();
                if (Host != null && !Host.Open)
                {
                    Log.Info("Host was closed.", MessageSource.Engine);
                    Host = null;
                    Quit();
                    break;
                }

                // If the host is not focused - don't run the loop.
                if (IsUnfocused)
                {
                    SceneManager.Unfocused();
                    continue;
                }

                // For more information on how the timing of the loop works -> https://medium.com/@tglaiel/how-to-make-your-game-run-at-60fps-24c61210fe75
                double curTime = timer.Elapsed.TotalMilliseconds;
                double deltaTime = (curTime - lastTick) / 1000f;
                lastTick = curTime;

                // Snap delta.
                if (Math.Abs(targetTime - deltaTime) <= 0.001) deltaTime = targetTime;

                // Add to the accumulator and write off as raw frame time.
                RawFrameTime = deltaTime * 1000f;
                accumulator += deltaTime;

                // Check if reached max delta.
                if (accumulator > targetTime * 5f) accumulator = targetTime * 5f;

                bool updated = false;
                while (accumulator >= targetTime)
                {
                    // Assign frame time trackers.
                    FrameTime = (float) (fixedStep ? targetTime * 1000f : RawFrameTime);
                    TotalTime += FrameTime;

                    updated = true;
                    Update();
                    accumulator -= targetTime;
                    if (accumulator < 0f) accumulator = 0f;

                    if (fixedStep) continue;
                    accumulator = 0f;
                    break;
                }

                // It is possible for the update to close the host - which will error the draw as the OGL context will be gone.
                if (Host == null || !Host.Open) break;

                // Don't draw if not updated.
                if (updated) Draw();
            }

            // The loop has exited - because the engine is no longer running.
            // Perform closure and cleanup.
            InternalQuit();
        }

        /// <summary>
        /// Stop running everything.
        /// </summary>
        public static void Quit()
        {
            Log?.Info("Stopping engine...", MessageSource.Engine);

            // Set the loop as not running.
            IsRunning = false;

            // Mark as quit.
            _quit = true;
        }

        private static void InternalQuit()
        {
            Log?.Info("Engine was stopped. Performing cleanup.", MessageSource.Engine);

            SoundManager?.Dispose();

            // If the host still exists, it should be closed after cleanup.
            // If it doesn't, graphics cleanup cannot happen.
            if (Host != null && Host.Open)
            {
                // Destroy the renderer.
                Renderer?.Destroy();

                // Close the host.
                Host.Dispose();
            }

            // Flush logs.
            Log?.Dispose();
        }

        #region Loop

        private static void Update()
        {
            // Update the input manager.
            InputManager.Update();

            // Update user code.
            SceneManager.Update();

            // Update plugins.
            foreach (Plugin plugin in _plugins)
            {
                plugin.Update();
            }
        }

        private static void Draw()
        {
            // Run the thread manager.
            GLThread.Run();

            // Clear the screen.
            Renderer.Clear();
            GraphicsManager.CheckError("renderer clear");

            // Draw the user scene.
            SceneManager.Draw();
            GraphicsManager.CheckError("scene draw");

#if DEBUG

            // Draw debug.
            _rationaleDrawHook?.Invoke();

#endif

            // Finish rendering.
            Renderer.End();
            GraphicsManager.CheckError("renderer end");

            // Swap buffers.
            Host.SwapBuffers();
        }

        #endregion

        #region Helpers, Events, Misc

#if DEBUG

        /// <summary>
        /// Force the engine to act as if unfocused.
        /// Used by the tests to test unfocused behavior.
        /// </summary>
        /// <param name="unfocused">Whether to act as unfocused.</param>
        public static void ForceUnfocus(bool unfocused)
        {
            _forceUnfocused = unfocused;
        }
#endif

        /// <summary>
        /// Calculate the render size.
        /// </summary>
        /// <param name="builder">The engine builder.</param>
        /// <returns>The render size calculated from the settings in the engine builder.</returns>
        private static Vector2 CalculateRenderSize(EngineBuilder builder)
        {
            // Check whether to automatically rescale.
            if (!builder.RescaleAutomatic) return builder.RenderSize;

            string aspectRatio = Helpers.GetAspectRatio(builder.RenderSize.X, builder.RenderSize.Y);
            Vector2 screenSize = Host.GetScreenSize();
            string screenAspectRatio = Helpers.GetAspectRatio(screenSize.X, screenSize.Y);

            if (aspectRatio == screenAspectRatio) return builder.RenderSize;

            string[] aspectParams = screenAspectRatio.Split(':');
            float.TryParse(aspectParams[0], out float screenWidth);
            float.TryParse(aspectParams[1], out float screenHeight);

            float majorValue = Math.Max(builder.RenderSize.X, builder.RenderSize.Y);
            bool majorIsWidth = majorValue == builder.RenderSize.X;

            float w;
            float h;

            // Calculate the appropriate aspect's render size.
            if (majorIsWidth)
            {
                w = builder.RenderSize.X;
                h = screenHeight * builder.RenderSize.X / screenWidth;
            }
            else
            {
                w = screenWidth * builder.RenderSize.Y / screenHeight;
                h = builder.RenderSize.Y;
            }

            return new Vector2(w, h);
        }

        /// <summary>
        /// Build the default configuration of an asset loader.
        /// </summary>
        /// <param name="assetsFolder">The folder in which assets will be located.</param>
        /// <param name="additionalSources">Additional asset sources.</param>
        /// <returns>A built asset loader.</returns>
        private static AssetLoader BuildDefaultAssetLoader(string assetsFolder, IEnumerable<AssetSource> additionalSources)
        {
            List<AssetSource> sources = new List<AssetSource> {new FileAssetSource(assetsFolder)};

            // Add default embedded sources.
            List<Assembly> sourceAssemblies = new List<Assembly>
            {
                Assembly.GetCallingAssembly(), // This is the assembly which called this function. Can be the game or the engine.
                Assembly.GetExecutingAssembly(), // Is the engine.
                Assembly.GetEntryAssembly() // Is game or debugger.
            };

            // Remove duplicate assemblies and null assemblies.
            sourceAssemblies = sourceAssemblies.Distinct().Where(x => x != null).ToList();

            // Create sources.
            foreach (Assembly assembly in sourceAssemblies)
            {
                sources.Add(new EmbeddedAssetSource(assembly, assetsFolder));
            }

            AssetLoader loader = new AssetLoader(sources);

            // Add additional sources.
            if (additionalSources == null) return loader;
            foreach (AssetSource source in additionalSources)
            {
                loader.AddSource(source);
                Log.Trace($"Added additional source {source} to default AssetLoader.", MessageSource.Engine);
            }

            return loader;
        }

        #endregion
    }
}