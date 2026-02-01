#nullable enable

#region Using

using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Runtime;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Emotion.Core.Platform;
using Emotion.Core.Systems.Audio;
using Emotion.Core.Systems.IO;
using Emotion.Core.Systems.JobSystem;
using Emotion.Core.Systems.Localization;
using Emotion.Core.Systems.Scenography;
using Emotion.Core.Utility.Coroutines;
using Emotion.Core.Utility.Profiling;
using Emotion.Core.Utility.Threading;
using Emotion.Editor;
using Emotion.Game.Systems.UI.New;
using Emotion.Graphics.Shading;
using Emotion.Network;
using Emotion.Standard.Reflector;
using OpenGL;

#endregion

namespace Emotion.Core;

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
    /// [LightSetup Module]
    /// </summary>
    public static LoggingProvider Log { get; private set; } = new PreSetupLogger();

    /// <summary>
    /// Provides an interface for user input.
    /// [Default Module]
    /// </summary>
    public static InputManager Input { get; private set; } = new InputManager();

    /// <summary>
    /// Handles loading assets and storing assets.
    /// [LightSetup Module]
    /// </summary>
    public static AssetLoader AssetLoader { get; private set; }

    /// <summary>
    /// The platform reference.
    /// [Setup Module]
    /// </summary>
    public static PlatformBase Host { get; private set; }

    /// <summary>
    /// Handles graphics and graphics context state, provides rendering APIs,
    /// and holds objects related to rendering.
    /// [Host Module]
    /// </summary>
    public static Renderer Renderer { get; private set; }

    /// <summary>
    /// The audio context of the platform. A redirect of Host.Audio.
    /// [Host Module]
    /// </summary>
    public static AudioContext Audio { get; private set; }

    /// <summary>
    /// Module which manages loading and unloading of scenes.
    /// [Setup Module]
    /// </summary>
    public static SceneManager SceneManager { get; private set; }

    /// <summary>
    /// The global coroutine manager, executes coroutines on update ticks.
    /// [Default Module]
    /// </summary>
    public static CoroutineManager CoroutineManager { get; private set; } = new CoroutineManager();

    /// <summary>
    /// The global coroutine manager that executes coroutines on update ticks and is considered in "game time".
    /// In singleplayer games these routines are paused when the game is paused, in multiplayer this is used for time sync, etc.
    /// [Default Module]
    /// </summary>
    public static CoroutineManagerGameTime CoroutineManagerGameTime { get; private set; } = new CoroutineManagerGameTime();

    /// <summary>
    /// The job system allows for running tasks (as coroutines) on other threads.
    /// [Setup Module]
    /// </summary>
    public static AsyncJobManager Jobs { get; private set; }

    /// <summary>
    /// Global UI system.
    /// [Setup Module]
    /// </summary>
    public static UISystem UI { get; private set; }

    /// <summary>
    /// Manages multiplayer communication.
    /// [Setup Module]
    /// </summary>
    public static MultiplayerSystem Multiplayer { get; private set; }

    #endregion

    /// <summary>
    /// The status of the engine.
    /// </summary>
    public static EngineState Status { get; private set; } = EngineState.Initial;

    /// <summary>
    /// The time you should assume passed between ticks (in milliseconds), for smoothest operation.
    /// Depends on the Configuration's TPS, and should always be constant.
    /// </summary>
    public static float DeltaTime { get; set; }

    /// <summary>
    /// The current game time. The meaning of this is a bit dependant on the game, but is
    /// the time within the CoroutineManagerGameTime.
    /// </summary>
    public static float CurrentGameTime => CoroutineManagerGameTime.Time;

    public static float CurrentRealTime => _realTimeTracker.ElapsedMilliseconds;
    private static Stopwatch _realTimeTracker = new Stopwatch();

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
    private static void LightSetup(Configurator? configurator = null)
    {
        if (Status >= EngineState.LightSetup) return;
        PerfProfiler.ProfilerEventStart("LightSetup", "Loading");

        // If no config provided - use default.
        Configuration = configurator ?? new Configurator();

        // Setup runtime settings and error handling.
        CultureInfo.DefaultThreadCurrentCulture = CultureInfo.InvariantCulture;
        CultureInfo.DefaultThreadCurrentUICulture = CultureInfo.InvariantCulture;

        if (RuntimeInformation.OSDescription != "Browser")
            Thread.CurrentThread.Priority = ThreadPriority.AboveNormal;

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

        Exception? lastBreakException = null;
        AppDomain.CurrentDomain.FirstChanceException += (_, a) =>
        {
            // As an exception bubbles up it will raise this event every time. Dedupe.
            Exception exception = a.Exception;
            if (a.Exception == lastBreakException) return;
            lastBreakException = exception;

            // If log.error throws an exception here, we're screwed.
            // It wouldn't...but what if it does.
            if (!_logExceptions) return;
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

        // Correct the startup directory to the directory of the executable.
        // We want to do this as soon as possible to make sure the correct one is logged,
        // and we want to do it before the logger so that logs are in the correct folder.
        AssetLoader.SetupGameDirectory();

        Log = Configuration.Logger ?? new NetIOAsyncLogger(Configuration.DebugMode);
        Log.Info($"Emotion V{MetaData.Version} [{MetaData.BuildConfig}] {MetaData.GitHash}", MessageSource.Engine);
        string[] args = Configuration.GetExecutionArguments();
        if (args.Length > 1) Log.Info($"Execution Args: {string.Join(", ", args, 1, args.Length - 1)}", MessageSource.Engine);
        Log.Info("--------------", MessageSource.Engine);
        Log.Info($" CPU Cores: {Environment.ProcessorCount}, SIMD: {Vector.IsHardwareAccelerated}, x64 Process: {Environment.Is64BitProcess}", MessageSource.Engine);
        Log.Info($" Runtime: {Environment.Version} {RuntimeInformation.OSDescription} {(Environment.Is64BitOperatingSystem ? "x64" : "x86")}", MessageSource.Engine);
        Log.Info($" Debug Mode: {Configuration.DebugMode}, Debugger Attached: {Debugger.IsAttached}", MessageSource.Engine);
        Log.Info($" Execution Directory: {Environment.CurrentDirectory}", MessageSource.Engine);
        Log.Info($" Game Directory: {AssetLoader.GameFolder}", MessageSource.Engine);
        Log.Info($" Entry Assembly: {Assembly.GetEntryAssembly()}", MessageSource.Engine);
        Log.Info("--------------", MessageSource.Engine);

        // Mount embedded engine assets.
        // The host will add its own specific sources and stores later.
        AssetLoader = AssetLoader.CreateDefaultAssetLoader();

        Status = EngineState.LightSetup;

        PerfProfiler.ProfilerEventEnd("LightSetup", "Loading");
    }

    /// <summary>
    /// Start the engine with the specified config, calling the passing in coroutine as soon as
    /// the engine is setup async on another thread.
    /// </summary>
    public static void Start(Configurator config, Func<IEnumerator> entryPointAsyncRoutine)
    {
        Setup(config);
        Run(entryPointAsyncRoutine);
    }

    /// <summary>
    /// Start the engine without the main loop, but initializing its internal systems.
    /// Used for testing.
    /// </summary>
    public static void StartHeadless(Configurator config)
    {
        Setup(config);
    }

    /// <summary>
    /// Perform engine setup.
    /// </summary>
    /// <param name="configurator">An optional engine configuration - will be passed to the light setup.</param>
    private static void Setup(Configurator? configurator = null)
    {
        // Call light setup if needed.
        if (Status < EngineState.LightSetup) LightSetup(configurator);
        if (Status >= EngineState.Setup) return;
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
        if (Status == EngineState.Stopped) return; // Errors in host initialization can cause this.
        PerfProfiler.ProfilerEventEnd("Platform Creation", "Loading");

        // Setup reflector for serialization/deserialization.
        // Doesn't really depend on anything, but good to have the host setup first.
        ReflectorEngine.Init();

        // Async jobs - dependant on host.
        Jobs = new AsyncJobManager();
        Jobs.Init();

        // Renderer depends on this in order to load base assets.
        // Though the system assets should be accessible before late init, so its fine?
        AssetLoader.LateInit();

        // Now that the context is created, the renderer can be created.
        Renderer = new Renderer();
        Renderer.Initialize();

        // Now "game-mode" modules can be created.
        SceneManager = new SceneManager();
        UI = new UISystem();
        Multiplayer = new MultiplayerSystem();

        // Load game data.
        GameDatabase.Initialize();
        LocalizationEngine.Initialize();

        // Debuggers
        EngineEditor.Initialize();

        // Setup plugins.
        PerfProfiler.ProfilerEventStart("Plugin Setup", "Loading");
        foreach (IPlugin p in Configuration.Plugins)
        {
            p.Initialize();
        }

        PerfProfiler.ProfilerEventEnd("Plugin Setup", "Loading");

        if (Status == EngineState.Stopped) return;
        Status = EngineState.Setup;

        PerfProfiler.ProfilerEventEnd("Setup", "Loading");
    }

    /// <summary>
    /// Start running the engine. This call could be blocking depending on the loop.
    /// </summary>
    private static void Run(Func<IEnumerator> entryPointAsyncRoutine)
    {
        // This will prevent running when a startup error occured or when not setup at all.
        if (Status != EngineState.Setup) return;

        // Sanity check.
        if (Host == null) return;

        Status = EngineState.Running;

        // Start running real time.
        _realTimeTracker.Start();

        // Start the entry async entry point as a job, and start the loop.
        Jobs.Add(entryPointAsyncRoutine());

        if (Configuration.LoopFactory == null)
        {
            Log.Info("Using default loop.", MessageSource.Engine);
            Configuration.LoopFactory = DefaultMainLoop;
        }

        Log.Info("Starting loop...", MessageSource.Engine);
        CoroutineManager.StartCoroutine(MainLoopRoutine());
        Configuration.LoopFactory(MainLoopTick, RunFrame);
    }

    /// <summary>
    /// The default main loop.
    /// For more information on how the timing of the loop works ->
    /// https://medium.com/@tglaiel/how-to-make-your-game-run-at-60fps-24c61210fe75
    /// </summary>
    private static void DefaultMainLoop(Action tick, Action frame)
    {
        DetectVSync();

        GCSettings.LatencyMode = GCLatencyMode.SustainedLowLatency;

        while (Status == EngineState.Running)
        {
            // Run the host events, and check whether it is closing.
            if (!Host.Update())
            {
                Log?.Info("Host was closed.", MessageSource.Engine);
                break;
            }

            // Update modules that are outside the simulation tick.
            AssetLoader.Update();

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

    private static long _lastTickAt;

    private static void MainLoopTick()
    {
        long curTime = _realTimeTracker.ElapsedMilliseconds;
        long deltaTime = curTime - _lastTickAt;
        _lastTickAt = curTime;

        CoroutineManager.Update(deltaTime);
    }

    private static IEnumerator MainLoopRoutine()
    {
        int desiredStepMs = (int)Math.Floor(1000f / Configuration.DesiredTicksPerSecond);
        desiredStepMs = Math.Max(1, desiredStepMs);
        DeltaTime = desiredStepMs;

        Coroutine gameTimeRoutine = Engine.CoroutineManagerGameTime.StartCoroutine(SimulationTickRoutine(desiredStepMs));

        while (Engine.Status == EngineState.Running)
        {
            // Resetting game time will stop all routines...
            // This should probably be managed by the SceneManager by registering different routines for different scenes?
            if (gameTimeRoutine.Stopped)
            {
                gameTimeRoutine = Engine.CoroutineManagerGameTime.StartCoroutine(SimulationTickRoutine(desiredStepMs));
            }

            RunMainLoopTick();
            yield return desiredStepMs;
        }
    }

    private static IEnumerator SimulationTickRoutine(int desiredStepMs)
    {
        while (Engine.Status == EngineState.Running)
        {
            SceneManager.Update(desiredStepMs);
            yield return desiredStepMs;
        }
    }

    private static void RunMainLoopTick()
    {
        TotalTime += DeltaTime;
        TickCount++;

        PerformanceMetrics.TickStart();

        Input.Update();
        Host.UpdateInput(); // This refers to the legacy IM input only, event based input is on Input.Update()

#if DEBUG || AUTOBUILD
        if (Configuration.UpdateUIAutomatically)
            UI.UpdateSystem();
#else
        UI.UpdateSystem();
#endif
        EngineEditor.UpdateEditor();

        Multiplayer.Update();
        CoroutineManagerGameTime.Update(DeltaTime); // This will run SceneManagerTick

        // Updates camera input and target tracking
        // todo: split these, input should be probably outside simulation for least latency,
        // and the target tracking should be in the renderer probably.
        Renderer.UpdateCamera();

        PerformanceMetrics.TickEnd();
    }

    private static void RunFrame()
    {
        if (!Renderer.ReadyToRender)
        {
            GLThread.Run();
            Host.Context.SwapBuffers();
            return;
        }

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
            foreach (KeyValuePair<TextureTarget, uint[]> boundPair in TextureObjectBase.Bound)
            {
                uint[] bound = boundPair.Value;
                for (int i = 0; i < bound.Length; i++)
                {
                    bound[i] = 0;
                }
            }
        }

        PerfProfiler.FrameEventStart("StartFrame");
        Renderer.StartFrame();
        PerfProfiler.FrameEventEnd("StartFrame");

        if (Configuration.DebugMode && EngineEditor.IsOpen)
        {
            Renderer.SetUseViewMatrix(false);
            Renderer.RenderSprite(Vector3.Zero, Renderer.CurrentTarget.Size, Color.CornflowerBlue);
            Renderer.ClearDepth();
            Renderer.SetUseViewMatrix(true);
        }

        EngineEditor.OnSceneRenderStart(Renderer);
        PerfProfiler.FrameEventStart("Scene.Draw");
        SceneManager.Draw(Renderer);
        PerfProfiler.FrameEventEnd("Scene.Draw");
        EngineEditor.OnSceneRenderEnd(Renderer);

        PerfProfiler.FrameEventStart("Editor.Draw");
        EngineEditor.RenderEditor(Renderer);
        PerfProfiler.FrameEventEnd("Editor.Draw");

        PerfProfiler.FrameEventStart("Render UI");
        Renderer.SetUseViewMatrix(false);
        Renderer.SetDepthTest(false);
        Renderer.ClearDepth();
#if DEBUG || AUTOBUILD
        if (Configuration.UpdateUIAutomatically)
            UI.RenderSystem(Renderer);
#else
        UI.RenderSystem(Renderer);
#endif
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
        if (Status == EngineState.Stopped) return;

        Status = EngineState.Stopped;
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

    private static bool _logExceptions = true;

    /// <summary>
    /// Whether to temporarily suppress logging of uncaught exceptions.
    /// Don't forget to call it with the inverse value when you're done!
    /// </summary>
    public static void SuppressLogExceptions(bool suppress)
    {
        _logExceptions = !suppress;
    }
}
