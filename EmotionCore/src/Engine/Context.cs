// Emotion - https://github.com/Cryru/Emotion

#region Using

using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using Emotion.Debug;
using Emotion.Engine.Threading;
using Emotion.External;
using Emotion.Game.Layering;
using Emotion.Graphics;
using Emotion.Host;
using Emotion.Input;
using Emotion.IO;
using Emotion.Libraries;
using Emotion.Sound;
using Emotion.Utils;
using Debugger = Emotion.Debug.Debugger;

#endregion

namespace Emotion.Engine
{
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
        /// The context's host. This can be the window, the activity or whatever.
        /// </summary>
        public static IHost Host { get; private set; }

        #endregion

        #region Initialization

        /// <summary>
        /// Prepare the Emotion engine.
        /// </summary>
        /// <param name="config">A function to apply initial settings.</param>
        public static void Setup(Action<Settings> config = null)
        {
            // Check if it was already setup.
            if (IsSetup) throw new Exception("Context is already setup.");
            IsSetup = true;

            // Initialize debugger so we have access to logging afterward.
            Debugger.Initialize();

            Debugger.Log(MessageType.Info, MessageSource.Engine, $"Starting Emotion v{Meta.Version}");

            // If the debugger is attached, don't wrap in a try-catch so that exceptions can be traced easier.
            if (System.Diagnostics.Debugger.IsAttached)
            {
                InternalSetup(config);
                return;
            }

            // If no debugger is attached, wrap in a try-catch so that exception logs are generated.
            try
            {
                InternalSetup(config);
            }
            catch (Exception ex)
            {
                File.WriteAllText($"Logs{Path.DirectorySeparatorChar}InitCrash_{DateTime.Now.ToFileTime()}", ex.ToString());
                Debugger.Log(MessageType.Error, MessageSource.Engine, $"Emotion engine was unable to initialize.\n{ex}");

                // Flush logs.
                while (Debugger.LogInProgress()) Task.Delay(1).Wait();

                // Close.
                Environment.Exit(1);
            }
        }

