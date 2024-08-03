#region Using

using System.Collections;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using Emotion.Audio;
using Emotion.Game.Data;
using Emotion.Game.Localization;
using Emotion.Game.Time.Routines;
using Emotion.Graphics.Shading;
using Emotion.IO;
using Emotion.Platform;
using Emotion.Scenography;
using Emotion.Utility;
using Emotion.WIPUpdates.NewUIUpdate;
using Emotion.WIPUpdates.One;

#endregion

namespace Emotion.Common
{
    /// <summary>
    /// Singleton managing the state of the whole engine.
    /// </summary>
    public static class Engine
    {
        /// <summary>
        /// The default engine configuration.
        /// </summary>
        public static Configurator Configuration { get; private set; } = new();

        #region Modules

        /// <summary>
        /// Logs runtime information.
        /// [LightSetup] Module
        /// </summary>
        public static LoggingProvider Log { get; private set; } = new PreSetupLogger();

        /// <summary>
        /// Handles loading assets and storing assets.
        /// [LightSetup] Module
        /// </summary>
        public static AssetLoader AssetLoader { get; private set; }

        /// <summary>
        /// The platform reference.
        /// </summary>
        public static PlatformBase Host { get; private set; }

        /// <summary>
        /// Handles graphics and graphics context state, provides rendering APIs,
        /// and holds objects related to rendering.
        /// </summary>
        public static RenderComposer Renderer { get; private set; }

        /// <summary>
        /// Module which manages loading and unloading of scenes.
        /// </summary>
        public static SceneManager SceneManager { get; private set; }

        /// <summary>
        /// The audio context of the platform. A redirect of Host.Audio.
        /// </summary>
        public static AudioContext Audio { get; private set; }

        /// <summary>
        /// The global coroutine manager, executes coroutines on update ticks.
        /// [Default Module]
        /// </summary>
        public static CoroutineManager CoroutineManager { get; private set; } = new CoroutineManager();

        /// <summary>
        /// Global coroutine manager that executes coroutines
        /// on another thread.
        /// [Default Module]
        /// </summary>
        public static CoroutineManager CoroutineManagerAsync { get; private set; } = new CoroutineManagerSleeping();

        #endregion

        /// <summary>
        /// The status of the engine.
        /// </summary>
        public static EngineStatus Status { get; private set; } = EngineStatus.Initial;

        /// <summary>
        /// The time you should assume passed between ticks (in milliseconds), for smoothest operation.
        /// Depends on the Configuration's TPS, and should always be constant.
        /// </summary>
        public static float DeltaTime { get; set; }

        /// <summary>
        /// The total time passed since the start of the engine, in milliseconds.
        /// </summary>
        public static float TotalTime { get; set; }

        /// <summary>
        /// The index of the current tick.
        /// </summary>
        public static uint TickCount { get; set; }

        /// <summary>
        /// The index of the current frame.
        /// </summary>
        public static uint FrameCount { get; set; }

        static Engine()
        {
            // This is the assembly which called this function. Should be the game.
            Helpers.AddAssociatedAssembly(Assembly.GetCallingAssembly());

            // Is the engine.
            Helpers.AddAssociatedAssembly(Assembly.GetExecutingAssembly());

            // Is game or debugger. Some platforms don't provide this.
            Helpers.AddAssociatedAssembly(Assembly.GetEntryAssembly());
        }

