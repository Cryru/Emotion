using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Soul.Engine.Modules;
using Soul.Engine.Enums;

namespace Soul.Engine
{
    /// <summary>
    /// Handles errors within SoulEngine.
    /// </summary>
    public static class Error
    {
        /// <summary>
        /// Raises an error.
        /// </summary>
        /// <param name="code">The error's code.</param>
        /// <param name="message">The error's message.</param>
        /// <param name="severity">The severity of the error.</param>
        public static void Raise(int code, string message, Severity severity = Severity.Normal)
        {
            // Format the error.
            string errorMessage = DebugModule.FormatDebugMessage(code.ToString(), message);

            // Log the error.
            Logger.Add(errorMessage);

            // Write the error to the debug.
            DebugModule.DebugMessage(DebugMessageSource.Error, errorMessage);

            // If severe error dump the log and exit.
            if (severity == Severity.Critical)
            {
                Logger.ForceDump();
                throw new Exception(errorMessage);
            }
        }

        // Error List:

        // 1 - Invalid function called.
        // 3 - Invalid argument passed.
        // 50 - Script execution error.
        // 51 - Script thread timed out.
        // 100 - Module failed to load.
        // 101 - Expected module wasn't loaded.
        // 180 - Duplicate scene name.
        // 181 - Tried to unload the current scene.
        // 182 - Tried to swap to the current scene.
        // 183 - Tried to swap to a scene that wasn't loaded.
        // 240 - Global assets failed to load.
        // 241 - meta.soul - Missing.
        // 242 - meta.soul - Wrong format.
        // 243 - meta.soul - Hash doesn't match expected.
        // 244 - Asset validation failed.
        // 244 - Failed to validate file.
    }

    public enum Severity
    {
        Normal,
        Critical
    }
}
