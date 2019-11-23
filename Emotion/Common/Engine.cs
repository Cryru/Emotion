#region Using

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using Emotion.Common.Threading;
using Emotion.Graphics;
using Emotion.IO;
using Emotion.Platform;
using Emotion.Platform.Config;
using Emotion.Platform.Implementation;
using Emotion.Scenography;
using Emotion.Standard.Logging;

//using Emotion.Native;

#endregion

namespace Emotion.Common
{
    public static class Engine
    {
        public static Configurator Configuration { get; private set; }

        #region Modules

        public static LoggingProvider Log { get; private set; } = new PreSetupLogger();

        /// <summary>
        /// Handles loading assets and storing assets.
        /// </summary>
        public static AssetLoader AssetLoader { get; private set; }

        /// <summary>
        /// The platform reference.
        /// </summary>
        public static PlatformBase Host { get; private set; }

        /// <summary>
        /// Handles rendering.
        /// </summary>
        public static Renderer Renderer { get; private set; }

        /// <summary>
        /// The legacy input manager.
        /// </summary>
        public static InputManager InputManager { get; private set; }

        /// <summary>
        /// Module which manages loading and unloading of scenes.
        /// </summary>
        public static SceneManager SceneManager { get; private set; }

        #endregion

        #region Flags

        /// <summary>
        /// Whether setting up the engine is complete.
        /// </summary>
        public static bool SetupComplete { get; set; }

        /// <summary>
        /// Whether the engine's loop is running.
        /// </summary>
        public static bool Running { get; set; }

        /// <summary>
        /// Whether the engine was stopped.
        /// The engine cannot recover from this state, and the application must restart.
        /// </summary>
        public static bool Stopped { get; set; }

        #endregion

        /// <summary>
        /// The time you should assume passed between ticks (in milliseconds), for smoothest operation.
        /// Depends on the Configuration's TPS, and should always be constant.
        /// </summary>
        public static float DeltaTime { get; set; }

        /// <summary>
        /// The total time passed since the start of the engine, in milliseconds.
        /// </summary>
        public static float TotalTime { get; set; }

        public static void Setup(Configurator configurator = null)
        {
            // Correct the startup directory to the directory of the executable.
            Directory.SetCurrentDirectory(AppDomain.CurrentDomain.BaseDirectory);

            Process.GetCurrentProcess().PriorityClass = ProcessPriorityClass.High;
            Thread.CurrentThread.Priority = ThreadPriority.Highest;

            // If no config provided - use default.
            Configuration = configurator ?? new Configurator();
            Configuration.Lock();

            Log = Configuration.Logger ?? new DefaultLogger(Configuration.DebugMode);
            Log.Info("Emotion V0.0.0", MessageSource.Engine);
            Log.Info("--------------", MessageSource.Engine);

            // Attach to unhandled exceptions if the debugger is not attached.
            if (!Debugger.IsAttached)
                AppDomain.CurrentDomain.UnhandledException += (e, a) => { SubmitError((Exception) a.ExceptionObject); };

            // Ensure quit is called on exit.
            AppDomain.CurrentDomain.ProcessExit += (e, a) => { Quit(); };

            // --------- System Setup Complete Beyond This Point ---------

            // Mount assets. This doesn't depend on the platform.
            // Even if it does in the future the platform should add sources to it instead of initializing it.
            AssetLoader = LoadDefaultAssetLoader();

            // Create the window, and graphics context.
            Host = Loader.Setup(null, new PlatformConfig
            {
                MinWidth = (int) Configuration.RenderSize.X,
                MinHeight = (int) Configuration.RenderSize.Y,
                Width = (int) Configuration.HostSize.X,
                Height = (int) Configuration.HostSize.Y,
                Title = Configuration.HostTitle
            });
            // Check if the platform initialization was successful.
            if (Host == null)
            {
                Stopped = true;
                Log.Error("Platform couldn't initialize", MessageSource.Engine);
                return;
            }

            if (Host.Window == null)
            {
                Stopped = true;
                SubmitError(new Exception("Platform couldn't create window."));
                return;
            }

            if (Host.Window.Context == null)
            {
                Stopped = true;
                SubmitError(new Exception("Platform couldn't create context."));
                return;
            }

            if (Stopped) return;

            InputManager = new InputManager();

            // Now that the context is created, the renderer can be created.
            GLThread.BindThread();
            Renderer = new Renderer();
            Renderer.Setup();

            SceneManager = new SceneManager();

            // Setup plugins.
            foreach (IPlugin p in Configuration.Plugins)
            {
                p.Initialize();
            }

            if (Stopped) return;
            SetupComplete = true;
        }

