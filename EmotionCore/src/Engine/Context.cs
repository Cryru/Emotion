// Emotion - https://github.com/Cryru/Emotion

#region Using

using System;
using Emotion.Debug;
using Emotion.External;
using Emotion.Game.Layering;
using Emotion.GLES;
using Emotion.IO;
using Emotion.Utils;
using OpenTK;

#if DEBUG

#endif

#endregion

namespace Emotion.Engine
{
    public class Context
    {
        #region Properties

        /// <summary>
        /// Whether the context is running.
        /// </summary>
        public bool Running { get; protected set; }

        /// <summary>
        /// The time it took to render the last frame.
        /// </summary>
        public float FrameTime { get; protected set; }

        #endregion

        #region Objects

        /// <summary>
        /// The context's initial settings.
        /// </summary>
        public Settings Settings;

        #endregion

        #region Modules

        /// <summary>
        /// Handles rendering.
        /// </summary>
        public Renderer Renderer { get; protected set; }

        /// <summary>
        /// Handles loading assets and storing assets.
        /// </summary>
        public AssetLoader AssetLoader { get; protected set; }

        /// <summary>
        /// Handles input from the mouse, keyboard, and other devices.
        /// </summary>
        public Input.Input Input { get; protected set; }

        /// <summary>
        /// A javascript engine.
        /// </summary>
        public ScriptingEngine ScriptingEngine { get; protected set; }

        /// <summary>
        /// Module which manages the different layers which make up the game.
        /// </summary>
        public LayerManager LayerManager { get; protected set; }

        #endregion

        #region Objects

        /// <summary>
        /// The window the game is opened in.
        /// </summary>
        public Window Window { get; protected set; }

        #endregion

        #region Initialization

        /// <summary>
        /// Initiates external libraries and pre-initialization. Is run once for all instances.
        /// </summary>
        static Context()
        {
            // Set the DLL path on Windows.
            if (Environment.OSVersion.Platform == PlatformID.Win32NT) Windows.SetDllDirectory(Environment.CurrentDirectory + "\\Libraries\\" + (Environment.Is64BitProcess ? "x64" : "x86"));
        }

        /// <summary>
        /// Create a new Emotion context.
        /// </summary>
        /// <param name="settings">The settings to start the context win.</param>
        internal Context(Settings settings)
        {
            Settings = settings;

            Debugger.Log(MessageType.Info, MessageSource.Engine, "Starting Emotion version " + Meta.Version);

            // Start loading modules.
            Debugger.Log(MessageType.Trace, MessageSource.Engine, "Creating window...");
            Window = new Window(this);

            Debugger.Log(MessageType.Trace, MessageSource.Engine, "Creating renderer...");
            Renderer = new Renderer(this);

            Debugger.Log(MessageType.Trace, MessageSource.Engine, "Creating asset loader...");
            AssetLoader = new AssetLoader(this);

            Debugger.Log(MessageType.Trace, MessageSource.Engine, "Creating scripting engine...");
            ScriptingEngine = new ScriptingEngine(this);

            Debugger.Log(MessageType.Trace, MessageSource.Engine, "Creating layer manager...");
            LayerManager = new LayerManager(this);

            Debugger.Log(MessageType.Trace, MessageSource.Engine, "Creating input manager...");
            Input = new Input.Input(this);
        }

        #endregion

        #region API

        /// <summary>
        /// Start running the engine loop. Blocking.
        /// </summary>
        public void Start()
        {
            // Set running to true.
            Running = true;

            // Setup the debugger if not already setup.
            Debugger.Setup(this);

            // Start running the loops. Blocking.
            Window.Run(0, 0);

            // Context has stopped running - cleanup.
            Window.Destroy();
            Renderer.Destroy();

            // Platform cleanup.
            Window.Dispose();

            // Dereference objects.
            Window = null;
            Renderer = null;
            Settings = null;

            // Close application.
            Environment.Exit(1);
        }

        /// <summary>
        /// Stops running the engine.
        /// </summary>
        public void Quit()
        {
            // Stops the window unblocking the Start function.
            Window.Exit();
        }

        /// <summary>
        /// Applies any changes made to the settings object.
        /// </summary>
        public void ApplySettings()
        {
            // Apply window mode.
            switch (Settings.WindowMode)
            {
                case WindowMode.Borderless:
                    Window.WindowBorder = WindowBorder.Hidden;
                    Window.WindowState = WindowState.Normal;
                    Window.Width = DisplayDevice.Default.Width;
                    Window.Height = DisplayDevice.Default.Height;
                    Window.X = 0;
                    Window.Y = 0;
                    break;
                case WindowMode.Fullscreen:
                    Window.WindowBorder = WindowBorder.Fixed;
                    Window.WindowState = WindowState.Fullscreen;
                    break;
                default:
                    Window.Width = Settings.WindowWidth;
                    Window.Height = Settings.WindowHeight;
                    Window.X = DisplayDevice.Default.Width / 2 - Settings.WindowWidth / 2;
                    Window.Y = DisplayDevice.Default.Height / 2 - Settings.WindowHeight / 2;
                    // Should by WindowBorder.Fixed always but an OpenTK anomaly causes Resizable to be Fixed.
                    Window.WindowBorder = Window.WindowBorder == WindowBorder.Hidden ? WindowBorder.Resizable : WindowBorder.Fixed;
                    Window.WindowState = WindowState.Normal;
                    break;
            }
        }

        #endregion

        #region Loop

        /// <summary>
        /// Is run every tick by the OpenTK context.
        /// </summary>
        /// <param name="frameTime">The time between this tick and the last.</param>
        internal void LoopUpdate(float frameTime)
        {
            // Update the thread manager.
            ThreadManager.Run();

            FrameTime = frameTime;

            // Update debugger.
            Debugger.Update(ScriptingEngine);

            // Run modules.
            LayerManager.Update();
            Input.Update();

            // Check for errors.
            Helpers.CheckError("update");
        }

        /// <summary>
        /// Is run every frame by the OpenTK context.
        /// </summary>
        internal void LoopDraw()
        {
            // Clear the screen.
            Renderer.Clear();

            // First draw the layers.
            LayerManager.Draw();

            // Draw debug.
            Debugger.Draw(Renderer, this);

            // Swap buffers.
            Renderer.Present();
        }

        #endregion
    }
}