        /// <summary>
        /// Perform light setup - no platform is created. Only the logger and critical systems are initialized.
        /// </summary>
        /// <param name="configurator">Optional engine configuration.</param>
        public static void LightSetup(Configurator configurator = null)
        {
            if (Status >= EngineStatus.LightSetup) return;
            PerfProfiler.ProfilerEventStart("LightSetup", "Loading");

            // Correct the startup directory to the directory of the executable.
            if (RuntimeInformation.OSDescription != "Browser")
            {
                Directory.SetCurrentDirectory(AppDomain.CurrentDomain.BaseDirectory);
                Thread.CurrentThread.Priority = ThreadPriority.AboveNormal;
            }

            CultureInfo.DefaultThreadCurrentCulture = CultureInfo.InvariantCulture;
            CultureInfo.DefaultThreadCurrentUICulture = CultureInfo.InvariantCulture;

            // If no config provided - use default.
            Configuration = configurator ?? new Configurator();

            Log = Configuration.Logger ?? new NetIOAsyncLogger(Configuration.DebugMode);
            Log.Info($"Emotion V{MetaData.Version} [{MetaData.BuildConfig}] {MetaData.GitHash}", MessageSource.Engine);
            string[] args = Configuration.GetExecutionArguments();
            if (args.Length > 1) Log.Info($"Execution Args: {string.Join(", ", args, 1, args.Length - 1)}", MessageSource.Engine);
            Log.Info("--------------", MessageSource.Engine);
            Log.Info($" CPU Cores: {Environment.ProcessorCount}, SIMD: {Vector.IsHardwareAccelerated}, x64 Process: {Environment.Is64BitProcess}", MessageSource.Engine);
            Log.Info($" Runtime: {Environment.Version} {RuntimeInformation.OSDescription} {(Environment.Is64BitOperatingSystem ? "x64" : "x86")}", MessageSource.Engine);
            Log.Info($" Debug Mode: {Configuration.DebugMode}, Debugger Attached: {Debugger.IsAttached}", MessageSource.Engine);
            Log.Info($" Execution Directory: {Environment.CurrentDirectory}", MessageSource.Engine);
            Log.Info($" Entry Assembly: {Assembly.GetEntryAssembly()}", MessageSource.Engine);
            Log.Info("--------------", MessageSource.Engine);

            // Attach engine killer and popup to unhandled exceptions, when the debugger isn't attached.
            // This might be a bit overkill as not all unhandled exceptions are unrecoverable, but let's be pessimistically optimistic for now.
            // Note that async exceptions are considered caught.
            if (!Debugger.IsAttached) AppDomain.CurrentDomain.UnhandledException += (e, a) => { CriticalError((Exception)a.ExceptionObject, true); };

            // Attach logging to those pesky async exceptions, and log all exceptions as they happen (handled as well).
            TaskScheduler.UnobservedTaskException += (s, o) =>
            {
                AggregateException exception = o.Exception;
                Log.Error(exception.InnerException?.ToString() ?? exception.ToString(), MessageSource.StdErr);
            };

            Exception lastBreakException = null;
            AppDomain.CurrentDomain.FirstChanceException += (_, a) =>
            {
                // As an exception bubbles up it will raise this event every time. Dedupe.
                Exception exception = a.Exception;
                if (a.Exception == lastBreakException) return;
                lastBreakException = exception;

                // If log.error throws an exception here, we're screwed.
                // It wouldn't...but what if it does.
                try
                {
                    Log.Error(exception);
                }
                catch (Exception)
                {
                    // ignored
                }

                //if (Debugger.IsAttached) Debugger.Break();
            };

            // Ensure quit is called on exit so that logs are flushed etc.
            AppDomain.CurrentDomain.ProcessExit += (_, __) => { Quit(); };

            // Mount default assets. The platform should add it's own specific sources and stores.
            AssetLoader = AssetLoader.CreateDefaultAssetLoader();
            Status = EngineStatus.LightSetup;

            PerfProfiler.ProfilerEventEnd("LightSetup", "Loading");
        }

        /// <summary>
        /// Perform engine setup.
        /// </summary>
        /// <param name="configurator">An optional engine configuration - will be passed to the light setup.</param>
        public static void Setup(Configurator configurator = null)
        {
            // Call light setup if needed.
            if (Status < EngineStatus.LightSetup) LightSetup(configurator);
            if (Status >= EngineStatus.Setup) return;
            PerfProfiler.ProfilerEventStart("Setup", "Loading");

            // Create the platform, window, audio, and graphics context.
            PerfProfiler.ProfilerEventStart("Platform Creation", "Loading");
            Host = PlatformBase.CreateDetectedPlatform(Configuration);
            if (Host == null)
            {
                CriticalError(new Exception("Platform couldn't initialize."));
                return;
            }

            Audio = Host.Audio;
            if (Status == EngineStatus.Stopped) return; // Errors in host initialization can cause this.
            PerfProfiler.ProfilerEventEnd("Platform Creation", "Loading");

            // Now that the context is created, the renderer can be created.
            Renderer = new RenderComposer();
            Renderer.Setup();

            // Now "game-mode" modules can be created.
            SceneManager = new SceneManager();

            // Load game data.
            GameDataDatabase.Initialize();
            LocalizationEngine.Initialize();

            AssetLoader.LateInit();

            // Setup plugins.
            PerfProfiler.ProfilerEventStart("Plugin Setup", "Loading");
            foreach (IPlugin p in Configuration.Plugins)
            {
                p.Initialize();
            }

            PerfProfiler.ProfilerEventEnd("Plugin Setup", "Loading");

            if (Status == EngineStatus.Stopped) return;
            Status = EngineStatus.Setup;

            PerfProfiler.ProfilerEventEnd("Setup", "Loading");
        }

