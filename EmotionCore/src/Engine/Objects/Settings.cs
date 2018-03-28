// Emotion - https://github.com/Cryru/Emotion

using System;

namespace Emotion.Engine.Objects
{
    public sealed class Settings
    {
        #region Window Settings

        public string WindowTitle = "Untitled";
        public int WindowWidth = 960;
        public int WindowHeight = 540;

        #endregion

        #region Render Settings

        public int RenderWidth = 960;
        public int RenderHeight = 540;

        #endregion

        public TimeSpan ScriptTimeout = new TimeSpan(0, 0, 0, 0, 500);
    }
}