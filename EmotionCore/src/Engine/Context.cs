// Emotion - https://github.com/Cryru/Emotion

#region Using

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Numerics;
using System.Reflection;
using System.Threading.Tasks;
using Emotion.Debug;
using Emotion.Debug.Logging;
using Emotion.Engine.Configuration;
using Emotion.Engine.Hosting;
using Emotion.Engine.Hosting.Desktop;
using Emotion.Engine.Scenography;
using Emotion.External;
using Emotion.Graphics;
using Emotion.Input;
using Emotion.IO;
using Emotion.Libraries;
using Emotion.Libraries.Steamworks;
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
        /// The raw frame time unhindered by the semi-fixed step. Used for FPS meters and such.
        /// </summary>
        public static float RawFrameTime { get; private set; }

        /// <summary>
        /// The time which has passed since start. Used for tracking time in shaders and such.
        /// </summary>
        public static float TotalTime { get; private set; }

        /// <summary>
        /// The settings the context's settings.
        /// </summary>
        public static Settings Settings { get; private set; }

        /// <summary>
        /// Functionality related engine configuration.
        /// </summary>
        public static Flags Flags { get; private set; } = new Flags();

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
        /// A javascript engine.
        /// </summary>
        public static ScriptingEngine ScriptingEngine { get; private set; }

        /// <summary>
        /// Module which manages loading and unloading of scenes.
        /// </summary>
        public static SceneManager SceneManager { get; private set; }

        /// <summary>
        /// Manages sound.
        /// </summary>
        public static SoundManager SoundManager { get; private set; }

        /// <summary>
        /// A platform host for the engine. It must provide a GL context, surface, input, and the standard update-draw loop.
        /// </summary>
        public static IHost Host { get; set; }

        /// <summary>
        /// Handles input from the mouse, keyboard, and other devices. Is created and managed by the host.
        /// </summary>
        public static IInputManager InputManager { get; set; }

        /// <summary>
        /// The logging of the engine.
        /// </summary>
        public static LoggingProvider Log { get; set; }

        #endregion

        #region Plugin Modules

        /// <summary>
        /// Integration with Steam.
        /// </summary>
        public static SteamModule Steam { get; set; }

        #endregion

        #region Privates

        /// <summary>
        /// The raw frame timer.
        /// </summary>
        private static Stopwatch _rawTimer;

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
            Log.Info($"Executed by: {Assembly.GetCallingAssembly()}", MessageSource.Engine);
            Log.Info($"Debug Mode / Debugger Attached: {Debugger.DebugMode} / {System.Diagnostics.Debugger.IsAttached}", MessageSource.Engine);
            Log.Info($"64Bit: {Environment.Is64BitProcess}", MessageSource.Engine);
            Log.Info($"OS: {CurrentPlatform.OS} ({Environment.OSVersion})", MessageSource.Engine);
            Log.Info($"CPU: {Environment.ProcessorCount}", MessageSource.Engine);
            Log.Info($"SIMD Vectors: {Vector.IsHardwareAccelerated}", MessageSource.Engine);

            // Run platform specific boot.
            switch (CurrentPlatform.OS)
            {
                case PlatformName.Windows:
                    WindowsSetup();
                    break;
                case PlatformName.Linux:
                    LinuxSetup();
                    break;
                case PlatformName.Mac:
                    MacSetup();
                    break;
            }

            Log.Info("-------------------------------", MessageSource.Engine);
            Log.Info("Bootstrap complete.", MessageSource.Engine);

            // Apply settings and run initial setup function.
            Settings initial = new Settings();
            config?.Invoke(initial);
            Settings = initial;

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

            // Check if input was setup.
            if (InputManager == null) Log.Error("Host didn't setup an input manager.", MessageSource.Engine);

            // Apply settings and hook.
            Host.ApplySettings(Settings.HostSettings);
            Host.SetHooks(LoopUpdate, LoopDraw, Resize, Quit);

            // Setup graphics.
            GraphicsManager.Setup();

            // Start creating modules.

            // Scripting engine is first to provide the other modules the ability to expose functions.
            Log.Trace("Creating scripting engine...", MessageSource.Engine);
            ScriptingEngine = new ScriptingEngine();

            // Asset loader is next so other modules - especially the renderer, can access the file system.
            Log.Trace("Creating asset loader...", MessageSource.Engine);
            AssetLoader = new AssetLoader(Flags.AssetRootDirectory, Flags.AdditionalAssetAssemblies);

            // The order of the next modules doesn't matter.

            Debugger.InitializeModule();

            Log.Trace("Creating renderer...", MessageSource.Engine);
            Renderer = new Renderer();

            Log.Trace("Creating sound manager...", MessageSource.Engine);
            SoundManager = new SoundManager();

            Log.Trace("Creating scene manager...", MessageSource.Engine);
            SceneManager = new SceneManager();
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

            // Start tracking raw frame time.
            _rawTimer = Stopwatch.StartNew();

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

            // Close plugins.
            Steam?.Dispose();

            // Close application.
            if (Flags.CloseEnvironmentOnQuit) Environment.Exit(0);
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
            string libraryDirectory = Environment.CurrentDirectory + "\\Libraries\\" + (Environment.Is64BitProcess ? "Windowsx64" : "Windowsx86");
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

            // Log library folder.
            string libRoot = "./Libraries/Linux/";
            Log.Info($"Library Folder: {libRoot}", MessageSource.Engine);
            LoadUnixLibraries(libRoot, new List<string> {"libsndio.so.6.1"});
        }

        /// <summary>
        /// MacOS bootstrap.
        /// </summary>
        private static void MacSetup()
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

                Log.Warning($"Process directory was wrong, set to: {processPath}", MessageSource.Engine);
            }

            // Log library folder.
            string libRoot = "./Libraries/MacOS/";
            Log.Info($"Library Folder: {libRoot}", MessageSource.Engine);
            LoadUnixLibraries(libRoot, new List<string> {"libpng14.14.dylib"});
        }

        /// <summary>
        /// Loads libraries using libdl and dlopen on Unix platforms.
        /// </summary>
        /// <param name="libRoot">The root folder of the libraries.</param>
        /// <param name="libraries">List of libraries to load.</param>
        private static void LoadUnixLibraries(string libRoot, List<string> libraries)
        {
            // Load other libraries.
            if (File.Exists("load.txt")) libraries.AddRange(File.ReadAllLines("load.txt"));
            foreach (string library in libraries)
            {
                string libPath = libRoot + library;
                bool exists = File.Exists(libPath);
                Log.Warning($"{libPath} found: {exists}", MessageSource.Engine);
                Unix.dlopen(libPath, Unix.RTLD_NOW);
            }
        }

        #endregion

        #region Plugins

        /// <summary>
        /// Setups the Steam Emotion module using Steamworks NET. This can fail if:
        /// <para>   </para>
        /// [*] The Steam client isn't running. A running Steam client is required to provide implementations of the various
        /// Steamworks interfaces.
        /// <para>   </para>
        /// [*] The Steam client couldn't determine the App ID of game. If you're running your application from the executable or
        /// debugger directly then you must have a [code-inline]steam_appid.txt[/code-inline] in your game directory next to the
        /// executable, with your app ID in it and nothing else. Steam will look for this file in the current working directory. If
        /// you are running your executable from a different directory you may need to relocate the
        /// [code-inline]steam_appid.txt[/code-inline] file.
        /// <para>   </para>
        /// [*] Your application is not running under the same OS user context as the Steam client, such as a different user or
        /// administration access level.
        /// <para>   </para>
        /// [*] Ensure that you own a license for the App ID on the currently active Steam account. Your game must show up in your
        /// Steam library.
        /// <para>   </para>
        /// [*] Your App ID is not completely set up, i.e. in Release State: Unavailable, or it's missing default packages.
        /// </summary>
        /// <param name="appId">The id of your steam app.</param>
        public static void EnableSteam(uint appId)
        {
            if (!IsSetup) throw new Exception("Context needs to be setup to enable Steam integration.");

            Steam = new SteamModule(appId);
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

            // Get frame time and increment total time.
            FrameTime = frameTime;
            TotalTime += frameTime;

            // Update debugger.
            Debugger.Update();

            // Update the renderer.
            Renderer.Update();

            // Update user code.
            SceneManager.Update();

            // Update plugins.
            Steam?.Update();
        }

        /// <summary>
        /// Is run every frame by the host.
        /// </summary>
        private static void LoopDraw()
        {
            // Track raw time.
            RawFrameTime = _rawTimer.ElapsedMilliseconds;
            _rawTimer.Restart();

            // Run the thread manager.
            GLThread.Run();

            // If not focused, don't draw.
            if (!Host.Focused)
            {
                Task.Delay(1).Wait();
                return;
            }

            // Clear the screen.
            Renderer.Clear();
            GLThread.CheckError("renderer clear");

            // Draw the user scene.
            SceneManager.Draw();
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
            // Check if host size is 0.
            if (Host.Size.X == 0 || Host.Size.Y == 0) Log.Warning("Host reported a size of 0.", MessageSource.Engine);

            // Calculate borderbox / pillarbox.
            float targetAspectRatio = Settings.RenderSettings.Width / Settings.RenderSettings.Height;

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

            // Cache calculations in case someone needs to use them.
            Flags.ScaleResX = width;
            Flags.ScaleResY = height;
        }

        #endregion
    }
}