        /// <summary>
        /// Start running the engine. This call could be blocking depending on the loop.
        /// </summary>
        public static void Run()
        {
            // This will prevent running when a startup error occured or when not setup at all.
            if (Status != EngineStatus.Setup) return;

            // Sanity check.
            if (Host == null) return;

            // todo: these settings and objects might not be needed when using a non-default loop.
            byte targetStep = Configuration.DesiredStep;
            if (targetStep <= 0) targetStep = 60;
            TargetStep(targetStep);

            Status = EngineStatus.Running;
            if (Configuration.LoopFactory == null)
            {
                Log.Info("Using default loop.", MessageSource.Engine);
                Configuration.LoopFactory = DefaultMainLoop;
            }

            Log.Info("Starting loop...", MessageSource.Engine);
            //Configuration.LoopFactory(RunTickIfNeeded, RunFrame);
        }

        /// <summary>
        /// The default main loop.
        /// For more information on how the timing of the loop works ->
        /// https://medium.com/@tglaiel/how-to-make-your-game-run-at-60fps-24c61210fe75
        /// </summary>
        public static void DefaultMainLoop(Action tick, Action frame)
        {
            DetectVSync();

            GCSettings.LatencyMode = GCLatencyMode.SustainedLowLatency;

            while (Status == EngineStatus.Running)
            {
                // Run the host events, and check whether it is closing.
                if (!Host.Update())
                {
                    Log?.Info("Host was closed.", MessageSource.Engine);
                    break;
                }

                tick();
                // After tick, as it might close the host.
                if (!Host.IsOpen) break;
                frame();

                // Tell the GC that now is a good time to collect (between frames).
                //GC.Collect(2, GCCollectionMode.Optimized);
            }

            Quit();
        }

        #region Loop Parts

        private static void DetectVSync()
        {
            // Check if a host is available.
            if (Host == null || !Host.IsOpen) return;

            var timer = new Stopwatch();

            // Detect VSync (yes I know)
            // Because life is unfair and we cannot have nice things, in order to detect
            // whether the GPU lord has enforced VSync on us (not that we can do anything about it)
            // we have to swap buffers around while messing with the settings and see if it
            // changes anything. If it doesn't - it means VSync is either forced on or off.
            const int jitLoops = 5;
            const int loopCount = 15;
            var timings = new double[(loopCount - jitLoops) * 2];
            Host.Context.SwapInterval = 0;
            for (var i = 0; i < timings.Length + jitLoops; i++)
            {
                timer.Restart();
                Host.Context.SwapBuffers();
                // Run a couple of loops to get the JIT warmed up.
                if (i >= jitLoops)
                    timings[i - jitLoops] = timer.ElapsedMilliseconds;
                // Alright, now turn v-sync on.
                if (i == loopCount - 1)
                    Host.Context.SwapInterval = 1;
            }

            double averageTimeOff = timings.Take(loopCount - jitLoops).Sum() / (timings.Length / 2);
            double averageTimeOn = timings.Skip(loopCount - jitLoops).Sum() / (timings.Length / 2);
            Renderer.ForcedVSync = Math.Abs(averageTimeOff - averageTimeOn) <= 1;

            // Restore settings.
            Renderer.ApplySettings();
        }

        private static byte _desiredStep;
        private static Stopwatch _updateTimer;
        private static double _targetTime;
        private static double _accumulator;
        private static double _lastTick;
        private static double _targetTimeFuzzyLower;
        private static double _targetTimeFuzzyUpper;

        // Setup some loop timing settings and objects.
        private static void TargetStep(byte step)
        {
            _desiredStep = step;
            _updateTimer = Stopwatch.StartNew();
            _targetTime = 1000f / _desiredStep;
            _accumulator = 0;
            _lastTick = 0;

            // Fuzzy time tracking, as no system has that kind of perfect timing.
            _targetTimeFuzzyLower = 1000d / (_desiredStep - 1);
            _targetTimeFuzzyUpper = 1000d / (_desiredStep + 1);

            // Keep delta time constant.
            DeltaTime = (float)_targetTime;
        }