        public static void Run()
        {
            // Some sanity checks before starting.
            if (!SetupComplete) return;
            if (Host == null) return;

            // Check if it was stopped before even running.
            // This indicates an error in the setup.
            if (Stopped) return;

            Running = true;
            if (Configuration.DebugMode && Configuration.DebugLoop)
            {
                Log.Info("Starting debug mode loop...", MessageSource.Engine);
                DebugModeLoop();
            }
            else
            {
                Log.Info("Starting loop...", MessageSource.Engine);
                Loop();
            }

            Quit();
        }

        private static void Loop()
        {
            uint desiredStep = Configuration.DesiredStep;
            if (desiredStep == 0) desiredStep = 60;

            // Setup tick time trackers.
            Stopwatch timer = Stopwatch.StartNew();
            double targetTime = 1000f / desiredStep;
            double accumulator = 0f;
            double lastTick = 0f;

            double targetTimeFuzzyLower = 1000f / (desiredStep - 1);
            double targetTimeFuzzyUpper = 1000f / (desiredStep + 1);

            DeltaTime = (float) targetTime;

            while (Running)
            {
#if SIMULATE_LAG
                Task.Delay(Helpers.GenerateRandomNumber(0, 16)).Wait();
#endif
                // For more information on how the timing of the loop works -> https://medium.com/@tglaiel/how-to-make-your-game-run-at-60fps-24c61210fe75
                if (SpecialRenderThread()) break;
                double curTime = timer.ElapsedMilliseconds;
                double deltaTime = curTime - lastTick;
                lastTick = curTime;

                // Snap delta.
                if (Math.Abs(targetTime - deltaTime) <= 1) deltaTime = targetTime;

                // Add to the accumulator.
                accumulator += deltaTime;

                // Update as many times as needed.
                var updates = 0;
                var updated = false;
                while (accumulator > targetTimeFuzzyUpper)
                {
                    RunTick();
                    accumulator -= targetTime;
                    updated = true;

                    if (accumulator < targetTimeFuzzyLower - targetTime) accumulator = 0;
                    updates++;
                    // Max updates are 5 - to prevent large spikes.
                    // This does somewhat break the simulation - but these shouldn't happen expect as a last resort.
                    if (updates <= 5) continue;
                    accumulator = 0;
                    break;
                }

                if (!Host.IsOpen) break;
                if (updated) RunFrame();
            }
        }

        #region Main Loop Variants

        private static void DebugModeLoop()
        {
            uint desiredStep = Configuration.DesiredStep;
            if (desiredStep == 0) desiredStep = 60;

            // Setup tick time trackers.
            Stopwatch timer = Stopwatch.StartNew();
            double targetTime = 1000f / desiredStep;
            double accumulator = 0f;
            double lastTick = 0f;

            double targetTimeFuzzyLower = 1000f / (desiredStep - 1);
            double targetTimeFuzzyUpper = 1000f / (desiredStep + 1);

            DeltaTime = (float) targetTime;

            while (Running)
            {
                // For more information on how the timing of the loop works -> https://medium.com/@tglaiel/how-to-make-your-game-run-at-60fps-24c61210fe75
                if (SpecialRenderThread()) break;

                double curTime = timer.ElapsedMilliseconds;
                double deltaTime = curTime - lastTick;
                lastTick = curTime;

                // Snap delta.
                if (Math.Abs(targetTime - deltaTime) <= 1) deltaTime = targetTime;

                // Add to the accumulator.
                accumulator += deltaTime;

                // Update as many times as needed.
                var updated = false;
                while (accumulator > targetTimeFuzzyUpper)
                {
                    // If the function exists, test whether to run the frame.
                    var runFrame = true;
                    if (Configuration.FrameByFrame != null) runFrame = Configuration.FrameByFrame();
                    if (runFrame)
                    {
                        RunTick();
                        updated = true;
                    }

                    accumulator -= targetTime;
                    if (accumulator < targetTimeFuzzyLower - targetTime) accumulator = 0;
                }

                if (!Host.IsOpen) break;
                if (updated) RunFrame();
            }
        }

