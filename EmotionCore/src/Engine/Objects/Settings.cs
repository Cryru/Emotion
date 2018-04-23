// Emotion - https://github.com/Cryru/Emotion

#region Using

using System;
using Emotion.Engine.Enums;
using Emotion.Primitives;

#endregion

namespace Emotion.Engine.Objects
{
    public sealed class Settings
    {
        #region Window Settings

        public string WindowTitle = "Untitled";
        public int WindowWidth = 960;
        public int WindowHeight = 540;
        public WindowMode WindowMode = WindowMode.Windowed;

        #endregion

        #region Render Settings

        public Color ClearColor = Color.Black;

        public int RenderWidth = 960;
        public int RenderHeight = 540;

        public int CapFPS = 60;

        #endregion

        #region Scripting Settings

        /// <summary>
        /// The maximum time a script can run before it is stopped.
        /// </summary>
        public TimeSpan ScriptTimeout = new TimeSpan(0, 0, 0, 0, 500);

        /// <summary>
        /// Whether to crash on scripting errors.
        /// </summary>
        public bool StrictScripts = false;

        #endregion
    }
}