        private static void InternalSetup(Action<Settings> config = null)
        {
            // Initiate bootstrap.
            Debugger.Log(MessageType.Info, MessageSource.Engine, "-------------------------------");
            Debugger.Log(MessageType.Info, MessageSource.Engine, $"Executed at: {Environment.CurrentDirectory}");
            Debugger.Log(MessageType.Info, MessageSource.Engine, $"64Bit: {Environment.Is64BitProcess}");
            Debugger.Log(MessageType.Info, MessageSource.Engine, $"OS: {CurrentPlatform.OS} ({Environment.OSVersion})");
            Debugger.Log(MessageType.Info, MessageSource.Engine, $"CPU: {Environment.ProcessorCount}");

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

            Debugger.Log(MessageType.Info, MessageSource.Engine, "-------------------------------");
            Debugger.Log(MessageType.Info, MessageSource.Engine, "Bootstrap complete.");

            // Apply settings and run initial setup function.
            Settings initial = new Settings();
            config?.Invoke(initial);
            Settings = initial;

            // Setup thread manager.
            GLThread.BindThread();

            try
            {
                Debugger.Log(MessageType.Trace, MessageSource.Engine, "Creating host...");
                if (CurrentPlatform.OS == PlatformName.Windows || CurrentPlatform.OS == PlatformName.Linux || CurrentPlatform.OS == PlatformName.Mac)
                {
                    Host = new Window(Settings);
                    Host.SetHooks(LoopUpdate, LoopDraw);
                    Debugger.Log(MessageType.Trace, MessageSource.Engine, "Created window host.");
                }
                else
                {
                    throw new Exception("Unsupported platform.");
                }
            }
            catch (Exception ex)
            {
                Debugger.Log(MessageType.Error, MessageSource.Engine, "Could not create host. Is the system capable of running the engine?");
                Debugger.Log(MessageType.Error, MessageSource.Engine, ex.ToString());
                throw;
            }

            // Start creating modules.

            // Scripting engine is first to provide the other modules the ability to expose functions.
            Debugger.Log(MessageType.Trace, MessageSource.Engine, "Creating scripting engine...");
            ScriptingEngine = new ScriptingEngine();

            // Asset loader is next so other modules - especially the renderer, can access the file system.
            Debugger.Log(MessageType.Trace, MessageSource.Engine, "Creating asset loader...");
            AssetLoader = new AssetLoader();

            // The order of the next modules doesn't matter.

            Debugger.InitializeModule();

            Debugger.Log(MessageType.Trace, MessageSource.Engine, "Creating renderer...");
            Renderer = new Renderer();

            Debugger.Log(MessageType.Trace, MessageSource.Engine, "Creating sound manager...");
            SoundManager = new SoundManager();

            Debugger.Log(MessageType.Trace, MessageSource.Engine, "Creating layer manager...");
            LayerManager = new LayerManager();

            Debugger.Log(MessageType.Trace, MessageSource.Engine, "Creating input manager...");
            InputManager = new InputManager();
        }

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
                Debugger.Log(MessageType.Warning, MessageSource.Engine, $"Process directory was wrong, set to: {Environment.CurrentDirectory}");
            }

            // Set the DLL path on Windows.
            string libraryDirectory = Environment.CurrentDirectory + "\\Libraries\\" + (Environment.Is64BitProcess ? "x64" : "x86");
            Windows.SetDllDirectory(libraryDirectory);
            string path = Environment.GetEnvironmentVariable("PATH", EnvironmentVariableTarget.Process);
            Environment.SetEnvironmentVariable("PATH", path + ";" + libraryDirectory, EnvironmentVariableTarget.Process);

            Debugger.Log(MessageType.Info, MessageSource.Engine, "Library Folder: " + libraryDirectory);
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

                Debugger.Log(MessageType.Warning, MessageSource.Engine, "It seems the process directory is not the executable directory. Will restart from correct directory.");
                Debugger.Log(MessageType.Warning, MessageSource.Engine, $"Proper directory is: {processPath}");
                Debugger.Log(MessageType.Warning, MessageSource.Engine, $"Executable is: {executableName}");

                // Stop the debugger so that the new instance can attach itself to the console and perform logging in peace.
                Debugger.Stop();

                // Restart the process.
                Process.Start(executableName)?.WaitForExit();
                Environment.Exit(0);
            }

            // Open libraries.
            Debugger.Log(MessageType.Warning, MessageSource.Engine, $"libsndio.so.6.1 found: {File.Exists("./Libraries/x64/libsndio.so.6.1")}");
            Unix.dlopen("./Libraries/x64/libsndio.so.6.1", Unix.RTLD_NOW);
        }

        #endregion

        #region API

        /// <summary>
        /// Start running the engine loop. Blocking.
        /// </summary>
        public static void Run()
        {
            // If the debugger is attached, don't wrap in a try-catch so that exceptions can be traced easier.
            if (System.Diagnostics.Debugger.IsAttached)
            {
                InternalRun();
                return;
            }

            // If no debugger is attached, wrap in a try-catch so that exception logs are generated.
            try
            {
                InternalRun();
            }
            catch (Exception ex)
            {
                File.WriteAllText($"Logs{Path.DirectorySeparatorChar}FatalCrash_{DateTime.Now.ToFileTime()}", ex.ToString());
                Debugger.Log(MessageType.Error, MessageSource.Engine, $"Emotion engine has encountered a crash.\n{ex}");

                // Flush logs.
                while (Debugger.LogInProgress()) Task.Delay(1).Wait();

                // Close.
                Environment.Exit(1);
            }
        }

        private static void InternalRun()
        {
            // Check if setup.
            if (!IsSetup) throw new Exception("You must call Context.Setup before calling Context.Run");

            // Set running to true.
            IsRunning = true;

            // Start running the loops. Blocking.
            Host.Run();

            // Context has stopped running - cleanup.
            Host.Close();
            Renderer.Destroy();

            // Platform cleanup.
            Host.Dispose();

            // Dereference objects.
            Host = null;
            Renderer = null;
            Settings = null;
            IsRunning = false;

            // Close application.
            Environment.Exit(0);
        }

        /// <summary>
        /// Stops running the engine.
        /// </summary>
        public static void Quit()
        {
            // Stops the window unblocking the Start function.
            Host.Close();
        }

        #endregion

        #region Loops

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

            // If not rendering, then don't update.
            // The reason for this is because we are only skipping rendering when frame by frame mode is active, and the layer manager shouldn't trigger at all then.
            if (Renderer.RenderFrame())
            {
                // Update the renderer.
                Renderer.Update(frameTime);

                // Update the user code. When the context doesn't have focus the layer manager will perform a light update.
                LayerManager.Update();
            }

            // Run input. This is outside of a focus check so it can capture the first input when focus is claimed.
            InputManager.Update();
        }

        /// <summary>
        /// Is run every frame by the host.
        /// </summary>
        private static void LoopDraw(float frameTime)
        {
            // If not focused, or the renderer tells us not to render the frame - don't draw.
            if (!Host.Focused || !Renderer.RenderFrame())
            {
                Task.Delay(1).Wait();
                return;
            }

            // Run the thread manager.
            GLThread.Run();

            FrameTime = frameTime;

            // Add to time.
            TotalTime += frameTime;

            // Clear the screen.
            Renderer.Clear();
            Helpers.CheckError("renderer clear");

            // Draw the layers.
            LayerManager.Draw();
            Helpers.CheckError("layer draw");

            // Finish rendering.
            Renderer.End();
            Helpers.CheckError("renderer end");

            // Draw debug.
            Debugger.Draw();

            // Swap buffers.
            Host.SwapBuffers();
        }

        #endregion
    }
}