// Emotion - https://github.com/Cryru/Emotion

#region Using

using Emotion.Engine;
using Emotion.Engine.Objects;
#if DEBUG
using Emotion.Engine.Debugging;

#endif

#endregion

namespace Emotion.Platform.Base
{
    public abstract class ContextBase
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

        #region Engine Modules

        /// <summary>
        /// A javascript engine.
        /// </summary>
        public ScriptingEngine ScriptingEngine { get; protected set; }
#if DEBUG
        /// <summary>
        /// Module used for debugging and diagnostics.
        /// </summary>
        public Debugger Debugger { get; protected set; }
#endif

        /// <summary>
        /// Module which manages the different layers which make up the game.
        /// </summary>
        public LayerManager LayerManager { get; protected set; }

        #endregion

        #region Platform Modules

        /// <summary>
        /// Handles rendering.
        /// </summary>
        public IRenderer Renderer { get; protected set; }

        /// <summary>
        /// Handles input from the mouse, keyboard, and other devices.
        /// </summary>
        public IInput Input { get; protected set; }

        #endregion

        /// <summary>
        /// Start running the engine loop. Blocking.
        /// </summary>
        public abstract void Start();
        
        /// <summary>
        /// Stops running the engine.
        /// </summary>
        public abstract void Quit();

        /// <summary>
        /// Applies any changes made to the settings object.
        /// </summary>
        public abstract void ApplySettings();
    }
}