        private static bool RunTickIfNeeded()
        {
            double curTime = _updateTimer.ElapsedMilliseconds;
            double deltaTime = curTime - _lastTick;
            _lastTick = curTime;

            // Snap delta.
            if (Math.Abs(_targetTime - deltaTime) <= 1) deltaTime = _targetTime;

            // Add to the accumulator.
            _accumulator += deltaTime;

            // Update as many times as needed.
            byte updates = 0;
            while (_accumulator > _targetTimeFuzzyUpper)
            {
                RunTickInternal();
                _accumulator -= _targetTime;

                if (_accumulator < _targetTimeFuzzyLower - _targetTime) _accumulator = 0;
                updates++;

                // Max updates are 5 - to prevent large spikes.
                // This does somewhat break the simulation - but these shouldn't happen except as a last resort.
                if (updates <= 5) continue;
                _accumulator = 0;
                break;
            }
            return updates > 0;
        }

        private static void RunTickInternal()
        {
            TotalTime += DeltaTime;
            TickCount++;

            PerformanceMetrics.TickStart();

            Host.UpdateInput(); // This refers to the IM input only. Event based input will update on loop tick, not simulation tick.
            CoroutineManager.Update(DeltaTime);
            UI.Update();
            SceneManager.Update();
            EngineEditor.UpdateEditor();
            Renderer.UpdateCamera(); // Done after game logic to apply the new movement.

            PerformanceMetrics.TickEnd();
        }

        private static void RunFrame()
        {
            FrameCount++;
            PerfProfiler.FrameStart();

            // Reset cached bound state, because on some drivers SwapBuffers unbinds all objects.
            // Added in c1b965c3741d6cfb4c7f6174a95860deb9867e5f
            if (Configuration.RendererCompatMode)
            {
                FrameBuffer.Bound = 0;
                VertexBuffer.Bound = 0;
                IndexBuffer.Bound = 0;
                VertexArrayObject.Bound = 0;
                ShaderProgram.Bound = 0;
                for (var i = 0; i < Texture.Bound.Length; i++)
                {
                    Texture.Bound[i] = 0;
                }
            }

            PerfProfiler.FrameEventStart("StartFrame");
            Renderer.StartFrame();
            PerfProfiler.FrameEventEnd("StartFrame");

            if (Configuration.DebugMode)
            {
                Renderer.SetUseViewMatrix(false);
                Renderer.RenderSprite(Vector3.Zero, Renderer.CurrentTarget.Size, Color.CornflowerBlue);
                Renderer.ClearDepth();
                Renderer.SetUseViewMatrix(true);
            }

            PerfProfiler.FrameEventStart("Scene.Draw");
            SceneManager.Draw(Renderer);
            PerfProfiler.FrameEventEnd("Scene.Draw");

            PerfProfiler.FrameEventStart("Editor.Draw");
            EngineEditor.RenderEditor(Renderer);
            PerfProfiler.FrameEventEnd("Editor.Draw");

            PerfProfiler.FrameEventStart("Render UI");
            Renderer.SetUseViewMatrix(false);
            Renderer.SetDepthTest(false);
            Renderer.ClearDepth();
            UI.Render(Renderer);
            PerfProfiler.FrameEventEnd("Render UI");

            PerfProfiler.FrameEventStart("EndFrame");
            Renderer.EndFrame();
            PerfProfiler.FrameEventEnd("EndFrame");

            PerfProfiler.FrameEventStart("BufferSwap");
            Host.Context.SwapBuffers();
            PerfProfiler.FrameEventEnd("BufferSwap");

            PerfProfiler.FrameEnd();

            PerformanceMetrics.RegisterFrame();
        }

        #endregion

        /// <summary>
        /// Stop running the engine.
        /// This should unblock the "Run" call if blocking and make modules exit safely.
        /// </summary>
        public static void Quit()
        {
            if (Status == EngineStatus.Stopped) return;

            Status = EngineStatus.Stopped;
            Log?.Info("Quitting...", MessageSource.Engine);

            // This will close the host if it is open.
            // No additional cleanup is needed as the program is assumed to be stopping.
            Host?.Close();

            // Flush logs.
            Log?.Dispose();

            // Dispose of plugins.
            foreach (IPlugin p in Configuration.Plugins)
            {
                p.Dispose();
            }
        }

