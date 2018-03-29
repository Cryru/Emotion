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
        #region Objects

        /// <summary>
        /// The context's initial settings.
        /// </summary>
        internal Settings InitialSettings;

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
    }
}