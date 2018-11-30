// Emotion - https://github.com/Cryru/Emotion

#region Using

using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using Emotion.Debug;
using Emotion.Debug.Logging;
using Emotion.Engine.Hosting;
using Emotion.Engine.Hosting.Desktop;
using Emotion.External;
using Emotion.Game.Layering;
using Emotion.Graphics;
using Emotion.Input;
using Emotion.IO;
using Emotion.Libraries;
using Emotion.Sound;
using OpenTK.Graphics.ES30;
using Debugger = Emotion.Debug.Debugger;

#endregion

namespace Emotion.Engine
{
    /// <summary>
    /// The Emotion engine context. Manages everything done by the engine, and provides access to the different systems.
    /// </summary>
    public static class Context
    {
        #region Properties

        /// <summary>
        /// Whether the engine was setup.
        /// </summary>
        public static bool IsSetup { get; private set; }

        /// <summary>
        /// Whether the context is running.
        /// </summary>
        public static bool IsRunning { get; private set; }

        /// <summary>
        /// The time it took to render the last frame.
        /// </summary>
        public static float FrameTime { get; private set; }

        /// <summary>
        /// The time which has passed since start. Used for tracking time in shaders and such.
        /// </summary>
        public static float TotalTime { get; private set; }

        /// <summary>
        /// The settings the context's settings.
        /// </summary>
        public static Settings Settings { get; private set; }

        #endregion

        #region Modules

        /// <summary>
        /// Handles rendering.
        /// </summary>
        public static Renderer Renderer { get; private set; }

        /// <summary>
        /// Handles loading assets and storing assets.
        /// </summary>
        public static AssetLoader AssetLoader { get; private set; }

        /// <summary>
        /// Handles input from the mouse, keyboard, and other devices.
        /// </summary>
        public static InputManager InputManager { get; private set; }

        /// <summary>
        /// A javascript engine.
        /// </summary>
        public static ScriptingEngine ScriptingEngine { get; private set; }

        /// <summary>
        /// Module which manages the different layers which make up the game.
        /// </summary>
        public static LayerManager LayerManager { get; private set; }

        /// <summary>
        /// Manages sound.
        /// </summary>
        public static SoundManager SoundManager { get; private set; }

        /// <summary>
        /// A platform host for the engine. It must provide a GL context, surface, input, and the standard update-draw loop.
        /// </summary>
        public static IHost Host { get; set; }

        /// <summary>
        /// The logging of the engine.
        /// </summary>
        public static LoggingProvider Log { get; set; }

        #endregion

        #region API