        /// <summary>
        /// Submit that an error has happened. Handles logging and closing of the engine safely.
        /// Sometimes the exception is engine generated. Check the InnerException property in these cases.
        /// </summary>
        public static void CriticalError(Exception ex, bool dontLog = false)
        {
            if (Debugger.IsAttached) Debugger.Break();

            if (!dontLog) Log.Error(ex);
            if (!Configuration.NoErrorPopup) Host?.DisplayMessageBox($"Fatal error occured!\n{ex}");
            Quit();
        }

        #region ONE

        public static UISystem UI;

        public static int CurrentUpdateFrame;
        public static int CurrentRenderFrame;

        public static ManualResetEventSlim RenderPush = new ManualResetEventSlim(false);

        private static Thread _asyncRoutineThread;
        private static Thread _updateThread;

        public static void Start(Configurator config, Func<IEnumerator> entryPointAsyncRoutine)
        {
            Setup(config);

            UI = new UISystem();
            EngineEditor.Attach();

            // Sanity check.
            if (Host == null) return;

            // todo: these settings and objects might not be needed when using a non-default loop.
            byte targetStep = Configuration.DesiredStep;
            if (targetStep <= 0) targetStep = 60;
            TargetStep(targetStep);

            Status = EngineStatus.Running;

            Thread asyncRoutineThread = new Thread(() =>
            {
                if (Host?.NamedThreads ?? false) Thread.CurrentThread.Name ??= "Async Routine Thread";

                var lastUpdate = DateTime.Now;
                while (Status == EngineStatus.Running)
                {
                    int timePassed = DateTime.Now.Subtract(lastUpdate).Milliseconds;
                    CoroutineManagerAsync.Update(timePassed);
                    if (timePassed != 0) lastUpdate = DateTime.Now;
                    Thread.Yield();
                }
            });
            asyncRoutineThread.IsBackground = true;
            asyncRoutineThread.Start();
            _asyncRoutineThread = asyncRoutineThread;
            CoroutineManagerAsync.StartCoroutine(entryPointAsyncRoutine());

            Thread updateThread = new Thread(UpdateThread);
            updateThread.IsBackground = true;
            updateThread.Start();
            _updateThread = updateThread;

            if (Configuration.LoopFactory == null)
            {
                Log.Info("Using default loop.", MessageSource.Engine);
                Configuration.LoopFactory = NewDefaultMainLoopFactory;
            }

            Log.Info("Starting loop...", MessageSource.Engine);
            Configuration.LoopFactory(EmptyFunc, NewFrameRun);
        }

        private static void EmptyFunc()
        {

        }

        private static void NewDefaultMainLoopFactory(Action _, Action frame)
        {
            // Main loop
            DetectVSync();
            GCSettings.LatencyMode = GCLatencyMode.SustainedLowLatency;

            while (Status == EngineStatus.Running)
            {
                // Run the host events, and check whether it is closing.
                if (!Host.Update())
                {
                    Log?.Info("Host was closed.", MessageSource.Engine);
                    break;
                }

                // After tick, as it might close the host.
                if (!Host.IsOpen) break;

                frame();
            }

            Quit();
        }

        private static bool _multihreaded = false;

        private static void NewFrameRun()
        {
            if (CurrentRenderFrame > CurrentUpdateFrame) return;

            if (!_multihreaded)
            {
                //Single threaded mode!
                RunTickIfNeeded();
                CurrentUpdateFrame++;
            }

            RunFrame();
            CurrentRenderFrame++;
            RenderPush.Set();
        }

        private static void UpdateThread()
        {
            if (!_multihreaded) return;
            if (Host?.NamedThreads ?? false) Thread.CurrentThread.Name ??= "Update Thread";

            int updateFramesAhead = 1;
            while (true)
            {
                if (CurrentUpdateFrame >= CurrentRenderFrame + updateFramesAhead)
                {
                    RenderPush.Wait();
                    RenderPush.Reset();
                }

                bool tickRan = RunTickIfNeeded();
                if (tickRan)
                    CurrentUpdateFrame++;
            }
        }

        #endregion
    }
}
