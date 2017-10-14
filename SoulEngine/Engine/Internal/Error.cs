using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Soul.Engine.Modules;
using Soul.Engine.Enums;

namespace Soul.Engine.Internal
{
    /// <summary>
    /// Handles errors within SoulEngine.
    /// </summary>
    internal static class Error
    {
        /// <summary>
        /// Raises an error.
        /// </summary>
        /// <param name="code">The error's code.</param>
        /// <param name="message">The error's message.</param>
        /// <param name="severity">The severity of the error.</param>
        internal static void Raise(int code, string message, Severity severity = Severity.Normal)
        {
            // Format the error.
            string errorMessage = Debugger.FormatDebugMessage(code.ToString(), message);

            // Log the error.
            Logger.Add(errorMessage);

            // Write the error to the debug.
            Debugger.DebugMessage(DebugMessageSource.Error, errorMessage);

            // If severe error dump the log and exit.
            if (severity == Severity.Critical)
            {
                Logger.ForceDump();
                throw new Exception(errorMessage);
            }
        }

        // Error List:

        // Core Errors
        // 0 - The core hasn't been started.
        // 1 - Invalid function called.
        // 3 - Invalid argument passed.
        // 4 - The child or child name is already attached to this parent.
        // Scripting Errors
        // 50 - Script execution error.
        // 51 - Script thread timed out.
        // 52 - Registered script function execution error.
        // SceneLoader Errors
        // 180 - Duplicate scene name.
        // 181 - Tried to unload the current scene.
        // 182 - Tried to swap to the current scene.
        // 183 - Tried to swap to a scene that wasn't loaded.
        // AssetLoader Errors
        // 238 - Tried loading an asset insecurely with no asset folder.
        // 239 - Tried loading an asset insecurely which doesn't exist.
        // 240 - Exception when locking assets.
        // 241 - Missing assets meta file.
        // 242 - Wrong assets meta hash.
        // 243 - Missing assets blob.
        // 244 - Failed to validate file.
        // Special Errors
        // 999 - SoulLib Error
    }

    public enum Severity
    {
        Normal,
        Critical
    }
}