        #endregion

        #region Main Loop Parts

        // These functions are here until a scene system is made.
        public static Action<RenderComposer> DebugDrawAction;
        public static Action DebugUpdateAction;

        /// <summary>
        /// Performs special operations that must be performed on the render thread - but are not part of it.
        /// </summary>
        /// <returns>Whether the loop should finish.</returns>
        private static bool SpecialRenderThread()
        {
            // Run the host events, and check whether it is closing.
            bool open = Host.Update();
            if (!open)
            {
                Log?.Info("Host was closed.", MessageSource.Engine);
                return true;
            }

            // Run the GLThread queued commands.
            GLThread.Run();

            return false;
        }

        private static void RunTick()
        {
#if TIMING_DEBUG
            if(_curFrameId == _frameId)
            {
                _curUpdateC++;
            }
            else
            {
                _curUpdateC = 1;
                _curFrameId = _frameId;
            }
#endif
            TotalTime += DeltaTime;

            Renderer.Update();
            InputManager.Update();
            SceneManager.Update();
            DebugUpdateAction?.Invoke();

            // Update plugins.
            foreach (IPlugin p in Configuration.Plugins)
            {
                p.Update();
            }
        }

#if TIMING_DEBUG
        private static int _curFrameId = 0;
        private static int _curUpdateC = 0;
        private static int _frameId = 0;
#endif

        private static void RunFrame()
        {
            RenderComposer composer = Renderer.StartFrame();
            DebugDrawAction?.Invoke(composer);
            SceneManager.Draw(composer);
            Renderer.EndFrame();
            Host.Window.Context.SwapBuffers();
#if TIMING_DEBUG
            _frameId++;
            Console.Write(_curUpdateC);
#endif
        }

        #endregion

        public static void Quit()
        {
            if (Stopped) return;

            Stopped = true;
            Running = false;
            Log?.Info("Quitting...", MessageSource.Engine);

            // This will close the host if it is open.
            // No additional cleanup is needed as the program is assumed to be stopping.
            Host?.Dispose();

            // Flush logs.
            Log?.Dispose();
        }

        #region Helpers

        /// <summary>
        /// Create the default asset loader which loads the engine assembly, the game assembly, the setup calling assembly, and the
        /// file system.
        /// Duplicate assemblies are not loaded.
        /// </summary>
        /// <returns>The default asset loader.</returns>
        private static AssetLoader LoadDefaultAssetLoader()
        {
            var loader = new AssetLoader();

            // Add default embedded sources.
            var sourceAssemblies = new List<Assembly>
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
                loader.AddSource(new EmbeddedAssetSource(assembly, "Assets"));
            }

            // Add file system source.
            loader.AddSource(new FileAssetSource("Assets"));

            return loader;
        }

        #endregion

        #region Error Handling

        /// <summary>
        /// Submit that an error has happened. Handles logging and closing of the engine safely.
        /// </summary>
        /// <param name="ex">The exception connected with the error occured.</param>
        public static void SubmitError(Exception ex)
        {
            if (Debugger.IsAttached) Debugger.Break();

            Log.Error(ex);

            try
            {
                Host?.DisplayMessageBox($"Fatal error occured!\n{ex}");
            }
            catch (Exception e)
            {
                Log.Error($"Couldn't natively display error message {ex} because of {e}", MessageSource.Engine);
            }

            Quit();
        }

        #endregion
    }
}