// Emotion - https://github.com/Cryru/Emotion

#region Using

using System;
using System.Threading;
using Emotion.Debug;
using Emotion.Game.Layering;
using Emotion.GLES;
using Emotion.IO;
using Emotion.Sound;
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

        /// <summary>
        /// The window the game is opened in.
        /// </summary>
        public Window Window { get; protected set; }

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
            Debugger.Log(MessageType.Trace, MessageSource.Engine, "Creating window...");
            Window = new Window(this);

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
            Environment.Exit(0);
        }

        /// <summary>
        /// Stops running the engine.
        /// </summary>
        public void Quit()
        {
            // Stops the window unblocking the Start function.
            Window.Exit();
        }

        #endregion

        #region Loop

        /// <summary>
        /// Is run every tick by the OpenTK context.
        /// </summary>
        /// <param name="frameTime">The time between this tick and the last.</param>
        internal void LoopUpdate(float frameTime)
        {
            FrameTime = frameTime;

            // Update the thread manager.
            ThreadManager.Run();

            // Update debugger.
            Debugger.Update(ScriptingEngine);

            // Run modules.
            SoundManager.Update();

            // Run user layers.
            LayerManager.Update();

            // Run input.
            Input.Update();

            // Check for errors.
            Helpers.CheckError("update");

            Thread.Sleep(1);
        }

        /// <summary>
        /// Is run every frame by the OpenTK context.
        /// </summary>
        internal void LoopDraw()
        {
            // If not focused, don't draw.
            if (!Window.Focused)
            {
                Thread.Sleep(1);
                return;
            }

            // Clear the screen.
            Renderer.Clear();

            // First draw the layers.
            LayerManager.Draw();

            // Draw debug.
            Debugger.DebugDraw(this);

            // Swap buffers.
            Renderer.Present();
        }

        #endregion
    }
}