        /// <summary>
        /// Prepare the Emotion engine.
        /// </summary>
        /// <param name="config">A function to apply initial settings.</param>
        public static void Setup(Action<Settings> config = null)
        {
            // Check if it was already setup.
            if (IsSetup) throw new Exception("Context is already setup.");
            IsSetup = true;

            // Initialize logger first of all.
            if (Log == null) Log = new DefaultLogger();

            // Initialize debugger.
            Debugger.Initialize();

            Log.Info($"Starting Emotion v{Meta.Version}", MessageSource.Engine);

#if !DEBUG
            try
            {
#endif
            // Initiate bootstrap.
            Log.Info("-------------------------------", MessageSource.Engine);
            Log.Info($"Executed at: {Environment.CurrentDirectory}", MessageSource.Engine);
            Log.Info($"Debug Mode / Debugger Attached: {Debugger.DebugMode} / {System.Diagnostics.Debugger.IsAttached}", MessageSource.Engine);
            Log.Info($"64Bit: {Environment.Is64BitProcess}", MessageSource.Engine);
            Log.Info($"OS: {CurrentPlatform.OS} ({Environment.OSVersion})", MessageSource.Engine);
            Log.Info($"CPU: {Environment.ProcessorCount}", MessageSource.Engine);

            // Run platform specific boot.
            switch (CurrentPlatform.OS)
            {
                case PlatformName.Windows:
                    WindowsSetup();
                    break;
                case PlatformName.Linux:
                    LinuxSetup();
                    break;
            }

            Log.Info("-------------------------------", MessageSource.Engine);
            Log.Info("Bootstrap complete.", MessageSource.Engine);

            // Apply settings and run initial setup function.
            Settings initial = new Settings();
            config?.Invoke(initial);
            Settings = initial;

            // Setup thread manager.
            GLThread.BindThread();

            // Create host if not created.
            if (Host == null)
                try
                {
                    Log.Trace("Creating host...", MessageSource.Engine);
                    if (CurrentPlatform.OS == PlatformName.Windows || CurrentPlatform.OS == PlatformName.Linux || CurrentPlatform.OS == PlatformName.Mac)
                    {
                        Host = new OtkWindow();
                        Log.Info("Created OpenTK window host.", MessageSource.Engine);
                    }
                    else
                    {
                        throw new Exception("Unsupported platform.");
                    }
                }
                catch (Exception ex)
                {
                    Log.Error("Could not create host. Is the system supported?", ex, MessageSource.Engine);
                    return;
                }

            // Apply settings and hook.
            Host.ApplySettings(Settings);
            Host.SetHooks(LoopUpdate, LoopDraw, Resize, Quit);

            // Start creating modules.

            // Scripting engine is first to provide the other modules the ability to expose functions.
            Log.Trace("Creating scripting engine...", MessageSource.Engine);
            ScriptingEngine = new ScriptingEngine();

            // Asset loader is next so other modules - especially the renderer, can access the file system.
            Log.Trace("Creating asset loader...", MessageSource.Engine);
            AssetLoader = new AssetLoader();

            // The order of the next modules doesn't matter.

            Debugger.InitializeModule();

            Log.Trace("Creating renderer...", MessageSource.Engine);
            Renderer = new Renderer();

            Log.Trace("Creating sound manager...", MessageSource.Engine);
            SoundManager = new SoundManager();

            Log.Trace("Creating layer manager...", MessageSource.Engine);
            LayerManager = new LayerManager();

            Log.Trace("Creating input manager...", MessageSource.Engine);
            InputManager = new InputManager();
#if !DEBUG
            }
            catch (Exception ex)
            {
                Log.Error("Emotion engine was unable to initialize.", ex, MessageSource.Engine);
            }
#endif
        }

        /// <summary>
        /// Start running the engine loop. Can be blocking depending on the host.
        /// </summary>
        public static void Run()
        {
#if !DEBUG
            try
            {
#endif
            // Check if setup.
            if (!IsSetup) throw new Exception("You must call Context.Setup before calling Context.Run");

            // Set running to true.
            IsRunning = true;

            // Start running the loops. Blocking.
            Host.Run();
#if !DEBUG
            }
            catch (Exception ex)
            {
                Log.Error("Emotion engine has encountered a crash.", ex, MessageSource.Engine);
            }
#endif
        }

        /// <summary>
        /// Stops running the engine.
        /// </summary>
        public static void Quit()
        {
            // Switch running to false.
            IsRunning = false;

            // Close the host.
            Host?.Close();

            // Cleanup modules.
            Renderer?.Destroy();
            SoundManager?.Dispose();

            // Cleanup the host.
            Host?.Dispose();
            Log?.Dispose();

            // Close application.
            Environment.Exit(0);
        }

        #endregion

        #region OS Specific Initialization

        /// <summary>
        /// WindowsNT bootstrap.
        /// </summary>
        private static void WindowsSetup()
        {
            // Set current directory to the process 
            string processPath = AppDomain.CurrentDomain.BaseDirectory;
            if (processPath != Environment.CurrentDirectory + "\\")
            {
                Directory.SetCurrentDirectory(AppDomain.CurrentDomain.BaseDirectory);
                Log.Warning($"Process directory was wrong, set to: {Environment.CurrentDirectory}", MessageSource.Engine);
            }

            // Set the DLL path on Windows.
            string libraryDirectory = Environment.CurrentDirectory + "\\Libraries\\" + (Environment.Is64BitProcess ? "x64" : "x86");
            Windows.SetDllDirectory(libraryDirectory);
            string path = Environment.GetEnvironmentVariable("PATH", EnvironmentVariableTarget.Process);
            Environment.SetEnvironmentVariable("PATH", path + ";" + libraryDirectory, EnvironmentVariableTarget.Process);

            Log.Info($"Library Folder: {libraryDirectory}", MessageSource.Engine);
        }

