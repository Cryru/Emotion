// Emotion - https://github.com/Cryru/Emotion

#region Using

using System;
using System.Drawing;
using Emotion.Engine.Hosting.Desktop;

#endregion

namespace Emotion.Engine.Configuration
{
    /// <summary>
    /// Settings pertaining to the scripting engine module.
    /// </summary>
    public class ScriptingSettings
    {
        /// <summary>
        /// The maximum time a script can run before it is stopped.
        /// </summary>
        public TimeSpan Timeout = new TimeSpan(0, 0, 0, 0, 500);

        /// <summary>
        /// Whether to crash on scripting errors.
        /// </summary>
        public bool StrictScripts = false;
    }
}