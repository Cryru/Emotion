#region Using

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using Emotion.Audio;
using Emotion.Common.Threading;
using Emotion.Game.Time.Routines;
using Emotion.Graphics;
using Emotion.Graphics.Objects;
using Emotion.Graphics.Shading;
using Emotion.IO;
using Emotion.Platform;
using Emotion.Scenography;
using Emotion.Standard.Logging;
using Emotion.Utility;

#endregion

namespace Emotion.Common
{
    public static class Engine
    {
        /// <summary>
        /// The default engine configuration.
        /// </summary>
        public static Configurator Configuration { get; private set; } = new();

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
        public static RenderComposer Renderer { get; private set; }

        /// <summary>
        /// The legacy input manager. Is just a redirect of Host.
        /// todo: Should probably deprecate this at some point.
        /// </summary>
        [Obsolete("Use Engine.Host")]
        public static PlatformBase InputManager
        {
            get => Host;
        }

        /// <summary>
        /// Module which manages loading and unloading of scenes.
        /// </summary>
        public static SceneManager SceneManager { get; private set; }

        /// <summary>
        /// The audio context of the platform. A redirect of Host.Audio.
        /// </summary>
        public static AudioContext Audio { get; private set; }

        /// <summary>
        /// The global coroutine manager.
        /// </summary>
        public static CoroutineManager CoroutineManager { get; private set; } = new CoroutineManager();

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

#if DEBUG

        public static event EventHandler DebugOnUpdateStart;
        public static event EventHandler DebugOnUpdateEnd;
        public static event EventHandler DebugOnFrameStart;
        public static event EventHandler DebugOnFrameEnd;

#endif

        static Engine()
        {
            Helpers.AssociatedAssemblies = new List<Assembly>
            {
                // This is the assembly which called this function. Should be the game. Moving this to Setup/LightSetup will invalidate this assembly.
                Assembly.GetCallingAssembly(),
                // Is the engine.
                Assembly.GetExecutingAssembly(),
                // Is game or debugger. Some platforms don't provide this.
                Assembly.GetEntryAssembly()
            }.Distinct().Where(x => x != null).ToArray();
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

            Log = Configuration.Logger ?? new DefaultLogger(Configuration.DebugMode);
            Log.Info($"Emotion V{MetaData.Version} [{MetaData.BuildConfig}] {MetaData.GitHash}", MessageSource.Engine);
            Log.Info("--------------", MessageSource.Engine);
            Log.Info($" CPU Cores: {Environment.ProcessorCount}, SIMD: {Vector.IsHardwareAccelerated}, x64 Process: {Environment.Is64BitProcess}", MessageSource.Engine);
            Log.Info($" Runtime: {Environment.Version} {RuntimeInformation.OSDescription} {(Environment.Is64BitOperatingSystem ? "x64" : "x86")}", MessageSource.Engine);
            Log.Info($" Debug Mode: {Configuration.DebugMode}, Debugger Attached: {Debugger.IsAttached}", MessageSource.Engine);
            Log.Info($" Execution Directory: {Environment.CurrentDirectory}", MessageSource.Engine);
            Log.Info($" Entry Assembly: {Assembly.GetEntryAssembly()}", MessageSource.Engine);

            // Attach to unhandled exceptions if the debugger is not attached.
            if (!Debugger.IsAttached)
                AppDomain.CurrentDomain.UnhandledException += (e, a) => { CriticalError((Exception)a.ExceptionObject); };
            TaskScheduler.UnobservedTaskException += (s, o) =>
            {
                AggregateException exception = o.Exception;
                Log.Error(exception.InnerException?.ToString() ?? exception.ToString(), MessageSource.StdErr);
            };
            AppDomain.CurrentDomain.FirstChanceException += (e, a) => { Log.Error(a.Exception); };

            // Ensure quit is called on exit.
            AppDomain.CurrentDomain.ProcessExit += (e, a) => { Quit(); };

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

            Audio = new AudioContext(Host.Audio);
            if (Status == EngineStatus.Stopped) return; // Errors in host initialization can cause this.
            PerfProfiler.ProfilerEventEnd("Platform Creation", "Loading");

            // Now that the context is created, the renderer can be created.
            GLThread.BindThread();
            Renderer = new RenderComposer();
            Renderer.Setup();

            // Now "game-mode" modules can be created.
            SceneManager = new SceneManager();

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
            Configuration.LoopFactory(RunTickIfNeeded, RunFrame);
        }

        /// <summary>
        /// The default main loop.
        /// For more information on how the timing of the loop works ->
        /// https://medium.com/@tglaiel/how-to-make-your-game-run-at-60fps-24c61210fe75
        /// </summary>
        public static void DefaultMainLoop(Action tick, Action frame)
        {
            DetectVSync();

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

        private static void RunTickIfNeeded()
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
        }

        private static void RunTickInternal()
        {
#if DEBUG
            DebugOnUpdateStart?.Invoke(null, EventArgs.Empty);
#endif

            TotalTime += DeltaTime;

            Host.UpdateInput(); // This refers to the IM input only. Event based input will update on loop tick, not simulation tick.
            CoroutineManager.Update();
            SceneManager.Update();

#if DEBUG
            DebugOnUpdateEnd?.Invoke(null, EventArgs.Empty);
#endif
        }

        private static void RunFrame()
        {
#if DEBUG
            DebugOnFrameStart?.Invoke(null, EventArgs.Empty);
#endif

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

            // Run the GLThread queued commands.
            PerfProfiler.FrameEventStart("GLThread.Run");
            GLThread.Run();
            PerfProfiler.FrameEventEnd("GLThread.Run");

            PerfProfiler.FrameEventStart("StartFrame");
            Renderer.StartFrame();
            PerfProfiler.FrameEventEnd("StartFrame");

            PerfProfiler.FrameEventStart("Scene.Draw");
            SceneManager.Draw(Renderer);
            PerfProfiler.FrameEventEnd("Scene.Draw");

            PerfProfiler.FrameEventStart("EndFrame");
            Renderer.EndFrame();
            PerfProfiler.FrameEventEnd("EndFrame");

#if DEBUG
            DebugOnFrameEnd?.Invoke(null, EventArgs.Empty);
#endif

            PerfProfiler.FrameEventStart("BufferSwap");
            Host.Context.SwapBuffers();
            PerfProfiler.FrameEventEnd("BufferSwap");

            PerfProfiler.FrameEnd();
        }

        #endregion

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
        /// <param name="ex">The exception connected with the error occured.</param>
        public static void CriticalError(Exception ex)
        {
            if (Debugger.IsAttached) Debugger.Break();

            Log.Error(ex);
            if (!Configuration.NoErrorPopup) Host?.DisplayMessageBox($"Fatal error occured!\n{ex}");
            Quit();
        }
    }
}