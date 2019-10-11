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
                Height = (int) Configuration.HostSize.Y
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

            // Setup tick time trackers.
            _timer = Stopwatch.StartNew();
            _fixedStep = Configuration.DesiredTPS > 0;
            _targetTime = _fixedStep ? 1f / Configuration.DesiredTPS : 0;
            _accumulator = 0f;
            _lastTick = 0f;

            Running = true;
            if (Configuration.DebugMode && Configuration.DebugLoop)
            {
                Log.Info("Starting debug mode loop...", MessageSource.Engine);
                DebugModeLoop();
            }
            else
            {
                Host.Window.Context.SetSwapInterval(1);

                if (!Configuration.UpdateThreadIsRenderThread)
                {
                    Log.Info("Starting multi-threaded loop...", MessageSource.Engine);

                    // Start the update thread.
                    var updateThread = new Thread(EngineUpdateLoop);
                    if (updateThread.Name == null) updateThread.Name = "Update Thread";
                    if (Thread.CurrentThread.Name == null) Thread.CurrentThread.Name = "Render Thread";
                    updateThread.Start();

                    EngineRenderLoop();
                }
                else
                {
                    Log.Info("Starting single-threaded loop...", MessageSource.Engine);

                    EngineLoopSingleThread();
                }
            }

            Quit();
        }

        #region Main Loop Variants

        private static void EngineLoopSingleThread()
        {
            while (Running)
            {
                if (SpecialRenderThread()) break;

                bool updated = RunTick();

                // It is possible for the update to close the host - which will error the draw as the context will be gone.
                if (!Host.IsOpen) break;

                if (updated) RunFrame();
            }
        }

        private static void EngineRenderLoop()
        {
            while (Running)
            {
                if (SpecialRenderThread()) break;

                if (_unfocusedHostEvent != null)
                {
                    _unfocusedHostEvent?.Set();
                    continue;
                }

                RunFrame();
            }
        }

        private static ManualResetEvent _unfocusedHostEvent; // Used to block the update loop when the render loop is not running.

        private static void EngineUpdateLoop()
        {
            while (Running)
            {
                if (!Host.Window.Focused)
                {
                    _unfocusedHostEvent = new ManualResetEvent(false);
                    _unfocusedHostEvent.WaitOne();
                    _unfocusedHostEvent = null;
                    continue;
                }

                RunTick();

                // It is possible for the update to close the host.
                if (!Host.IsOpen) break;
            }
        }

        private static void DebugModeLoop()
        {
            if (Thread.CurrentThread.Name == null) Thread.CurrentThread.Name = "Render Thread";

            DeltaTime = 1000f / Configuration.DesiredTPS;

            while (Running)
            {
                if (SpecialRenderThread()) break;

                // If the function exists, test whether to run the frame.
                var runFrame = true;
                if (Configuration.FrameByFrame != null) runFrame = Configuration.FrameByFrame();
                if (!runFrame) continue;

                RunTick();
                RunFrame();
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

        // Time trackers for run tick.
        private static Stopwatch _timer;
        private static bool _fixedStep;
        private static double _targetTime;
        private static double _accumulator;
        private static double _lastTick;

        private static bool RunTick()
        {
            // For more information on how the timing of the loop works -> https://medium.com/@tglaiel/how-to-make-your-game-run-at-60fps-24c61210fe75
            double curTime = _timer.ElapsedMilliseconds;
            double deltaTime = (curTime - _lastTick) / 1000f;
            _lastTick = curTime;

            // Snap delta.
            if (Math.Abs(_targetTime - deltaTime) <= 0.001) deltaTime = _targetTime;

            // Add to the accumulator.
            _accumulator += deltaTime;

            // Check if reached max delta.
            if (_accumulator > _targetTime * 5f) _accumulator = _targetTime * 5f;

            var updated = false;
            while (_accumulator >= _targetTime)
            {
                // Assign frame time trackers.
                DeltaTime = (float) (_fixedStep ? _targetTime * 1000f : (deltaTime * 1000f));
                TotalTime += DeltaTime;

                RunTickInternal();
                updated = true;
                _accumulator -= _targetTime;
                if (_accumulator < 0f) _accumulator = 0f;

                if (_fixedStep) continue;
                _accumulator = 0f;
                break;
            }

            return updated;
        }

        private static void RunTickInternal()
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
            InputManager.Update();
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