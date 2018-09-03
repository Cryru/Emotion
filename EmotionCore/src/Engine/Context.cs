// Emotion - https://github.com/Cryru/Emotion

#region Using

using System;
using System.Threading;
using Emotion.Debug;
using Emotion.Game.Layering;
using Emotion.Graphics;
using Emotion.Host;
using Emotion.IO;
using Emotion.Sound;
using Emotion.Utils;

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

        /// <summary>
        /// The time which has passed since start. Used for tracking time in shaders and such.
        /// </summary>
        public float Time { get; protected set; }

        /// <summary>
        /// The settings the context's settings.
        /// </summary>
        public Settings Settings { get; set; }

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

        /// <summary>
        /// Manages sound.
        /// </summary>
        public SoundManager SoundManager { get; protected set; }

        /// <summary>
        /// The context's host. This can be the window, the activity or whatever.
        /// </summary>
        public IHost Host { get; protected set; }

        #endregion

        #region Initialization

        /// <summary>
        /// Create a new Emotion context.
        /// </summary>
        /// <param name="settings">The settings to start the context win.</param>
        internal Context(Settings settings)
        {
            Settings = settings;

            Debugger.Log(MessageType.Info, MessageSource.Engine, "Starting Emotion version " + Meta.Version);

            // Start loading modules.
            Debugger.Log(MessageType.Trace, MessageSource.Engine, "Creating host...");
            if (CurrentPlatform.OS == PlatformID.Win32NT || CurrentPlatform.OS == PlatformID.Unix || CurrentPlatform.OS == PlatformID.MacOSX)
            {
                Host = new Window(settings);
                Debugger.Log(MessageType.Trace, MessageSource.Engine, "Created window host.");
            }
            else
            {
                throw new Exception("Unsupported platform.");
            }

            Host.SetHooks(LoopUpdate, LoopDraw);

            Debugger.Log(MessageType.Trace, MessageSource.Engine, "Creating renderer...");
            Renderer = new Renderer(this);

            Debugger.Log(MessageType.Trace, MessageSource.Engine, "Creating sound manager...");
            SoundManager = new SoundManager(this);

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
            Running = false;

            // Close application.
            Environment.Exit(0);
        }

        /// <summary>
        /// Stops running the engine.
        /// </summary>
        public void Quit()
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
        protected void LoopUpdate(float frameTime)
        {
            // Update the thread manager.
            ThreadManager.Run();

            // Update debugger.
            Debugger.Update(ScriptingEngine);

            // Update the renderer.
            Renderer.Update(frameTime);

            // Run modules.
            SoundManager.Update();

            // Run user layers.
            LayerManager.Update();

            // Run input.
            Input.Update();

            // Check for errors.
            Helpers.CheckError("update");
        }

        /// <summary>
        /// Is run every frame by the host.
        /// </summary>
        protected void LoopDraw(float frameTime)
        {
            // If not focused, don't draw.
            if (!Host.Focused)
            {
                Thread.Sleep(1);
                return;
            }

            FrameTime = frameTime;

            // Add to time.
            Time += frameTime;

            // Clear the screen.
            Renderer.Clear();
            Helpers.CheckError("renderer clear");

            // Draw the layers.
            LayerManager.Draw();
            Helpers.CheckError("layer draw");

            // Draw debug.
            Debugger.DebugDraw(this);
            Helpers.CheckError("debugger draw");

            // Finish rendering.
            Renderer.End();
            Helpers.CheckError("renderer end");

            // Swap buffers.
            Host.SwapBuffers();
        }

        #endregion
    }
}