        /// <summary>
        /// Linux bootstrap.
        /// </summary>
        private static void LinuxSetup()
        {
            // Get the path of the process AKA where the engine was launched from.
            string processPath = Path.GetDirectoryName(Process.GetCurrentProcess().MainModule.FileName);
            if (processPath == null) throw new Exception("Failed to get the process path.");

            // Check if the process path is the current path.
            if (processPath != Environment.CurrentDirectory)
            {
                // Set the current path to the process path.
                Directory.SetCurrentDirectory(processPath);
                Unix.chdir(processPath);

                string processName = Process.GetCurrentProcess().ProcessName;
                string executableName = processName.Replace(processPath + "/", "");

                Log.Warning("It seems the process directory is not the executable directory. Will restart from correct directory.", MessageSource.Engine);
                Log.Warning($"Proper directory is: {processPath}", MessageSource.Engine);
                Log.Warning($"Executable is: {executableName}", MessageSource.Engine);

                // Stop the debugger so that the new instance can attach itself to the console and perform logging in peace.
                Debugger.Stop();

                // Restart the process.
                Process.Start(executableName)?.WaitForExit();
                Environment.Exit(0);
            }

            // Open libraries.
            Log.Warning($"libsndio.so.6.1 found: {File.Exists("./Libraries/x64/libsndio.so.6.1")}", MessageSource.Engine);
            Unix.dlopen("./Libraries/x64/libsndio.so.6.1", Unix.RTLD_NOW);
        }

        #endregion

        #region Loops and Host Events

        /// <summary>
        /// Is run every tick by the host.
        /// </summary>
        /// <param name="frameTime">The time between this tick and the last.</param>
        private static void LoopUpdate(float frameTime)
        {
            // Throttle so the whole CPU isn't used up by update cycles. Useful only when the Update loop runs on a separate thread.
            //Task.Delay(1).Wait();

            // Update debugger.
            Debugger.Update();

            // Update the renderer.
            Renderer.Update();

            // If not focused don't update user code.
            if (Host.Focused) LayerManager.Update();

            // Run input. This is outside of a focus check so it can capture the first input when focus is claimed.
            InputManager.Update();
        }

        /// <summary>
        /// Is run every frame by the host.
        /// </summary>
        private static void LoopDraw(float frameTime)
        {
            // If not focused, don't draw.
            if (!Host.Focused)
            {
                Task.Delay(1).Wait();
                return;
            }

            // Run the thread manager.
            GLThread.Run();

            // Get frame time and increment total time.
            FrameTime = frameTime;
            TotalTime += frameTime;

            // Clear the screen.
            Renderer.Clear();
            GLThread.CheckError("renderer clear");

            // Draw the layers.
            LayerManager.Draw();
            GLThread.CheckError("layer draw");

            // Finish rendering.
            Renderer.End();
            GLThread.CheckError("renderer end");

            // Draw debug.
            Debugger.Draw();

            // Swap buffers.
            Host.SwapBuffers();
        }

        private static void Resize()
        {
            // Calculate borderbox / pillarbox.
            float targetAspectRatio = Settings.RenderWidth / Settings.RenderHeight;

            float width = Host.Size.X;
            float height = (int) (width / targetAspectRatio + 0.5f);

            // If the height is bigger then the black bars will appear on the top and bottom, otherwise they will be on the left and right.
            if (height > Host.Size.Y)
            {
                height = Host.Size.Y;
                width = (int) (height * targetAspectRatio + 0.5f);
            }

            int vpX = (int) (Host.Size.X / 2 - width / 2);
            int vpY = (int) (Host.Size.Y / 2 - height / 2);

            // Set viewport.
            GL.Viewport(vpX, vpY, (int) width, (int) height);
            GL.Scissor(vpX, vpY, (int) width, (int) height);
        }

        #endregion
    }
}