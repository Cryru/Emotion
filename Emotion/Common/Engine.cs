#region Using

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;
using Emotion.Common.Threading;
using Emotion.Game.Time.Routines;
using Emotion.Graphics;
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
        public static Configurator Configuration { get; private set; } = new Configurator();

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
        /// The legacy input manager.
        /// </summary>
        public static IInputManager InputManager { get; private set; }

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

        static Engine()
        {
            Helpers.AssociatedAssemblies = new List<Assembly>
            {
                Assembly.GetCallingAssembly(), // This is the assembly which called this function. Can be the game or the engine.
                Assembly.GetExecutingAssembly(), // Is the engine.
                Assembly.GetEntryAssembly() // Is game or debugger.
            }.Distinct().Where(x => x != null).ToArray();
        }

        /// <summary>
        /// Perform light setup - no platform is created. Only the logger and critical systems are initialized.
        /// </summary>
        /// <param name="configurator">Optional engine configuration.</param>
        public static void LightSetup(Configurator configurator = null)
        {
            if (Status >= EngineStatus.LightSetup) return;

            // Correct the startup directory to the directory of the executable.
            Directory.SetCurrentDirectory(AppDomain.CurrentDomain.BaseDirectory);

            Process.GetCurrentProcess().PriorityClass = ProcessPriorityClass.High;
            Thread.CurrentThread.Priority = ThreadPriority.Highest;

            // If no config provided - use default.
            Configuration = configurator ?? new Configurator();

            Log = Configuration.Logger ?? new DefaultLogger(Configuration.DebugMode);
            Log.Info("Emotion V0.0.0", MessageSource.Engine);
            Log.Info("--------------", MessageSource.Engine);
            Log.Info($" CPU Cores: {Environment.ProcessorCount}, SIMD: {Vector.IsHardwareAccelerated}, x64 Process: {Environment.Is64BitProcess}", MessageSource.Engine);
            Log.Info($" Runtime: {Environment.Version} {RuntimeInformation.OSDescription} {(Environment.Is64BitOperatingSystem ? "x64" : "x86")}", MessageSource.Engine);
            Log.Info($" Debug Mode: {Configuration.DebugMode}, Debugger Attached: {Debugger.IsAttached}", MessageSource.Engine);
            Log.Info($" Execution Directory: {Environment.CurrentDirectory}", MessageSource.Engine);
            Log.Info($" Entry Assembly: {Assembly.GetEntryAssembly()}", MessageSource.Engine);

            // Attach to unhandled exceptions if the debugger is not attached.
            if (!Debugger.IsAttached)
                AppDomain.CurrentDomain.UnhandledException += (e, a) => { SubmitError((Exception) a.ExceptionObject); };

            // Ensure quit is called on exit.
            AppDomain.CurrentDomain.ProcessExit += (e, a) => { Quit(); };

            Status = EngineStatus.LightSetup;
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

            // Mount default assets. The platform should add it's own specific sources and stores.
            AssetLoader = LoadDefaultAssetLoader();

            // Create the platform, window, audio, and graphics context.
            Host = PlatformBase.GetInstanceOfDetected(configurator);
            if (Host == null)
            {
                SubmitError(new Exception("Platform couldn't initialize."));
                return;
            }

            InputManager = Host;
            Audio = Host.Audio;

            // Errors in host initialization can cause this.
            if (Status == EngineStatus.Stopped) return;

            // Now that the context is created, the renderer can be created.
            GLThread.BindThread();
            Renderer = new RenderComposer();
            Renderer.Setup();

            // Now "game-mode" modules can be created.
            SceneManager = new SceneManager();

            // Setup plugins.
            foreach (IPlugin p in Configuration.Plugins)
            {
                p.Initialize();
            }

            if (Status == EngineStatus.Stopped) return;
            Status = EngineStatus.Setup;
        }

        public static void Run()
        {
            // Some sanity checks before starting.

            // This will prevent running when stopped (as in a startup error) or not setup.
            if (Status != EngineStatus.Setup) return;

            // Just to make sure.
            if (Host == null) return;

            Status = EngineStatus.Running;
            if (Configuration.LoopFactory == null)
            {
                Log.Info("Using default loop.", MessageSource.Engine);
                Configuration.LoopFactory = DefaultMainLoop;
            }

            Log.Info("Starting loop...", MessageSource.Engine);
            Configuration.LoopFactory(RunTick, RunFrame);
        }

        /// <summary>
        /// The default main loop.
        /// For more information on how the timing of the loop works ->
        /// https://medium.com/@tglaiel/how-to-make-your-game-run-at-60fps-24c61210fe75
        /// </summary>
        public static void DefaultMainLoop(Action tick, Action frame)
        {
            DetectVSync();

            byte desiredStep = Configuration.DesiredStep;
            if (desiredStep == 0) desiredStep = 60;

            // Setup tick time trackers.
            Stopwatch timer = Stopwatch.StartNew();
            double targetTime = 1000f / desiredStep;
            double accumulator = 0f;
            double lastTick = 0f;

            // Fuzzy time tracking, as no system has that kind of perfect timing.
            double targetTimeFuzzyLower = 1000d / (desiredStep - 1);
            double targetTimeFuzzyUpper = 1000d / (desiredStep + 1);

            DeltaTime = (float) targetTime;

            // Tracks slow updates to switch the desired step.
            const byte sampleSize = 100;
            const float loopSwapCooldown = 1000 * 10;
            byte currentSampleIdx = 0;
            float averageUpdates = 0;
            float lastUpdateTimeStamp = 0;

            while (Status == EngineStatus.Running)
            {
#if SIMULATE_LAG
                Task.Delay(Helpers.GenerateRandomNumber(0, 16)).Wait();
#endif
                // Run the host events, and check whether it is closing.
                if (!Host.Update())
                {
                    Log?.Info("Host was closed.", MessageSource.Engine);
                    break;
                }

                double curTime = timer.ElapsedMilliseconds;
                double deltaTime = curTime - lastTick;
                lastTick = curTime;

                // Snap delta.
                if (Math.Abs(targetTime - deltaTime) <= 1) deltaTime = targetTime;

                // Add to the accumulator.
                accumulator += deltaTime;

                // Update as many times as needed.
                byte updates = 0;
                var updated = false;
                while (accumulator > targetTimeFuzzyUpper)
                {
                    tick();
                    accumulator -= targetTime;
                    updated = true;

                    if (accumulator < targetTimeFuzzyLower - targetTime) accumulator = 0;
                    updates++;
                    // Max updates are 5 - to prevent large spikes.
                    // This does somewhat break the simulation - but these shouldn't happen except as a last resort.
                    if (updates <= 5) continue;
                    accumulator = 0;
                    break;
                }

                // Don't check for loop speed up, slow down if loading, not focused, or a loop speed swap occured recently.
                if (Configuration.VariableLoopSpeed && !SceneManager.Loading && Host.IsFocused && TotalTime - lastUpdateTimeStamp > loopSwapCooldown)
                {
                    // Sample the current update count.
                    averageUpdates += updates;
                    currentSampleIdx++;
                    // If enough sample are gathered, average them.
                    if (currentSampleIdx == sampleSize)
                    {
                        averageUpdates /= sampleSize;
                        byte speedDirection = 0;

                        // If churning out less than one update per cycle that means we're running fast, if
                        // we are having to update twice per cycle that means we're running slow.
                        if (averageUpdates < 1)
                            speedDirection = 1;
                        else if (averageUpdates > 2)
                            speedDirection = 2;

                        // Check if any differences in the direction are needed.
                        if (speedDirection != 0)
                        {
                            byte newLoopStep = 0;

                            // If going up...
                            if (speedDirection == 1)
                            {
                                // If currently at the desired step, that means we're going faster - this is impossible if vSync is on.
                                if (desiredStep == Configuration.DesiredStep)
                                {
                                    if (Configuration.ScaleStepUp && !(Renderer.ForcedVSync || Renderer.VSync))
                                        // Go at twice the speed.
                                        newLoopStep = (byte) (desiredStep * 2);
                                }
                                // If going slower than the desired step, this means we're recovering from a slowdown.
                                else if (desiredStep < Configuration.DesiredStep)
                                {
                                    newLoopStep = Configuration.DesiredStep;
                                }
                            }
                            else
                                // If going down...
                            {
                                // Don't go slower than that.
                                if (desiredStep > Configuration.DesiredStep / 4)
                                    // Go twice as slow.
                                    newLoopStep = (byte) (desiredStep / 2);
                            }

                            // Apply changes if any.
                            if (newLoopStep != 0)
                            {
                                desiredStep = newLoopStep;
                                targetTime = 1000f / desiredStep;
                                targetTimeFuzzyLower = 1000f / (desiredStep - 1);
                                targetTimeFuzzyUpper = 1000f / (desiredStep + 1);
                                DeltaTime = (float) targetTime;

                                Log.Info($"Loop speed swapped to {desiredStep} because average updates are {averageUpdates}.", MessageSource.Engine);
                            }
                        }

                        lastUpdateTimeStamp = TotalTime;
                        currentSampleIdx = 0;
                        averageUpdates = 0;
                    }
                }

                if (!Host.IsOpen) break;

                // If vSync is on, only render if updated because we don't want to be throttled by the buffer swap.
                if (updated) frame();
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
            // Because life is unfair and we cannot have nice things in order to detect
            // whether the GPU lord has enforced VSync on us (not that we can do anything about it)
            // we have to swap buffers around while messing with the settings and see if it
            // changes anything. If it doesn't - it means VSync is either forced on or off.
            const int jitLoops = 5;
            const int loopCount = 15;
            var timings = new double[(loopCount - jitLoops) * 2];
            Host.Window.Context.SwapInterval = 0;
            for (var i = 0; i < timings.Length + jitLoops; i++)
            {
                timer.Restart();
                Host.Window.Context.SwapBuffers();
                // Run a couple of loops to get the JIT warmed up.
                if (i >= jitLoops)
                    timings[i - jitLoops] = timer.ElapsedMilliseconds;
                // Alright, now turn v-sync on.
                if (i == loopCount - 1)
                    Host.Window.Context.SwapInterval = 1;
            }

            double averageTimeOff = timings.Take(loopCount - jitLoops).Sum() / (timings.Length / 2);
            double averageTimeOn = timings.Skip(loopCount - jitLoops).Sum() / (timings.Length / 2);
            Renderer.ForcedVSync = Math.Abs(averageTimeOff - averageTimeOn) <= 1;

            // Restore settings.
            Renderer.ApplySettings();
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

            Host.UpdateInput();
            Renderer.Update();
            CoroutineManager.Update();
            SceneManager.Update();

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
            PerfProfiler.FrameStart();

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

            PerfProfiler.FrameEventStart("BufferSwap");
            Host.Window.Context.SwapBuffers();
            PerfProfiler.FrameEventEnd("BufferSwap");
#if TIMING_DEBUG
            _frameId++;
            Console.Write(_curUpdateC);
#endif

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

            // Create sources.
            for (var i = 0; i < Helpers.AssociatedAssemblies.Length; i++)
            {
                Assembly assembly = Helpers.AssociatedAssemblies[i];
                loader.AddSource(new EmbeddedAssetSource(assembly, "Assets"));
            }

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